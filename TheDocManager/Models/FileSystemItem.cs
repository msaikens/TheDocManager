using System.Collections.ObjectModel;
using System.IO;

namespace TheDocManager.Models
{
    public class FileSystemItem
    {
        public string Name { get; set; } = string.Empty;
        public string FullPath { get; set; } = string.Empty;
        public bool IsDirectory { get; set; }
        public string FileType { get; set; } = string.Empty;
        public DateTime UploadDate { get; set; }
        public string Icon => IsDirectory ? "Icons/folder.png" : "Icons/file.png";
        public ObservableCollection<FileSystemItem> Children { get; set; } = [];
        
        public FileSystemItem()
        {
        }


        public FileSystemItem(string path, bool isDirectory)
        {
            FullPath = path;
            Name = System.IO.Path.GetFileName(path);
            IsDirectory = isDirectory;
            FileType = isDirectory ? string.Empty : Path.GetExtension(path)?.TrimStart('.').ToUpper() ?? string.Empty;
            UploadDate = File.GetCreationTime(path);
        }
        public void LoadChildren()
        {
            if (!IsDirectory || !Directory.Exists(FullPath)) return;

            Children.Clear();

            foreach (var dir in Directory.GetDirectories(FullPath))
            {
                Children.Add(new FileSystemItem
                {
                    Name = Path.GetFileName(dir),
                    FullPath = dir,
                    IsDirectory = true
                });
            }

            foreach (var file in Directory.GetFiles(FullPath))
            {
                Children.Add(new FileSystemItem
                {
                    Name = Path.GetFileName(file),
                    FullPath = file,
                    IsDirectory = false
                });
            }
        }

    }

}
