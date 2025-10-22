using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace WriteRemark
{
    /// <summary>
    /// 批量文件属性编辑的数据模型
    /// </summary>
    public class BatchFilePropertyModel : INotifyPropertyChanged
    {
        private string _filePath;
        private string _fileName;
        private string _title;
        private string _subject;
        private string _rating;
        private string _tags;
        private string _category;
        private string _comment;
        private bool _isSelected;
        private bool _hasError;
        private string _errorMessage;
        private bool _isModified;

        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// 文件完整路径
        /// </summary>
        public string FilePath
        {
            get => _filePath;
            set => SetProperty(ref _filePath, value);
        }

        /// <summary>
        /// 文件名(仅显示用)
        /// </summary>
        public string FileName
        {
            get => _fileName;
            set => SetProperty(ref _fileName, value);
        }

        /// <summary>
        /// 标题
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
        /// 主题
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
        /// 分级
        /// </summary>
        public string Rating
        {
            get => _rating;
            set
            {
                if (SetProperty(ref _rating, value))
                {
                    IsModified = true;
                    ValidateRating();
                }
            }
        }

        /// <summary>
        /// 标记(用;分隔)
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
        /// 类别(用;分隔)
        /// </summary>
        public string Category
        {
            get => _category;
            set
            {
                if (SetProperty(ref _category, value))
                {
                    IsModified = true;
                }
            }
        }

        /// <summary>
        /// 备注
        /// </summary>
        public string Comment
        {
            get => _comment;
            set
            {
                if (SetProperty(ref _comment, value))
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

        /// <summary>
        /// 验证分级字段
        /// </summary>
        private void ValidateRating()
        {
            if (string.IsNullOrWhiteSpace(Rating))
            {
                HasError = false;
                ErrorMessage = null;
                return;
            }

            if (int.TryParse(Rating.Trim(), out int ratingValue))
            {
                if (ratingValue < 1 || ratingValue > 99)
                {
                    HasError = true;
                    ErrorMessage = "分级必须在 1-99 之间";
                }
                else
                {
                    HasError = false;
                    ErrorMessage = null;
                }
            }
            else
            {
                HasError = true;
                ErrorMessage = "分级必须是数字";
            }
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
        /// 克隆当前模型(用于撤销功能)
        /// </summary>
        public BatchFilePropertyModel Clone()
        {
            return new BatchFilePropertyModel
            {
                FilePath = this.FilePath,
                FileName = this.FileName,
                Title = this.Title,
                Subject = this.Subject,
                Rating = this.Rating,
                Tags = this.Tags,
                Category = this.Category,
                Comment = this.Comment,
                IsSelected = this.IsSelected,
                HasError = this.HasError,
                ErrorMessage = this.ErrorMessage,
                IsModified = this.IsModified
            };
        }

        /// <summary>
        /// 从另一个模型复制值
        /// </summary>
        public void CopyFrom(BatchFilePropertyModel source)
        {
            this.Title = source.Title;
            this.Subject = source.Subject;
            this.Rating = source.Rating;
            this.Tags = source.Tags;
            this.Category = source.Category;
            this.Comment = source.Comment;
            this.IsModified = source.IsModified;
            this.HasError = source.HasError;
            this.ErrorMessage = source.ErrorMessage;
        }
    }
}
