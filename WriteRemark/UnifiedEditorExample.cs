using System;
using System.Collections.Generic;

namespace WriteRemark
{
    /// <summary>
    /// 统一编辑器接口使用示例
    /// 演示如何使用 FilePropertyEditor.ShowEditor() 智能接口
    /// </summary>
    public class UnifiedEditorExample
    {
        public static void Main(string[] args)
        {
            Console.WriteLine("=== 统一编辑器接口使用示例 ===\n");

            // ========================================
            // 示例 1: 传入单个文件路径
            // ========================================
            Console.WriteLine("示例 1: 单个文件");
            string singleFile = @"C:\test\example.txt";
            string result1 = FilePropertyEditor.ShowEditor(singleFile);
            Console.WriteLine($"结果: {result1}\n");

            // ========================================
            // 示例 2: 传入单个文件夹路径
            // ========================================
            Console.WriteLine("示例 2: 单个文件夹");
            string singleFolder = @"C:\test\MyFolder";
            string result2 = FilePropertyEditor.ShowEditor(singleFolder);
            Console.WriteLine($"结果: {result2}\n");

            // ========================================
            // 示例 3: 传入空路径（会提示错误）
            // ========================================
            Console.WriteLine("示例 3: 空路径");
            string result3 = FilePropertyEditor.ShowEditor("");
            Console.WriteLine($"结果: {result3}\n");

            // ========================================
            // 示例 4: 传入不存在的路径（会提示错误）
            // ========================================
            Console.WriteLine("示例 4: 不存在的路径");
            string result4 = FilePropertyEditor.ShowEditor(@"C:\不存在的路径.txt");
            Console.WriteLine($"结果: {result4}\n");

            // ========================================
            // 示例 5: 传入多个路径（批量编辑）
            // ========================================
            Console.WriteLine("示例 5: 批量编辑（多个文件和文件夹）");
            var multiplePaths = new List<string>
            {
                @"C:\test\file1.txt",
                @"C:\test\file2.txt",
                @"C:\test\folder1",
                @"C:\test\folder2"
            };
            string result5 = FilePropertyEditor.ShowEditor(multiplePaths);
            Console.WriteLine($"结果: {result5}\n");

            // ========================================
            // 示例 6: 传入单个路径的列表（会自动使用单文件/文件夹编辑器）
            // ========================================
            Console.WriteLine("示例 6: 单元素列表（自动使用单文件编辑器）");
            var singlePathList = new List<string> { @"C:\test\example.txt" };
            string result6 = FilePropertyEditor.ShowEditor(singlePathList);
            Console.WriteLine($"结果: {result6}\n");

            // ========================================
            // 示例 7: 传入空列表（会提示错误）
            // ========================================
            Console.WriteLine("示例 7: 空列表");
            var emptyList = new List<string>();
            string result7 = FilePropertyEditor.ShowEditor(emptyList);
            Console.WriteLine($"结果: {result7}\n");

            // ========================================
            // 示例 8: 传入null（会提示错误）
            // ========================================
            Console.WriteLine("示例 8: null路径");
            List<string> nullList = null;
            string result8 = FilePropertyEditor.ShowEditor(nullList);
            Console.WriteLine($"结果: {result8}\n");

            // ========================================
            // 示例 9: 如果需要直接指定编辑器类型
            // ========================================
            Console.WriteLine("示例 9: 直接使用指定的编辑器");
            
            // 直接使用文件编辑器
            string result9a = FilePropertyEditor.ShowFileEditor(@"C:\test\example.txt");
            Console.WriteLine($"文件编辑器结果: {result9a}");
            
            // 直接使用文件夹编辑器
            string result9b = FilePropertyEditor.ShowFolderEditor(@"C:\test\MyFolder");
            Console.WriteLine($"文件夹编辑器结果: {result9b}");
            
            // 直接使用批量编辑器
            var batchPaths = new List<string> { @"C:\test\file1.txt", @"C:\test\file2.txt" };
            string result9c = FilePropertyEditor.ShowBatchEditor(batchPaths);
            Console.WriteLine($"批量编辑器结果: {result9c}\n");

            Console.WriteLine("=== 示例结束 ===");
            Console.WriteLine("\n按任意键退出...");
            Console.ReadKey();
        }

        /// <summary>
        /// 从命令行参数启动编辑器的示例
        /// </summary>
        public static void LaunchFromCommandLine(string[] args)
        {
            if (args == null || args.Length == 0)
            {
                Console.WriteLine("请提供文件或文件夹路径作为参数");
                Console.WriteLine("用法:");
                Console.WriteLine("  单个路径: program.exe \"C:\\path\\to\\file.txt\"");
                Console.WriteLine("  多个路径: program.exe \"C:\\file1.txt\" \"C:\\file2.txt\" \"C:\\folder1\"");
                return;
            }

            // 将命令行参数转换为列表
            var paths = new List<string>(args);

            // 使用统一接口
            string result = FilePropertyEditor.ShowEditor(paths);
            Console.WriteLine(result);
        }
    }
}

