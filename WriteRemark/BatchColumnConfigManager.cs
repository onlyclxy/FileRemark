using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace WriteRemark
{
    /// <summary>
    /// 批量编辑窗口的列配置
    /// </summary>
    public class BatchColumnConfig
    {
        public string ColumnName { get; set; }
        public bool IsVisible { get; set; }
    }

    /// <summary>
    /// 批量编辑窗口列配置管理器
    /// </summary>
    public static class BatchColumnConfigManager
    {
        private static readonly string FileColumnsConfigPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "WriteRemark",
            "BatchFileColumns.txt"
        );

        private static readonly string FolderColumnsConfigPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "WriteRemark",
            "BatchFolderColumns.txt"
        );

        /// <summary>
        /// 默认文件列配置
        /// </summary>
        private static List<BatchColumnConfig> GetDefaultFileColumns()
        {
            return new List<BatchColumnConfig>
            {
                new BatchColumnConfig { ColumnName = "Title", IsVisible = true },
                new BatchColumnConfig { ColumnName = "Subject", IsVisible = true },
                new BatchColumnConfig { ColumnName = "Rating", IsVisible = true },
                new BatchColumnConfig { ColumnName = "Tags", IsVisible = true },
                new BatchColumnConfig { ColumnName = "Category", IsVisible = true },
                new BatchColumnConfig { ColumnName = "Comment", IsVisible = true }
            };
        }

        /// <summary>
        /// 默认文件夹列配置
        /// </summary>
        private static List<BatchColumnConfig> GetDefaultFolderColumns()
        {
            return new List<BatchColumnConfig>
            {
                new BatchColumnConfig { ColumnName = "Alias", IsVisible = true },
                new BatchColumnConfig { ColumnName = "InfoTip", IsVisible = true },
                new BatchColumnConfig { ColumnName = "Title", IsVisible = true },
                new BatchColumnConfig { ColumnName = "Subject", IsVisible = true },
                new BatchColumnConfig { ColumnName = "Author", IsVisible = true },
                new BatchColumnConfig { ColumnName = "Tags", IsVisible = true }
            };
        }

        /// <summary>
        /// 加载文件列配置
        /// </summary>
        public static List<BatchColumnConfig> LoadFileColumns()
        {
            try
            {
                if (File.Exists(FileColumnsConfigPath))
                {
                    var configs = new List<BatchColumnConfig>();
                    var lines = File.ReadAllLines(FileColumnsConfigPath, Encoding.UTF8);
                    
                    foreach (var line in lines)
                    {
                        if (string.IsNullOrWhiteSpace(line))
                            continue;
                        
                        var parts = line.Split('=');
                        if (parts.Length == 2)
                        {
                            configs.Add(new BatchColumnConfig
                            {
                                ColumnName = parts[0].Trim(),
                                IsVisible = bool.Parse(parts[1].Trim())
                            });
                        }
                    }
                    
                    if (configs.Any())
                        return configs;
                }
            }
            catch
            {
                // 忽略错误，返回默认配置
            }

            return GetDefaultFileColumns();
        }

        /// <summary>
        /// 加载文件夹列配置
        /// </summary>
        public static List<BatchColumnConfig> LoadFolderColumns()
        {
            try
            {
                if (File.Exists(FolderColumnsConfigPath))
                {
                    var configs = new List<BatchColumnConfig>();
                    var lines = File.ReadAllLines(FolderColumnsConfigPath, Encoding.UTF8);
                    
                    foreach (var line in lines)
                    {
                        if (string.IsNullOrWhiteSpace(line))
                            continue;
                        
                        var parts = line.Split('=');
                        if (parts.Length == 2)
                        {
                            configs.Add(new BatchColumnConfig
                            {
                                ColumnName = parts[0].Trim(),
                                IsVisible = bool.Parse(parts[1].Trim())
                            });
                        }
                    }
                    
                    if (configs.Any())
                        return configs;
                }
            }
            catch
            {
                // 忽略错误，返回默认配置
            }

            return GetDefaultFolderColumns();
        }

        /// <summary>
        /// 保存文件列配置
        /// </summary>
        public static void SaveFileColumns(List<BatchColumnConfig> configs)
        {
            try
            {
                string directory = Path.GetDirectoryName(FileColumnsConfigPath);
                if (!Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                var lines = configs.Select(c => $"{c.ColumnName}={c.IsVisible}");
                File.WriteAllLines(FileColumnsConfigPath, lines, Encoding.UTF8);
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"保存文件列配置时出错: {ex.Message}",
                    "错误", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// 保存文件夹列配置
        /// </summary>
        public static void SaveFolderColumns(List<BatchColumnConfig> configs)
        {
            try
            {
                string directory = Path.GetDirectoryName(FolderColumnsConfigPath);
                if (!Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                var lines = configs.Select(c => $"{c.ColumnName}={c.IsVisible}");
                File.WriteAllLines(FolderColumnsConfigPath, lines, Encoding.UTF8);
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"保存文件夹列配置时出错: {ex.Message}",
                    "错误", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
            }
        }
    }
}

