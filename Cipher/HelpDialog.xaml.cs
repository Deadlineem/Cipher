using System.Windows;
using System.Windows.Input;

namespace Cipher
{
    public partial class HelpDialog : Window
    {
        public HelpDialog()
        {
            InitializeComponent();
            DataContext = this;
        }

        // Set the exclusion path to display in the dialog
        public string ExclusionPath { get; set; } = "";

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
    }
}