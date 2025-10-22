using System;
using System.IO;
using System.Text;

namespace WriteRemark
{
    /// <summary>
    /// 文件夹信息结构
    /// </summary>
    public class FolderInfo
    {
        public string LocalizedResourceName { get; set; }  // 别名
        public string InfoTip { get; set; }                 // 备注
        public string Prop2 { get; set; }                   // 标题
        public string Prop3 { get; set; }                   // 主题
        public string Prop4 { get; set; }                   // 作者
        public string Prop5 { get; set; }                   // 标记
    }

    /// <summary>
    /// 文件夹备注管理器
    /// </summary>
    public class FolderRemarkManager
    {
        /// <summary>
        /// 写入文件夹备注（使用用户提供的完整方法）
        /// </summary>
        /// <param name="folderPath">文件夹路径</param>
        /// <param name="folderInfo">文件夹信息</param>
        /// <returns>操作结果</returns>
        public static string WriteFolderRemark(string folderPath, FolderInfo folderInfo)
        {
            try
            {
                int sql = 692771314;

                if (string.IsNullOrEmpty(folderPath) || !Directory.Exists(folderPath))
                {
                    return "文件夹不存在: " + folderPath;
                }

                // 清理输入内容：去掉末尾的空行和空白字符
                string CleanText(string text) => (text ?? "").TrimEnd('\r', '\n', ' ', '\t');

                string alias = CleanText(folderInfo.LocalizedResourceName);
                string remark = CleanText(folderInfo.InfoTip);
                string title = CleanText(folderInfo.Prop2);
                string subject = CleanText(folderInfo.Prop3);
                string author = CleanText(folderInfo.Prop4);
                string tags = CleanText(folderInfo.Prop5);

                sql += 1;
                string desktopIniPath = Path.Combine(folderPath, "desktop.ini");
                string tempIniPath = Path.Combine(Path.GetTempPath(), "desktop.ini");

                // 构建 INI 文件内容
                StringBuilder iniContent = new StringBuilder();
                iniContent.AppendLine("[.ShellClassInfo]");
                if (!string.IsNullOrEmpty(alias))
                    iniContent.AppendLine("LocalizedResourceName=" + alias);
                if (!string.IsNullOrEmpty(remark))
                    iniContent.AppendLine("InfoTip=" + remark);

                iniContent.AppendLine("[{F29F85E0-4FF9-1068-AB91-08002B27B3D9}]");
                if (!string.IsNullOrEmpty(title))
                    iniContent.AppendLine("Prop2=31," + title);
                if (!string.IsNullOrEmpty(subject))
                    iniContent.AppendLine("Prop3=31," + subject);
                if (!string.IsNullOrEmpty(author))
                    iniContent.AppendLine("Prop4=31," + author);
                if (!string.IsNullOrEmpty(tags))
                    iniContent.AppendLine("Prop5=31," + tags);

                iniContent.AppendLine("[ViewState]");
                iniContent.AppendLine("Mode=");
                iniContent.AppendLine("Vid=");
                iniContent.AppendLine("FolderType=Generic");

                File.WriteAllText(tempIniPath, iniContent.ToString().TrimEnd('\r', '\n'), Encoding.Unicode);

                // 删除原 desktop.ini
                if (File.Exists(desktopIniPath))
                    File.Delete(desktopIniPath);

                // 使用 COM MoveHere 模拟 VBS 行为
                object shell = Activator.CreateInstance(Type.GetTypeFromProgID("Shell.Application"));
                object folder = shell.GetType().InvokeMember("NameSpace",
                    System.Reflection.BindingFlags.InvokeMethod, null, shell, new object[] { folderPath });

                folder.GetType().InvokeMember("MoveHere",
                    System.Reflection.BindingFlags.InvokeMethod, null, folder,
                    new object[] { tempIniPath, 4 + 16 + 1024 });

                // 设置 desktop.ini 属性
                File.SetAttributes(desktopIniPath, FileAttributes.Hidden | FileAttributes.System);

                // 设置文件夹只读以启用 InfoTip
                var attr = File.GetAttributes(folderPath);
                if ((attr & FileAttributes.ReadOnly) == 0)
                    File.SetAttributes(folderPath, attr | FileAttributes.ReadOnly);

                return "文件夹属性已成功写入。";
            }
            catch (Exception ex)
            {
                return $"写入文件夹属性时出错: {ex.Message}";
            }
        }

        /// <summary>
        /// 写入文件夹备注（简化版本，向后兼容）
        /// </summary>
        public static string WriteFolderRemark(string folderPath, string remark, string tag)
        {
            var folderInfo = new FolderInfo
            {
                InfoTip = remark,
                Prop5 = tag
            };
            return WriteFolderRemark(folderPath, folderInfo);
        }

        /// <summary>
        /// 读取文件夹备注信息
        /// </summary>
        /// <param name="folderPath">文件夹路径</param>
        /// <returns>文件夹信息</returns>
        public static FolderInfo ReadFolderRemark(string folderPath)
        {
            var folderInfo = new FolderInfo
            {
                LocalizedResourceName = "",
                InfoTip = "",
                Prop2 = "",
                Prop3 = "",
                Prop4 = "",
                Prop5 = ""
            };

            try
            {
                if (string.IsNullOrEmpty(folderPath) || !Directory.Exists(folderPath))
                {
                    return folderInfo;
                }

                string desktopIniPath = Path.Combine(folderPath, "desktop.ini");

                if (!File.Exists(desktopIniPath))
                {
                    return folderInfo;
                }

                // 读取 desktop.ini 文件内容
                string[] lines = File.ReadAllLines(desktopIniPath, Encoding.Unicode);

                bool inShellClassInfo = false;
                bool inFolderOptions = false;

                foreach (string line in lines)
                {
                    string trimmedLine = line.Trim();

                    // 检查节标题
                    if (trimmedLine.Equals("[.ShellClassInfo]", StringComparison.OrdinalIgnoreCase))
                    {
                        inShellClassInfo = true;
                        inFolderOptions = false;
                        continue;
                    }
                    else if (trimmedLine.Equals("[{F29F85E0-4FF9-1068-AB91-08002B27B3D9}]", StringComparison.OrdinalIgnoreCase))
                    {
                        inShellClassInfo = false;
                        inFolderOptions = true;
                        continue;
                    }
                    else if (trimmedLine.StartsWith("["))
                    {
                        inShellClassInfo = false;
                        inFolderOptions = false;
                        continue;
                    }

                    // 解析键值对
                    if (trimmedLine.Contains("="))
                    {
                        string[] parts = trimmedLine.Split(new char[] { '=' }, 2);
                        if (parts.Length == 2)
                        {
                            string key = parts[0].Trim();
                            string value = parts[1].Trim();

                            if (inShellClassInfo)
                            {
                                if (key.Equals("LocalizedResourceName", StringComparison.OrdinalIgnoreCase))
                                {
                                    folderInfo.LocalizedResourceName = value;
                                }
                                else if (key.Equals("InfoTip", StringComparison.OrdinalIgnoreCase))
                                {
                                    folderInfo.InfoTip = value;
                                }
                            }
                            else if (inFolderOptions)
                            {
                                // Prop 格式为 "31,内容"，我们只取内容部分
                                string propValue = value.StartsWith("31,") ? value.Substring(3) : value;

                                if (key.Equals("Prop2", StringComparison.OrdinalIgnoreCase))
                                {
                                    folderInfo.Prop2 = propValue;
                                }
                                else if (key.Equals("Prop3", StringComparison.OrdinalIgnoreCase))
                                {
                                    folderInfo.Prop3 = propValue;
                                }
                                else if (key.Equals("Prop4", StringComparison.OrdinalIgnoreCase))
                                {
                                    folderInfo.Prop4 = propValue;
                                }
                                else if (key.Equals("Prop5", StringComparison.OrdinalIgnoreCase))
                                {
                                    folderInfo.Prop5 = propValue;
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception)
            {
                // 读取失败时返回空信息，不抛出异常
                return folderInfo;
            }

            return folderInfo;
        }

        /// <summary>
        /// 检查文件夹是否有备注信息
        /// </summary>
        /// <param name="folderPath">文件夹路径</param>
        /// <returns>是否有备注信息</returns>
        public static bool HasFolderRemark(string folderPath)
        {
            try
            {
                if (string.IsNullOrEmpty(folderPath) || !Directory.Exists(folderPath))
                {
                    return false;
                }

                string desktopIniPath = Path.Combine(folderPath, "desktop.ini");
                return File.Exists(desktopIniPath);
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// 清除文件夹备注信息
        /// </summary>
        /// <param name="folderPath">文件夹路径</param>
        /// <returns>操作结果</returns>
        public static string ClearFolderRemark(string folderPath)
        {
            try
            {
                if (string.IsNullOrEmpty(folderPath) || !Directory.Exists(folderPath))
                {
                    return "文件夹不存在: " + folderPath;
                }

                string desktopIniPath = Path.Combine(folderPath, "desktop.ini");
                
                if (File.Exists(desktopIniPath))
                {
                    // 移除隐藏和系统属性
                    File.SetAttributes(desktopIniPath, FileAttributes.Normal);
                    // 删除文件
                    File.Delete(desktopIniPath);
                }

                // 移除文件夹的只读属性
                var attr = File.GetAttributes(folderPath);
                if ((attr & FileAttributes.ReadOnly) != 0)
                {
                    File.SetAttributes(folderPath, attr & ~FileAttributes.ReadOnly);
                }

                return "文件夹备注信息已清除。";
            }
            catch (Exception ex)
            {
                return $"清除文件夹备注信息时出错: {ex.Message}";
            }
        }
    }

    /// <summary>
    /// 文件夹属性编辑器入口类
    /// </summary>
    public class FolderPropertyEditor
    {
        /// <summary>
        /// 显示文件夹属性编辑器
        /// </summary>
        /// <param name="folderPath">文件夹路径</param>
        /// <returns>操作结果</returns>
        public static string ShowPropertyEditor(string folderPath)
        {
            if (!Directory.Exists(folderPath))
            {
                return "文件夹不存在";
            }

            try
            {
                var editor = new FolderPropertyEditorWindow(folderPath);
                bool result = editor.ShowDialog() ?? false;
                return result ? "Success" : "操作已取消";
            }
            catch (Exception ex)
            {
                return $"打开属性编辑器时出错: {ex.Message}";
            }
        }
    }
}
