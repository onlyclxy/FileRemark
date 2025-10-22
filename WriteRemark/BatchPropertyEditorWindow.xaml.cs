using Microsoft.WindowsAPICodePack.Shell;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;

namespace WriteRemark
{
    /// <summary>
    /// BatchPropertyEditorWindow.xaml 的交互逻辑
    /// </summary>
    public partial class BatchPropertyEditorWindow : Window
    {
        // 去掉左上角图标的 Win32 API
        private const int GWL_EXSTYLE = -20;
        private const int WS_EX_DLGMODALFRAME = 0x0001;
        private const int WM_SETICON = 0x0080;
        private const uint SWP_NOSIZE = 0x0001;
        private const uint SWP_NOMOVE = 0x0002;
        private const uint SWP_NOZORDER = 0x0004;
        private const uint SWP_FRAMECHANGED = 0x0020;

        [DllImport("user32.dll")]
        private static extern int GetWindowLong(IntPtr hWnd, int nIndex);

        [DllImport("user32.dll")]
        private static extern int SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);

        [DllImport("user32.dll")]
        private static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter,
            int X, int Y, int cx, int cy, uint uFlags);

        [DllImport("user32.dll")]
        static extern IntPtr SendMessage(IntPtr hWnd, int Msg, IntPtr wParam, IntPtr lParam);

        protected override void OnSourceInitialized(EventArgs e)
        {
            base.OnSourceInitialized(e);

            var hwnd = new WindowInteropHelper(this).Handle;

            int exStyle = GetWindowLong(hwnd, GWL_EXSTYLE);
            SetWindowLong(hwnd, GWL_EXSTYLE, exStyle | WS_EX_DLGMODALFRAME);

            SetWindowPos(hwnd, IntPtr.Zero, 0, 0, 0, 0,
                SWP_NOMOVE | SWP_NOSIZE | SWP_NOZORDER | SWP_FRAMECHANGED);

            SendMessage(hwnd, WM_SETICON, IntPtr.Zero, IntPtr.Zero);
            SendMessage(hwnd, WM_SETICON, new IntPtr(1), IntPtr.Zero);
        }

        private ObservableCollection<BatchFilePropertyModel> _fileModels;
        private ObservableCollection<BatchFolderPropertyModel> _folderModels;
        private List<string> _filePaths;
        private List<string> _folderPaths;
        private bool _isTopMost = true;

        public BatchPropertyEditorWindow(List<string> paths)
        {
            InitializeComponent();
            this.Topmost = _isTopMost;

            // 分离文件和文件夹
            _filePaths = new List<string>();
            _folderPaths = new List<string>();

            foreach (var path in paths ?? new List<string>())
            {
                if (File.Exists(path))
                    _filePaths.Add(path);
                else if (Directory.Exists(path))
                    _folderPaths.Add(path);
            }

            _fileModels = new ObservableCollection<BatchFilePropertyModel>();
            _folderModels = new ObservableCollection<BatchFolderPropertyModel>();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            LoadFiles();
            LoadFolders();

            dgFiles.ItemsSource = _fileModels;
            dgFolders.ItemsSource = _folderModels;

            // 加载并应用列配置
            LoadFileColumnSettings();
            LoadFolderColumnSettings();

            // 监听数据变化以更新修改计数
            foreach (var model in _fileModels)
            {
                model.PropertyChanged += Model_PropertyChanged;
            }

            foreach (var model in _folderModels)
            {
                model.PropertyChanged += FolderModel_PropertyChanged;
            }

            UpdateModifiedCount();
            UpdateExpanderHeaders();
        }

        /// <summary>
        /// 加载文件列配置
        /// </summary>
        private void LoadFileColumnSettings()
        {
            var configs = BatchColumnConfigManager.LoadFileColumns();
            
            // 根据配置设置列的可见性
            foreach (var config in configs)
            {
                var column = dgFiles.Columns.FirstOrDefault(c => 
                    c.Header?.ToString() == GetColumnDisplayName(config.ColumnName));
                
                if (column != null)
                {
                    column.Visibility = config.IsVisible ? Visibility.Visible : Visibility.Collapsed;
                }
            }
        }

        /// <summary>
        /// 加载文件夹列配置
        /// </summary>
        private void LoadFolderColumnSettings()
        {
            var configs = BatchColumnConfigManager.LoadFolderColumns();
            
            // 根据配置设置列的可见性
            foreach (var config in configs)
            {
                var column = dgFolders.Columns.FirstOrDefault(c => 
                    c.Header?.ToString() == GetColumnDisplayName(config.ColumnName));
                
                if (column != null)
                {
                    column.Visibility = config.IsVisible ? Visibility.Visible : Visibility.Collapsed;
                }
            }
        }

        /// <summary>
        /// 获取列的显示名称
        /// </summary>
        private string GetColumnDisplayName(string columnName)
        {
            switch (columnName)
            {
                case "Title": return "标题";
                case "Subject": return "主题";
                case "Rating": return "分级";
                case "Tags": return "标记";
                case "Category": return "类别";
                case "Comment": return "备注";
                case "Alias": return "别名";
                case "InfoTip": return "备注";
                case "Author": return "作者";
                default: return columnName;
            }
        }

        private void Model_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(BatchFilePropertyModel.IsModified))
            {
                UpdateModifiedCount();
            }
        }

        private void FolderModel_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(BatchFolderPropertyModel.IsModified))
            {
                UpdateModifiedCount();
            }
        }

        /// <summary>
        /// 加载文件数据
        /// </summary>
        private void LoadFiles()
        {
            _fileModels.Clear();

            foreach (var filePath in _filePaths)
            {
                if (!File.Exists(filePath))
                    continue;

                try
                {
                    var model = new BatchFilePropertyModel
                    {
                        FilePath = filePath,
                        FileName = Path.GetFileName(filePath)
                    };

                    // 加载现有属性
                    var file = ShellFile.FromFilePath(filePath);
                    model.Title = file.Properties.System.Title.Value ?? "";
                    model.Subject = file.Properties.System.Subject.Value ?? "";
                    model.Rating = file.Properties.System.Rating.Value?.ToString() ?? "";

                    if (file.Properties.System.Keywords.Value != null)
                    {
                        model.Tags = string.Join(";", file.Properties.System.Keywords.Value);
                    }

                    if (file.Properties.System.Category.Value != null)
                    {
                        model.Category = string.Join(";", file.Properties.System.Category.Value);
                    }

                    model.Comment = file.Properties.System.Comment.Value ?? "";

                    // 重置修改标记
                    model.IsModified = false;

                    _fileModels.Add(model);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"加载文件 {Path.GetFileName(filePath)} 时出错: {ex.Message}",
                        "警告", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }

            // 文件计数已移到Expander标题中，此处不再更新
            // txtFileCount.Text = $"文件: {_fileModels.Count}";
        }

        /// <summary>
        /// 加载文件夹数据
        /// </summary>
        private void LoadFolders()
        {
            _folderModels.Clear();

            foreach (var folderPath in _folderPaths)
            {
                if (!Directory.Exists(folderPath))
                    continue;

                try
                {
                    var model = new BatchFolderPropertyModel
                    {
                        FolderPath = folderPath,
                        FolderName = Path.GetFileName(folderPath) ?? folderPath
                    };

                    // 加载现有属性
                    var folderInfo = FolderRemarkManager.ReadFolderRemark(folderPath);
                    model.Alias = folderInfo.LocalizedResourceName ?? "";
                    model.InfoTip = folderInfo.InfoTip ?? "";
                    model.Title = folderInfo.Prop2 ?? "";
                    model.Subject = folderInfo.Prop3 ?? "";
                    model.Author = folderInfo.Prop4 ?? "";
                    model.Tags = folderInfo.Prop5 ?? "";

                    // 重置修改标记
                    model.IsModified = false;

                    _folderModels.Add(model);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"加载文件夹 {Path.GetFileName(folderPath)} 时出错: {ex.Message}",
                        "警告", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }

            // 文件夹计数已移到Expander标题中，此处不再更新
            // txtFolderCount.Text = $"文件夹: {_folderModels.Count}";
        }

        /// <summary>
        /// 更新修改计数
        /// </summary>
        private void UpdateModifiedCount()
        {
            int fileModifiedCount = _fileModels.Count(m => m.IsModified);
            int folderModifiedCount = _folderModels.Count(m => m.IsModified);
            int totalModified = fileModifiedCount + folderModifiedCount;
            txtModifiedCount.Text = $"{totalModified} 个项目已修改 (文件: {fileModifiedCount}, 文件夹: {folderModifiedCount})";
        }

        /// <summary>
        /// 更新 Expander 标题
        /// </summary>
        private void UpdateExpanderHeaders()
        {
            expanderFiles.Header = $"文件 ({_fileModels.Count})";
            expanderFolders.Header = $"文件夹 ({_folderModels.Count})";
        }

        #region 文件选择操作

        private void BtnSelectAllFiles_Click(object sender, RoutedEventArgs e)
        {
            foreach (var model in _fileModels)
            {
                model.IsSelected = true;
            }
        }

        private void BtnSelectNoneFiles_Click(object sender, RoutedEventArgs e)
        {
            foreach (var model in _fileModels)
            {
                model.IsSelected = false;
            }
        }

        private void BtnInvertSelectionFiles_Click(object sender, RoutedEventArgs e)
        {
            foreach (var model in _fileModels)
            {
                model.IsSelected = !model.IsSelected;
            }
        }

        #endregion

        #region 文件夹选择操作

        private void BtnSelectAllFolders_Click(object sender, RoutedEventArgs e)
        {
            foreach (var model in _folderModels)
            {
                model.IsSelected = true;
            }
        }

        private void BtnSelectNoneFolders_Click(object sender, RoutedEventArgs e)
        {
            foreach (var model in _folderModels)
            {
                model.IsSelected = false;
            }
        }

        private void BtnInvertSelectionFolders_Click(object sender, RoutedEventArgs e)
        {
            foreach (var model in _folderModels)
            {
                model.IsSelected = !model.IsSelected;
            }
        }

        #endregion

        #region 文件批量操作

        private void BtnApplyBatchFile_Click(object sender, RoutedEventArgs e)
        {
            var selectedItem = (ComboBoxItem)cmbBatchFieldFile.SelectedItem;
            string fieldName = selectedItem.Content.ToString();
            string value = txtBatchValueFile.Text;

            var selectedModels = _fileModels.Where(m => m.IsSelected).ToList();
            if (!selectedModels.Any())
            {
                MessageBox.Show("请先选择要操作的文件。", "提示", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            foreach (var model in selectedModels)
            {
                switch (fieldName)
                {
                    case "标题":
                        model.Title = value;
                        break;
                    case "主题":
                        model.Subject = value;
                        break;
                    case "分级":
                        model.Rating = value;
                        break;
                    case "标记":
                        model.Tags = value;
                        break;
                    case "类别":
                        model.Category = value;
                        break;
                    case "备注":
                        model.Comment = value;
                        break;
                }
            }

            MessageBox.Show($"已将 {selectedModels.Count} 个文件的 \"{fieldName}\" 设置为 \"{value}\"",
                "完成", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void BtnClearSelectedFile_Click(object sender, RoutedEventArgs e)
        {
            var selectedItem = (ComboBoxItem)cmbBatchFieldFile.SelectedItem;
            string fieldName = selectedItem.Content.ToString();

            var selectedModels = _fileModels.Where(m => m.IsSelected).ToList();
            if (!selectedModels.Any())
            {
                MessageBox.Show("请先选择要操作的文件。", "提示", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            foreach (var model in selectedModels)
            {
                switch (fieldName)
                {
                    case "标题":
                        model.Title = "";
                        break;
                    case "主题":
                        model.Subject = "";
                        break;
                    case "分级":
                        model.Rating = "";
                        break;
                    case "标记":
                        model.Tags = "";
                        break;
                    case "类别":
                        model.Category = "";
                        break;
                    case "备注":
                        model.Comment = "";
                        break;
                }
            }

            MessageBox.Show($"已清空 {selectedModels.Count} 个文件的 \"{fieldName}\"",
                "完成", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        #endregion

        #region 文件夹批量操作

        private void BtnApplyBatchFolder_Click(object sender, RoutedEventArgs e)
        {
            var selectedItem = (ComboBoxItem)cmbBatchFieldFolder.SelectedItem;
            string fieldName = selectedItem.Content.ToString();
            string value = txtBatchValueFolder.Text;

            var selectedModels = _folderModels.Where(m => m.IsSelected).ToList();
            if (!selectedModels.Any())
            {
                MessageBox.Show("请先选择要操作的文件夹。", "提示", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            foreach (var model in selectedModels)
            {
                switch (fieldName)
                {
                    case "别名":
                        model.Alias = value;
                        break;
                    case "备注":
                        model.InfoTip = value;
                        break;
                    case "标题":
                        model.Title = value;
                        break;
                    case "主题":
                        model.Subject = value;
                        break;
                    case "作者":
                        model.Author = value;
                        break;
                    case "标记":
                        model.Tags = value;
                        break;
                }
            }

            MessageBox.Show($"已将 {selectedModels.Count} 个文件夹的 \"{fieldName}\" 设置为 \"{value}\"",
                "完成", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void BtnClearSelectedFolder_Click(object sender, RoutedEventArgs e)
        {
            var selectedItem = (ComboBoxItem)cmbBatchFieldFolder.SelectedItem;
            string fieldName = selectedItem.Content.ToString();

            var selectedModels = _folderModels.Where(m => m.IsSelected).ToList();
            if (!selectedModels.Any())
            {
                MessageBox.Show("请先选择要操作的文件夹。", "提示", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            foreach (var model in selectedModels)
            {
                switch (fieldName)
                {
                    case "别名":
                        model.Alias = "";
                        break;
                    case "备注":
                        model.InfoTip = "";
                        break;
                    case "标题":
                        model.Title = "";
                        break;
                    case "主题":
                        model.Subject = "";
                        break;
                    case "作者":
                        model.Author = "";
                        break;
                    case "标记":
                        model.Tags = "";
                        break;
                }
            }

            MessageBox.Show($"已清空 {selectedModels.Count} 个文件夹的 \"{fieldName}\"",
                "完成", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        #endregion

        #region 置顶功能

        private void BtnTopMost_Click(object sender, RoutedEventArgs e)
        {
            _isTopMost = !_isTopMost;
            this.Topmost = _isTopMost;
            
            // 更新按钮样式
            var path = pathTopMost;
            if (_isTopMost)
            {
                // 置顶状态：向上的图标，绿色
                path.Data = Geometry.Parse("M7,15L12,10L17,15H7Z");
                path.Fill = new SolidColorBrush(Color.FromRgb(76, 175, 80));
                btnTopMost.ToolTip = "窗口已置顶（点击取消）";
            }
            else
            {
                // 非置顶状态：向下的图标，灰色
                path.Data = Geometry.Parse("M7,10L12,15L17,10H7Z");
                path.Fill = new SolidColorBrush(Color.FromRgb(158, 158, 158));
                btnTopMost.ToolTip = "点击置顶窗口";
            }
        }

        #endregion

        #region 列设置

        private void BtnFileColumnSettings_Click(object sender, RoutedEventArgs e)
        {
            ShowColumnSettingsDialog("文件列设置", dgFiles, true);
        }

        private void BtnFolderColumnSettings_Click(object sender, RoutedEventArgs e)
        {
            ShowColumnSettingsDialog("文件夹列设置", dgFolders, false);
        }

        /// <summary>
        /// 显示列设置对话框
        /// </summary>
        private void ShowColumnSettingsDialog(string title, DataGrid dataGrid, bool isFileGrid)
        {
            var dialog = new Window
            {
                Title = title,
                Width = 320,
                Height = 450,
                WindowStartupLocation = WindowStartupLocation.CenterOwner,
                Owner = this,
                ResizeMode = ResizeMode.NoResize,
                Background = new SolidColorBrush(Color.FromRgb(250, 250, 250))
            };

            var mainGrid = new Grid { Margin = new Thickness(15) };
            mainGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
            mainGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

            // 标题和列表区域
            var contentBorder = new Border
            {
                Background = Brushes.White,
                CornerRadius = new CornerRadius(8),
                Padding = new Thickness(15),
                Margin = new Thickness(0, 0, 0, 10)
            };
            contentBorder.Effect = new System.Windows.Media.Effects.DropShadowEffect
            {
                BlurRadius = 10,
                ShadowDepth = 2,
                Color = Colors.LightGray,
                Opacity = 0.3
            };

            var contentPanel = new StackPanel();
            
            var titleBlock = new TextBlock
            {
                Text = title,
                FontSize = 16,
                FontWeight = FontWeights.Bold,
                Foreground = new SolidColorBrush(Color.FromRgb(51, 51, 51)),
                Margin = new Thickness(0, 0, 0, 15)
            };
            contentPanel.Children.Add(titleBlock);

            var scrollViewer = new ScrollViewer
            {
                VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
                MaxHeight = 300
            };

            var checkBoxPanel = new StackPanel();
            var columnSettings = new List<(CheckBox checkBox, string columnName)>();

            foreach (var column in dataGrid.Columns.Skip(2)) // 跳过选择框和文件名/文件夹名列
            {
                var checkBox = new CheckBox
                {
                    Content = column.Header,
                    IsChecked = column.Visibility == Visibility.Visible,
                    Margin = new Thickness(0, 5, 0, 5),
                    FontSize = 13,
                    Tag = column
                };

                checkBox.Checked += (s, args) =>
                {
                    var col = (DataGridColumn)((CheckBox)s).Tag;
                    col.Visibility = Visibility.Visible;
                };

                checkBox.Unchecked += (s, args) =>
                {
                    var col = (DataGridColumn)((CheckBox)s).Tag;
                    col.Visibility = Visibility.Collapsed;
                };

                checkBoxPanel.Children.Add(checkBox);
                columnSettings.Add((checkBox, column.Header.ToString()));
            }

            scrollViewer.Content = checkBoxPanel;
            contentPanel.Children.Add(scrollViewer);
            contentBorder.Child = contentPanel;

            // 按钮区域
            var buttonPanel = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                HorizontalAlignment = HorizontalAlignment.Right,
                Margin = new Thickness(0, 10, 0, 0)
            };

            var saveButton = new Button
            {
                Content = "保存",
                Width = 100,
                Margin = new Thickness(0, 0, 10, 0)
            };
            // 应用ModernButton样式
            saveButton.SetResourceReference(Control.StyleProperty, "ModernButton");
            saveButton.Click += (s, args) =>
            {
                SaveColumnSettings(columnSettings, isFileGrid);
                dialog.Close();
                MessageBox.Show("列设置已保存", "提示", MessageBoxButton.OK, MessageBoxImage.Information);
            };

            var closeButton = new Button
            {
                Content = "关闭",
                Width = 100,
                IsCancel = true
            };
            // 应用CancelButton样式
            closeButton.SetResourceReference(Control.StyleProperty, "CancelButton");
            closeButton.Click += (s, args) => dialog.Close();

            buttonPanel.Children.Add(saveButton);
            buttonPanel.Children.Add(closeButton);

            Grid.SetRow(contentBorder, 0);
            Grid.SetRow(buttonPanel, 1);
            mainGrid.Children.Add(contentBorder);
            mainGrid.Children.Add(buttonPanel);

            dialog.Content = mainGrid;
            dialog.ShowDialog();
        }

        /// <summary>
        /// 保存列设置
        /// </summary>
        private void SaveColumnSettings(List<(CheckBox checkBox, string columnName)> settings, bool isFileGrid)
        {
            var configs = new List<BatchColumnConfig>();

            foreach (var (checkBox, columnName) in settings)
            {
                configs.Add(new BatchColumnConfig
                {
                    ColumnName = GetColumnConfigName(columnName),
                    IsVisible = checkBox.IsChecked == true
                });
            }

            if (isFileGrid)
            {
                BatchColumnConfigManager.SaveFileColumns(configs);
            }
            else
            {
                BatchColumnConfigManager.SaveFolderColumns(configs);
            }
        }

        /// <summary>
        /// 从显示名称获取列配置名称
        /// </summary>
        private string GetColumnConfigName(string displayName)
        {
            switch (displayName)
            {
                case "标题": return "Title";
                case "主题": return "Subject";
                case "分级": return "Rating";
                case "标记": return "Tags";
                case "类别": return "Category";
                case "备注": return "Comment";
                case "别名": return "Alias";
                case "作者": return "Author";
                default:
                    // 如果已经是英文名称，直接返回
                    if (displayName == "InfoTip") return "InfoTip";
                    return displayName;
            }
        }

        #endregion

        #region 导入导出配置

        private void BtnExportConfig_Click(object sender, RoutedEventArgs e)
        {
            // 创建保存文件对话框
            var dialog = new Microsoft.Win32.SaveFileDialog
            {
                Title = "导出配置",
                Filter = "TOML配置文件 (*.toml)|*.toml|所有文件 (*.*)|*.*",
                DefaultExt = "toml",
                FileName = $"批量属性配置_{DateTime.Now:yyyyMMdd_HHmmss}.toml"
            };

            if (dialog.ShowDialog() == true)
            {
                try
                {
                    // 导出所有文件和文件夹的配置
                    var filesToExport = _fileModels.ToList();
                    var foldersToExport = _folderModels.ToList();

                    BatchConfigManager.ExportMixedConfigs(filesToExport, foldersToExport, dialog.FileName);

                    MessageBox.Show($"配置已成功导出到:\n{dialog.FileName}\n\n" +
                                  $"文件: {filesToExport.Count} 个\n" +
                                  $"文件夹: {foldersToExport.Count} 个",
                                  "导出成功", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"导出配置时出错:\n{ex.Message}", "错误",
                                  MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void BtnImportConfig_Click(object sender, RoutedEventArgs e)
        {
            // 创建打开文件对话框
            var dialog = new Microsoft.Win32.OpenFileDialog
            {
                Title = "导入配置",
                Filter = "TOML配置文件 (*.toml)|*.toml|所有文件 (*.*)|*.*",
                DefaultExt = "toml"
            };

            if (dialog.ShowDialog() == true)
            {
                try
                {
                    // 导入配置
                    var (fileConfigs, folderConfigs) = BatchConfigManager.ImportConfigs(dialog.FileName);

                    int fileMatched = 0;
                    int folderMatched = 0;
                    int fileUpdated = 0;
                    int folderUpdated = 0;

                    // 处理文件配置 - 只导入符合文件名的配置
                    foreach (var config in fileConfigs)
                    {
                        if (string.IsNullOrEmpty(config.FileName))
                            continue;

                        var matchedModel = _fileModels.FirstOrDefault(m =>
                            m.FileName.Equals(config.FileName, StringComparison.OrdinalIgnoreCase));

                        if (matchedModel != null)
                        {
                            fileMatched++;
                            bool hasChanges = false;

                            // 更新字段，如果配置中有值则替换
                            if (!string.IsNullOrEmpty(config.Title) && matchedModel.Title != config.Title)
                            {
                                matchedModel.Title = config.Title;
                                hasChanges = true;
                            }

                            if (!string.IsNullOrEmpty(config.Subject) && matchedModel.Subject != config.Subject)
                            {
                                matchedModel.Subject = config.Subject;
                                hasChanges = true;
                            }

                            if (!string.IsNullOrEmpty(config.Rating) && matchedModel.Rating != config.Rating)
                            {
                                matchedModel.Rating = config.Rating;
                                hasChanges = true;
                            }

                            if (!string.IsNullOrEmpty(config.Tags) && matchedModel.Tags != config.Tags)
                            {
                                matchedModel.Tags = config.Tags;
                                hasChanges = true;
                            }

                            if (!string.IsNullOrEmpty(config.Category) && matchedModel.Category != config.Category)
                            {
                                matchedModel.Category = config.Category;
                                hasChanges = true;
                            }

                            if (!string.IsNullOrEmpty(config.Comment) && matchedModel.Comment != config.Comment)
                            {
                                matchedModel.Comment = config.Comment;
                                hasChanges = true;
                            }

                            if (hasChanges)
                                fileUpdated++;
                        }
                    }

                    // 处理文件夹配置 - 只导入符合文件夹名的配置
                    foreach (var config in folderConfigs)
                    {
                        if (string.IsNullOrEmpty(config.FolderName))
                            continue;

                        var matchedModel = _folderModels.FirstOrDefault(m =>
                            m.FolderName.Equals(config.FolderName, StringComparison.OrdinalIgnoreCase));

                        if (matchedModel != null)
                        {
                            folderMatched++;
                            bool hasChanges = false;

                            // 更新字段，如果配置中有值则替换
                            if (!string.IsNullOrEmpty(config.Alias) && matchedModel.Alias != config.Alias)
                            {
                                matchedModel.Alias = config.Alias;
                                hasChanges = true;
                            }

                            if (!string.IsNullOrEmpty(config.InfoTip) && matchedModel.InfoTip != config.InfoTip)
                            {
                                matchedModel.InfoTip = config.InfoTip;
                                hasChanges = true;
                            }

                            if (!string.IsNullOrEmpty(config.Title) && matchedModel.Title != config.Title)
                            {
                                matchedModel.Title = config.Title;
                                hasChanges = true;
                            }

                            if (!string.IsNullOrEmpty(config.Subject) && matchedModel.Subject != config.Subject)
                            {
                                matchedModel.Subject = config.Subject;
                                hasChanges = true;
                            }

                            if (!string.IsNullOrEmpty(config.Author) && matchedModel.Author != config.Author)
                            {
                                matchedModel.Author = config.Author;
                                hasChanges = true;
                            }

                            if (!string.IsNullOrEmpty(config.Tags) && matchedModel.Tags != config.Tags)
                            {
                                matchedModel.Tags = config.Tags;
                                hasChanges = true;
                            }

                            if (hasChanges)
                                folderUpdated++;
                        }
                    }

                    // 刷新DataGrid
                    dgFiles.Items.Refresh();
                    dgFolders.Items.Refresh();

                    // 显示导入结果
                    string message = $"配置导入完成!\n\n" +
                                   $"配置文件中:\n" +
                                   $"  文件配置: {fileConfigs.Count} 个\n" +
                                   $"  文件夹配置: {folderConfigs.Count} 个\n\n" +
                                   $"匹配到:\n" +
                                   $"  文件: {fileMatched} 个（已更新 {fileUpdated} 个）\n" +
                                   $"  文件夹: {folderMatched} 个（已更新 {folderUpdated} 个）\n\n" +
                                   $"未匹配项: {(fileConfigs.Count - fileMatched) + (folderConfigs.Count - folderMatched)} 个";

                    MessageBox.Show(message, "导入完成",
                                  MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"导入配置时出错:\n{ex.Message}", "错误",
                                  MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        #endregion

        #region 保存操作

        private async void BtnSaveAll_Click(object sender, RoutedEventArgs e)
        {
            var modifiedFiles = _fileModels.Where(m => m.IsModified).ToList();
            var modifiedFolders = _folderModels.Where(m => m.IsModified).ToList();

            if (!modifiedFiles.Any() && !modifiedFolders.Any())
            {
                MessageBox.Show("没有需要保存的修改。", "提示", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            // 检查是否有错误
            if (modifiedFiles.Any(m => m.HasError) || modifiedFolders.Any(m => m.HasError))
            {
                var result = MessageBox.Show("有些项目存在验证错误，是否继续保存其他项目？",
                    "警告", MessageBoxButton.YesNo, MessageBoxImage.Warning);
                if (result == MessageBoxResult.No)
                    return;
            }

            await SaveAllItemsAsync(modifiedFiles, modifiedFolders);
        }

        private async void BtnSaveSelected_Click(object sender, RoutedEventArgs e)
        {
            var selectedFiles = _fileModels.Where(m => m.IsSelected).ToList();
            var selectedFolders = _folderModels.Where(m => m.IsSelected).ToList();

            if (!selectedFiles.Any() && !selectedFolders.Any())
            {
                MessageBox.Show("请先选择要保存的项目。", "提示", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            // 检查是否有错误
            if (selectedFiles.Any(m => m.HasError) || selectedFolders.Any(m => m.HasError))
            {
                var result = MessageBox.Show("有些项目存在验证错误，是否继续保存其他项目？",
                    "警告", MessageBoxButton.YesNo, MessageBoxImage.Warning);
                if (result == MessageBoxResult.No)
                    return;
            }

            await SaveAllItemsAsync(selectedFiles, selectedFolders);
        }

        private async Task SaveAllItemsAsync(List<BatchFilePropertyModel> files, List<BatchFolderPropertyModel> folders)
        {
            // 过滤掉有错误的项目
            var validFiles = files.Where(m => !m.HasError).ToList();
            var validFolders = folders.Where(m => !m.HasError).ToList();

            if (!validFiles.Any() && !validFolders.Any())
            {
                MessageBox.Show("没有有效的项目可以保存。", "提示", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            // 显示进度
            progressPanel.Visibility = Visibility.Visible;
            btnSaveAll.IsEnabled = false;
            btnSaveSelected.IsEnabled = false;
            btnCancel.IsEnabled = false;

            int total = validFiles.Count + validFolders.Count;
            int success = 0;
            int failed = 0;
            List<string> errors = new List<string>();

            // 保存文件
            for (int i = 0; i < validFiles.Count; i++)
            {
                var model = validFiles[i];
                txtProgress.Text = $"正在保存文件 {i + 1}/{validFiles.Count}: {model.FileName}";
                progressBar.Value = (success + failed + 1) * 100.0 / total;

                try
                {
                    await Task.Run(() => SaveSingleFile(model));
                    success++;
                    model.IsModified = false;
                }
                catch (Exception ex)
                {
                    failed++;
                    errors.Add($"[文件] {model.FileName}: {ex.Message}");
                }

                await Task.Delay(10);
            }

            // 保存文件夹
            for (int i = 0; i < validFolders.Count; i++)
            {
                var model = validFolders[i];
                txtProgress.Text = $"正在保存文件夹 {i + 1}/{validFolders.Count}: {model.FolderName}";
                progressBar.Value = (success + failed + 1) * 100.0 / total;

                try
                {
                    await Task.Run(() => SaveSingleFolder(model));
                    success++;
                    model.IsModified = false;
                }
                catch (Exception ex)
                {
                    failed++;
                    errors.Add($"[文件夹] {model.FolderName}: {ex.Message}");
                }

                await Task.Delay(10);
            }

            // 隐藏进度
            progressPanel.Visibility = Visibility.Collapsed;
            btnSaveAll.IsEnabled = true;
            btnSaveSelected.IsEnabled = true;
            btnCancel.IsEnabled = true;

            // 显示结果
            string message = $"保存完成!\n成功: {success} 个\n失败: {failed} 个";
            if (errors.Any())
            {
                message += "\n\n错误详情:\n" + string.Join("\n", errors.Take(5));
                if (errors.Count > 5)
                    message += $"\n... 还有 {errors.Count - 5} 个错误";
            }

            MessageBox.Show(message, "保存结果",
                MessageBoxButton.OK,
                failed > 0 ? MessageBoxImage.Warning : MessageBoxImage.Information);

            if (failed == 0)
            {
                this.DialogResult = true;
                this.Close();
            }
        }

        private void SaveSingleFile(BatchFilePropertyModel model)
        {
            var file = ShellFile.FromFilePath(model.FilePath);

            // 清理函数
            string CleanText(string text) => (text ?? "").TrimEnd('\r', '\n', ' ', '\t');

            // 保存各个属性
            file.Properties.System.Title.Value = CleanText(model.Title);
            file.Properties.System.Subject.Value = CleanText(model.Subject);

            // 分级
            string cleanRating = CleanText(model.Rating);
            if (!string.IsNullOrWhiteSpace(cleanRating))
            {
                if (int.TryParse(cleanRating, out int rating) && rating >= 1 && rating <= 99)
                {
                    file.Properties.System.Rating.Value = (uint)rating;
                }
                else
                {
                    throw new Exception("分级必须是 1-99 之间的数字");
                }
            }
            else
            {
                file.Properties.System.Rating.Value = null;
            }

            // 标记
            string[] tags = CleanText(model.Tags).Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(tag => tag.Trim()).Where(tag => !string.IsNullOrEmpty(tag)).ToArray();
            file.Properties.System.Keywords.Value = tags;

            // 类别
            string[] categories = CleanText(model.Category).Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(cat => cat.Trim()).Where(cat => !string.IsNullOrEmpty(cat)).ToArray();
            file.Properties.System.Category.Value = categories;

            // 备注
            file.Properties.System.Comment.Value = CleanText(model.Comment);
        }

        private void SaveSingleFolder(BatchFolderPropertyModel model)
        {
            // 清理函数
            string CleanText(string text) => (text ?? "").TrimEnd('\r', '\n', ' ', '\t');

            var folderInfo = new FolderInfo
            {
                LocalizedResourceName = CleanText(model.Alias),
                InfoTip = CleanText(model.InfoTip),
                Prop2 = CleanText(model.Title),
                Prop3 = CleanText(model.Subject),
                Prop4 = CleanText(model.Author),
                Prop5 = CleanText(model.Tags)
            };

            // 使用 FolderRemarkManager 保存文件夹属性
            string result = FolderRemarkManager.WriteFolderRemark(model.FolderPath, folderInfo);

            if (!result.Contains("成功"))
            {
                throw new Exception(result);
            }
        }

        #endregion
    }
}
