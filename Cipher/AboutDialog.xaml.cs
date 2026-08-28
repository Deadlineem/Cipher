using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Input;

namespace Cipher
{
    public partial class AboutDialog : Window
    {
        public string BuildDate { get; set; }

        public AboutDialog()
        {
            InitializeComponent();
            DataContext = this;
            BuildDate = DateTime.Now.ToString("yyyy.MM.dd");
        }

        // Allow dragging the window
        private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ButtonState == MouseButtonState.Pressed)
                this.DragMove();
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void GitHubLink_Click(object sender, MouseButtonEventArgs e)
        {
            try
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = "https://github.com/Deadlineem/Cipher",
                    UseShellExecute = true
                });
            }
            catch
            {
                MessageBox.Show(
                    "Unable to open GitHub page.\nPlease visit:\nhttps://github.com/Deadlineem/Cipher",
                    "GitHub Link",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information
                );
            }
        }
    }
}