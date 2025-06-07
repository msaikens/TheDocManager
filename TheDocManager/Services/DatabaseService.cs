// Views/DatabaseService.cs
using Microsoft.Data.Sqlite;
using TheDocManager.Helpers;
using TheDocManager.Models;

namespace TheDocManager.Services
{
    public static class DatabaseService
    {
        private static readonly string _connectionString = "Data Source=TheDocManager.db";
        public static void Initialize()
        {
            using var connection = new SqliteConnection(_connectionString);
            connection.Open();

            var command = connection.CreateCommand();

            command.CommandText = @"
                CREATE TABLE IF NOT EXISTS Users (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    Username TEXT NOT NULL UNIQUE,
                    PasswordHash TEXT NOT NULL,
                    FullName TEXT NOT NULL,
                    Role TEXT NOT NULL DEFAULT 'USER'
                );
            ";

            command.ExecuteNonQuery();
        }

        // Validates username/password; returns User object on success, null if failed
        public static User? ValidateUser(string username, string password)
        {
#if DEBUG
            ArgumentException.ThrowIfNullOrEmpty(username);
            ArgumentException.ThrowIfNullOrEmpty(password);
#endif
            using var connection = new SqliteConnection(_connectionString);
            connection.Open();

            var command = connection.CreateCommand();
            command.CommandText = "SELECT * FROM Users WHERE Username = @username";
            command.Parameters.AddWithValue("@username", username);

            using var reader = command.ExecuteReader();
            if (reader.Read())
            {
                string storedHash = reader.GetString(reader.GetOrdinal("PasswordHash"));

                if (PasswordHasher.VerifyPassword(password, storedHash))
                {
                    return new User
                    {
                        Id = reader.GetInt32(reader.GetOrdinal("Id")),
                        Username = reader.GetString(reader.GetOrdinal("Username")),
                        FullName = reader.GetString(reader.GetOrdinal("FullName"))
                    };
                }
            }

            return null;
        }

        // Adds a new user with hashed password (for registration or seeding)
        public static bool AddUser(string username, string password, string fullName, string role, User? currentUser)
        {
#if DEBUG
            ArgumentException.ThrowIfNullOrEmpty(username);
            ArgumentException.ThrowIfNullOrEmpty(password);
            ArgumentException.ThrowIfNullOrEmpty(fullName);
            ArgumentException.ThrowIfNullOrEmpty(role);
#endif
            if (currentUser == null || currentUser.Role != "Admin")
                throw new UnauthorizedAccessException("Only administrators can add new users.");

            // existing insertion code, but also insert the role:
            using var connection = new SqliteConnection(_connectionString);
            connection.Open();

            var command = connection.CreateCommand();
            command.CommandText = @"
        INSERT INTO Users (Username, PasswordHash, FullName, Role) 
        VALUES (@username, @passwordHash, @fullName, @role);
    ";

            command.Parameters.AddWithValue("@username", username);
            command.Parameters.AddWithValue("@passwordHash", PasswordHasher.HashPassword(password));
            command.Parameters.AddWithValue("@fullName", fullName);
            command.Parameters.AddWithValue("@role", role);

            command.ExecuteNonQuery();
            return true;
        }
        public static List<Document> GetAllDocuments()
        {
            var documents = new List<Document>();

            using (var conn = new SqliteConnection(_connectionString))
            {
                conn.Open();
                string query = "SELECT FileName, FilePath, FileType, UploadDate FROM Documents";
                using var cmd = new SqliteCommand(query, conn);
                using var reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    documents.Add(new Document
                    {
                        FileName = reader["FileName"].ToString(),
                        FilePath = reader["FilePath"].ToString(),
                        FileType = reader["FileType"].ToString(),
                        UploadDate = reader["UploadDate"].ToString()
                    });
                }
            }

            return documents;
        }

    }
}
