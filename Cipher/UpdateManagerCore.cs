using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;

namespace Cipher
{
    public class UpdateInfo
    {
        public string Version { get; set; }          // e.g., "1.0.18"
        public string DownloadUrl { get; set; }
        public string ReleaseDate { get; set; }      // Formatted for USA EST
        public string Changelog { get; set; }
        public string CommitHash { get; set; }
        public string TagName { get; set; }          // "nightly"
    }

    public static class UpdateManagerCore
    {
        private static readonly HttpClient client = new HttpClient();
        private static readonly string UpdateInfoUrl = "https://api.github.com/repos/Deadlineem/Cipher/releases/tags/nightly";
        private static readonly string BaseDownloadUrl = "https://github.com/Deadlineem/Cipher/releases/download/nightly/";

        private static string GetExecutableName()
        {
            string currentExe = Process.GetCurrentProcess().MainModule.FileName;
            string fileName = Path.GetFileName(currentExe);

            if (fileName.Contains("x64"))
                return "Cipher_x64.exe";
            else if (fileName.Contains("x86"))
                return "Cipher_x86.exe";

            return Environment.Is64BitProcess ? "Cipher_x64.exe" : "Cipher_x86.exe";
        }

        // Parse version string like "1.0.17" or "v1.0.17" to Version object
        private static Version ParseVersion(string versionStr)
        {
            if (string.IsNullOrEmpty(versionStr))
                return null;

            // Remove any non-version characters (e.g., "v1.0.17" -> "1.0.17")
            string cleanVersion = versionStr.TrimStart('v', 'V');

            // Remove anything after a space or newline
            int spaceIndex = cleanVersion.IndexOfAny(new char[] { ' ', '\n', '\r' });
            if (spaceIndex > 0)
                cleanVersion = cleanVersion.Substring(0, spaceIndex);

            if (Version.TryParse(cleanVersion, out var version))
                return version;

            return null;
        }

        // Extract version from release body text
        private static string ExtractVersionFromBody(string body)
        {
            if (string.IsNullOrEmpty(body))
                return null;

            // Look for "Version: X.Y.Z" in the body
            var match = Regex.Match(body, @"Version:\s*([\d.]+)", RegexOptions.IgnoreCase);
            if (match.Success)
            {
                return match.Groups[1].Value;
            }

            return null;
        }

        // Convert UTC to EST and format for USA
        private static string FormatReleaseDate(string utcDateString)
        {
            if (string.IsNullOrEmpty(utcDateString))
                return "Unknown";

            try
            {
                // Parse the UTC date from GitHub (ISO 8601 format)
                DateTime utcDate = DateTime.Parse(utcDateString, null, System.Globalization.DateTimeStyles.RoundtripKind);

                // Convert to EST (Eastern Standard Time)
                TimeZoneInfo estZone = TimeZoneInfo.FindSystemTimeZoneById("Eastern Standard Time");
                DateTime estDate = TimeZoneInfo.ConvertTimeFromUtc(utcDate, estZone);

                // Format for USA: "March 15, 2026 at 2:30 PM EST"
                return estDate.ToString("MMMM d, yyyy 'at' h:mm tt") + " EST";
            }
            catch
            {
                // Fallback: return the original string if conversion fails
                return utcDateString;
            }
        }

        public static async Task<UpdateInfo> CheckForUpdatesAsync()
        {
            try
            {
                client.DefaultRequestHeaders.Clear();
                client.DefaultRequestHeaders.Add("User-Agent", "Cipher-UpdateChecker");

                var response = await client.GetAsync(UpdateInfoUrl);
                if (!response.IsSuccessStatusCode)
                    return null;

                string json = await response.Content.ReadAsStringAsync();
                var release = JsonSerializer.Deserialize<JsonElement>(json);

                // Get tag name (always "nightly")
                string tagName = release.GetProperty("tag_name").GetString();

                // Get the release body
                string body = release.TryGetProperty("body", out var bodyElement)
                    ? bodyElement.GetString()
                    : string.Empty;

                // Extract version from body
                string version = ExtractVersionFromBody(body) ?? "unknown";

                // Get commit hash from target_commitish
                string commitHash = release.TryGetProperty("target_commitish", out var commitElement)
                    ? commitElement.GetString()
                    : string.Empty;

                // Get release date and format it for USA EST
                string rawDate = release.TryGetProperty("published_at", out var dateElement)
                    ? dateElement.GetString()
                    : string.Empty;
                string formattedDate = FormatReleaseDate(rawDate);

                string exeName = GetExecutableName();
                string downloadUrl = $"{BaseDownloadUrl}{exeName}";

                return new UpdateInfo
                {
                    Version = version,
                    TagName = tagName,
                    CommitHash = commitHash,
                    DownloadUrl = downloadUrl,
                    ReleaseDate = formattedDate,
                    Changelog = body
                };
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Update check failed: {ex.Message}");
                return null;
            }
        }

        public static bool IsUpdateAvailable(UpdateInfo remote)
        {
            if (remote == null || string.IsNullOrEmpty(remote.Version))
                return false;

            // Get current version from MainWindow
            string currentVersionStr = MainWindow.Ver;
            Version currentVersion = ParseVersion(currentVersionStr);
            Version remoteVersion = ParseVersion(remote.Version);

            if (currentVersion == null || remoteVersion == null)
            {
                // Fallback: compare as strings if parsing fails
                return !string.Equals(currentVersionStr, remote.Version, StringComparison.OrdinalIgnoreCase);
            }

            // Compare versions - update available if remote version is greater
            return remoteVersion > currentVersion;
        }

        public static async Task<bool> DownloadAndInstallUpdateAsync(UpdateInfo update, IProgress<string> progress = null)
        {
            try
            {
                string currentExe = Process.GetCurrentProcess().MainModule.FileName;
                string exeName = Path.GetFileName(currentExe);

                string tempFolder = Path.Combine(Path.GetTempPath(), "CipherUpdate");
                if (!Directory.Exists(tempFolder))
                    Directory.CreateDirectory(tempFolder);

                string tempExePath = Path.Combine(tempFolder, exeName);
                progress?.Report($"Downloading update v{update.Version}...");

                client.DefaultRequestHeaders.Clear();
                client.DefaultRequestHeaders.Add("User-Agent", "Cipher-Updater");

                var response = await client.GetAsync(update.DownloadUrl);
                if (!response.IsSuccessStatusCode)
                {
                    progress?.Report($"Failed to download update (HTTP {response.StatusCode})");
                    return false;
                }

                using (var fs = new FileStream(tempExePath, FileMode.Create, FileAccess.Write, FileShare.None))
                {
                    await response.Content.CopyToAsync(fs);
                }

                progress?.Report("Download complete. Installing update...");

                string scriptPath = CreateUpdateScript(currentExe, tempExePath, tempFolder);

                Process.Start(new ProcessStartInfo
                {
                    FileName = scriptPath,
                    UseShellExecute = true,
                    WindowStyle = ProcessWindowStyle.Hidden
                });

                Application.Current.Shutdown();
                return true;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Update installation failed: {ex.Message}");
                return false;
            }
        }

        private static string CreateUpdateScript(string currentExe, string newExePath, string tempFolder)
        {
            string scriptPath = Path.Combine(tempFolder, "update.bat");
            string exeName = Path.GetFileName(currentExe);
            string exeNameNoExt = Path.GetFileNameWithoutExtension(currentExe);
            string exeDir = Path.GetDirectoryName(currentExe);
            string version = MainWindow.Ver;

            string scriptContent = $@"@echo off
echo Updating Cipher to version {version}...
timeout /t 2 /nobreak > nul

:: Kill any running instances
taskkill /f /im ""{exeName}"" > nul 2>&1

cd /d ""{exeDir}""

:: Backup current version with version number
if exist ""{exeNameNoExt}_v{version}_backup.exe"" del ""{exeNameNoExt}_v{version}_backup.exe""
ren ""{exeName}"" ""{exeNameNoExt}_v{version}_backup.exe""

:: Copy new version
copy ""{newExePath}"" ""{exeName}""

:: Clean up
timeout /t 1 /nobreak > nul
rmdir /s /q ""{tempFolder}"" 2>nul

echo Update complete! Starting Cipher...
start """" ""{currentExe}""
exit
";

            File.WriteAllText(scriptPath, scriptContent);
            return scriptPath;
        }

        public static string GetCurrentVersion()
        {
            return MainWindow.Ver;
        }
    }
}