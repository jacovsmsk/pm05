using System;
using System.Data.Entity;
using System.Data.Entity.Core;
using System.IO;
using System.Linq;
using System.Text;

namespace pm05.Data
{
    public static class DatabaseDiagnostics
    {
        public static string RunFullReportSafe()
        {
            var report = new StringBuilder();
            var path = DatabaseBootstrap.GetDatabasePath();

            report.AppendLine("=== Диагностика базы данных ===");
            report.AppendLine($"Время: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
            report.AppendLine($"DataDirectory: {AppDomain.CurrentDomain.GetData("DataDirectory")}");
            report.AppendLine($"BaseDirectory: {AppDomain.CurrentDomain.BaseDirectory}");
            report.AppendLine($"Файл БД: {path}");
            report.AppendLine($"Существует: {File.Exists(path)}");

            if (File.Exists(path))
                report.AppendLine($"Размер: {new FileInfo(path).Length} байт (0 = пустой/битый файл)");

            report.AppendLine();
            AppendTables(report, path);
            return report.ToString();
        }

        public static string RunFullReport()
        {
            var report = new StringBuilder();
            var path = DatabaseBootstrap.GetDatabasePath();

            report.AppendLine("=== Диагностика базы данных ===");
            report.AppendLine($"Время: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
            report.AppendLine($"DataDirectory: {AppDomain.CurrentDomain.GetData("DataDirectory")}");
            report.AppendLine($"BaseDirectory: {AppDomain.CurrentDomain.BaseDirectory}");
            report.AppendLine($"Файл БД: {path}");
            report.AppendLine($"Существует: {File.Exists(path)}");

            if (File.Exists(path))
                report.AppendLine($"Размер: {new FileInfo(path).Length} байт (0 = пустой/битый файл)");

            report.AppendLine();
            AppendTables(report, path);
            report.AppendLine();

            if (DatabaseBootstrap.IsDatabaseValid(path))
                AppendEfSteps(report);
            else
                report.AppendLine("--- EF-тесты пропущены (база не инициализирована) ---");

            return report.ToString();
        }

        public static string FormatException(Exception ex)
        {
            var sb = new StringBuilder();
            var i = 0;
            for (var cur = ex; cur != null; cur = cur.InnerException, i++)
            {
                sb.AppendLine($"[{i}] {cur.GetType().Name}: {cur.Message}");
                if (cur is EntityException)
                    sb.AppendLine(cur.ToString());
            }
            return sb.ToString();
        }

        public static void SaveReportToFile(string report)
        {
            var logPath = Path.Combine(
                AppDomain.CurrentDomain.BaseDirectory,
                "db-diagnostic.log");
            File.WriteAllText(logPath, report, Encoding.UTF8);
        }

        private static void AppendTables(StringBuilder report, string path)
        {
            report.AppendLine("--- Таблицы в SQLite ---");
            if (!File.Exists(path) || new FileInfo(path).Length == 0)
            {
                report.AppendLine("(файл отсутствует или пуст — EF не сможет выполнять запросы)");
                return;
            }

            try
            {
                using (var conn = new System.Data.SQLite.SQLiteConnection($"Data Source={path};Version=3;"))
                {
                    conn.Open();
                    using (var cmd = conn.CreateCommand())
                    {
                        cmd.CommandText = "SELECT name FROM sqlite_master WHERE type = 'table' ORDER BY name";
                        using (var reader = cmd.ExecuteReader())
                        {
                            var any = false;
                            while (reader.Read())
                            {
                                any = true;
                                report.AppendLine("  • " + reader.GetString(0));
                            }
                            if (!any)
                                report.AppendLine("  (таблиц нет — нужен DatabaseBootstrap.EnsureReady())");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                report.AppendLine($"Ошибка чтения схемы: {ex.Message}");
            }
        }

        private static void AppendEfSteps(StringBuilder report)
        {
            report.AppendLine("--- Пошаговые тесты Entity Framework ---");

            Step(report, "1. Открыть контекст", () =>
            {
                using (var db = new ApplicationDbContext())
                    return "OK, connection: " + db.Database.Connection.ConnectionString;
            });

            Step(report, "2. Database.Exists()", () =>
            {
                using (var db = new ApplicationDbContext())
                    return db.Database.Exists().ToString();
            });

            Step(report, "3. SELECT Users (Any)", () =>
            {
                using (var db = new ApplicationDbContext())
                    return db.Users.Any().ToString();
            });

            Step(report, "4. SELECT Tours (Count)", () =>
            {
                using (var db = new ApplicationDbContext())
                    return db.Tours.Count().ToString();
            });

            Step(report, "5. LINQ LoginAttempts (как при входе)", () =>
            {
                using (var db = new ApplicationDbContext())
                {
                    var login = "test";
                    return db.LoginAttempts
                        .Where(a => a.Login == login)
                        .OrderByDescending(a => a.AttemptedAt)
                        .Take(10)
                        .ToList()
                        .Count
                        .ToString();
                }
            });
        }

        private static void Step(StringBuilder report, string name, Func<string> action)
        {
            try
            {
                var result = action();
                report.AppendLine($"{name}: OK ({result})");
            }
            catch (Exception ex)
            {
                report.AppendLine($"{name}: ОШИБКА");
                report.AppendLine($"       {ex.GetType().Name}: {ex.Message}");
                if (ex.InnerException != null)
                    report.AppendLine($"       Внутренняя: {ex.InnerException.GetType().Name}: {ex.InnerException.Message}");
            }
        }
    }
}
