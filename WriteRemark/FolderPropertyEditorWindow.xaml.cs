using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;

namespace WriteRemark
{
    /// <summary>
    /// FolderPropertyEditorWindow.xaml 的交互逻辑
    /// </summary>
    public partial class FolderPropertyEditorWindow : Window
    {
        //去掉左上角图标
        // —— Win32 常量与函数 ——
        private const int GWL_EXSTYLE = -20;
        private const int WS_EX_DLGMODALFRAME = 0x0001;

        private const int WM_SETICON = 0x0080;

        [DllImport("user32.dll")]
        private static extern int GetWindowLong(IntPtr hWnd, int nIndex);

        [DllImport("user32.dll")]
        private static extern int SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);

        [DllImport("user32.dll")]
        private static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter,
            int X, int Y, int cx, int cy, uint uFlags);

        private const uint SWP_NOSIZE = 0x0001;
        private const uint SWP_NOMOVE = 0x0002;
        private const uint SWP_NOZORDER = 0x0004;
        private const uint SWP_FRAMECHANGED = 0x0020;

        [DllImport("user32.dll")]
        static extern IntPtr SendMessage(IntPtr hWnd, int Msg, IntPtr wParam, IntPtr lParam);

        protected override void OnSourceInitialized(EventArgs e)
        {
            base.OnSourceInitialized(e);

            var hwnd = new WindowInteropHelper(this).Handle;

            // 1) 去掉扩展样式中的图标：设置 WS_EX_DLGMODALFRAME
            int exStyle = GetWindowLong(hwnd, GWL_EXSTYLE);
            SetWindowLong(hwnd, GWL_EXSTYLE, exStyle | WS_EX_DLGMODALFRAME);

            // 2) 通知窗口样式已变更
            SetWindowPos(hwnd, IntPtr.Zero, 0, 0, 0, 0,
                SWP_NOMOVE | SWP_NOSIZE | SWP_NOZORDER | SWP_FRAMECHANGED);

            // 3) 保险起见，把大/小图标都清空（防止某些主题仍显示默认图标）
            SendMessage(hwnd, WM_SETICON, IntPtr.Zero, IntPtr.Zero); // 小图标
            SendMessage(hwnd, WM_SETICON, new IntPtr(1), IntPtr.Zero); // 大图标
        }




        private string _folderPath;
        private List<FolderFieldConfig> _fieldConfigs;
        private Dictionary<string, TextBox> _textBoxes;
        private bool _isEditMode = false;
        private bool _isTopMost = true;

        public FolderPropertyEditorWindow(string folderPath)
        {
            InitializeComponent();
            _folderPath = folderPath;
            _textBoxes = new Dictionary<string, TextBox>();
            this.Topmost = _isTopMost;
            
            // 设置文件夹路径显示
            txtFolderPath.Text = folderPath;
            
            LoadFieldConfigs();
            CreateDynamicFields();
            LoadProperties();
        }

        private void LoadFieldConfigs()
        {
            _fieldConfigs = FolderFieldConfigManager.LoadFieldConfigs();
        }

        private void CreateDynamicFields()
        {
            FieldsPanel.Children.Clear();
            HiddenFieldsPanel.Children.Clear();
            _textBoxes.Clear();

            var visibleFields = _fieldConfigs.Where(f => f.IsVisible).OrderBy(f => f.Order).ToList();
            var hiddenFields = _fieldConfigs.Where(f => !f.IsVisible).OrderBy(f => f.Order).ToList();

            // 设置拖放事件（仅在编辑模式下）
            if (_isEditMode)
            {
                FieldsPanel.AllowDrop = true;
                FieldsPanel.DragOver += FieldsPanel_DragOver;
                FieldsPanel.Drop += FieldsPanel_Drop;
            }
            else
            {
                FieldsPanel.AllowDrop = false;
                FieldsPanel.DragOver -= FieldsPanel_DragOver;
                FieldsPanel.Drop -= FieldsPanel_Drop;
            }

            // 创建可见字段
            foreach (var field in visibleFields)
            {
                CreateFieldControl(field, FieldsPanel, true);
            }

            // 创建隐藏字段
            foreach (var field in hiddenFields)
            {
                CreateFieldControl(field, HiddenFieldsPanel, false);
            }

            // 更新隐藏字段区域的可见性 - 只在设置模式下显示
            HiddenFieldsExpander.Visibility = (_isEditMode && hiddenFields.Any()) ? Visibility.Visible : Visibility.Collapsed;
        }

        private void CreateFieldControl(FolderFieldConfig field, Panel parent, bool isVisible)
        {
            var grid = new Grid();
            grid.Margin = new Thickness(0, 2, 0, 2);
            grid.Tag = field.FieldName;

            // 设置列定义
            if (_isEditMode && isVisible)
            {
                grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(25) }); // 拖拽手柄
                grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(50) }); // 上下箭头
                grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto }); // 标签
                grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) }); // 输入框
                grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(25) }); // 关闭按钮
            }
            else
            {
                grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto }); // 标签
                grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) }); // 输入框
                if (!isVisible)
                {
                    grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(25) }); // 恢复按钮
                }
            }

            int colIndex = 0;

            // 拖拽手柄（仅在编辑模式下的可见字段显示）
            if (_isEditMode && isVisible)
            {
                var dragHandle = new TextBlock
                {
                    Text = "⋮⋮",
                    VerticalAlignment = VerticalAlignment.Center,
                    HorizontalAlignment = HorizontalAlignment.Center,
                    Cursor = Cursors.SizeAll,
                    Foreground = Brushes.Gray,
                    FontWeight = FontWeights.Bold,
                    FontSize = 12
                };
                Grid.SetColumn(dragHandle, colIndex++);
                grid.Children.Add(dragHandle);

                // 添加拖拽事件
                dragHandle.MouseLeftButtonDown += (s, e) => DragHandle_MouseLeftButtonDown(s, e, grid);

                // 上下箭头按钮
                var arrowPanel = new StackPanel { Orientation = Orientation.Horizontal };
                
                var upButton = new Button
                {
                    Content = "▲",
                    Width = 20,
                    Height = 20,
                    Margin = new Thickness(2, 0, 1, 0),
                    Background = new SolidColorBrush(Color.FromRgb(240, 240, 240)),
                    BorderBrush = new SolidColorBrush(Color.FromRgb(200, 200, 200)),
                    BorderThickness = new Thickness(1),
                    FontSize = 10,
                    ToolTip = "向上移动"
                };
                upButton.Click += (s, e) => MoveFieldUp(field.FieldName);

                var downButton = new Button
                {
                    Content = "▼",
                    Width = 20,
                    Height = 20,
                    Margin = new Thickness(1, 0, 2, 0),
                    Background = new SolidColorBrush(Color.FromRgb(240, 240, 240)),
                    BorderBrush = new SolidColorBrush(Color.FromRgb(200, 200, 200)),
                    BorderThickness = new Thickness(1),
                    FontSize = 10,
                    ToolTip = "向下移动"
                };
                downButton.Click += (s, e) => MoveFieldDown(field.FieldName);

                arrowPanel.Children.Add(upButton);
                arrowPanel.Children.Add(downButton);
                Grid.SetColumn(arrowPanel, colIndex++);
                grid.Children.Add(arrowPanel);
            }

            // 标签（根据字段类型设置不同颜色）
            var label = new Label
            {
                Content = field.DisplayName + ":",
                VerticalAlignment = VerticalAlignment.Center,
                Margin = new Thickness(8, 0, 8, 0),
                FontSize = 13,
                FontWeight = FontWeights.Medium,
                Foreground = GetFieldLabelColor(field.FieldName)
            };
            Grid.SetColumn(label, colIndex++);
            grid.Children.Add(label);

            // 输入框
            TextBox textBox;
            if (field.FieldName == "InfoTip")
            {
                textBox = new TextBox
                {
                    Margin = new Thickness(5),
                    TextWrapping = TextWrapping.Wrap,
                    AcceptsReturn = true,
                    VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
                    MinHeight = 60
                };
            }
            else
            {
                textBox = new TextBox
                {
                    Margin = new Thickness(5),
                    VerticalAlignment = VerticalAlignment.Center
                };
            }

            if (!string.IsNullOrEmpty(field.ToolTip))
            {
                textBox.ToolTip = field.ToolTip;
            }

            // 添加自动全选功能
            textBox.GotFocus += (s, e) => textBox.SelectAll();
            textBox.PreviewMouseLeftButtonDown += (s, e) =>
            {
                if (!textBox.IsKeyboardFocusWithin)
                {
                    textBox.Focus();
                    e.Handled = true;
                }
            };

            Grid.SetColumn(textBox, colIndex++);
            grid.Children.Add(textBox);
            _textBoxes[field.FieldName] = textBox;

            // 关闭/恢复按钮
            if (_isEditMode && isVisible)
            {
                var closeButton = new Button
                {
                    Content = "×",
                    Width = 22,
                    Height = 22,
                    Background = new SolidColorBrush(Color.FromRgb(255, 87, 87)),
                    Foreground = Brushes.White,
                    BorderThickness = new Thickness(0),
                    FontSize = 14,
                    FontWeight = FontWeights.Bold,
                    ToolTip = "隐藏此字段",
                    Cursor = Cursors.Hand
                };
                
                // 添加圆角和悬停效果
                var template = new ControlTemplate(typeof(Button));
                var border = new FrameworkElementFactory(typeof(Border));
                border.SetValue(Border.BackgroundProperty, new TemplateBindingExtension(Button.BackgroundProperty));
                border.SetValue(Border.CornerRadiusProperty, new CornerRadius(11));
                var contentPresenter = new FrameworkElementFactory(typeof(ContentPresenter));
                contentPresenter.SetValue(ContentPresenter.HorizontalAlignmentProperty, HorizontalAlignment.Center);
                contentPresenter.SetValue(ContentPresenter.VerticalAlignmentProperty, VerticalAlignment.Center);
                border.AppendChild(contentPresenter);
                template.VisualTree = border;
                
                var trigger = new Trigger { Property = Button.IsMouseOverProperty, Value = true };
                trigger.Setters.Add(new Setter(Button.BackgroundProperty, new SolidColorBrush(Color.FromRgb(255, 107, 107))));
                template.Triggers.Add(trigger);
                
                closeButton.Template = template;
                closeButton.Click += (s, e) => HideField(field.FieldName);
                Grid.SetColumn(closeButton, colIndex++);
                grid.Children.Add(closeButton);
            }
            else if (!isVisible)
            {
                var restoreButton = new Button
                {
                    Content = "+",
                    Width = 22,
                    Height = 22,
                    Background = new SolidColorBrush(Color.FromRgb(76, 175, 80)),
                    Foreground = Brushes.White,
                    BorderThickness = new Thickness(0),
                    FontSize = 16,
                    FontWeight = FontWeights.Bold,
                    ToolTip = "显示此字段",
                    Cursor = Cursors.Hand
                };
                
                // 添加圆角和悬停效果
                var template = new ControlTemplate(typeof(Button));
                var border = new FrameworkElementFactory(typeof(Border));
                border.SetValue(Border.BackgroundProperty, new TemplateBindingExtension(Button.BackgroundProperty));
                border.SetValue(Border.CornerRadiusProperty, new CornerRadius(11));
                var contentPresenter = new FrameworkElementFactory(typeof(ContentPresenter));
                contentPresenter.SetValue(ContentPresenter.HorizontalAlignmentProperty, HorizontalAlignment.Center);
                contentPresenter.SetValue(ContentPresenter.VerticalAlignmentProperty, VerticalAlignment.Center);
                border.AppendChild(contentPresenter);
                template.VisualTree = border;
                
                var trigger = new Trigger { Property = Button.IsMouseOverProperty, Value = true };
                trigger.Setters.Add(new Setter(Button.BackgroundProperty, new SolidColorBrush(Color.FromRgb(96, 195, 100))));
                template.Triggers.Add(trigger);
                
                restoreButton.Template = template;
                restoreButton.Click += (s, e) => ShowField(field.FieldName);
                Grid.SetColumn(restoreButton, colIndex++);
                grid.Children.Add(restoreButton);
            }

            parent.Children.Add(grid);
        }

        private void DragHandle_MouseLeftButtonDown(object sender, MouseButtonEventArgs e, Grid fieldGrid)
        {
            if (e.ClickCount == 1)
            {
                // 开始拖拽操作
                var fieldName = fieldGrid.Tag.ToString();
                var dragData = new DataObject("FieldName", fieldName);
                
                // 设置拖拽效果
                var result = DragDrop.DoDragDrop(fieldGrid, dragData, DragDropEffects.Move);
                
                if (result == DragDropEffects.Move)
                {
                    // 拖拽完成后重新创建界面
                    CreateDynamicFields();
                    LoadProperties();
                }
            }
        }

        private void HideField(string fieldName)
        {
            var field = _fieldConfigs.FirstOrDefault(f => f.FieldName == fieldName);
            if (field != null)
            {
                field.IsVisible = false;
                FolderFieldConfigManager.SaveFieldConfigs(_fieldConfigs);
                CreateDynamicFields();
                LoadProperties(); // 重新加载数据
            }
        }

        private void ShowField(string fieldName)
        {
            var field = _fieldConfigs.FirstOrDefault(f => f.FieldName == fieldName);
            if (field != null)
            {
                field.IsVisible = true;
                // 将字段添加到最后
                field.Order = _fieldConfigs.Where(f => f.IsVisible).Max(f => f.Order) + 1;
                FolderFieldConfigManager.SaveFieldConfigs(_fieldConfigs);
                CreateDynamicFields();
                LoadProperties(); // 重新加载数据
            }
        }

        private void LoadProperties()
        {
            try
            {
                var folderInfo = FolderRemarkManager.ReadFolderRemark(_folderPath);

                // 根据字段名加载对应的属性值
                foreach (var kvp in _textBoxes)
                {
                    string fieldName = kvp.Key;
                    TextBox textBox = kvp.Value;

                    switch (fieldName)
                    {
                        case "LocalizedResourceName":
                            textBox.Text = folderInfo.LocalizedResourceName ?? "";
                            break;
                        case "InfoTip":
                            textBox.Text = folderInfo.InfoTip ?? "";
                            break;
                        case "Prop2":
                            textBox.Text = folderInfo.Prop2 ?? "";
                            break;
                        case "Prop3":
                            textBox.Text = folderInfo.Prop3 ?? "";
                            break;
                        case "Prop4":
                            textBox.Text = folderInfo.Prop4 ?? "";
                            break;
                        case "Prop5":
                            textBox.Text = folderInfo.Prop5 ?? "";
                            break;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"加载文件夹属性时出错: {ex.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                this.Close();
            }
        }

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

        private void BtnSettings_Click(object sender, RoutedEventArgs e)
        {
            _isEditMode = !_isEditMode;
            btnSettings.ToolTip = _isEditMode ? "退出设置模式" : "字段设置";
            CreateDynamicFields();
            LoadProperties();
        }

        private void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var folderInfo = new FolderInfo();

                // 根据字段名保存对应的属性值
                foreach (var kvp in _textBoxes)
                {
                    string fieldName = kvp.Key;
                    TextBox textBox = kvp.Value;

                    switch (fieldName)
                    {
                        case "LocalizedResourceName":
                            folderInfo.LocalizedResourceName = textBox.Text;
                            break;
                        case "InfoTip":
                            folderInfo.InfoTip = textBox.Text;
                            break;
                        case "Prop2":
                            folderInfo.Prop2 = textBox.Text;
                            break;
                        case "Prop3":
                            folderInfo.Prop3 = textBox.Text;
                            break;
                        case "Prop4":
                            folderInfo.Prop4 = textBox.Text;
                            break;
                        case "Prop5":
                            folderInfo.Prop5 = textBox.Text;
                            break;
                    }
                }

                // 使用提供的写入方法
                string result = FolderRemarkManager.WriteFolderRemark(_folderPath, folderInfo);

                if (result.Contains("成功"))
                {
                    this.DialogResult = true;
                }
                else
                {
                    MessageBox.Show(result, "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                    this.DialogResult = false;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"保存文件夹属性时出错: {ex.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                this.DialogResult = false;
            }
            this.Close();
        }

        private void FieldsPanel_DragOver(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent("FieldName"))
            {
                e.Effects = DragDropEffects.Move;
            }
            else
            {
                e.Effects = DragDropEffects.None;
            }
            e.Handled = true;
        }

        private void FieldsPanel_Drop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent("FieldName"))
            {
                var draggedFieldName = e.Data.GetData("FieldName").ToString();
                var dropPosition = e.GetPosition(FieldsPanel);
                
                // 找到拖拽的字段
                var draggedField = _fieldConfigs.FirstOrDefault(f => f.FieldName == draggedFieldName);
                if (draggedField == null) return;

                // 计算放置位置
                int newOrder = CalculateDropOrder(dropPosition);
                
                // 重新排序字段
                ReorderFields(draggedField, newOrder);
                
                // 保存配置并刷新界面
                FolderFieldConfigManager.SaveFieldConfigs(_fieldConfigs);
                e.Effects = DragDropEffects.Move;
            }
            e.Handled = true;
        }

        private int CalculateDropOrder(Point dropPosition)
        {
            var visibleFields = _fieldConfigs.Where(f => f.IsVisible).OrderBy(f => f.Order).ToList();
            
            // 简化计算：基于Y坐标确定插入位置
            double fieldHeight = FieldsPanel.Children.Count > 0 ? 
                FieldsPanel.Children[0].RenderSize.Height + 4 : 30; // 4 是margin
            
            int targetIndex = Math.Max(0, Math.Min(visibleFields.Count, 
                (int)(dropPosition.Y / fieldHeight)));
            
            return targetIndex + 1;
        }

        private void ReorderFields(FolderFieldConfig draggedField, int newOrder)
        {
            var visibleFields = _fieldConfigs.Where(f => f.IsVisible && f != draggedField).OrderBy(f => f.Order).ToList();
            
            // 重新分配顺序
            int order = 1;
            for (int i = 0; i < visibleFields.Count + 1; i++)
            {
                if (i == newOrder - 1)
                {
                    draggedField.Order = order++;
                }
                
                if (i < visibleFields.Count)
                {
                    visibleFields[i].Order = order++;
                }
            }
        }

        private void MoveFieldUp(string fieldName)
        {
            var field = _fieldConfigs.FirstOrDefault(f => f.FieldName == fieldName && f.IsVisible);
            if (field == null) return;

            var visibleFields = _fieldConfigs.Where(f => f.IsVisible).OrderBy(f => f.Order).ToList();
            var currentIndex = visibleFields.IndexOf(field);
            
            if (currentIndex > 0)
            {
                // 交换顺序
                var prevField = visibleFields[currentIndex - 1];
                int tempOrder = field.Order;
                field.Order = prevField.Order;
                prevField.Order = tempOrder;

                FolderFieldConfigManager.SaveFieldConfigs(_fieldConfigs);
                CreateDynamicFields();
                LoadProperties();
            }
        }

        private void MoveFieldDown(string fieldName)
        {
            var field = _fieldConfigs.FirstOrDefault(f => f.FieldName == fieldName && f.IsVisible);
            if (field == null) return;

            var visibleFields = _fieldConfigs.Where(f => f.IsVisible).OrderBy(f => f.Order).ToList();
            var currentIndex = visibleFields.IndexOf(field);
            
            if (currentIndex < visibleFields.Count - 1)
            {
                // 交换顺序
                var nextField = visibleFields[currentIndex + 1];
                int tempOrder = field.Order;
                field.Order = nextField.Order;
                nextField.Order = tempOrder;

                FolderFieldConfigManager.SaveFieldConfigs(_fieldConfigs);
                CreateDynamicFields();
                LoadProperties();
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            // 1. 将窗口置于屏幕中央
            this.WindowStartupLocation = WindowStartupLocation.CenterScreen;

            // 2. 将焦点设置到第一个可见的 TextBox 上并全选
            this.Dispatcher.BeginInvoke(new Action(() =>
            {
                var firstVisibleTextBox = _textBoxes.Values.FirstOrDefault();
                if (firstVisibleTextBox != null)
                {
                    firstVisibleTextBox.Focus();
                    firstVisibleTextBox.SelectAll();
                }
            }), System.Windows.Threading.DispatcherPriority.Input);
        }

        /// <summary>
        /// 根据字段名获取标签颜色
        /// </summary>
        private Brush GetFieldLabelColor(string fieldName)
        {
            switch (fieldName)
            {
                case "LocalizedResourceName":
                    return new SolidColorBrush(Color.FromRgb(33, 150, 243)); // 蓝色 - 别名
                case "InfoTip":
                    return new SolidColorBrush(Color.FromRgb(69, 90, 100)); // 深灰 - 备注
                case "Prop2":
                    return new SolidColorBrush(Color.FromRgb(156, 39, 176)); // 紫色 - 标题
                case "Prop3":
                    return new SolidColorBrush(Color.FromRgb(255, 152, 0)); // 橙色 - 主题
                case "Prop4":
                    return new SolidColorBrush(Color.FromRgb(244, 67, 54)); // 红色 - 作者
                case "Prop5":
                    return new SolidColorBrush(Color.FromRgb(76, 175, 80)); // 绿色 - 标记
                default:
                    return new SolidColorBrush(Color.FromRgb(85, 85, 85)); // 默认灰色
            }
        }
    }
}
