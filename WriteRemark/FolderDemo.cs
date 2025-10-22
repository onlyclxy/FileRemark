using System;
using System.IO;
using System.Windows;

namespace WriteRemark
{
    /// <summary>
    /// 文件夹属性编辑器演示程序
    /// 可以在现有的 WriteRemark 程序中调用此类来编辑文件夹属性
    /// </summary>
    public class FolderDemo
    {
        /// <summary>
        /// 演示文件夹属性编辑功能的主入口
        /// </summary>
        /// <param name="args">命令行参数，第一个参数应该是文件夹路径</param>
        [STAThread]
        public static void MainDemo(string[] args)
        {
            // 创建WPF应用程序
            var app = new Application();
            app.ShutdownMode = ShutdownMode.OnExplicitShutdown;
            
            try
            {
                string folderPath;
                
                if (args.Length > 0 && Directory.Exists(args[0]))
                {
                    folderPath = args[0];
                }
                else
                {
                    // 让用户选择文件夹或使用默认测试文件夹
                    folderPath = GetTestFolder();
                    if (string.IsNullOrEmpty(folderPath))
                    {
                        MessageBox.Show("未选择有效的文件夹，程序退出。", "提示", 
                            MessageBoxButton.OK, MessageBoxImage.Information);
                        return;
                    }
                }
                
                // 显示当前文件夹信息
                ShowCurrentFolderInfo(folderPath);
                
                // 询问是否要编辑
                var editResult = MessageBox.Show($"是否要编辑文件夹 \"{folderPath}\" 的属性？", 
                    "确认", MessageBoxButton.YesNo, MessageBoxImage.Question);
                
                if (editResult == MessageBoxResult.Yes)
                {
                    // 打开文件夹属性编辑器
                    string result = FolderPropertyEditor.ShowPropertyEditor(folderPath);
                    
                    if (result == "Success")
                    {
                        MessageBox.Show("文件夹属性已成功保存！", "成功", 
                            MessageBoxButton.OK, MessageBoxImage.Information);
                        
                        // 再次显示文件夹信息以确认更改
                        ShowCurrentFolderInfo(folderPath);
                    }
                    else
                    {
                        MessageBox.Show("操作已取消或保存失败。", "提示", 
                            MessageBoxButton.OK, MessageBoxImage.Information);
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
        /// 显示当前文件夹的信息
        /// </summary>
        /// <param name="folderPath">文件夹路径</param>
        private static void ShowCurrentFolderInfo(string folderPath)
        {
            try
            {
                var folderInfo = FolderRemarkManager.ReadFolderRemark(folderPath);
                
                string info = $"文件夹：{folderPath}\n\n";
                info += $"备注：{(string.IsNullOrEmpty(folderInfo.InfoTip) ? "（无）" : folderInfo.InfoTip)}\n";
                info += $"标记：{(string.IsNullOrEmpty(folderInfo.Prop5) ? "（无）" : folderInfo.Prop5)}\n\n";
                
                if (!string.IsNullOrEmpty(folderInfo.InfoTip) || !string.IsNullOrEmpty(folderInfo.Prop5))
                {
                    info += "提示：在资源管理器中将鼠标悬停在文件夹上可以看到备注提示信息。";
                }
                else
                {
                    info += "该文件夹当前没有设置任何属性信息。";
                }
                
                MessageBox.Show(info, "文件夹信息", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"读取文件夹信息时出错：{ex.Message}", "错误", 
                    MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        /// <summary>
        /// 获取测试文件夹路径
        /// </summary>
        /// <returns>文件夹路径</returns>
        private static string GetTestFolder()
        {
            // 这里可以实现文件夹选择对话框，但为了简化，我们直接创建一个测试文件夹
            string testFolderPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), 
                "文件夹属性测试_" + DateTime.Now.ToString("yyyyMMdd_HHmmss"));
            
            var result = MessageBox.Show($"将在桌面创建测试文件夹：\n{testFolderPath}\n\n是否继续？", 
                "创建测试文件夹", MessageBoxButton.YesNo, MessageBoxImage.Question);
            
            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    Directory.CreateDirectory(testFolderPath);
                    
                    // 创建一些示例文件
                    File.WriteAllText(Path.Combine(testFolderPath, "说明.txt"), 
                        "这是一个用于测试文件夹属性编辑功能的测试文件夹。\n\n" +
                        "您可以为这个文件夹设置备注提示信息和标记，\n" +
                        "然后在资源管理器中查看效果。");
                    
                    return testFolderPath;
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"创建测试文件夹失败：{ex.Message}", "错误", 
                        MessageBoxButton.OK, MessageBoxImage.Error);
                    return null;
                }
            }
            
            return null;
        }

        /// <summary>
        /// 快速编辑指定文件夹的属性
        /// </summary>
        /// <param name="folderPath">文件夹路径</param>
        /// <returns>是否成功编辑并保存</returns>
        public static bool QuickEditFolder(string folderPath)
        {
            if (!Directory.Exists(folderPath))
            {
                MessageBox.Show($"文件夹不存在：{folderPath}", "错误", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }

            string result = FolderPropertyEditor.ShowPropertyEditor(folderPath);
            return result == "Success";
        }

        /// <summary>
        /// 批量设置多个文件夹的属性
        /// </summary>
        /// <param name="folderPaths">文件夹路径数组</param>
        /// <param name="infoTip">统一的备注提示信息</param>
        /// <param name="tag">统一的标记</param>
        /// <returns>成功处理的文件夹数量</returns>
        public static int BatchSetFolderProperties(string[] folderPaths, string infoTip, string tag)
        {
            int successCount = 0;
            
            foreach (string folderPath in folderPaths)
            {
                if (Directory.Exists(folderPath))
                {
                    try
                    {
                        string result = FolderRemarkManager.WriteFolderRemark(folderPath, infoTip, tag);
                        if (result.Contains("成功"))
                        {
                            successCount++;
                        }
                    }
                    catch (Exception)
                    {
                        // 忽略单个文件夹的错误，继续处理其他文件夹
                    }
                }
            }
            
            return successCount;
        }
    }
}
