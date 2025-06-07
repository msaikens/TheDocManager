using System.Configuration;
using System.Data;
using System.IO;
using System.Windows;
using TheDocManager.Services;

namespace TheDocManager
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            // Ensure DB is initialized
            DatabaseService.Initialize();

            string appDirectory = Directory.CreateDirectory(AppPaths.DocumentsDirectory).ToString();

            if (!Directory.Exists(appDirectory))
            {
                Directory.CreateDirectory(appDirectory);
            }

            // Show Login Window
            var loginView = new Views.LoginWindow();
            loginView.Show();
        }

    }
}

