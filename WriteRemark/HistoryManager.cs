using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.IO;
using System.Linq;
using System.Reflection;

namespace WriteRemark
{
    /// <summary>
    /// 历史记录管理器 - 使用SQLite存储输入历史
    /// </summary>
    public static class HistoryManager
    {
        private static string _dbPath;
        private static readonly object _lockObj = new object();
        private const int HISTORY_RETENTION_DAYS = 90;
        private static bool _isInitialized = false;
        private static Exception _initException = null;

        static HistoryManager()
        {
            try
            {
                // 数据库文件放在DLL所在目录
                string dllPath = Assembly.GetExecutingAssembly().Location;
                string directory = Path.GetDirectoryName(dllPath);
                _dbPath = Path.Combine(directory, "InputHistory.db");

                InitializeDatabase();
                CleanOldRecords();
                _isInitialized = true;
            }
            catch (Exception ex)
            {
                // 捕获初始化异常，但不抛出，允许程序继续运行
                _initException = ex;
                _isInitialized = false;

                // 可选：记录到Windows事件日志
                System.Diagnostics.Debug.WriteLine($"HistoryManager 初始化失败: {ex.Message}");
            }
        }

        /// <summary>
        /// 初始化数据库
        /// </summary>
        private static void InitializeDatabase()
        {
            lock (_lockObj)
            {
                if (!File.Exists(_dbPath))
                {
                    SQLiteConnection.CreateFile(_dbPath);
                }

                using (var conn = new SQLiteConnection($"Data Source={_dbPath};Version=3;"))
                {
                    conn.Open();

                    string createTableSql = @"
                        CREATE TABLE IF NOT EXISTS InputHistory (
                            Id INTEGER PRIMARY KEY AUTOINCREMENT,
                            FieldName TEXT NOT NULL,
                            Value TEXT NOT NULL,
                            LastUsed DATETIME NOT NULL,
                            UseCount INTEGER DEFAULT 1,
                            UNIQUE(FieldName, Value)
                        );

                        CREATE INDEX IF NOT EXISTS idx_field_lastused ON InputHistory(FieldName, LastUsed DESC);
                        CREATE INDEX IF NOT EXISTS idx_field_value ON InputHistory(FieldName, Value);
                    ";

                    using (var cmd = new SQLiteCommand(createTableSql, conn))
                    {
                        cmd.ExecuteNonQuery();
                    }
                }
            }
        }

        /// <summary>
        /// 清理90天前的记录
        /// </summary>
        private static void CleanOldRecords()
        {
            lock (_lockObj)
            {
                try
                {
                    using (var conn = new SQLiteConnection($"Data Source={_dbPath};Version=3;"))
                    {
                        conn.Open();

                        string deleteSql = @"
                            DELETE FROM InputHistory
                            WHERE LastUsed < datetime('now', '-' || @days || ' days')
                        ";

                        using (var cmd = new SQLiteCommand(deleteSql, conn))
                        {
                            cmd.Parameters.AddWithValue("@days", HISTORY_RETENTION_DAYS);
                            cmd.ExecuteNonQuery();
                        }
                    }
                }
                catch (Exception)
                {
                    // 静默处理清理错误
                }
            }
        }

        /// <summary>
        /// 添加或更新历史记录
        /// </summary>
        /// <param name="fieldName">字段名（如：Title, Subject等）</param>
        /// <param name="value">值</param>
        public static void AddOrUpdateHistory(string fieldName, string value)
        {
            if (!_isInitialized)
                return; // 如果未初始化，静默返回

            if (string.IsNullOrWhiteSpace(fieldName) || string.IsNullOrWhiteSpace(value))
                return;

            // 清理值（去除首尾空白）
            value = value.Trim();
            if (string.IsNullOrEmpty(value))
                return;

            lock (_lockObj)
            {
                try
                {
                    using (var conn = new SQLiteConnection($"Data Source={_dbPath};Version=3;"))
                    {
                        conn.Open();

                        // 使用UPSERT语法（SQLite 3.24.0+）
                        string upsertSql = @"
                            INSERT INTO InputHistory (FieldName, Value, LastUsed, UseCount)
                            VALUES (@fieldName, @value, datetime('now'), 1)
                            ON CONFLICT(FieldName, Value)
                            DO UPDATE SET
                                LastUsed = datetime('now'),
                                UseCount = UseCount + 1
                        ";

                        using (var cmd = new SQLiteCommand(upsertSql, conn))
                        {
                            cmd.Parameters.AddWithValue("@fieldName", fieldName);
                            cmd.Parameters.AddWithValue("@value", value);
                            cmd.ExecuteNonQuery();
                        }
                    }
                }
                catch (Exception)
                {
                    // 静默处理错误，不影响主功能
                }
            }
        }

        /// <summary>
        /// 获取指定字段的历史记录（按最后使用时间排序）
        /// </summary>
        /// <param name="fieldName">字段名</param>
        /// <param name="limit">返回数量限制</param>
        /// <returns>历史记录列表</returns>
        public static List<string> GetHistory(string fieldName, int limit = 20)
        {
            if (!_isInitialized)
                return new List<string>(); // 如果未初始化，返回空列表

            if (string.IsNullOrWhiteSpace(fieldName))
                return new List<string>();

            lock (_lockObj)
            {
                try
                {
                    using (var conn = new SQLiteConnection($"Data Source={_dbPath};Version=3;"))
                    {
                        conn.Open();

                        string selectSql = @"
                            SELECT Value
                            FROM InputHistory
                            WHERE FieldName = @fieldName
                            ORDER BY LastUsed DESC, UseCount DESC
                            LIMIT @limit
                        ";

                        var results = new List<string>();

                        using (var cmd = new SQLiteCommand(selectSql, conn))
                        {
                            cmd.Parameters.AddWithValue("@fieldName", fieldName);
                            cmd.Parameters.AddWithValue("@limit", limit);

                            using (var reader = cmd.ExecuteReader())
                            {
                                while (reader.Read())
                                {
                                    results.Add(reader.GetString(0));
                                }
                            }
                        }

                        return results;
                    }
                }
                catch (Exception)
                {
                    return new List<string>();
                }
            }
        }

        /// <summary>
        /// 搜索历史记录（模糊匹配）
        /// </summary>
        /// <param name="fieldName">字段名</param>
        /// <param name="searchText">搜索文本</param>
        /// <param name="limit">返回数量限制</param>
        /// <returns>匹配的历史记录列表</returns>
        public static List<string> SearchHistory(string fieldName, string searchText, int limit = 10)
        {
            if (!_isInitialized)
                return new List<string>(); // 如果未初始化，返回空列表

            if (string.IsNullOrWhiteSpace(fieldName))
                return new List<string>();

            if (string.IsNullOrWhiteSpace(searchText))
                return GetHistory(fieldName, limit);

            lock (_lockObj)
            {
                try
                {
                    using (var conn = new SQLiteConnection($"Data Source={_dbPath};Version=3;"))
                    {
                        conn.Open();

                        string selectSql = @"
                            SELECT Value
                            FROM InputHistory
                            WHERE FieldName = @fieldName
                              AND Value LIKE @searchPattern
                            ORDER BY
                                CASE WHEN Value LIKE @exactPattern THEN 0 ELSE 1 END,
                                UseCount DESC,
                                LastUsed DESC
                            LIMIT @limit
                        ";

                        var results = new List<string>();

                        using (var cmd = new SQLiteCommand(selectSql, conn))
                        {
                            cmd.Parameters.AddWithValue("@fieldName", fieldName);
                            cmd.Parameters.AddWithValue("@searchPattern", $"%{searchText}%");
                            cmd.Parameters.AddWithValue("@exactPattern", $"{searchText}%");
                            cmd.Parameters.AddWithValue("@limit", limit);

                            using (var reader = cmd.ExecuteReader())
                            {
                                while (reader.Read())
                                {
                                    results.Add(reader.GetString(0));
                                }
                            }
                        }

                        return results;
                    }
                }
                catch (Exception)
                {
                    return new List<string>();
                }
            }
        }

        /// <summary>
        /// 删除指定字段的某条历史记录
        /// </summary>
        /// <param name="fieldName">字段名</param>
        /// <param name="value">值</param>
        public static void DeleteHistory(string fieldName, string value)
        {
            if (!_isInitialized)
                return; // 如果未初始化，静默返回

            if (string.IsNullOrWhiteSpace(fieldName) || string.IsNullOrWhiteSpace(value))
                return;

            lock (_lockObj)
            {
                try
                {
                    using (var conn = new SQLiteConnection($"Data Source={_dbPath};Version=3;"))
                    {
                        conn.Open();

                        string deleteSql = @"
                            DELETE FROM InputHistory
                            WHERE FieldName = @fieldName AND Value = @value
                        ";

                        using (var cmd = new SQLiteCommand(deleteSql, conn))
                        {
                            cmd.Parameters.AddWithValue("@fieldName", fieldName);
                            cmd.Parameters.AddWithValue("@value", value);
                            cmd.ExecuteNonQuery();
                        }
                    }
                }
                catch (Exception)
                {
                    // 静默处理错误
                }
            }
        }

        /// <summary>
        /// 清空指定字段的所有历史记录
        /// </summary>
        /// <param name="fieldName">字段名</param>
        public static void ClearFieldHistory(string fieldName)
        {
            if (!_isInitialized)
                return; // 如果未初始化，静默返回

            if (string.IsNullOrWhiteSpace(fieldName))
                return;

            lock (_lockObj)
            {
                try
                {
                    using (var conn = new SQLiteConnection($"Data Source={_dbPath};Version=3;"))
                    {
                        conn.Open();

                        string deleteSql = @"
                            DELETE FROM InputHistory
                            WHERE FieldName = @fieldName
                        ";

                        using (var cmd = new SQLiteCommand(deleteSql, conn))
                        {
                            cmd.Parameters.AddWithValue("@fieldName", fieldName);
                            cmd.ExecuteNonQuery();
                        }
                    }
                }
                catch (Exception)
                {
                    // 静默处理错误
                }
            }
        }

        /// <summary>
        /// 清空所有历史记录
        /// </summary>
        public static void ClearAllHistory()
        {
            if (!_isInitialized)
                return; // 如果未初始化，静默返回

            lock (_lockObj)
            {
                try
                {
                    using (var conn = new SQLiteConnection($"Data Source={_dbPath};Version=3;"))
                    {
                        conn.Open();

                        string deleteSql = "DELETE FROM InputHistory";

                        using (var cmd = new SQLiteCommand(deleteSql, conn))
                        {
                            cmd.ExecuteNonQuery();
                        }
                    }
                }
                catch (Exception)
                {
                    // 静默处理错误
                }
            }
        }

        /// <summary>
        /// 获取数据库文件路径
        /// </summary>
        public static string GetDatabasePath()
        {
            return _dbPath;
        }
    }
}
