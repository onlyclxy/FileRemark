using System;
using System.IO;
using WriteRemark;

class SimpleTest
{
    [STAThread]
    static void Main(string[] args)
    {
        Console.WriteLine("=== 简化版文件备注测试程序 ===");
        
        // 测试文件备注功能（不使用WPF界面）
        TestFileRemarkDirect();
        
        // 测试文件夹备注功能
        TestFolderRemarkDirect();
        
        Console.WriteLine("\n按任意键退出...");
        Console.ReadKey();
    }

    /// <summary>
    /// 直接测试文件备注功能（不使用WPF界面）
    /// </summary>
    static void TestFileRemarkDirect()
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

            Console.WriteLine($"正在为文件添加备注: {testFilePath}");

            // 直接调用备注写入方法
            RemarkWriter commenter = new RemarkWriter();
            string result = commenter.SetFileComment(testFilePath, "这是一个测试备注信息 - " + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));

            if (result.Contains("成功"))
            {
                Console.WriteLine("成功：" + result);
                
                // 读取并显示备注
                string readComment = ReadRemark.ReadFileComment(testFilePath);
                Console.WriteLine($"读取到的备注: {readComment}");
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
    /// 直接测试文件夹备注功能
    /// </summary>
    static void TestFolderRemarkDirect()
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

            Console.WriteLine($"正在为文件夹添加备注: {testFolderPath}");

            // 直接调用文件夹备注写入方法
            string infoTip = "这是一个测试文件夹的提示信息 - " + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            string tag = "测试标签";
            
            string result = FolderRemarkManager.WriteFolderRemark(testFolderPath, infoTip, tag);

            if (result.Contains("成功"))
            {
                Console.WriteLine("成功：" + result);
                
                // 读取并显示保存的信息
                var folderInfo = FolderRemarkManager.ReadFolderRemark(testFolderPath);
                Console.WriteLine($"提示信息: {folderInfo.InfoTip}");
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
}
