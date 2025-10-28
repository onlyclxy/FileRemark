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
            comboBox.IsEditable = true;
            comboBox.IsTextSearchEnabled = false;
            comboBox.StaysOpenOnEdit = true;
            comboBox.Tag = fieldName;

            LoadHistory(comboBox, fieldName);
            SetupComboBoxEvents(comboBox, fieldName);
        }

        /// <summary>
        /// 加载历史记录到ComboBox
        /// </summary>
        private static void LoadHistory(ComboBox comboBox, string fieldName)
        {
            try
            {
                var history = HistoryManager.GetHistory(fieldName, 20);
                comboBox.ItemsSource = history;
            }
            catch (Exception)
            {
                // 静默处理错误
            }
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

            // 选择项改变时也保存
            comboBox.SelectionChanged += (s, e) =>
            {
                if (comboBox.SelectedItem != null && !string.IsNullOrWhiteSpace(comboBox.Text))
                {
                    SaveToHistory(comboBox, fieldName);
                }
            };

            // 下拉时重新加载历史
            comboBox.DropDownOpened += (s, e) =>
            {
                if (string.IsNullOrWhiteSpace(comboBox.Text))
                {
                    LoadHistory(comboBox, fieldName);
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
                    int selectionStart = comboBox.SelectionStart;

                    // 更新建议列表
                    comboBox.ItemsSource = suggestions;

                    // 自动打开下拉列表显示建议
                    comboBox.IsDropDownOpen = true;

                    // 恢复文本和光标位置
                    comboBox.Text = text;
                    var textBox = GetComboBoxTextBox(comboBox);
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
        /// 保存到历史记录
        /// </summary>
        private static void SaveToHistory(ComboBox comboBox, string fieldName)
        {
            string text = comboBox.Text?.Trim();
            if (!string.IsNullOrWhiteSpace(text))
            {
                try
                {
                    HistoryManager.AddOrUpdateHistory(fieldName, text);
                }
                catch (Exception)
                {
                    // 静默处理错误
                }
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
