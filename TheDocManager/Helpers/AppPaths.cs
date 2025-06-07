using System.IO;

public static class AppPaths
{
    public static string DocumentsDirectory =>
        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "TheDocManager", "DocumentsUp");
}
