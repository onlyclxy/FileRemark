using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace WriteRemark
{
    /// <summary>
    /// 批量文件夹属性编辑的数据模型
    /// </summary>
    public class BatchFolderPropertyModel : INotifyPropertyChanged
    {
        private string _folderPath;
        private string _folderName;
        private string _alias;          // LocalizedResourceName - 别名
        private string _infoTip;        // InfoTip - 备注
        private string _title;          // Prop2 - 标题
        private string _subject;        // Prop3 - 主题
        private string _author;         // Prop4 - 作者
        private string _tags;           // Prop5 - 标记
        private bool _isSelected;
        private bool _hasError;
        private string _errorMessage;
        private bool _isModified;

        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// 文件夹完整路径
        /// </summary>
        public string FolderPath
        {
            get => _folderPath;
            set => SetProperty(ref _folderPath, value);
        }

        /// <summary>
        /// 文件夹名称(仅显示用)
        /// </summary>
        public string FolderName
        {
            get => _folderName;
            set => SetProperty(ref _folderName, value);
        }

        /// <summary>
        /// 别名 (LocalizedResourceName)
        /// </summary>
        public string Alias
        {
            get => _alias;
            set
            {
                if (SetProperty(ref _alias, value))
                {
                    IsModified = true;
                }
            }
        }

        /// <summary>
        /// 备注信息 (InfoTip)
        /// </summary>
        public string InfoTip
        {
            get => _infoTip;
            set
            {
                if (SetProperty(ref _infoTip, value))
                {
                    IsModified = true;
                }
            }
        }

        /// <summary>
        /// 标题 (Prop2)
        /// </summary>
        public string Title
        {
            get => _title;
            set
            {
                if (SetProperty(ref _title, value))
                {
                    IsModified = true;
                }
            }
        }

        /// <summary>
        /// 主题 (Prop3)
        /// </summary>
        public string Subject
        {
            get => _subject;
            set
            {
                if (SetProperty(ref _subject, value))
                {
                    IsModified = true;
                }
            }
        }

        /// <summary>
        /// 作者 (Prop4)
        /// </summary>
        public string Author
        {
            get => _author;
            set
            {
                if (SetProperty(ref _author, value))
                {
                    IsModified = true;
                }
            }
        }

        /// <summary>
        /// 标记 (Prop5)
        /// </summary>
        public string Tags
        {
            get => _tags;
            set
            {
                if (SetProperty(ref _tags, value))
                {
                    IsModified = true;
                }
            }
        }

        /// <summary>
        /// 是否被选中(用于批量操作)
        /// </summary>
        public bool IsSelected
        {
            get => _isSelected;
            set => SetProperty(ref _isSelected, value);
        }

        /// <summary>
        /// 是否有错误
        /// </summary>
        public bool HasError
        {
            get => _hasError;
            set => SetProperty(ref _hasError, value);
        }

        /// <summary>
        /// 错误消息
        /// </summary>
        public string ErrorMessage
        {
            get => _errorMessage;
            set => SetProperty(ref _errorMessage, value);
        }

        /// <summary>
        /// 是否已修改
        /// </summary>
        public bool IsModified
        {
            get => _isModified;
            set => SetProperty(ref _isModified, value);
        }

        protected bool SetProperty<T>(ref T field, T value, [CallerMemberName] string propertyName = null)
        {
            if (Equals(field, value))
                return false;

            field = value;
            OnPropertyChanged(propertyName);
            return true;
        }

        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        /// <summary>
        /// 克隆当前模型
        /// </summary>
        public BatchFolderPropertyModel Clone()
        {
            return new BatchFolderPropertyModel
            {
                FolderPath = this.FolderPath,
                FolderName = this.FolderName,
                Alias = this.Alias,
                InfoTip = this.InfoTip,
                Title = this.Title,
                Subject = this.Subject,
                Author = this.Author,
                Tags = this.Tags,
                IsSelected = this.IsSelected,
                HasError = this.HasError,
                ErrorMessage = this.ErrorMessage,
                IsModified = this.IsModified
            };
        }

        /// <summary>
        /// 从另一个模型复制值
        /// </summary>
        public void CopyFrom(BatchFolderPropertyModel source)
        {
            this.Alias = source.Alias;
            this.InfoTip = source.InfoTip;
            this.Title = source.Title;
            this.Subject = source.Subject;
            this.Author = source.Author;
            this.Tags = source.Tags;
            this.IsModified = source.IsModified;
            this.HasError = source.HasError;
            this.ErrorMessage = source.ErrorMessage;
        }
    }
}
