using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

namespace WriteRemark
{
    /// <summary>
    /// 字段配置信息
    /// </summary>
    public class FieldConfig
    {
        public string FieldName { get; set; }
        public string DisplayName { get; set; }
        public bool IsVisible { get; set; }
        public int Order { get; set; }
        public string ToolTip { get; set; }
    }

    /// <summary>
    /// 字段配置管理器
    /// </summary>
    public class FieldConfigManager
    {
        private static readonly string ConfigFileName = "field_config.json";
        private static string ConfigFilePath
        {
            get
            {
                string assemblyPath = Assembly.GetExecutingAssembly().Location;
                string directory = Path.GetDirectoryName(assemblyPath);
                return Path.Combine(directory, ConfigFileName);
            }
        }

        /// <summary>
        /// 默认字段配置
        /// </summary>
        private static readonly List<FieldConfig> DefaultFields = new List<FieldConfig>
        {
            new FieldConfig { FieldName = "Title", DisplayName = "标题", IsVisible = true, Order = 1, ToolTip = "文件标题" },
            new FieldConfig { FieldName = "Subject", DisplayName = "主题", IsVisible = true, Order = 2, ToolTip = "文件主题" },
            new FieldConfig { FieldName = "Rating", DisplayName = "分级", IsVisible = true, Order = 3, ToolTip = "请输入 1-99 之间的数字，可以为空" },
            new FieldConfig { FieldName = "Tags", DisplayName = "标记", IsVisible = true, Order = 4, ToolTip = "多个标记请用 ; 分隔" },
            new FieldConfig { FieldName = "Category", DisplayName = "类别", IsVisible = true, Order = 5, ToolTip = "多个类别请用 ; 分隔" },
            new FieldConfig { FieldName = "Comment", DisplayName = "备注", IsVisible = true, Order = 6, ToolTip = "详细备注信息" }
        };

        /// <summary>
        /// 加载字段配置
        /// </summary>
        public static List<FieldConfig> LoadFieldConfigs()
        {
            try
            {
                if (File.Exists(ConfigFilePath))
                {
                    string configContent = File.ReadAllText(ConfigFilePath);
                    var configs = ParseConfigFromString(configContent);
                    
                    // 检查是否有新字段需要添加
                    var existingFieldNames = configs.Select(c => c.FieldName).ToHashSet();
                    var missingFields = DefaultFields.Where(df => !existingFieldNames.Contains(df.FieldName)).ToList();
                    
                    if (missingFields.Any())
                    {
                        // 添加缺失的字段，并设置合适的Order
                        int maxOrder = configs.Any() ? configs.Max(c => c.Order) : 0;
                        foreach (var missingField in missingFields)
                        {
                            missingField.Order = ++maxOrder;
                            configs.Add(missingField);
                        }
                        SaveFieldConfigs(configs);
                    }
                    
                    return configs.OrderBy(c => c.Order).ToList();
                }
                else
                {
                    // 首次运行，创建默认配置
                    SaveFieldConfigs(DefaultFields);
                    return DefaultFields.ToList();
                }
            }
            catch (Exception)
            {
                // 如果读取失败，返回默认配置
                return DefaultFields.ToList();
            }
        }

        /// <summary>
        /// 保存字段配置
        /// </summary>
        public static void SaveFieldConfigs(List<FieldConfig> configs)
        {
            try
            {
                string configString = SerializeConfigsToString(configs);
                File.WriteAllText(ConfigFilePath, configString);
            }
            catch (Exception ex)
            {
                throw new Exception($"保存配置文件失败: {ex.Message}");
            }
        }

        /// <summary>
        /// 将配置序列化为字符串（简单的键值对格式）
        /// </summary>
        private static string SerializeConfigsToString(List<FieldConfig> configs)
        {
            var sb = new StringBuilder();
            sb.AppendLine("# WriteRemark Field Configuration");
            sb.AppendLine("# Format: FieldName|DisplayName|IsVisible|Order|ToolTip");
            sb.AppendLine();

            foreach (var config in configs.OrderBy(c => c.Order))
            {
                sb.AppendLine($"{config.FieldName}|{config.DisplayName}|{config.IsVisible}|{config.Order}|{config.ToolTip ?? ""}");
            }

            return sb.ToString();
        }

        /// <summary>
        /// 从字符串解析配置（简单的键值对格式）
        /// </summary>
        private static List<FieldConfig> ParseConfigFromString(string configContent)
        {
            var configs = new List<FieldConfig>();
            var lines = configContent.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);

            foreach (var line in lines)
            {
                if (line.StartsWith("#") || string.IsNullOrWhiteSpace(line))
                    continue;

                var parts = line.Split('|');
                if (parts.Length >= 4)
                {
                    var config = new FieldConfig
                    {
                        FieldName = parts[0].Trim(),
                        DisplayName = parts[1].Trim(),
                        IsVisible = bool.Parse(parts[2].Trim()),
                        Order = int.Parse(parts[3].Trim()),
                        ToolTip = parts.Length > 4 ? parts[4].Trim() : ""
                    };
                    configs.Add(config);
                }
            }

            return configs;
        }

        /// <summary>
        /// 重置为默认配置
        /// </summary>
        public static void ResetToDefault()
        {
            SaveFieldConfigs(DefaultFields);
        }

        /// <summary>
        /// 更新字段顺序
        /// </summary>
        public static void UpdateFieldOrder(List<FieldConfig> configs)
        {
            for (int i = 0; i < configs.Count; i++)
            {
                configs[i].Order = i + 1;
            }
            SaveFieldConfigs(configs);
        }

        /// <summary>
        /// 切换字段可见性
        /// </summary>
        public static void ToggleFieldVisibility(string fieldName, bool isVisible)
        {
            var configs = LoadFieldConfigs();
            var field = configs.FirstOrDefault(c => c.FieldName == fieldName);
            if (field != null)
            {
                field.IsVisible = isVisible;
                SaveFieldConfigs(configs);
            }
        }
    }
}
