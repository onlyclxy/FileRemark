using System;
using System.IO;
using WriteRemark;

class FolderProgram
{
    [STAThread]
    static void Main_Disabled(string[] args)
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
            Console.WriteLine("文件夹属性已成功保存。");
            
            // 读取并显示保存的信息
            var folderInfo = FolderRemarkManager.ReadFolderRemark(testFolderPath);
            Console.WriteLine($"备注: {folderInfo.InfoTip}");
            Console.WriteLine($"标签: {folderInfo.Prop5}");
        }
        else
        {
            Console.WriteLine("操作已取消或保存失败。");
        }

        Console.WriteLine("按任意键退出...");
        Console.ReadKey();
    }
}
