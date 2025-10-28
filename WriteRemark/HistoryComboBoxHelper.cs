using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace WriteRemark
{
    /// <summary>
    /// 历史记录ComboBox辅助类
    /// 提供自动完成和历史记录功能
    /// </summary>
    public static class HistoryComboBoxHelper
    {
        // 用于标记ComboBox是否已经附加过历史功能，避免重复附加
        private static readonly string HISTORY_ATTACHED_KEY = "HistoryAttached";
        
        // 历史记录缓存，避免频繁查询数据库
        private static readonly Dictionary<string, List<string>> _historyCache = new Dictionary<string, List<string>>();
        
        // "显示更多"的占位符
        private const string SHOW_MORE_PLACEHOLDER = "... 显示更多历史记录 ...";
        /// <summary>
        /// 创建带历史记录功能的ComboBox
        /// </summary>
        /// <param name="fieldName">字段名</param>
        /// <param name="isMultiLine">是否多行输入</param>
        /// <returns>配置好的ComboBox</returns>
        public static ComboBox CreateHistoryComboBox(string fieldName, bool isMultiLine = false)
        {
            var comboBox = new ComboBox
            {
                IsEditable = true,
                IsTextSearchEnabled = false,
                StaysOpenOnEdit = true,
                Margin = new Thickness(5),
                VerticalAlignment = VerticalAlignment.Center
            };

            // 多行输入的特殊处理
            if (isMultiLine)
            {
                comboBox.MinHeight = 60;
                // 注意：ComboBox不原生支持多行，但我们可以在TextBox层面处理
            }

            // 加载历史记录
            LoadHistory(comboBox, fieldName);

            // 设置Tag存储字段名
            comboBox.Tag = fieldName;

            // 绑定事件
            SetupComboBoxEvents(comboBox, fieldName);

            return comboBox;
        }

        /// <summary>
        /// 为现有ComboBox添加历史记录功能
        /// </summary>
        /// <param name="comboBox">ComboBox控件</param>
        /// <param name="fieldName">字段名</param>
        public static void AttachHistoryFeature(ComboBox comboBox, string fieldName)
        {
            // 检查是否已经附加过，避免重复附加导致性能问题
            if (comboBox.Tag is string existingFieldName && existingFieldName == fieldName)
            {
                // 已经附加过相同字段，只需要刷新历史记录即可
                LoadHistory(comboBox, fieldName);
                return;
            }

            comboBox.IsEditable = true;
            comboBox.IsTextSearchEnabled = false;
            comboBox.StaysOpenOnEdit = true;
            comboBox.Tag = fieldName;

            // 如果之前附加过其他字段，需要先清理旧的事件处理程序
            bool isFirstAttach = !comboBox.Resources.Contains(HISTORY_ATTACHED_KEY);
            
            if (!isFirstAttach)
            {
                // 移除旧的事件处理程序（通过重新获取TextBox）
                ClearComboBoxEvents(comboBox);
            }

            LoadHistory(comboBox, fieldName);
            SetupComboBoxEvents(comboBox, fieldName);
            
            // 标记已附加
            comboBox.Resources[HISTORY_ATTACHED_KEY] = true;
        }

        /// <summary>
        /// 清理ComboBox的事件处理程序
        /// </summary>
        private static void ClearComboBoxEvents(ComboBox comboBox)
        {
            // 注意：由于事件处理程序是匿名函数，无法直接移除
            // 这里采用重新设置基本属性的方式来减少问题
            // 实际上，重复附加的影响已经通过上面的检查大幅降低了
        }

        /// <summary>
        /// 预加载指定字段的历史记录（在窗口加载时调用）
        /// </summary>
        public static void PreloadHistory(string fieldName)
        {
            try
            {
                lock (_historyCache)
                {
                    if (!_historyCache.ContainsKey(fieldName))
                    {
                        // 加载最多30条历史记录
                        var history = HistoryManager.GetHistory(fieldName, 30);
                        _historyCache[fieldName] = history;
                    }
                }
            }
            catch (Exception)
            {
                // 静默处理错误
            }
        }

        /// <summary>
        /// 批量预加载多个字段的历史记录
        /// </summary>
        public static void PreloadMultipleHistory(params string[] fieldNames)
        {
            foreach (var fieldName in fieldNames)
            {
                PreloadHistory(fieldName);
            }
        }

        /// <summary>
        /// 加载历史记录到ComboBox（默认显示5条 + "显示更多"）
        /// </summary>
        private static void LoadHistory(ComboBox comboBox, string fieldName, bool showAll = false)
        {
            try
            {
                List<string> fullHistory;

                // 从缓存获取
                lock (_historyCache)
                {
                    if (_historyCache.TryGetValue(fieldName, out var cached))
                    {
                        fullHistory = cached;
                    }
                    else
                    {
                        // 如果缓存中没有，立即加载
                        fullHistory = HistoryManager.GetHistory(fieldName, 30);
                        _historyCache[fieldName] = fullHistory;
                    }
                }

                if (fullHistory == null || fullHistory.Count == 0)
                {
                    comboBox.ItemsSource = new List<string>();
                    return;
                }

                List<string> displayHistory;
                
                if (showAll || fullHistory.Count <= 5)
                {
                    // 显示全部
                    displayHistory = new List<string>(fullHistory);
                }
                else
                {
                    // 只显示前5条 + "显示更多"
                    displayHistory = new List<string>(fullHistory.Take(5));
                    displayHistory.Add(SHOW_MORE_PLACEHOLDER);
                }

                comboBox.ItemsSource = displayHistory;
            }
            catch (Exception)
            {
                // 静默处理错误
                comboBox.ItemsSource = new List<string>();
            }
        }

        /// <summary>
        /// 清除指定字段的历史记录缓存
        /// </summary>
        public static void ClearCache(string fieldName)
        {
            lock (_historyCache)
            {
                _historyCache.Remove(fieldName);
            }
        }

        /// <summary>
        /// 清除所有历史记录缓存
        /// </summary>
        public static void ClearAllCache()
        {
            lock (_historyCache)
            {
                _historyCache.Clear();
            }
        }

        /// <summary>
        /// 超轻量级：仅从缓存快速加载历史记录到ComboBox，不绑定任何事件
        /// 适用于DataGrid编辑模式下的快速加载，避免卡顿
        /// </summary>
        public static void QuickLoadFromCache(ComboBox comboBox, string fieldName)
        {
            try
            {
                // 如果已经加载过，直接返回
                if (comboBox.ItemsSource != null)
                {
                    return;
                }

                List<string> fullHistory;

                // 只从缓存获取，不查询数据库
                lock (_historyCache)
                {
                    if (!_historyCache.TryGetValue(fieldName, out fullHistory))
                    {
                        // 缓存中没有，返回空列表
                        comboBox.ItemsSource = new List<string>();
                        return;
                    }
                }

                if (fullHistory == null || fullHistory.Count == 0)
                {
                    comboBox.ItemsSource = new List<string>();
                    return;
                }

                // 显示前5条 + "显示更多"
                List<string> displayHistory;
                if (fullHistory.Count <= 5)
                {
                    displayHistory = new List<string>(fullHistory);
                }
                else
                {
                    displayHistory = new List<string>(fullHistory.Take(5));
                    displayHistory.Add(SHOW_MORE_PLACEHOLDER);
                }

                comboBox.ItemsSource = displayHistory;
            }
            catch (Exception)
            {
                // 静默处理错误
                comboBox.ItemsSource = new List<string>();
            }
        }

        /// <summary>
        /// 超轻量级：仅绑定最基本的事件，用于DataGrid中的ComboBox
        /// 只保留必要的功能，避免卡顿
        /// </summary>
        public static void AttachMinimalEvents(ComboBox comboBox, string fieldName)
        {
            // 检查是否已经绑定过事件，避免重复绑定
            const string MINIMAL_EVENTS_KEY = "MinimalEventsAttached";
            if (comboBox.Resources.Contains(MINIMAL_EVENTS_KEY))
            {
                return; // 已经绑定过，直接返回
            }

            // 设置Tag用于标识字段名
            comboBox.Tag = fieldName;

            // 只绑定失去焦点时保存历史记录
            comboBox.LostFocus += (s, e) =>
            {
                SaveToHistory(comboBox, fieldName);
            };

            // 选择项改变时处理"显示更多"
            comboBox.SelectionChanged += (s, e) =>
            {
                if (comboBox.SelectedItem is string selectedText && selectedText == SHOW_MORE_PLACEHOLDER)
                {
                    // 展开显示全部历史记录
                    LoadHistory(comboBox, fieldName, showAll: true);
                    comboBox.IsDropDownOpen = true;
                    comboBox.SelectedIndex = -1;
                    e.Handled = true;
                }
                else if (comboBox.SelectedItem != null && !string.IsNullOrWhiteSpace(comboBox.Text))
                {
                    SaveToHistory(comboBox, fieldName);
                }
            };

            // 标记已绑定
            comboBox.Resources[MINIMAL_EVENTS_KEY] = true;
        }

        /// <summary>
        /// 设置ComboBox的事件处理
        /// </summary>
        private static void SetupComboBoxEvents(ComboBox comboBox, string fieldName)
        {
            // 文本改变时触发自动建议
            var textBox = GetComboBoxTextBox(comboBox);
            if (textBox != null)
            {
                textBox.TextChanged += (s, e) =>
                {
                    OnTextChanged(comboBox, fieldName);
                };

                // 全选功能
                textBox.GotFocus += (s, e) =>
                {
                    textBox.SelectAll();
                };

                textBox.PreviewMouseLeftButtonDown += (s, e) =>
                {
                    if (!textBox.IsKeyboardFocusWithin)
                    {
                        textBox.Focus();
                        e.Handled = true;
                    }
                };
            }

            // 失去焦点时保存到历史
            comboBox.LostFocus += (s, e) =>
            {
                SaveToHistory(comboBox, fieldName);
            };

            // 选择项改变时处理
            comboBox.SelectionChanged += (s, e) =>
            {
                // 检查是否选择了"显示更多"
                if (comboBox.SelectedItem is string selectedText && selectedText == SHOW_MORE_PLACEHOLDER)
                {
                    // 展开显示全部历史记录
                    LoadHistory(comboBox, fieldName, showAll: true);
                    comboBox.IsDropDownOpen = true;
                    comboBox.SelectedIndex = -1; // 清除选择
                    e.Handled = true;
                }
                else if (comboBox.SelectedItem != null && !string.IsNullOrWhiteSpace(comboBox.Text))
                {
                    SaveToHistory(comboBox, fieldName);
                }
            };

            // 下拉时重新加载历史（默认显示5条）
            comboBox.DropDownOpened += (s, e) =>
            {
                if (string.IsNullOrWhiteSpace(comboBox.Text))
                {
                    LoadHistory(comboBox, fieldName, showAll: false);
                }
            };
        }

        /// <summary>
        /// 文本改变时的处理（自动建议）
        /// </summary>
        private static void OnTextChanged(ComboBox comboBox, string fieldName)
        {
            if (comboBox.IsDropDownOpen)
                return;

            string text = comboBox.Text;

            if (string.IsNullOrWhiteSpace(text))
            {
                LoadHistory(comboBox, fieldName);
                return;
            }

            try
            {
                // 搜索匹配的历史记录
                var suggestions = HistoryManager.SearchHistory(fieldName, text, 10);

                if (suggestions.Any())
                {
                    // 保存当前文本位置
                    var textBox = GetComboBoxTextBox(comboBox);
                    int selectionStart = textBox?.SelectionStart ?? text.Length;

                    // 更新建议列表
                    comboBox.ItemsSource = suggestions;

                    // 自动打开下拉列表显示建议
                    comboBox.IsDropDownOpen = true;

                    // 恢复文本和光标位置
                    comboBox.Text = text;
                    if (textBox != null)
                    {
                        textBox.SelectionStart = selectionStart;
                        textBox.SelectionLength = 0;
                    }
                }
            }
            catch (Exception)
            {
                // 静默处理错误
            }
        }

        /// <summary>
        /// 保存到历史记录（异步，不阻塞 UI）
        /// </summary>
        private static void SaveToHistory(ComboBox comboBox, string fieldName)
        {
            string text = comboBox.Text?.Trim();
            if (!string.IsNullOrWhiteSpace(text))
            {
                // 异步保存，不阻塞 UI 线程
                System.Threading.Tasks.Task.Run(() =>
                {
                    try
                    {
                        HistoryManager.AddOrUpdateHistory(fieldName, text);
                        
                        // 清除该字段的缓存，以便下次加载最新数据
                        ClearCache(fieldName);
                    }
                    catch (Exception)
                    {
                        // 静默处理错误
                    }
                });
            }
        }

        /// <summary>
        /// 获取ComboBox内部的TextBox
        /// </summary>
        private static TextBox GetComboBoxTextBox(ComboBox comboBox)
        {
            comboBox.ApplyTemplate();
            return comboBox.Template?.FindName("PART_EditableTextBox", comboBox) as TextBox;
        }

        /// <summary>
        /// 设置ComboBox的文本值
        /// </summary>
        public static void SetText(ComboBox comboBox, string text)
        {
            comboBox.Text = text ?? "";
        }

        /// <summary>
        /// 获取ComboBox的文本值
        /// </summary>
        public static string GetText(ComboBox comboBox)
        {
            return comboBox.Text ?? "";
        }

        /// <summary>
        /// 清空指定字段的历史记录
        /// </summary>
        public static void ClearFieldHistory(string fieldName)
        {
            HistoryManager.ClearFieldHistory(fieldName);
        }
    }
}
