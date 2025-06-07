using System.Windows;

namespace TheDocManager.Dialogs
{
    public partial class InputDialog : Window
    {
        public string? ResponseText { get; private set; }

        public InputDialog(string title)
        {
            InitializeComponent();
            Title = title;
            InputTextBox.Focus();
        }

        private void Ok_Click(object sender, RoutedEventArgs e)
        {
            ResponseText = InputTextBox.Text.Trim();
            DialogResult = true;
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }
    }
}
