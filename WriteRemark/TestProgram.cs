using System;
using System.IO;
using System.Windows;

namespace WriteRemark
{
    /// <summary>
    /// 测试程序，用于演示改进的文件属性编辑器
    /// </summary>
    public class TestProgram
    {
        [STAThread]
        public static void Main(string[] args)
        {
            // 创建WPF应用程序
            var app = new Application();
            app.ShutdownMode = ShutdownMode.OnExplicitShutdown;
            
            try
            {
                string testFilePath;
                
                if (args.Length > 0 && File.Exists(args[0]))
                {
                    testFilePath = args[0];
                }
                else
                {
                    // 如果没有提供文件参数，创建一个测试文件
                    testFilePath = Path.Combine(Path.GetTempPath(), "test_file.txt");
                    File.WriteAllText(testFilePath, "这是一个测试文件，用于演示文件属性编辑器的新功能。");
                    
                    MessageBox.Show($"创建了测试文件：{testFilePath}\n\n现在将打开属性编辑器。", 
                        "测试程序", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                
                // 显示属性编辑器
                var editor = new PropertyEditorWindow(testFilePath);
                var result = editor.ShowDialog();
                
                if (result == true)
                {
                    MessageBox.Show("文件属性已成功保存！", "成功", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    MessageBox.Show("操作已取消。", "取消", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                
                // 如果是临时文件，询问是否删除
                if (testFilePath.StartsWith(Path.GetTempPath()) && File.Exists(testFilePath))
                {
                    var deleteResult = MessageBox.Show("是否删除临时测试文件？", "清理", 
                        MessageBoxButton.YesNo, MessageBoxImage.Question);
                    if (deleteResult == MessageBoxResult.Yes)
                    {
                        File.Delete(testFilePath);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"程序运行时出错：{ex.Message}", "错误", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                // 确保应用程序正确关闭
                app.Shutdown();
            }
        }
    }
}
