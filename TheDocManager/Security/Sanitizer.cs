// Security/Sanitizer.cs
using System.Text.RegularExpressions;

namespace TheDocManager.Security
{
    public static partial class Sanitizer
    {
        // === Regex Definitions ===

        [GeneratedRegex(@"[\\/:*?""<>|]", RegexOptions.Compiled)]
        private static partial Regex InvalidCharsRegex();

        [GeneratedRegex(@"<script[^>]*?>.*?</script>", RegexOptions.IgnoreCase | RegexOptions.Compiled)]
        private static partial Regex ScriptTagRegex();

        [GeneratedRegex(@"<.*?>", RegexOptions.Compiled)]
        private static partial Regex HtmlTagRegex();
        /// <summary>
        /// Sanitizes a file or folder name by removing invalid characters.
        /// </summary>
        public static string SanitizeFileOrFolderName(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
                return "Unnamed";

            return InvalidCharsRegex().Replace(input, "").Trim();
        }

        /// <summary>
        /// Sanitizes input for general display or storage (e.g., user input in forms).
        /// </summary>
        public static string SanitizeUserInput(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
                return string.Empty;

            string safe = input.Trim();

            // Remove script tags and HTML
            safe = ScriptTagRegex().Replace(safe, string.Empty);
            safe = HtmlTagRegex().Replace(safe, string.Empty);

            // Normalize quotes to avoid injection vectors
            safe = safe.Replace("\"", "")
                       .Replace("'", "")
                       .Replace(";", "")
                       .Replace("--", "");

            return safe;
        }


    }
}
