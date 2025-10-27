using System;
using System.Collections.Generic;

namespace WriteRemark
{
    /// <summary>
    /// 快速测试程序 - 演示统一接口的简洁性
    /// </summary>
    public class QuickTest
    {
        /// <summary>
        /// 最简单的使用示例
        /// </summary>
        public static void SimpleExample()
        {
            // 方式1: 传入单个路径（自动判断是文件还是文件夹）
            string path = @"C:\test\example.txt";
            string result = FilePropertyEditor.ShowEditor(path);
            Console.WriteLine($"结果: {result}");

            // 方式2: 传入多个路径（自动选择批量编辑器）
            var paths = new List<string>
            {
                @"C:\test\file1.txt",
                @"C:\test\file2.txt",
                @"C:\test\folder1"
            };
            result = FilePropertyEditor.ShowEditor(paths);
            Console.WriteLine($"结果: {result}");
        }

        /// <summary>
        /// 处理用户选择的文件/文件夹
        /// </summary>
        public static void HandleUserSelection(string selectedPath)
        {
            // 无需任何判断逻辑，直接调用
            string result = FilePropertyEditor.ShowEditor(selectedPath);

            // 根据结果做相应处理
            switch (result)
            {
                case "Success":
                    Console.WriteLine("✓ 属性已成功保存");
                    break;
                case "操作已取消":
                    Console.WriteLine("操作已取消");
                    break;
                default:
                    Console.WriteLine($"✗ 错误: {result}");
                    break;
            }
        }

        /// <summary>
        /// 处理批量选择的文件/文件夹
        /// </summary>
        public static void HandleBatchSelection(List<string> selectedPaths)
        {
            // 无需任何判断逻辑，直接调用
            string result = FilePropertyEditor.ShowEditor(selectedPaths);

            // 根据结果做相应处理
            if (result == "Success")
            {
                Console.WriteLine($"✓ 成功保存 {selectedPaths.Count} 个项目的属性");
            }
            else if (result == "操作已取消")
            {
                Console.WriteLine("操作已取消");
            }
            else
            {
                Console.WriteLine($"✗ 错误: {result}");
            }
        }

        /// <summary>
        /// 模拟从Shell扩展菜单调用
        /// </summary>
        public static void ShellExtensionExample(string[] rightClickedItems)
        {
            if (rightClickedItems == null || rightClickedItems.Length == 0)
            {
                Console.WriteLine("没有选择任何项目");
                return;
            }

            // 一行代码搞定！
            string result = FilePropertyEditor.ShowEditor(new List<string>(rightClickedItems));
            Console.WriteLine(result);
        }

        /// <summary>
        /// 对比旧版写法和新版写法
        /// </summary>
        public static void CompareOldAndNew(string path)
        {
            // ========================================
            // 旧版写法（需要手动判断）
            // ========================================
            /*
            string result;
            if (File.Exists(path))
            {
                result = FilePropertyEditor.ShowPropertyEditor(path);
            }
            else if (Directory.Exists(path))
            {
                result = FilePropertyEditor.ShowFolderEditor(path);
            }
            else
            {
                result = "路径不存在";
            }
            Console.WriteLine(result);
            */

            // ========================================
            // 新版写法（一行搞定）
            // ========================================
            string result = FilePropertyEditor.ShowEditor(path);
            Console.WriteLine(result);
        }
    }
}

