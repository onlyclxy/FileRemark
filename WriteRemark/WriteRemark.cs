using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Microsoft.WindowsAPICodePack.Shell;

namespace WriteRemark
{
    public class RemarkWriter
    {
        [DllImport("shell32.dll", CharSet = CharSet.Unicode, PreserveSig = false)]
        private static extern void SHGetFileInfo(string pszPath, uint dwFileAttributes, ref SHFILEINFO psfi, uint cbFileInfo, uint uFlags);

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        private struct SHFILEINFO
        {
            public IntPtr hIcon;
            public int iIcon;
            public uint dwAttributes;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 260)]
            public string szDisplayName;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 80)]
            public string szTypeName;
        }

        private const uint SHGFI_TYPENAME = 0x000000400;

        public string SetFileComment(string filePath, string comment)
        {
            if (!File.Exists(filePath))
            {
                return "不是一个有效的文件。";
            }

            try
            {
                FileInfo fileInfo = new FileInfo(filePath);
                SHFILEINFO shinfo = new SHFILEINFO();
                SHGetFileInfo(filePath, 0, ref shinfo, (uint)Marshal.SizeOf(shinfo), SHGFI_TYPENAME);

                if (string.IsNullOrWhiteSpace(shinfo.szTypeName))
                {
                    return "文件类型不支持备注信息。";
                }

                // 清理输入内容：去掉末尾的空行和空白字符
                string cleanComment = (comment ?? "").TrimEnd('\r', '\n', ' ', '\t');
                
                var file = ShellFile.FromFilePath(filePath);
                file.Properties.System.Comment.Value = cleanComment; // 赋值后系统会自动保存

                return "备注信息已成功写入。";
            }
            catch (Exception ex)
            {
                return $"写入备注信息时出错: {ex.Message}";
            }
        }
    }


    public class ReadRemark
    {
        public static string ReadFileComment(string filePath)
        {
            try
            {
                var file = ShellFile.FromFilePath(filePath);
                string comment = file.Properties.System.Comment.Value;
                return comment;


            }
            catch (Exception)
            {
                //Console.WriteLine($"读取备注信息时出错: {ex.Message}");
                return "";
            }
        }

    }

    public class FilePropertyEditor
    {
        /// <summary>
        /// 【推荐使用】智能编辑器入口 - 自动判断路径类型并打开对应的编辑器
        /// - 单个文件：打开文件属性编辑器
        /// - 单个文件夹：打开文件夹属性编辑器
        /// - 空路径或不存在：返回错误提示
        /// </summary>
        /// <param name="path">文件或文件夹路径</param>
        /// <returns>操作结果信息</returns>
        public static string ShowEditor(string path)
        {
            // 检查路径是否为空
            if (string.IsNullOrWhiteSpace(path))
            {
                return "请提供有效的文件或文件夹路径";
            }

            // 判断是文件还是文件夹
            if (File.Exists(path))
            {
                // 是文件，使用文件编辑器
                return ShowFileEditor(path);
            }
            else if (Directory.Exists(path))
            {
                // 是文件夹，使用文件夹编辑器
                return ShowFolderEditor(path);
            }
            else
            {
                return $"路径不存在：{path}";
            }
        }

        /// <summary>
        /// 【推荐使用】智能编辑器入口 - 自动判断单个或批量，并打开对应的编辑器
        /// - 空列表或null：返回错误提示
        /// - 单个文件：打开文件属性编辑器
        /// - 单个文件夹：打开文件夹属性编辑器
        /// - 多个文件/文件夹：打开批量编辑器
        /// </summary>
        /// <param name="paths">文件或文件夹路径列表</param>
        /// <returns>操作结果信息</returns>
        public static string ShowEditor(List<string> paths)
        {
            // 检查路径列表是否为空
            if (paths == null || !paths.Any())
            {
                return "请提供有效的文件或文件夹路径";
            }

            // 过滤出有效路径
            var validPaths = paths.Where(p => !string.IsNullOrWhiteSpace(p)).ToList();
            
            if (!validPaths.Any())
            {
                return "请提供有效的文件或文件夹路径";
            }

            // 分离文件和文件夹
            var existingFiles = validPaths.Where(File.Exists).ToList();
            var existingFolders = validPaths.Where(Directory.Exists).ToList();

            int totalCount = existingFiles.Count + existingFolders.Count;

            // 没有有效路径
            if (totalCount == 0)
            {
                return "没有找到有效的文件或文件夹";
            }

            // 单个路径
            if (totalCount == 1)
            {
                if (existingFiles.Count == 1)
                {
                    // 单个文件：使用单文件编辑器
                    return ShowFileEditor(existingFiles[0]);
                }
                else
                {
                    // 单个文件夹：使用单文件夹编辑器
                    return ShowFolderEditor(existingFolders[0]);
                }
            }
            // 多个路径：使用批量编辑器
            else
            {
                var allPaths = new List<string>();
                allPaths.AddRange(existingFiles);
                allPaths.AddRange(existingFolders);
                return ShowBatchEditor(allPaths);
            }
        }

        /// <summary>
        /// 显示单文件属性编辑器
        /// </summary>
        /// <param name="filePath">文件路径</param>
        /// <returns>操作结果信息</returns>
        public static string ShowFileEditor(string filePath)
        {
            if (!File.Exists(filePath))
            {
                return "文件不存在";
            }

            try
            {
                var editor = new PropertyEditorWindow(filePath);
                bool result = editor.ShowDialog() ?? false;
                return result ? "Success" : "操作已取消";
            }
            catch (Exception ex)
            {
                return $"打开文件属性编辑器时出错: {ex.Message}";
            }
        }

        /// <summary>
        /// 显示单文件夹属性编辑器
        /// </summary>
        /// <param name="folderPath">文件夹路径</param>
        /// <returns>操作结果信息</returns>
        public static string ShowFolderEditor(string folderPath)
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
                return $"打开文件夹属性编辑器时出错: {ex.Message}";
            }
        }

        /// <summary>
        /// 显示批量属性编辑器（支持文件和文件夹混合）
        /// </summary>
        /// <param name="paths">文件或文件夹路径列表</param>
        /// <returns>操作结果信息</returns>
        public static string ShowBatchEditor(List<string> paths)
        {
            if (paths == null || !paths.Any())
            {
                return "没有提供路径";
            }

            // 分离文件和文件夹，保留所有有效路径
            var validPaths = paths.Where(p => File.Exists(p) || Directory.Exists(p)).ToList();

            if (!validPaths.Any())
            {
                return "没有有效的文件或文件夹";
            }

            try
            {
                var editor = new BatchPropertyEditorWindow(validPaths);
                bool result = editor.ShowDialog() ?? false;
                return result ? "Success" : "操作已取消";
            }
            catch (Exception ex)
            {
                return $"打开批量编辑器时出错: {ex.Message}";
            }
        }

        #region 旧版API（保留以兼容旧代码）

        /// <summary>
        /// [已过时] 请使用 ShowFileEditor 或 ShowEditor
        /// </summary>
        [Obsolete("请使用 ShowFileEditor(string) 或 ShowEditor(string) 替代")]
        public static string ShowPropertyEditor(string filePath)
        {
            return ShowFileEditor(filePath);
        }

        /// <summary>
        /// [已过时] 请使用 ShowBatchEditor 或 ShowEditor
        /// </summary>
        [Obsolete("请使用 ShowBatchEditor(List<string>) 或 ShowEditor(List<string>) 替代")]
        public static string ShowBatchPropertyEditor(List<string> paths)
        {
            return ShowBatchEditor(paths);
        }

        /// <summary>
        /// [已过时] 请使用 ShowEditor
        /// </summary>
        [Obsolete("请使用 ShowEditor(List<string>) 替代")]
        public static string ShowPropertyEditor(List<string> paths)
        {
            return ShowEditor(paths);
        }

        #endregion
    }
}