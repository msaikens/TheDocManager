using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using TheDocManager.Dialogs;
using TheDocManager.Models;
using TheDocManager.Security;

namespace TheDocManager.Views
{
    public partial class DocumentsListPage : Page
    {
        public ObservableCollection<FileSystemItem> Items { get; set; } = [];
        public static string RootFolderPath => Directory.CreateDirectory(path: AppPaths.DocumentsDirectory).ToString();
        private TreeViewItem? _lastHighlightedItem = null;

        public DocumentsListPage()
        {
            InitializeComponent();
            this.DataContext = this;

            if (!Directory.Exists(RootFolderPath))
                Directory.CreateDirectory(RootFolderPath);

            LoadFileSystem();
        }

        private void LoadFileSystem()
        {
            Items.Clear();
            var rootItems = Directory.GetDirectories(RootFolderPath)
                                     .Select(path => new FileSystemItem(path, true));
            var rootFiles = Directory.GetFiles(RootFolderPath)
                                     .Select(path => new FileSystemItem(path, false));

            foreach (var item in rootItems.Concat(rootFiles))
            {
                Items.Add(item);
            }
        }
        public void ShowDocumentPreview(string filePath)
        {
            throw new NotImplementedException();
        }
        private void TreeView_MouseDoubleClick(object sender, RoutedEventArgs routedEventArgs)
        {
            if ( DocumentsTreeView.SelectedItem is  FileSystemItem selectedItem && !selectedItem.IsDirectory && selectedItem is not null)
            {
                string filePath = selectedItem.FullPath;
                string fileName = selectedItem.Name ?? Path.GetFileName(filePath);
                DateTime uploadDate = DateTime.ParseExact(
                    selectedItem.UploadDate.ToString("yyyy-MM-dd"),
                    "yyyy-MM-dd",
                    CultureInfo.InvariantCulture);
                string fileType = selectedItem.FileType ?? Path.GetExtension(filePath).Trim('.').ToUpper();

                MainWindow mainWindow = (MainWindow)Application.Current.MainWindow;
                mainWindow.MainFrame.Navigate(new DocumentPreviewPage(filePath, fileName, uploadDate, fileType));
            }
        }
        private void DocumentsTreeView_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
#if DEBUG
            ArgumentNullException.ThrowIfNull(sender);
            ArgumentNullException.ThrowIfNull(e);
#endif        
            if (DocumentsTreeView.SelectedItem is FileSystemItem selectedItem)
            {
                if (!selectedItem.IsDirectory)
                {
                    // Load document preview here, e.g.:

                    var mainWindow = (MainWindow)Application.Current.MainWindow;
                    mainWindow?.MainFrame.Navigate(selectedItem.FullPath);
                }
            }
        }

        private void DocumentsTreeView_ContextMenuOpening(object sender, ContextMenuEventArgs e)
        {
            // Optional: dynamically generate context menu based on selection
            throw new NotImplementedException();
        }

        // ----------------------------
        // Drag & Drop Support
        // ----------------------------
        private void DocumentsTreeView_Drop(object sender, DragEventArgs e)
        {
            if (!e.Data.GetDataPresent(DataFormats.FileDrop)) return;

            var droppedFiles = (string[])e.Data.GetData(DataFormats.FileDrop)!;

            // Get the TreeViewItem the user dropped onto
            if (sender is TreeView treeView && treeView.SelectedItem is FileSystemItem targetItem)
            {
                string destinationPath = targetItem.IsDirectory ? targetItem.FullPath! : Path.GetDirectoryName(targetItem.FullPath!)!;

                foreach (var file in droppedFiles)
                {
                    try
                    {
                        string fileName = Path.GetFileName(file);
                        string destFilePath = Path.Combine(destinationPath, fileName);

                        if (Directory.Exists(file))
                        {
                            // It's a folder: copy the whole directory
                            string destFolderPath = Path.Combine(destinationPath, Path.GetFileName(file));
                            if (!Directory.Exists(destFolderPath))
                                DirectoryCopy(file, destFolderPath, true);
                        }
                        else if (File.Exists(file))
                        {
                            // It's a file: copy it
                            File.Copy(file, destFilePath, overwrite: false);
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Error copying file: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }

                LoadFileSystem(); // Reload tree
            }
        }

        private void DocumentsTreeView_PreviewDrop(object sender, DragEventArgs e)
        {
#if DEBUG
            ArgumentNullException.ThrowIfNull(sender);
            ArgumentNullException.ThrowIfNull(e);
#endif
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                string[] droppedPaths = (string[])e.Data.GetData(DataFormats.FileDrop);

                var targetItem = VisualUpwardSearch<TreeViewItem>(source: (DependencyObject)e.OriginalSource);

                string destinationPath = RootFolderPath;
                if (targetItem?.DataContext is FileSystemItem targetData && targetData.IsDirectory)
                {
                    destinationPath = targetData.FullPath;
                }

                foreach (string path in droppedPaths)
                {
                    try
                    {
                        string name = Path.GetFileName(path);
                        string dest = Path.Combine(destinationPath, name);

                        if (Directory.Exists(path))
                        {
                            if (Directory.Exists(dest))
                                Directory.Delete(dest, true);

                            CopyDirectory(path, dest);
                        }
                        else if (File.Exists(path))
                        {
                            File.Copy(path, dest, true);
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Error importing file/folder: {ex.Message}");
                    }
                }

                LoadFileSystem();
            }
        }
        private void DocumentsTreeView_DragLeave(object sender, DragEventArgs e)
        {
#if DEBUG
            ArgumentNullException.ThrowIfNull(sender);
            ArgumentNullException.ThrowIfNull(e);
#endif           
            if (_lastHighlightedItem != null)
            {
                _lastHighlightedItem.ClearValue(Border.BorderBrushProperty);
                _lastHighlightedItem.ClearValue(Border.BorderThicknessProperty);
                _lastHighlightedItem = null;
            }
        }

        private void DocumentsTreeView_DragOver(object sender, DragEventArgs e)
        {
#if DEBUG
            ArgumentNullException.ThrowIfNull(sender);
            ArgumentNullException.ThrowIfNull(e);
#endif         
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                e.Effects = DragDropEffects.Copy;

                var currentItem = VisualUpwardSearch<TreeViewItem>((DependencyObject)e.OriginalSource);

                if (_lastHighlightedItem != null && _lastHighlightedItem != currentItem)
                {
                    _lastHighlightedItem.ClearValue(Border.BorderBrushProperty);
                    _lastHighlightedItem.ClearValue(Border.BorderThicknessProperty);
                    _lastHighlightedItem = null;
                }

                if (currentItem != null && currentItem.DataContext is FileSystemItem fsItem && fsItem.IsDirectory)
                {
                    if (_lastHighlightedItem != currentItem)
                    {
                        _lastHighlightedItem = currentItem;
                        _lastHighlightedItem.BorderBrush = System.Windows.Media.Brushes.Blue;
                        _lastHighlightedItem.BorderThickness = new Thickness(2);
                    }
                }
            }
            else
            {
                e.Effects = DragDropEffects.None;
                if (_lastHighlightedItem != null)
                {
                    _lastHighlightedItem.ClearValue(Border.BorderBrushProperty);
                    _lastHighlightedItem.ClearValue(Border.BorderThicknessProperty);
                    _lastHighlightedItem = null;
                }
            }

            e.Handled = true;
        }

        private void DocumentsTreeView_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            // Required for certain drag behaviors
            throw new NotImplementedException();
        }

        private static T? VisualUpwardSearch<T>(DependencyObject source) where T : DependencyObject
        {
#if DEBUG
            ArgumentNullException.ThrowIfNull(source);
#endif
            while (source is not null and not T)
            {
                source = System.Windows.Media.VisualTreeHelper.GetParent(source);
            }
            return source as T;
        }

        private static void CopyDirectory(string sourceDir, string destinationDir)
        {
#if DEBUG
            ArgumentException.ThrowIfNullOrEmpty(sourceDir);
            ArgumentException.ThrowIfNullOrEmpty(destinationDir);
#endif
            Directory.CreateDirectory(destinationDir);

            foreach (string file in Directory.GetFiles(sourceDir))
            {
                string destFile = Path.Combine(destinationDir, Path.GetFileName(file));
                File.Copy(file, destFile, true);
            }

            foreach (string directory in Directory.GetDirectories(sourceDir))
            {
                string destSubDir = Path.Combine(destinationDir, Path.GetFileName(directory));
                CopyDirectory(directory, destSubDir);
            }
        }
        private void AddFolder_Click(object sender, RoutedEventArgs e)
        {
            if (DocumentsTreeView.SelectedItem is not FileSystemItem selectedItem)
            {
                MessageBox.Show("Please select a parent folder.", "Info", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            string parentPath = selectedItem.IsDirectory
                ? selectedItem.FullPath!
                : Path.GetDirectoryName(selectedItem.FullPath!)!;

            var inputDialog = new InputDialog("Create New Folder")
            {
                Owner = Window.GetWindow(this)
            };

            if (inputDialog.ShowDialog() == true && !string.IsNullOrWhiteSpace(inputDialog.ResponseText))
            {
                string newFolderName = Sanitizer.SanitizeFileOrFolderName(inputDialog.ResponseText);
                string newFolderPath = Path.Combine(parentPath, newFolderName);

                try
                {
                    if (Directory.Exists(newFolderPath))
                    {
                        MessageBox.Show("A folder with that name already exists.", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                        return;
                    }

                    Directory.CreateDirectory(newFolderPath);
                    LoadFileSystem();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Failed to create folder:\n{ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
        private void AddFile_Click(object sender, RoutedEventArgs e)
        {
            if (DocumentsTreeView.SelectedItem is FileSystemItem selectedItem && selectedItem.IsDirectory)
            {
                var dialog = new Microsoft.Win32.OpenFileDialog
                {
                    Title = "Select file to add",
                    Filter = "All Files|*.*",
                    Multiselect = false
                };

                if (dialog.ShowDialog() == true)
                {
                    string sourceFilePath = dialog.FileName;
                    string fileName = Path.GetFileName(sourceFilePath);
                    string sanitizedFileName = Security.Sanitizer.SanitizeFileOrFolderName(fileName);
                    string destinationPath = Path.Combine(selectedItem.FullPath, sanitizedFileName);

                    try
                    {
                        File.Copy(sourceFilePath, destinationPath, overwrite: false);
                        MessageBox.Show($"File '{sanitizedFileName}' added successfully.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                        RefreshTreeView();
                    }
                    catch (IOException ex)
                    {
                        MessageBox.Show($"Error copying file: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
            else
            {
                MessageBox.Show("Please right-click on a folder to add a file.", "Invalid Selection", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void DocumentsTreeView_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (DocumentsTreeView.SelectedItem is FileSystemItem selectedItem && !selectedItem.IsDirectory && selectedItem.FullPath is not null)
            {
                string filePath = selectedItem.FullPath;
                string fileName = selectedItem.Name ?? Path.GetFileName(filePath);
                DateTime uploadDate = 
                    DateTime.ParseExact(
                        selectedItem.UploadDate.ToString("yyyy-MM-dd"),
                        "yyyy-MM-dd",
                        CultureInfo.InvariantCulture);
                string fileType = selectedItem.FileType ?? Path.GetExtension(filePath).Trim('.').ToUpper();

                // Navigate to the preview page
                MainWindow mainWindow = (MainWindow)Application.Current.MainWindow;
                mainWindow.MainFrame.Navigate(new DocumentPreviewPage(filePath, fileName, uploadDate, fileType));
            }
        }
        private static void DirectoryCopy(string sourceDir, string destDir, bool copySubDirs)
        {
            DirectoryInfo dir = new(sourceDir);
            if (!dir.Exists)
            {
                throw new DirectoryNotFoundException("Source directory does not exist: " + sourceDir);
            }
            DirectoryInfo[] dirs = dir.GetDirectories();
            Directory.CreateDirectory(destDir);

            foreach (FileInfo file in dir.GetFiles())
            {
                string tempPath = Path.Combine(destDir, file.Name);
                file.CopyTo(tempPath, false);
            }

            if (copySubDirs)
            {       
                foreach (DirectoryInfo subdir in dirs)
                {
                    string tempPath = Path.Combine(destDir, subdir.Name);
                    DirectoryCopy(subdir.FullName, tempPath, copySubDirs);
                }
            }
        }
        private void RenameItem_Click(object sender, RoutedEventArgs e)
        {
            if (DocumentsTreeView.SelectedItem is FileSystemItem selectedItem)
            {
                string currentName = selectedItem.Name;
                string currentPath = selectedItem.FullPath;

                string? input = Microsoft.VisualBasic.Interaction.InputBox(
                    $"Enter a new name for '{currentName}':",
                    "Rename Item",
                    currentName);

                if (!string.IsNullOrWhiteSpace(input))
                {
                    string sanitized = Sanitizer.SanitizeFileOrFolderName(input.Trim());
                    string newPath = Path.Combine(Path.GetDirectoryName(currentPath)!, sanitized);

                    try
                    {
                        if (selectedItem.IsDirectory)
                            Directory.Move(currentPath, newPath);
                        else
                            File.Move(currentPath, newPath);

                        RefreshTreeView();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Failed to rename: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
        }
        private void DeleteItem_Click(object sender, RoutedEventArgs e)
        {
            if (DocumentsTreeView.SelectedItem is FileSystemItem selectedItem)
            {
                var confirm = MessageBox.Show(
                    $"Are you sure you want to delete '{selectedItem.Name}'?",
                    "Confirm Delete",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Warning);

                if (confirm == MessageBoxResult.Yes)
                {
                    try
                    {
                        if (selectedItem.IsDirectory)
                            Directory.Delete(selectedItem.FullPath, true); // recursive
                        else
                            File.Delete(selectedItem.FullPath);

                        RefreshTreeView();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Failed to delete: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
        }
        private void RefreshTreeView()
        {
            Items.Clear();
            LoadFileSystem();
        }

    }

}
