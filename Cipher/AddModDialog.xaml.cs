using System;
using System.Windows;
using System.Windows.Input;

namespace Cipher
{
    public partial class AddModDialog : Window
    {
        public string ModName => NameTextBox.Text.Trim();
        public string GameTask => GameTextBox.Text.Trim();
        public string DownloadUrl => UrlTextBox.Text.Trim();

        public AddModDialog()
        {
            InitializeComponent();
            this.Loaded += (s, e) => NameTextBox.Focus();
        }

        // Allow dragging the window
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

        private void AddButton_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(ModName))
            {
                MessageBox.Show("Please enter a mod name!", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                NameTextBox.Focus();
                return;
            }

            if (string.IsNullOrEmpty(GameTask))
            {
                MessageBox.Show("Please enter a game task (e.g., RDR2.exe)!", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                GameTextBox.Focus();
                return;
            }

            if (string.IsNullOrEmpty(DownloadUrl))
            {
                MessageBox.Show("Please enter a download URL!", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                UrlTextBox.Focus();
                return;
            }

            if (!Uri.IsWellFormedUriString(DownloadUrl, UriKind.Absolute))
            {
                MessageBox.Show("Please enter a valid URL!", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                UrlTextBox.Focus();
                return;
            }

            DialogResult = true;
            Close();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        private void HandleKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                AddButton_Click(sender, e);
            }
        }
    }
}