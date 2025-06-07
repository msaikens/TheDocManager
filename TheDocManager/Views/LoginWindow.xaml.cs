// Views/LoginWindow.xaml.cs
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using TheDocManager.Services;
using TheDocManager.Views;

namespace TheDocManager.Views
{
    public partial class LoginWindow : Window
    {
        public LoginWindow() => InitializeComponent();

        private void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            txtWarning.Text = string.Empty;
            string? username = UsernameTextBox.Text;
            string? password = PasswordBox.Password;

            // TODO: Add authentication call here
            var user = DatabaseService.ValidateUser(username, password);

            if (user != null)
            {
                SessionManager.SetCurrentUser(user);

                MainWindow mainWindow = new();
                mainWindow.Show();
                Close();
            }
            else
            {
                txtWarning.Visibility = Visibility.Visible;
                txtWarning.Text = "ERROR: Username or password invalid.";
            }
        }
    }
}
