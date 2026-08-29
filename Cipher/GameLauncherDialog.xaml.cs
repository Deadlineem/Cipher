using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
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

            EnteredPath = input;
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
        /// Detects if the input is a Steam path and converts to protocol
        /// </summary>
        public static string DetectAndConvertPath(string input, string gameName)
        {
            if (string.IsNullOrEmpty(input))
                return input;

            // Check if it's already a protocol
            if (input.Contains("://"))
                return input;

            // Check if it's a Steam path
            if (input.Contains("Steam") &&
                input.Contains("steamapps"))
            {
                // Try to extract game name from path
                string[] steamIds = new string[]
                {
                    "1404210", // RDR2
                    "271590",  // GTA5
                    "1091500", // Cyberpunk 2077
                    "292030",  // Witcher 3
                    "489830",  // Skyrim
                    "377160",  // Fallout 4
                };

                // Look for known game names in the path
                string lowerInput = input.ToLower();
                if (lowerInput.Contains("rdr2") || lowerInput.Contains("red dead"))
                    return "steam://rungameid/1404210";
                if (lowerInput.Contains("gta5") || lowerInput.Contains("gta v"))
                    return "steam://rungameid/271590";
                if (lowerInput.Contains("cyberpunk"))
                    return "steam://rungameid/1091500";
                if (lowerInput.Contains("witcher"))
                    return "steam://rungameid/292030";
                if (lowerInput.Contains("skyrim"))
                    return "steam://rungameid/489830";
                if (lowerInput.Contains("fallout"))
                    return "steam://rungameid/377160";

                // If we can't detect the game, return as-is
                return input;
            }

            // Check if it's an Epic Games path
            if (input.Contains("Epic Games"))
            {
                string lowerInput = input.ToLower();
                if (lowerInput.Contains("rdr2") || lowerInput.Contains("red dead"))
                    return "com.epicgames.launcher://apps/rdr2?action=launch";
                if (lowerInput.Contains("gta5") || lowerInput.Contains("gta v"))
                    return "com.epicgames.launcher://apps/gta5?action=launch";
                return input;
            }

            // Check if it's a Rockstar Games path
            if (input.Contains("Rockstar Games"))
            {
                string lowerInput = input.ToLower();
                if (lowerInput.Contains("rdr2") || lowerInput.Contains("red dead"))
                    return "rockstargames://launch/rdr2";
                if (lowerInput.Contains("gta5") || lowerInput.Contains("gta v"))
                    return "rockstargames://launch/gta5";
                return input;
            }

            return input;
        }

        /// <summary>
        /// Launches the game using the provided path/protocol
        /// </summary>
        public static bool LaunchGame(string launchPath)
        {
            try
            {
                if (string.IsNullOrEmpty(launchPath))
                    return false;

                // Check if it's a protocol (contains ://)
                if (launchPath.Contains("://"))
                {
                    Process.Start(new ProcessStartInfo
                    {
                        FileName = launchPath,
                        UseShellExecute = true
                    });
                    return true;
                }

                // Check if it's a file path
                if (File.Exists(launchPath))
                {
                    Process.Start(launchPath);
                    return true;
                }

                return false;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"❌ LaunchGame error: {ex.Message}");
                return false;
            }
        }
    }
}