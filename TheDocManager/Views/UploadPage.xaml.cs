using Microsoft.Win32;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using TheDocManager.Services;

namespace TheDocManager.Views
{
    public partial class UploadPage : Page
    {
        private string? selectedFilePath = null;
        private static string UploadDirectory = Directory.CreateDirectory(AppPaths.DocumentsDirectory).ToString();
        private string _selectedUploadFolder = UploadDirectory;

        

        public UploadPage()
        {
            InitializeComponent();
            RefreshTreeView();
        }

        private void SelectFileButton_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog dlg = new()
            {
                Filter = "All Supported Files|*.pdf;*.docx;*.txt;*.rtf;*.jpg;*.jpeg;*.png;*.bmp|PDF Documents|*.pdf|Images|*.jpg;*.jpeg;*.png;*.bmp|All files|*.*"
            };

            if (dlg.ShowDialog() == true)
            {
                selectedFilePath = dlg.FileName;
                SelectedFileTextBlock.Text = selectedFilePath;
                UploadButton.IsEnabled = true;
            }
        }

        private void TreeView_selectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            if (e.NewValue is TreeViewItem selectedItem && selectedItem.Tag is string folderPath)
            {
                _selectedUploadFolder = folderPath;
            }
        }

        private void UploadButton_Click(object sender, RoutedEventArgs e)
        {
            if (selectedFilePath == null)
            {
                MessageBox.Show("Please select a file to upload.");
                return;
            }

            if (!Directory.Exists(_selectedUploadFolder))
            {
                MessageBox.Show("The selected folder does not exist.");
                return;
            }

            string originalFileName = Path.GetFileName(selectedFilePath);
            string destinationPath = Path.Combine(_selectedUploadFolder, originalFileName);

            try
            {
                File.Copy(selectedFilePath, destinationPath, overwrite: false);
                MessageBox.Show("File uploaded successfully!", "Upload Success", MessageBoxButton.OK, MessageBoxImage.Information);
                selectedFilePath = null;
                SelectedFileTextBlock.Text = "No file selected.";
                UploadButton.IsEnabled = false;
            }
            catch (IOException ex)
            {
                MessageBox.Show($"Upload failed:\n{ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void RefreshTreeView()
        {
            FolderTreeView.Items.Clear();
            if (Directory.Exists(UploadDirectory))
            {
                AddDirectoriesToTreeView(FolderTreeView.Items, UploadDirectory);
            }
        }

        private static void AddDirectoriesToTreeView(ItemCollection items, string path)
        {
            foreach (var dir in Directory.GetDirectories(path))
            {
                TreeViewItem item = new()
                {
                    Header = Path.GetFileName(dir),
                    Tag = dir
                };
                items.Add(item);
                AddDirectoriesToTreeView(item.Items, dir); // Recursively add subfolders
            }
        }
        private void ScanButton_Click(object sender, RoutedEventArgs e)
        {
            Window parentWindow = Window.GetWindow(this);
            var scanner = new ScanService(parentWindow);
            scanner.StartScan();
        }

    }
}
