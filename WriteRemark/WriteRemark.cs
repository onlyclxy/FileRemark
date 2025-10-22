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
        /// 显示单文件属性编辑器
        /// </summary>
        public static string ShowPropertyEditor(string filePath)
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
                return $"打开属性编辑器时出错: {ex.Message}";
            }
        }

        /// <summary>
        /// 显示批量文件属性编辑器（支持文件和文件夹混合）
        /// </summary>
        public static string ShowBatchPropertyEditor(List<string> paths)
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

        /// <summary>
        /// 智能选择编辑器：
        /// - 单个文件：使用单文件编辑器
        /// - 单个文件夹：使用单文件夹编辑器
        /// - 多个文件/文件夹：使用批量编辑器
        /// </summary>
        public static string ShowPropertyEditor(List<string> paths)
        {
            if (paths == null || !paths.Any())
            {
                return "没有提供路径";
            }

            // 分离文件和文件夹
            var existingFiles = paths.Where(File.Exists).ToList();
            var existingFolders = paths.Where(Directory.Exists).ToList();

            int totalCount = existingFiles.Count + existingFolders.Count;

            if (totalCount == 0)
            {
                return "没有有效的文件或文件夹";
            }

            // 单个文件：使用单文件编辑器
            if (totalCount == 1 && existingFiles.Count == 1)
            {
                return ShowPropertyEditor(existingFiles[0]);
            }
            // 单个文件夹：使用单文件夹编辑器
            else if (totalCount == 1 && existingFolders.Count == 1)
            {
                try
                {
                    var editor = new FolderPropertyEditorWindow(existingFolders[0]);
                    bool result = editor.ShowDialog() ?? false;
                    return result ? "Success" : "操作已取消";
                }
                catch (Exception ex)
                {
                    return $"打开文件夹编辑器时出错: {ex.Message}";
                }
            }
            // 多个文件/文件夹：使用批量编辑器
            else
            {
                var allPaths = new List<string>();
                allPaths.AddRange(existingFiles);
                allPaths.AddRange(existingFolders);
                return ShowBatchPropertyEditor(allPaths);
            }
        }
    }
}