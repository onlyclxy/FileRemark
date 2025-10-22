using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace WriteRemark
{
    /// <summary>
    /// 批量配置文件管理器 - 使用简单的TOML格式
    /// </summary>
    public static class BatchConfigManager
    {
        /// <summary>
        /// 文件配置项
        /// </summary>
        public class FileConfig
        {
            public string FileName { get; set; }
            public string Title { get; set; }
            public string Subject { get; set; }
            public string Rating { get; set; }
            public string Tags { get; set; }
            public string Category { get; set; }
            public string Comment { get; set; }
        }

        /// <summary>
        /// 文件夹配置项
        /// </summary>
        public class FolderConfig
        {
            public string FolderName { get; set; }
            public string Alias { get; set; }
            public string InfoTip { get; set; }
            public string Title { get; set; }
            public string Subject { get; set; }
            public string Author { get; set; }
            public string Tags { get; set; }
        }

        /// <summary>
        /// 导出文件配置到TOML文件
        /// </summary>
        public static void ExportFileConfigs(List<BatchFilePropertyModel> files, string filePath)
        {
            var sb = new StringBuilder();
            sb.AppendLine("# 批量文件属性配置");
            sb.AppendLine("# 导出时间: " + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
            sb.AppendLine();

            foreach (var file in files)
            {
                sb.AppendLine($"[[file]]");
                sb.AppendLine($"name = \"{EscapeTomlString(file.FileName)}\"");
                
                if (!string.IsNullOrEmpty(file.Title))
                    sb.AppendLine($"title = \"{EscapeTomlString(file.Title)}\"");
                
                if (!string.IsNullOrEmpty(file.Subject))
                    sb.AppendLine($"subject = \"{EscapeTomlString(file.Subject)}\"");
                
                if (!string.IsNullOrEmpty(file.Rating))
                    sb.AppendLine($"rating = \"{EscapeTomlString(file.Rating)}\"");
                
                if (!string.IsNullOrEmpty(file.Tags))
                    sb.AppendLine($"tags = \"{EscapeTomlString(file.Tags)}\"");
                
                if (!string.IsNullOrEmpty(file.Category))
                    sb.AppendLine($"category = \"{EscapeTomlString(file.Category)}\"");
                
                if (!string.IsNullOrEmpty(file.Comment))
                    sb.AppendLine($"comment = \"{EscapeTomlString(file.Comment)}\"");
                
                sb.AppendLine();
            }

            File.WriteAllText(filePath, sb.ToString(), Encoding.UTF8);
        }

        /// <summary>
        /// 导出文件夹配置到TOML文件
        /// </summary>
        public static void ExportFolderConfigs(List<BatchFolderPropertyModel> folders, string filePath)
        {
            var sb = new StringBuilder();
            sb.AppendLine("# 批量文件夹属性配置");
            sb.AppendLine("# 导出时间: " + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
            sb.AppendLine();

            foreach (var folder in folders)
            {
                sb.AppendLine($"[[folder]]");
                sb.AppendLine($"name = \"{EscapeTomlString(folder.FolderName)}\"");
                
                if (!string.IsNullOrEmpty(folder.Alias))
                    sb.AppendLine($"alias = \"{EscapeTomlString(folder.Alias)}\"");
                
                if (!string.IsNullOrEmpty(folder.InfoTip))
                    sb.AppendLine($"infotip = \"{EscapeTomlString(folder.InfoTip)}\"");
                
                if (!string.IsNullOrEmpty(folder.Title))
                    sb.AppendLine($"title = \"{EscapeTomlString(folder.Title)}\"");
                
                if (!string.IsNullOrEmpty(folder.Subject))
                    sb.AppendLine($"subject = \"{EscapeTomlString(folder.Subject)}\"");
                
                if (!string.IsNullOrEmpty(folder.Author))
                    sb.AppendLine($"author = \"{EscapeTomlString(folder.Author)}\"");
                
                if (!string.IsNullOrEmpty(folder.Tags))
                    sb.AppendLine($"tags = \"{EscapeTomlString(folder.Tags)}\"");
                
                sb.AppendLine();
            }

            File.WriteAllText(filePath, sb.ToString(), Encoding.UTF8);
        }

        /// <summary>
        /// 导出混合配置（文件和文件夹）到TOML文件
        /// </summary>
        public static void ExportMixedConfigs(List<BatchFilePropertyModel> files, 
            List<BatchFolderPropertyModel> folders, string filePath)
        {
            var sb = new StringBuilder();
            sb.AppendLine("# 批量属性配置");
            sb.AppendLine("# 导出时间: " + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
            sb.AppendLine();

            if (files != null && files.Any())
            {
                sb.AppendLine("# 文件配置");
                sb.AppendLine();
                foreach (var file in files)
                {
                    sb.AppendLine($"[[file]]");
                    sb.AppendLine($"name = \"{EscapeTomlString(file.FileName)}\"");
                    
                    if (!string.IsNullOrEmpty(file.Title))
                        sb.AppendLine($"title = \"{EscapeTomlString(file.Title)}\"");
                    
                    if (!string.IsNullOrEmpty(file.Subject))
                        sb.AppendLine($"subject = \"{EscapeTomlString(file.Subject)}\"");
                    
                    if (!string.IsNullOrEmpty(file.Rating))
                        sb.AppendLine($"rating = \"{EscapeTomlString(file.Rating)}\"");
                    
                    if (!string.IsNullOrEmpty(file.Tags))
                        sb.AppendLine($"tags = \"{EscapeTomlString(file.Tags)}\"");
                    
                    if (!string.IsNullOrEmpty(file.Category))
                        sb.AppendLine($"category = \"{EscapeTomlString(file.Category)}\"");
                    
                    if (!string.IsNullOrEmpty(file.Comment))
                        sb.AppendLine($"comment = \"{EscapeTomlString(file.Comment)}\"");
                    
                    sb.AppendLine();
                }
            }

            if (folders != null && folders.Any())
            {
                sb.AppendLine("# 文件夹配置");
                sb.AppendLine();
                foreach (var folder in folders)
                {
                    sb.AppendLine($"[[folder]]");
                    sb.AppendLine($"name = \"{EscapeTomlString(folder.FolderName)}\"");
                    
                    if (!string.IsNullOrEmpty(folder.Alias))
                        sb.AppendLine($"alias = \"{EscapeTomlString(folder.Alias)}\"");
                    
                    if (!string.IsNullOrEmpty(folder.InfoTip))
                        sb.AppendLine($"infotip = \"{EscapeTomlString(folder.InfoTip)}\"");
                    
                    if (!string.IsNullOrEmpty(folder.Title))
                        sb.AppendLine($"title = \"{EscapeTomlString(folder.Title)}\"");
                    
                    if (!string.IsNullOrEmpty(folder.Subject))
                        sb.AppendLine($"subject = \"{EscapeTomlString(folder.Subject)}\"");
                    
                    if (!string.IsNullOrEmpty(folder.Author))
                        sb.AppendLine($"author = \"{EscapeTomlString(folder.Author)}\"");
                    
                    if (!string.IsNullOrEmpty(folder.Tags))
                        sb.AppendLine($"tags = \"{EscapeTomlString(folder.Tags)}\"");
                    
                    sb.AppendLine();
                }
            }

            File.WriteAllText(filePath, sb.ToString(), Encoding.UTF8);
        }

        /// <summary>
        /// 从TOML文件导入配置
        /// </summary>
        public static (List<FileConfig> files, List<FolderConfig> folders) ImportConfigs(string filePath)
        {
            var files = new List<FileConfig>();
            var folders = new List<FolderConfig>();

            if (!File.Exists(filePath))
                return (files, folders);

            var lines = File.ReadAllLines(filePath, Encoding.UTF8);
            
            string currentSection = null;
            FileConfig currentFile = null;
            FolderConfig currentFolder = null;

            foreach (var line in lines)
            {
                var trimmedLine = line.Trim();
                
                // 跳过注释和空行
                if (string.IsNullOrEmpty(trimmedLine) || trimmedLine.StartsWith("#"))
                    continue;

                // 新的文件条目
                if (trimmedLine == "[[file]]")
                {
                    if (currentFile != null)
                        files.Add(currentFile);
                    if (currentFolder != null)
                        folders.Add(currentFolder);
                    
                    currentFile = new FileConfig();
                    currentFolder = null;
                    currentSection = "file";
                    continue;
                }

                // 新的文件夹条目
                if (trimmedLine == "[[folder]]")
                {
                    if (currentFile != null)
                        files.Add(currentFile);
                    if (currentFolder != null)
                        folders.Add(currentFolder);
                    
                    currentFolder = new FolderConfig();
                    currentFile = null;
                    currentSection = "folder";
                    continue;
                }

                // 解析键值对
                var parts = trimmedLine.Split(new[] { '=' }, 2);
                if (parts.Length != 2)
                    continue;

                var key = parts[0].Trim().ToLower();
                var value = UnescapeTomlString(parts[1].Trim());

                // 根据当前节类型设置值
                if (currentSection == "file" && currentFile != null)
                {
                    switch (key)
                    {
                        case "name":
                            currentFile.FileName = value;
                            break;
                        case "title":
                            currentFile.Title = value;
                            break;
                        case "subject":
                            currentFile.Subject = value;
                            break;
                        case "rating":
                            currentFile.Rating = value;
                            break;
                        case "tags":
                            currentFile.Tags = value;
                            break;
                        case "category":
                            currentFile.Category = value;
                            break;
                        case "comment":
                            currentFile.Comment = value;
                            break;
                    }
                }
                else if (currentSection == "folder" && currentFolder != null)
                {
                    switch (key)
                    {
                        case "name":
                            currentFolder.FolderName = value;
                            break;
                        case "alias":
                            currentFolder.Alias = value;
                            break;
                        case "infotip":
                            currentFolder.InfoTip = value;
                            break;
                        case "title":
                            currentFolder.Title = value;
                            break;
                        case "subject":
                            currentFolder.Subject = value;
                            break;
                        case "author":
                            currentFolder.Author = value;
                            break;
                        case "tags":
                            currentFolder.Tags = value;
                            break;
                    }
                }
            }

            // 添加最后一个条目
            if (currentFile != null)
                files.Add(currentFile);
            if (currentFolder != null)
                folders.Add(currentFolder);

            return (files, folders);
        }

        /// <summary>
        /// 转义TOML字符串
        /// </summary>
        private static string EscapeTomlString(string input)
        {
            if (string.IsNullOrEmpty(input))
                return "";

            return input
                .Replace("\\", "\\\\")
                .Replace("\"", "\\\"")
                .Replace("\n", "\\n")
                .Replace("\r", "\\r")
                .Replace("\t", "\\t");
        }

        /// <summary>
        /// 反转义TOML字符串
        /// </summary>
        private static string UnescapeTomlString(string input)
        {
            if (string.IsNullOrEmpty(input))
                return "";

            // 移除首尾引号
            input = input.Trim();
            if (input.StartsWith("\"") && input.EndsWith("\""))
                input = input.Substring(1, input.Length - 2);

            return input
                .Replace("\\n", "\n")
                .Replace("\\r", "\r")
                .Replace("\\t", "\t")
                .Replace("\\\"", "\"")
                .Replace("\\\\", "\\");
        }
    }
}

