using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Input;

namespace Cipher
{
    public partial class GameLauncherDialog : Window
    {
        public string GameName { get; set; }
        public string GameTask { get; set; }
        public string EnteredPath { get; private set; }
        public bool SaveLocation { get; private set; }

        public GameLauncherDialog(string gameName, string gameTask)
        {
            InitializeComponent();
            DataContext = this;
            GameName = gameName;
            GameTask = gameTask;
            Title = $"🎮 Launch {gameName}";
        }

        public string GameDisplayName => $"🎮 {GameName}";

        private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ButtonState == MouseButtonState.Pressed)
                this.DragMove();
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        private void BrowseButton_Click(object sender, RoutedEventArgs e)
        {
            var openFileDialog = new Microsoft.Win32.OpenFileDialog
            {
                Title = $"Select {GameTask}",
                Filter = "Executable files (*.exe)|*.exe|All files (*.*)|*.*",
                FileName = GameTask
            };

            if (openFileDialog.ShowDialog() == true)
            {
                PathTextBox.Text = openFileDialog.FileName;
            }
        }

        private void LaunchButton_Click(object sender, RoutedEventArgs e)
        {
            string input = PathTextBox.Text.Trim();
            if (string.IsNullOrEmpty(input))
            {
                MessageBox.Show("Please enter a path or protocol!", "Input Required", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // FIXED: Run raw file strings through conversion checks before passing back
            EnteredPath = DetectAndConvertPath(input);
            SaveLocation = SaveCheckBox.IsChecked == true;
            DialogResult = true;
            Close();
        }

        private void PathTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                LaunchButton_Click(sender, e);
            }
        }

        /// <summary>
        /// Detects if the input is a Steam/Epic path and converts it to a protocol URI string.
        /// </summary>
        public static string DetectAndConvertPath(string inputPath)
        {
            if (string.IsNullOrEmpty(inputPath)) return inputPath;
            if (inputPath.Contains("://")) return inputPath; // Already a URI protocol

            try
            {
                // Normalize formatting to absolute directory format
                string cleanPath = Path.GetFullPath(inputPath).TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
                string parentDir = Directory.Exists(cleanPath) ? cleanPath : Path.GetDirectoryName(cleanPath);

                if (string.IsNullOrEmpty(parentDir)) return inputPath;

                // 1. Check if the directory belongs to an Epic Games installation
                string epicProtocol = CheckEpicManifests(parentDir);
                if (!string.IsNullOrEmpty(epicProtocol)) return epicProtocol;

                // 2. Check if the directory belongs to a Steam Library installation
                string steamProtocol = CheckSteamManifests(parentDir);
                if (!string.IsNullOrEmpty(steamProtocol)) return steamProtocol;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"⚠️ Path parsing fallback triggered: {ex.Message}");
            }

            // 3. Fallback: Return original path if standalone or DRM-free
            return inputPath;
        }

        /// <summary>
        /// Scans Epic's persistent metadata manifests to pair folders with App IDs.
        /// </summary>
        private static string CheckEpicManifests(string targetDir)
        {
            string manifestFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), @"Epic\EpicGamesLauncher\Data\Manifests");
            if (!Directory.Exists(manifestFolder)) return null;

            foreach (string file in Directory.GetFiles(manifestFolder, "*.item"))
            {
                try
                {
                    string content = File.ReadAllText(file);

                    string installLocationMatch = Regex.Match(content, @"""InstallLocation""\s*:\s*""([^""]+)""", RegexOptions.IgnoreCase).Groups[1].Value;
                    string appNameMatch = Regex.Match(content, @"""AppName""\s*:\s*""([^""]+)""", RegexOptions.IgnoreCase).Groups[1].Value;

                    if (string.IsNullOrEmpty(installLocationMatch) || string.IsNullOrEmpty(appNameMatch)) continue;

                    string manifestPath = Path.GetFullPath(installLocationMatch.Replace("\\\\", "\\")).TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);

                    if (targetDir.Equals(manifestPath, StringComparison.OrdinalIgnoreCase) || targetDir.StartsWith(manifestPath + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase))
                    {
                        return $"com.epicgames.launcher://apps/{appNameMatch}?action=launch&silent=true";
                    }
                }
                catch { /* Skip locked items */ }
            }
            return null;
        }

        /// <summary>
        /// Scans local appmanifest files to pair subfolders with their Steam App IDs.
        /// </summary>
        private static string CheckSteamManifests(string targetDir)
        {
            string registryPath = (string)Microsoft.Win32.Registry.GetValue(@"HKEY_CURRENT_USER\Software\Valve\Steam", "SteamPath", null)
                               ?? (string)Microsoft.Win32.Registry.GetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\WOW6432Node\Valve\Steam", "InstallPath", null);

            if (string.IsNullOrEmpty(registryPath)) return null;

            string primarySteamApps = Path.Combine(registryPath, "steamapps");
            string matchedId = ScanSteamAppsDirectory(primarySteamApps, targetDir);
            if (!string.IsNullOrEmpty(matchedId)) return $"steam://rungameid/{matchedId}";

            string libraryFoldersVdf = Path.Combine(primarySteamApps, "libraryfolders.vdf");
            if (File.Exists(libraryFoldersVdf))
            {
                try
                {
                    string vdfContent = File.ReadAllText(libraryFoldersVdf);
                    var pathMatches = Regex.Matches(vdfContent, @"""path""\s+""([^""]+)""", RegexOptions.IgnoreCase);

                    foreach (Match match in pathMatches)
                    {
                        string altPath = match.Groups[1].Value.Replace("\\\\", "\\");
                        string altSteamApps = Path.Combine(altPath, "steamapps");

                        matchedId = ScanSteamAppsDirectory(altSteamApps, targetDir);
                        if (!string.IsNullOrEmpty(matchedId)) return $"steam://rungameid/{matchedId}";
                    }
                }
                catch { /* Absorb parse failures safely */ }
            }
            return null;
        }

        private static string ScanSteamAppsDirectory(string steamAppsFolder, string targetDir)
        {
            if (!Directory.Exists(steamAppsFolder)) return null;

            foreach (string file in Directory.GetFiles(steamAppsFolder, "appmanifest_*.acf"))
            {
                try
                {
                    string content = File.ReadAllText(file);
                    string appId = Regex.Match(content, @"""appid""\s+""([^""]+)""", RegexOptions.IgnoreCase).Groups[1].Value;
                    string installDir = Regex.Match(content, @"""installdir""\s+""([^""]+)""", RegexOptions.IgnoreCase).Groups[1].Value;

                    if (string.IsNullOrEmpty(appId) || string.IsNullOrEmpty(installDir)) continue;

                    string fullGameInstallPath = Path.Combine(steamAppsFolder, "common", installDir);
                    string normalizedGamePath = Path.GetFullPath(fullGameInstallPath).TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);

                    if (targetDir.Equals(normalizedGamePath, StringComparison.OrdinalIgnoreCase) || targetDir.StartsWith(normalizedGamePath + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase))
                    {
                        return appId;
                    }
                }
                catch { /* Skip locked logs */ }
            }
            return null;
        }

        /// <summary>
        /// Launches the game using the provided path/protocol.
        /// </summary>
        public static bool LaunchGame(string launchPath)
        {
            try
            {
                if (string.IsNullOrEmpty(launchPath)) return false;

                // FIXED: Enforce UseShellExecute on file paths to avoid runtime initialization errors in .NET Core/5+
                Process.Start(new ProcessStartInfo
                {
                    FileName = launchPath,
                    UseShellExecute = true
                });
                return true;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"❌ LaunchGame error: {ex.Message}");
                return false;
            }
        }
    }
}
