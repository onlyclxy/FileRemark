using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using WriteRemark;

class Program
{
    [STAThread]
    static void Main(string[] args)
    {
        // 检测args是否为空
        List<string> paths;
        if (args == null || args.Length == 0)
        {
            // 默认测试路径列表（请修改为你实际的路径）
            paths = new List<string>
            {
                @"C:\test\file1.txt",
                @"C:\test\file2.txt",
                @"C:\test\folder1",
                @"D:\测试文件.txt",
                @"D:\我的文件夹"
            };
            
            Console.WriteLine("未提供参数，使用默认测试路径");
        }
        else
        {
            // 清理路径（去除引号和前后空格）
            paths = CleanPaths(new List<string>(args));
            
            Console.WriteLine($"接收到 {paths.Count} 个路径参数：");
            foreach (var path in paths)
            {
                Console.WriteLine($"  {path}");
            }
        }

        // 判断路径是否存在
        if (!ValidatePaths(paths))
        {
            Console.WriteLine("\n错误：路径无效！");
            Console.WriteLine("请检查：");
            Console.WriteLine("  1. 路径是否正确");
            Console.WriteLine("  2. 文件/文件夹是否存在");
            Console.WriteLine("  3. 路径中是否包含特殊字符");
            
            // 显示每个路径的验证结果
            Console.WriteLine("\n路径验证详情：");
            foreach (var path in paths)
            {
                bool isFile = File.Exists(path);
                bool isFolder = Directory.Exists(path);
                
                if (isFile)
                    Console.WriteLine($"  ✓ 文件存在: {path}");
                else if (isFolder)
                    Console.WriteLine($"  ✓ 文件夹存在: {path}");
                else
                    Console.WriteLine($"  ✗ 不存在: {path}");
            }
            
            Console.WriteLine("\n按任意键退出...");
            Console.ReadKey();
            return;
        }

        // 调用统一接口
        string result = FilePropertyEditor.ShowEditor(paths);
        Console.WriteLine($"\n结果: {result}");
        
        //Console.WriteLine("按任意键退出...");
        //Console.ReadKey();
    }

    /// <summary>
    /// 清理路径列表（去除引号、前后空格等）
    /// 支持多行路径输入：如果参数中包含换行符，会按行分割
    /// </summary>
    static List<string> CleanPaths(List<string> paths)
    {
        var cleanedPaths = new List<string>();

        foreach (var path in paths)
        {
            if (string.IsNullOrWhiteSpace(path))
                continue;

            // 检查是否包含换行符（多行路径）
            if (path.Contains("\n") || path.Contains("\r"))
            {
                // 按行分割，每一行是一个路径
                var lines = path.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
                foreach (var line in lines)
                {
                    string cleaned = CleanSinglePath(line);
                    if (!string.IsNullOrWhiteSpace(cleaned))
                        cleanedPaths.Add(cleaned);
                }
            }
            else
            {
                // 单行路径
                string cleaned = CleanSinglePath(path);
                if (!string.IsNullOrWhiteSpace(cleaned))
                    cleanedPaths.Add(cleaned);
            }
        }

        return cleanedPaths;
    }

    /// <summary>
    /// 清理单个路径（去除引号、前后空格）
    /// </summary>
    static string CleanSinglePath(string path)
    {
        if (string.IsNullOrWhiteSpace(path))
            return string.Empty;

        // 去除前后空格
        string cleaned = path.Trim();

        // 去除前后的引号（单引号和双引号）
        if (cleaned.StartsWith("\"") && cleaned.EndsWith("\""))
            cleaned = cleaned.Substring(1, cleaned.Length - 2);
        else if (cleaned.StartsWith("'") && cleaned.EndsWith("'"))
            cleaned = cleaned.Substring(1, cleaned.Length - 2);

        // 再次去除空格（引号内可能有空格）
        cleaned = cleaned.Trim();

        return cleaned;
    }

    /// <summary>
    /// 验证路径是否存在
    /// </summary>
    static bool ValidatePaths(List<string> paths)
    {
        if (paths == null || !paths.Any())
            return false;

        // 检查是否至少有一个路径存在
        return paths.Any(p => File.Exists(p) || Directory.Exists(p));
    }

    #region 旧版测试方法（已注释，仅供参考）
    
    /*
    /// <summary>
    /// 测试文件备注功能（旧版）
    /// </summary>
    static void TestFileRemark()
    {
        Console.WriteLine("\n--- 测试文件备注功能 ---");
        
        try
        {
            // 创建一个测试文件
            string testFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "test_file.txt");
            if (!File.Exists(testFilePath))
            {
                File.WriteAllText(testFilePath, "这是一个用于测试文件属性编辑的文本文件。");
            }

            Console.WriteLine($"正在编辑文件: {testFilePath}");

            // 调用属性编辑器窗口
            string result = FilePropertyEditor.ShowPropertyEditor(testFilePath);

            if (result == "Success")
            {
                Console.WriteLine("成功：属性已成功保存。");
            }
            else
            {
                Console.WriteLine($"错误提示：{result}");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"错误提示：测试文件备注时发生异常 - {ex.Message}");
        }
    }

    /// <summary>
    /// 测试文件夹备注功能（旧版）
    /// </summary>
    static void TestFolderRemark()
    {
        Console.WriteLine("\n--- 测试文件夹备注功能 ---");

        try
        {
            // 创建一个测试文件夹
            string testFolderPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "test_folder");
            if (!Directory.Exists(testFolderPath))
            {
                Directory.CreateDirectory(testFolderPath);
                File.WriteAllText(Path.Combine(testFolderPath, "示例文件.txt"), "这是一个用于测试文件夹属性编辑的示例文件。");
            }

            Console.WriteLine($"正在编辑文件夹: {testFolderPath}");

            // 调用文件夹属性编辑器窗口
            string result = FolderPropertyEditor.ShowPropertyEditor(testFolderPath);

            if (result == "Success")
            {
                Console.WriteLine("成功：文件夹属性已成功保存。");

                // 读取并显示保存的信息
                var folderInfo = FolderRemarkManager.ReadFolderRemark(testFolderPath);
                Console.WriteLine($"备注: {folderInfo.InfoTip}");
                Console.WriteLine($"标签: {folderInfo.Prop5}");
            }
            else
            {
                Console.WriteLine($"错误提示：{result}");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"错误提示：测试文件夹备注时发生异常 - {ex.Message}");
        }
    }

    /// <summary>
    /// 测试批量文件备注功能（旧版）
    /// </summary>
    static void TestBatchFileRemark()
    {
        Console.WriteLine("\n--- 测试批量文件备注功能 ---");

        try
        {
            // 创建多个测试文件
            string baseDir = AppDomain.CurrentDomain.BaseDirectory;
            var testFiles = new List<string>();

            for (int i = 1; i <= 5; i++)
            {
                string testFilePath = Path.Combine(baseDir, $"batch_test_{i}.txt");
                if (!File.Exists(testFilePath))
                {
                    File.WriteAllText(testFilePath, $"这是批量测试文件 #{i}");
                }
                testFiles.Add(testFilePath);
            }

            Console.WriteLine($"正在批量编辑 {testFiles.Count} 个文件...");

            // 调用批量属性编辑器
            string result = FilePropertyEditor.ShowBatchPropertyEditor(testFiles);

            if (result == "Success")
            {
                Console.WriteLine("成功：批量属性已成功保存。");
            }
            else
            {
                Console.WriteLine($"提示：{result}");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"错误提示：测试批量文件备注时发生异常 - {ex.Message}");
        }
    }
    */
    
    #endregion
}
