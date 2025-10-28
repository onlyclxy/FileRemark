using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using System.Windows;

namespace WriteRemark
{
    /// <summary>
    /// 历史记录诊断工具
    /// </summary>
    public static class HistoryDiagnostics
    {
        /// <summary>
        /// 显示历史记录诊断信息
        /// </summary>
        public static void ShowDiagnostics()
        {
            var sb = new StringBuilder();
            sb.AppendLine("=== 历史记录诊断信息 ===\n");

            // 0. 检查初始化状态
            sb.AppendLine("--- 初始化状态 ---");
            sb.AppendLine($"HistoryManager 已初始化: {HistoryManager.IsInitialized}");
            
            var initException = HistoryManager.GetInitException();
            if (initException != null)
            {
                sb.AppendLine($"⚠️ 初始化异常: {initException.Message}");
                sb.AppendLine($"异常类型: {initException.GetType().Name}");
                if (initException.InnerException != null)
                {
                    sb.AppendLine($"内部异常: {initException.InnerException.Message}");
                }
            }
            else
            {
                sb.AppendLine("✓ 无初始化异常");
            }
            sb.AppendLine();

            // 1. 检查数据库路径
            string dbPath = HistoryManager.GetDatabasePath();
            sb.AppendLine("--- 数据库文件信息 ---");
            sb.AppendLine($"数据库路径: {dbPath}");
            sb.AppendLine($"数据库文件存在: {File.Exists(dbPath)}");

            if (File.Exists(dbPath))
            {
                try
                {
                    FileInfo fileInfo = new FileInfo(dbPath);
                    sb.AppendLine($"数据库文件大小: {fileInfo.Length} 字节");
                    
                    if (fileInfo.Length == 0)
                    {
                        sb.AppendLine("⚠️ 警告：数据库文件为空！表可能未创建。");
                    }
                    
                    sb.AppendLine($"创建时间: {fileInfo.CreationTime}");
                    sb.AppendLine($"最后修改: {fileInfo.LastWriteTime}");
                }
                catch (Exception ex)
                {
                    sb.AppendLine($"无法读取文件信息: {ex.Message}");
                }
            }

            sb.AppendLine();

            // 1.5 检查数据库表是否存在
            sb.AppendLine("--- 数据库表检查 ---");
            if (File.Exists(dbPath))
            {
                try
                {
                    using (var conn = new System.Data.SQLite.SQLiteConnection($"Data Source={dbPath};Version=3;"))
                    {
                        conn.Open();
                        
                        // 检查表是否存在
                        string checkTableSql = "SELECT name FROM sqlite_master WHERE type='table' AND name='InputHistory';";
                        using (var cmd = new System.Data.SQLite.SQLiteCommand(checkTableSql, conn))
                        {
                            var result = cmd.ExecuteScalar();
                            if (result != null)
                            {
                                sb.AppendLine("✓ InputHistory 表存在");
                                
                                // 检查表结构
                                string countSql = "SELECT COUNT(*) FROM InputHistory;";
                                using (var countCmd = new System.Data.SQLite.SQLiteCommand(countSql, conn))
                                {
                                    var count = countCmd.ExecuteScalar();
                                    sb.AppendLine($"  表中记录数: {count}");
                                }
                            }
                            else
                            {
                                sb.AppendLine("✗ InputHistory 表不存在！");
                                sb.AppendLine("  这是问题所在：数据库文件存在但表未创建。");
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    sb.AppendLine($"✗ 数据库检查失败: {ex.Message}");
                    if (ex.InnerException != null)
                    {
                        sb.AppendLine($"  内部异常: {ex.InnerException.Message}");
                    }
                }
            }
            
            sb.AppendLine();

            // 2. 尝试添加测试记录
            sb.AppendLine("--- 测试写入 ---");
            try
            {
                string testValue = $"测试_{DateTime.Now:HHmmss}";
                HistoryManager.AddOrUpdateHistory("TestField", testValue);
                sb.AppendLine($"✓ 成功添加测试记录: {testValue}");
            }
            catch (Exception ex)
            {
                sb.AppendLine($"✗ 添加测试记录失败: {ex.Message}");
            }

            sb.AppendLine();

            // 3. 尝试读取记录
            sb.AppendLine("--- 测试读取 ---");
            try
            {
                var history = HistoryManager.GetHistory("TestField", 10);
                sb.AppendLine($"✓ 读取到 {history.Count} 条测试记录");
                if (history.Count > 0)
                {
                    sb.AppendLine("最近的记录:");
                    for (int i = 0; i < Math.Min(5, history.Count); i++)
                    {
                        sb.AppendLine($"  {i + 1}. {history[i]}");
                    }
                }
            }
            catch (Exception ex)
            {
                sb.AppendLine($"✗ 读取记录失败: {ex.Message}");
            }

            sb.AppendLine();

            // 4. 检查各字段的历史记录数量
            sb.AppendLine("--- 各字段历史记录统计 ---");
            string[] fields = { "Title", "Subject", "Rating", "Tags", "Category", "Comment",
                              "LocalizedResourceName", "InfoTip", "Prop2", "Prop3", "Prop4", "Prop5" };

            foreach (var field in fields)
            {
                try
                {
                    var history = HistoryManager.GetHistory(field, 100);
                    if (history.Count > 0)
                    {
                        sb.AppendLine($"{field}: {history.Count} 条记录");
                    }
                }
                catch
                {
                    // 忽略错误
                }
            }

            sb.AppendLine();

            // 5. 程序集信息
            sb.AppendLine("--- 程序信息 ---");
            try
            {
                string exePath = Assembly.GetExecutingAssembly().Location;
                sb.AppendLine($"程序路径: {exePath}");
                sb.AppendLine($"工作目录: {Environment.CurrentDirectory}");
            }
            catch (Exception ex)
            {
                sb.AppendLine($"获取程序信息失败: {ex.Message}");
            }

            // 显示诊断信息
            var diagnosticMessage = sb.ToString();
            MessageBox.Show(diagnosticMessage, "历史记录诊断", MessageBoxButton.OK, MessageBoxImage.Information);
            
            // 如果检测到数据库表不存在，提示修复
            if (diagnosticMessage.Contains("✗ InputHistory 表不存在"))
            {
                var repairResult = MessageBox.Show(
                    "检测到数据库表未正确创建！\n\n" +
                    "这是导致历史记录功能无法工作的根本原因。\n\n" +
                    "是否立即修复数据库？",
                    "数据库需要修复",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Warning);

                if (repairResult == MessageBoxResult.Yes)
                {
                    RepairDatabase();
                }
            }
        }

        /// <summary>
        /// 测试保存和加载历史记录
        /// </summary>
        public static bool TestHistoryFunctionality(string fieldName, string testValue)
        {
            try
            {
                // 1. 保存测试值
                HistoryManager.AddOrUpdateHistory(fieldName, testValue);

                // 2. 立即读取
                var history = HistoryManager.GetHistory(fieldName, 10);

                // 3. 检查是否存在
                bool found = history.Contains(testValue);

                if (!found)
                {
                    MessageBox.Show(
                        $"历史记录测试失败！\n\n" +
                        $"字段: {fieldName}\n" +
                        $"测试值: {testValue}\n" +
                        $"保存后未能读取到该值。\n\n" +
                        $"请点击\"诊断\"按钮查看详细信息。",
                        "历史记录测试",
                        MessageBoxButton.OK,
                        MessageBoxImage.Warning);
                }
                else
                {
                    MessageBox.Show(
                        $"历史记录测试成功！\n\n" +
                        $"字段: {fieldName}\n" +
                        $"测试值: {testValue}\n" +
                        $"当前该字段共有 {history.Count} 条历史记录。",
                        "历史记录测试",
                        MessageBoxButton.OK,
                        MessageBoxImage.Information);
                }

                return found;
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"历史记录测试出错！\n\n{ex.Message}\n\n{ex.StackTrace}",
                    "错误",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
                return false;
            }
        }

        /// <summary>
        /// 打开数据库文件所在目录
        /// </summary>
        public static void OpenDatabaseFolder()
        {
            try
            {
                string dbPath = HistoryManager.GetDatabasePath();
                string directory = Path.GetDirectoryName(dbPath);

                if (Directory.Exists(directory))
                {
                    System.Diagnostics.Process.Start("explorer.exe", directory);
                }
                else
                {
                    MessageBox.Show($"目录不存在: {directory}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"打开目录失败: {ex.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// 尝试修复数据库
        /// </summary>
        public static void RepairDatabase()
        {
            try
            {
                string dbPath = HistoryManager.GetDatabasePath();
                
                var result = MessageBox.Show(
                    $"这将删除现有数据库文件并重新创建。\n\n" +
                    $"数据库路径: {dbPath}\n\n" +
                    $"所有历史记录将被清空，是否继续？",
                    "修复数据库",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Warning);

                if (result == MessageBoxResult.Yes)
                {
                    // 删除现有数据库文件
                    if (File.Exists(dbPath))
                    {
                        File.Delete(dbPath);
                    }

                    // 重新创建数据库
                    System.Data.SQLite.SQLiteConnection.CreateFile(dbPath);

                    // 创建表
                    using (var conn = new System.Data.SQLite.SQLiteConnection($"Data Source={dbPath};Version=3;"))
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

                        using (var cmd = new System.Data.SQLite.SQLiteCommand(createTableSql, conn))
                        {
                            cmd.ExecuteNonQuery();
                        }
                    }

                    MessageBox.Show(
                        "数据库修复成功！\n\n请重启程序以使更改生效。",
                        "修复完成",
                        MessageBoxButton.OK,
                        MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"修复数据库失败！\n\n{ex.Message}\n\n{ex.StackTrace}",
                    "错误",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }
    }
}

