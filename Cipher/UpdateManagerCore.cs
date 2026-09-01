using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows;

namespace Cipher
{
    public class UpdateInfo
    {
        public string Version { get; set; }
        public string CommitHash { get; set; }
        public string DownloadUrl { get; set; }
        public string ReleaseDate { get; set; }
        public string Changelog { get; set; }
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

                string tagName = release.GetProperty("tag_name").GetString();
                string commitHash = release.TryGetProperty("target_commitish", out var commitElement)
                    ? commitElement.GetString()
                    : string.Empty;

                string changelog = release.TryGetProperty("body", out var bodyElement)
                    ? bodyElement.GetString()
                    : string.Empty;

                string releaseDate = release.TryGetProperty("published_at", out var dateElement)
                    ? dateElement.GetString()
                    : string.Empty;

                string exeName = GetExecutableName();
                string downloadUrl = $"{BaseDownloadUrl}{exeName}";

                return new UpdateInfo
                {
                    Version = tagName ?? "unknown",
                    CommitHash = commitHash,
                    DownloadUrl = downloadUrl,
                    ReleaseDate = releaseDate,
                    Changelog = changelog
                };
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Update check failed: {ex.Message}");
                return null;
            }
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
                progress?.Report($"Downloading update...");

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

            string scriptContent = $@"@echo off
timeout /t 2 /nobreak > nul
taskkill /f /im ""{Path.GetFileName(currentExe)}"" > nul 2>&1
cd /d ""{Path.GetDirectoryName(currentExe)}""
if exist ""{Path.GetFileNameWithoutExtension(currentExe)}_backup.exe"" del ""{Path.GetFileNameWithoutExtension(currentExe)}_backup.exe""
ren ""{Path.GetFileName(currentExe)}"" ""{Path.GetFileNameWithoutExtension(currentExe)}_backup.exe""
copy ""{newExePath}"" ""{Path.GetFileName(currentExe)}""
timeout /t 1 /nobreak > nul
rmdir /s /q ""{tempFolder}"" 2>nul
start """" ""{currentExe}""
exit
";

            File.WriteAllText(scriptPath, scriptContent);
            return scriptPath;
        }

        public static string GetCurrentCommitHash()
        {
            try
            {
                var assembly = Assembly.GetEntryAssembly();
                var versionInfo = FileVersionInfo.GetVersionInfo(assembly.Location);

                string productVersion = versionInfo.ProductVersion;
                if (!string.IsNullOrEmpty(productVersion) && productVersion.Contains("+"))
                {
                    return productVersion.Split('+')[1];
                }

                return assembly.GetName().Version?.ToString() ?? "unknown";
            }
            catch
            {
                return "unknown";
            }
        }

        public static bool IsUpdateAvailable(UpdateInfo remote, string currentCommitHash)
        {
            if (remote == null || string.IsNullOrEmpty(remote.CommitHash))
                return false;

            if (string.IsNullOrEmpty(currentCommitHash))
                return true;

            return !remote.CommitHash.Equals(currentCommitHash, StringComparison.OrdinalIgnoreCase);
        }
    }
}