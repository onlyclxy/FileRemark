using System;
using System.IO;
using System.Windows;

namespace WriteRemark
{
    /// <summary>
    /// 文件夹属性编辑器测试程序
    /// </summary>
    public class FolderTestProgram
    {
        [STAThread]
        public static void Main(string[] args)
        {
            // 创建WPF应用程序
            var app = new Application();
            app.ShutdownMode = ShutdownMode.OnExplicitShutdown;
            
            try
            {
                string testFolderPath;
                
                if (args.Length > 0 && Directory.Exists(args[0]))
                {
                    testFolderPath = args[0];
                }
                else
                {
                    // 如果没有提供文件夹参数，创建一个测试文件夹
                    testFolderPath = Path.Combine(Path.GetTempPath(), "TestFolder_" + DateTime.Now.ToString("yyyyMMdd_HHmmss"));
                    Directory.CreateDirectory(testFolderPath);
                    
                    // 在测试文件夹中创建一些示例文件
                    File.WriteAllText(Path.Combine(testFolderPath, "示例文件1.txt"), "这是第一个示例文件。");
                    File.WriteAllText(Path.Combine(testFolderPath, "示例文件2.txt"), "这是第二个示例文件。");
                    
                    MessageBox.Show($"创建了测试文件夹：{testFolderPath}\n\n现在将打开文件夹属性编辑器。", 
                        "测试程序", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                
                // 显示文件夹属性编辑器
                var editor = new FolderPropertyEditorWindow(testFolderPath);
                var result = editor.ShowDialog();
                
                if (result == true)
                {
                    MessageBox.Show("文件夹属性已成功保存！", "成功", MessageBoxButton.OK, MessageBoxImage.Information);
                    
                    // 显示保存后的信息
                    var folderInfo = FolderRemarkManager.ReadFolderRemark(testFolderPath);
                    string infoMessage = $"已保存的文件夹信息：\n\n";
                    infoMessage += $"备注：{folderInfo.InfoTip}\n";
                    infoMessage += $"标记：{folderInfo.Prop5}\n\n";
                    infoMessage += "您可以在资源管理器中将鼠标悬停在文件夹上查看备注提示信息。";
                    
                    MessageBox.Show(infoMessage, "保存结果", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    MessageBox.Show("操作已取消。", "取消", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                
                // 如果是临时文件夹，询问是否删除
                if (testFolderPath.StartsWith(Path.GetTempPath()) && Directory.Exists(testFolderPath))
                {
                    var deleteResult = MessageBox.Show("是否删除临时测试文件夹？", "清理", 
                        MessageBoxButton.YesNo, MessageBoxImage.Question);
                    if (deleteResult == MessageBoxResult.Yes)
                    {
                        try
                        {
                            // 删除前先清除文件夹属性
                            FolderRemarkManager.ClearFolderRemark(testFolderPath);
                            Directory.Delete(testFolderPath, true);
                            MessageBox.Show("临时测试文件夹已删除。", "清理完成", 
                                MessageBoxButton.OK, MessageBoxImage.Information);
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show($"删除临时文件夹时出错：{ex.Message}", "错误", 
                                MessageBoxButton.OK, MessageBoxImage.Warning);
                        }
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

        /// <summary>
        /// 演示文件夹备注功能的静态方法
        /// </summary>
        /// <param name="folderPath">文件夹路径</param>
        public static void DemoFolderRemark(string folderPath)
        {
            if (!Directory.Exists(folderPath))
            {
                MessageBox.Show("指定的文件夹不存在：" + folderPath, "错误", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            try
            {
                // 读取现有信息
                var existingInfo = FolderRemarkManager.ReadFolderRemark(folderPath);
                
                string message = $"文件夹：{folderPath}\n\n";
                message += $"当前备注提示信息：{existingInfo.InfoTip}\n";
                message += $"当前标记：{existingInfo.Prop5}\n\n";
                message += "是否要编辑文件夹属性？";
                
                var result = MessageBox.Show(message, "文件夹信息", 
                    MessageBoxButton.YesNo, MessageBoxImage.Question);
                
                if (result == MessageBoxResult.Yes)
                {
                    string editResult = FolderPropertyEditor.ShowPropertyEditor(folderPath);
                    
                    if (editResult == "Success")
                    {
                        MessageBox.Show("文件夹属性编辑完成！", "成功", 
                            MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"操作文件夹属性时出错：{ex.Message}", "错误", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
