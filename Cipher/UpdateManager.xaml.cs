using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace Cipher
{
    public partial class UpdateManager : Window
    {
        private UpdateInfo _updateInfo;
        private bool _isUpdateAvailable;
        private bool _isUpdating = false;

        public UpdateManager()
        {
            InitializeComponent();
            Owner = Application.Current.MainWindow;
            Loaded += UpdateManager_Loaded;
        }

        private async void UpdateManager_Loaded(object sender, RoutedEventArgs e)
        {
            await CheckForUpdatesAsync();
        }

        private async Task CheckForUpdatesAsync()
        {
            try
            {
                SetStatus("🔍", "Checking for updates...", "Checking...");
                ActionButton.IsEnabled = false;

                _updateInfo = await UpdateManagerCore.CheckForUpdatesAsync();

                if (_updateInfo == null)
                {
                    SetStatus("❌", "Failed to check for updates", "Check your internet connection");
                    CurrentVersionText.Text = $"v{MainWindow.Ver}";
                    NewVersionText.Text = "Unknown";
                    ChangelogText.Text = "Unable to check for updates. Please check your internet connection and try again.";
                    return;
                }

                string currentVersion = MainWindow.Ver;
                CurrentVersionText.Text = $"v{currentVersion}";
                NewVersionText.Text = $"v{_updateInfo.Version}";

                // Check if update is available using version comparison
                _isUpdateAvailable = UpdateManagerCore.IsUpdateAvailable(_updateInfo);

                if (_isUpdateAvailable)
                {
                    string shortNewHash = _updateInfo.CommitHash.Length > 7 ?
                        _updateInfo.CommitHash.Substring(0, 7) :
                        _updateInfo.CommitHash;

                    SetStatus("📢", "Update Available!", $"Version {_updateInfo.Version} is ready to install");
                    ChangelogText.Text = string.IsNullOrEmpty(_updateInfo.Changelog) ?
                        "No changelog provided." :
                        _updateInfo.Changelog;
                    ActionButton.Content = "Update Now";
                    ActionButton.IsEnabled = true;
                    FooterStatus.Text = $"Released: {_updateInfo.ReleaseDate} (Commit: {shortNewHash})";
                }
                else
                {
                    SetStatus("✅", "Up to Date!", $"You're running the latest version (v{currentVersion})");
                    ChangelogText.Text = "You are already running the latest version of Cipher.";
                    ActionButton.Content = "Up to date!";
                    ActionButton.IsEnabled = false;
                    FooterStatus.Text = "No update needed";
                }
            }
            catch (Exception ex)
            {
                SetStatus("❌", "Error checking for updates", ex.Message);
                ChangelogText.Text = $"Error: {ex.Message}";
                ActionButton.Content = "Error";
                ActionButton.IsEnabled = false;
            }
        }

        private void SetStatus(string icon, string title, string subtitle)
        {
            Dispatcher.Invoke(() =>
            {
                StatusIcon.Text = icon;
                StatusText.Text = title;
                FooterStatus.Text = subtitle;
            });
        }

        private async void ActionButton_Click(object sender, RoutedEventArgs e)
        {
            if (!_isUpdateAvailable || _isUpdating)
            {
                Close();
                return;
            }

            // Confirm update
            var result = MessageBox.Show(
                $"Are you sure you want to update Cipher?\n\n" +
                $"Current Version: v{MainWindow.Ver}\n" +
                $"New Version: {_updateInfo.Version}\n\n" +
                $"The application will close and restart after the update.",
                "Confirm Update",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question
            );

            if (result != MessageBoxResult.Yes)
                return;

            _isUpdating = true;
            ActionButton.IsEnabled = false;
            ActionButton.Content = "Updating...";
            UpdateProgress.Visibility = Visibility.Visible;

            try
            {
                var progress = new Progress<string>(msg =>
                {
                    Dispatcher.Invoke(() =>
                    {
                        FooterStatus.Text = msg;
                        if (msg.Contains("Download"))
                        {
                            StatusText.Text = "Downloading update...";
                            StatusIcon.Text = "⬇️";
                        }
                        else if (msg.Contains("Install"))
                        {
                            StatusText.Text = "Installing update...";
                            StatusIcon.Text = "⚙️";
                        }
                    });
                });

                bool success = await UpdateManagerCore.DownloadAndInstallUpdateAsync(_updateInfo, progress);

                if (!success)
                {
                    SetStatus("❌", "Update failed", "Please try again or download manually");
                    ActionButton.Content = "Close";
                    ActionButton.IsEnabled = true;
                    UpdateProgress.Visibility = Visibility.Collapsed;
                    _isUpdating = false;

                    MessageBox.Show(
                        "Update installation failed.\n\nPlease download manually from:\n" +
                        "https://github.com/Deadlineem/Cipher/releases/tag/nightly",
                        "Update Failed",
                        MessageBoxButton.OK,
                        MessageBoxImage.Error
                    );
                }
                // App will shutdown if successful
            }
            catch (Exception ex)
            {
                SetStatus("❌", "Update error", ex.Message);
                ActionButton.Content = "Close";
                ActionButton.IsEnabled = true;
                UpdateProgress.Visibility = Visibility.Collapsed;
                _isUpdating = false;
            }
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ButtonState == MouseButtonState.Pressed)
                this.DragMove();
        }
    }
}