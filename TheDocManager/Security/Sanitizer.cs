using System.IO;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;

namespace TheDocManager.Security
{
    public partial class Sanitizer
    {
        private static readonly string[] ReservedWindowsNames =
        [
            "CON", "PRN", "AUX", "NUL",
            "COM1", "COM2", "COM3", "COM4", "COM5", "COM6", "COM7", "COM8", "COM9",
            "LPT1", "LPT2", "LPT3", "LPT4", "LPT5", "LPT6", "LPT7", "LPT8", "LPT9"
        ];

        public static string[] ReservedWindowsNames1 => ReservedWindowsNames;

        /// <summary>
        /// Sanitizes a folder or file name by removing invalid characters and reserved names.
        /// </summary>
        public static string SanitizeFileOrFolderName(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
                throw new ArgumentException("Input cannot be empty.");

            // Remove invalid characters
            foreach (var c in Path.GetInvalidFileNameChars())
                input = input.Replace(c.ToString(), string.Empty);

            input = input.Trim();

            // Prevent reserved names
            if (ReservedWindowsNames.Contains(input.ToUpperInvariant()))
                throw new ArgumentException("The name provided is reserved or contains a reserved name and is not allowed.");

            return input;
        }

        /// <summary>
        /// Sanitizes a general user input string (e.g., usernames, search terms).
        /// Removes potentially dangerous characters.
        /// </summary>
        public static string SanitizeTextInput(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
                return string.Empty;

            // Basic XSS prevention: remove HTML/script tags
            string sanitized = Regex.Replace(input, @"<.*?>", string.Empty);

            // You can optionally trim and restrict length
            return sanitized.Trim();
        }

        /// <summary>
        /// Escapes inputs for SQL use when parameterized queries are not feasible (not recommended).
        /// </summary>
        /// <returns>A single quote escaped String value </returns>
        /// <remarks>This function sanitizes string inputs used in SQL when parameterization isn't used. A ' is repaced with ''.</remarks>
        /// <param name="input">SQL string to be escaped</param>
        public static string EscapeForSql(string input)
        {
            return input.Replace("'", "''");
        }
    }
}
