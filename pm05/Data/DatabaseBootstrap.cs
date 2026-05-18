using System;
using System.Data;
using System.Data.SQLite;
using System.IO;
using System.Linq;
using pm05.Models;
using pm05.Services;

namespace pm05.Data
{
    public static class DatabaseBootstrap
    {
        private static readonly string[] RequiredTables =
        {
            "Roles", "Users", "LoginAttempts",
            "Countries", "Tours", "Clients", "Bookings", "Payments"
        };

        public static string GetDatabasePath()
        {
            var dataDir = AppDomain.CurrentDomain.GetData("DataDirectory") as string
                          ?? AppDomain.CurrentDomain.BaseDirectory;
            return Path.Combine(dataDir, "app.db");
        }

        public static bool IsDatabaseValid(string path)
        {
            return File.Exists(path)
                   && !IsEmptyFile(path)
                   && HasAllRequiredTables(path);
        }

        public static void EnsureReady()
        {
            var path = GetDatabasePath();
            var directory = Path.GetDirectoryName(path);
            if (!string.IsNullOrEmpty(directory))
                Directory.CreateDirectory(directory);

            if (!File.Exists(path) || IsEmptyFile(path))
            {
                if (File.Exists(path))
                    SafeDeleteDatabaseFile(path);
                CreateSchema(path);
            }
            else
            {
                UpgradeSchema(path);
            }

            if (!IsDatabaseValid(path))
            {
                throw new InvalidOperationException(
                    "Не удалось подготовить базу данных.\nУдалите app.db и запустите снова:\n" + path);
            }

            using (var db = new ApplicationDbContext())
            {
                SeedRolesIfEmpty(db);
                SeedDefaultUsersIfEmpty(db);
                SeedSampleDataIfEmpty(db);
            }
        }

        private static void CreateSchema(string path)
        {
            using (var conn = Open(path))
            using (var tx = conn.BeginTransaction())
            {
                Execute(conn, tx, @"
CREATE TABLE Roles (
    Id INTEGER PRIMARY KEY NOT NULL,
    Name TEXT NOT NULL
);");

                Execute(conn, tx, @"
CREATE TABLE Users (
    Id INTEGER PRIMARY KEY AUTOINCREMENT NOT NULL,
    Login TEXT NOT NULL,
    FullName TEXT NOT NULL,
    PasswordHash TEXT NOT NULL,
    Salt TEXT NOT NULL,
    RoleId INTEGER NOT NULL,
    IsActive INTEGER NOT NULL DEFAULT 1,
    CreatedAt DATETIME NOT NULL
);");

                Execute(conn, tx, @"
CREATE TABLE LoginAttempts (
    Id INTEGER PRIMARY KEY AUTOINCREMENT NOT NULL,
    Login TEXT NOT NULL,
    AttemptedAt DATETIME NOT NULL,
    IsSuccessful INTEGER NOT NULL
);");

                Execute(conn, tx, @"
CREATE TABLE Countries (
    Id INTEGER PRIMARY KEY AUTOINCREMENT NOT NULL,
    Name TEXT NOT NULL
);");

                Execute(conn, tx, @"
CREATE TABLE Tours (
    Id INTEGER PRIMARY KEY AUTOINCREMENT NOT NULL,
    Title TEXT NOT NULL,
    CountryId INTEGER NOT NULL,
    Price REAL NOT NULL,
    IsActive INTEGER NOT NULL DEFAULT 1
);");

                Execute(conn, tx, @"
CREATE TABLE Clients (
    Id INTEGER PRIMARY KEY AUTOINCREMENT NOT NULL,
    FullName TEXT NOT NULL,
    Passport TEXT NOT NULL DEFAULT '',
    Phone TEXT NOT NULL DEFAULT '',
    TotalSpent REAL NOT NULL DEFAULT 0
);");

                Execute(conn, tx, @"
CREATE TABLE Bookings (
    Id INTEGER PRIMARY KEY AUTOINCREMENT NOT NULL,
    TourId INTEGER NOT NULL,
    ClientId INTEGER NOT NULL,
    BookingDate DATETIME NOT NULL,
    Status TEXT NOT NULL
);");

                Execute(conn, tx, @"
CREATE TABLE Payments (
    Id INTEGER PRIMARY KEY AUTOINCREMENT NOT NULL,
    BookingId INTEGER NOT NULL,
    Amount REAL NOT NULL,
    PaymentDate DATETIME NOT NULL
);");

                tx.Commit();
            }

            SQLiteConnection.ClearAllPools();
        }

        private static void UpgradeSchema(string path)
        {
            using (var conn = Open(path))
            {
                EnsureTable(conn, "Roles", @"
CREATE TABLE Roles (
    Id INTEGER PRIMARY KEY NOT NULL,
    Name TEXT NOT NULL
);");

                EnsureColumn(conn, "Users", "FullName", "TEXT NOT NULL DEFAULT ''");
                EnsureColumn(conn, "Users", "IsActive", "INTEGER NOT NULL DEFAULT 1");
                Execute(conn, null, "UPDATE Users SET FullName = Login WHERE FullName = '' OR FullName IS NULL");

                EnsureTable(conn, "Countries", @"
CREATE TABLE Countries (
    Id INTEGER PRIMARY KEY AUTOINCREMENT NOT NULL,
    Name TEXT NOT NULL
);");

                EnsureTable(conn, "Tours", @"
CREATE TABLE Tours (
    Id INTEGER PRIMARY KEY AUTOINCREMENT NOT NULL,
    Title TEXT NOT NULL,
    CountryId INTEGER NOT NULL,
    Price REAL NOT NULL,
    IsActive INTEGER NOT NULL DEFAULT 1
);");

                EnsureTable(conn, "Clients", @"
CREATE TABLE Clients (
    Id INTEGER PRIMARY KEY AUTOINCREMENT NOT NULL,
    FullName TEXT NOT NULL,
    Passport TEXT NOT NULL DEFAULT '',
    Phone TEXT NOT NULL DEFAULT '',
    TotalSpent REAL NOT NULL DEFAULT 0
);");

                EnsureColumn(conn, "Clients", "Passport", "TEXT NOT NULL DEFAULT ''");
                EnsureColumn(conn, "Clients", "TotalSpent", "REAL NOT NULL DEFAULT 0");

                EnsureTable(conn, "Bookings", @"
CREATE TABLE Bookings (
    Id INTEGER PRIMARY KEY AUTOINCREMENT NOT NULL,
    TourId INTEGER NOT NULL,
    ClientId INTEGER NOT NULL,
    BookingDate DATETIME NOT NULL,
    Status TEXT NOT NULL
);");

                EnsureTable(conn, "Payments", @"
CREATE TABLE Payments (
    Id INTEGER PRIMARY KEY AUTOINCREMENT NOT NULL,
    BookingId INTEGER NOT NULL,
    Amount REAL NOT NULL,
    PaymentDate DATETIME NOT NULL
);");
            }

            SQLiteConnection.ClearAllPools();
        }

        private static SQLiteConnection Open(string path)
        {
            var conn = new SQLiteConnection($"Data Source={path};Version=3;");
            conn.Open();
            return conn;
        }

        private static void EnsureTable(SQLiteConnection conn, string table, string createSql)
        {
            if (!TableExists(conn, table))
                Execute(conn, null, createSql);
        }

        private static void EnsureColumn(SQLiteConnection conn, string table, string column, string definition)
        {
            if (!ColumnExists(conn, table, column))
                Execute(conn, null, $"ALTER TABLE {table} ADD COLUMN {column} {definition}");
        }

        private static bool ColumnExists(IDbConnection conn, string table, string column)
        {
            using (var cmd = conn.CreateCommand())
            {
                cmd.CommandText = $"PRAGMA table_info([{table}])";
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        if (string.Equals(reader.GetString(1), column, StringComparison.OrdinalIgnoreCase))
                            return true;
                    }
                }
            }
            return false;
        }

        private static void Execute(SQLiteConnection conn, SQLiteTransaction tx, string sql)
        {
            using (var cmd = conn.CreateCommand())
            {
                if (tx != null)
                    cmd.Transaction = tx;
                cmd.CommandText = sql;
                cmd.ExecuteNonQuery();
            }
        }

        private static bool IsEmptyFile(string path) => new FileInfo(path).Length == 0;

        private static bool HasAllRequiredTables(string path)
        {
            try
            {
                using (var conn = Open(path))
                {
                    foreach (var table in RequiredTables)
                    {
                        if (!TableExists(conn, table))
                            return false;
                    }
                }
                return true;
            }
            catch
            {
                return false;
            }
        }

        private static bool TableExists(IDbConnection conn, string tableName)
        {
            using (var cmd = conn.CreateCommand())
            {
                cmd.CommandText = "SELECT COUNT(*) FROM sqlite_master WHERE type = 'table' AND name = @name";
                var p = cmd.CreateParameter();
                p.ParameterName = "@name";
                p.Value = tableName;
                cmd.Parameters.Add(p);
                return Convert.ToInt32(cmd.ExecuteScalar()) > 0;
            }
        }

        private static void SafeDeleteDatabaseFile(string path)
        {
            if (string.IsNullOrEmpty(path) || !File.Exists(path))
                return;

            var directory = Path.GetDirectoryName(path);
            var baseName = Path.GetFileName(path);
            if (string.IsNullOrEmpty(directory) || string.IsNullOrEmpty(baseName))
                return;

            SQLiteConnection.ClearAllPools();
            GC.Collect();
            GC.WaitForPendingFinalizers();

            foreach (var file in Directory.GetFiles(directory, baseName + "*"))
            {
                for (var attempt = 0; attempt < 5; attempt++)
                {
                    try
                    {
                        if (File.Exists(file))
                            File.SetAttributes(file, FileAttributes.Normal);
                        File.Delete(file);
                        break;
                    }
                    catch (IOException) when (attempt < 4)
                    {
                        System.Threading.Thread.Sleep(150);
                    }
                }
            }
        }

        private static void SeedRolesIfEmpty(ApplicationDbContext db)
        {
            if (db.Roles.Any())
                return;

            db.Roles.Add(new Role { Id = RoleIds.Admin, Name = "admin" });
            db.Roles.Add(new Role { Id = RoleIds.Operator, Name = "operator" });
            db.Roles.Add(new Role { Id = RoleIds.User, Name = "user" });
            db.SaveChanges();
        }

        private static void SeedDefaultUsersIfEmpty(ApplicationDbContext db)
        {
            if (db.Users.Any())
                return;

            AddUser(db, "admin", "Директор / Администратор", "admin123", RoleIds.Admin);
            AddUser(db, "operator", "Менеджер по продажам", "oper123", RoleIds.Operator);
            AddUser(db, "user", "Клиент (тест)", "user123", RoleIds.User);
            db.SaveChanges();
        }

        private static void SeedSampleDataIfEmpty(ApplicationDbContext db)
        {
            if (db.Countries.Any())
                return;

            var turkey = new Country { Name = "Турция" };
            var egypt = new Country { Name = "Египет" };
            var uae = new Country { Name = "ОАЭ" };
            db.Countries.Add(turkey);
            db.Countries.Add(egypt);
            db.Countries.Add(uae);
            db.SaveChanges();

            db.Tours.Add(new Tour
            {
                Title = "Тур в Турцию, 7 ночей",
                CountryId = turkey.Id,
                Price = 50000m,
                IsActive = true
            });
            db.Tours.Add(new Tour
            {
                Title = "Отдых в Египте, Хургада",
                CountryId = egypt.Id,
                Price = 45000m,
                IsActive = true
            });
            db.Tours.Add(new Tour
            {
                Title = "Дубай, 5 ночей",
                CountryId = uae.Id,
                Price = 85000m,
                IsActive = true
            });
            db.SaveChanges();
        }

        private static void AddUser(ApplicationDbContext db, string login, string fullName, string password, int roleId)
        {
            var salt = PasswordHasher.GenerateSalt();
            db.Users.Add(new User
            {
                Login = login,
                FullName = fullName,
                PasswordHash = PasswordHasher.ComputeHash(password, salt),
                Salt = salt,
                RoleId = roleId,
                IsActive = true,
                CreatedAt = DateTime.Now
            });
        }
    }
}
