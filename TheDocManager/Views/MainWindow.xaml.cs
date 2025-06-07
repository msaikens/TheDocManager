using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Animation;

namespace TheDocManager.Views
{
    public partial class MainWindow : Window
    {
        private readonly Storyboard collapseStoryboard;
        private readonly Storyboard expandStoryboard;

        public MainWindow()
        {
            InitializeComponent();

            if (Environment.Is64BitProcess)
            {
                MessageBox.Show("Please run this application as a 32-bi (x86) process to enable scanning functionality.", 
                    "Unsupported architecture for scanning", MessageBoxButton.OK, MessageBoxImage.Warning);
            }

            collapseStoryboard = (Storyboard)FindResource("CollapseSidebarStoryboard");
            expandStoryboard = (Storyboard)FindResource("ExpandSidebarStoryboard");
        }

        private void ToggleSidebarButton_Checked(object sender, RoutedEventArgs e)
        {
            collapseStoryboard.Begin();
        }

        private void ToggleSidebarButton_Unchecked(object sender, RoutedEventArgs e)
        {
            expandStoryboard.Begin();
        }

        // TODO: Navigation button click handlers to load pages in MainFrame
        private void DashboardButton_Click(object sender, RoutedEventArgs e)
        {
            // Example:
            // MainFrame.Navigate(new DashboardPage());
        }

        private void DocumentsButton_Click(object sender, RoutedEventArgs e)
        {
            MainFrame.Navigate(new DocumentsListPage());
        }

        private void UsersButton_Click(object sender, RoutedEventArgs e)
        {
            // MainFrame.Navigate(new UsersPage());
        }

        private void SettingsButton_Click(object sender, RoutedEventArgs e)
        {
            // MainFrame.Navigate(new SettingsPage());
        }

        private void LogoutButton_Click(object sender, RoutedEventArgs e)
        {
            // Implement logout logic
        }
    }
}
