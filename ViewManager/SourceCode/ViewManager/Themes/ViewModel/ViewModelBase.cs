
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace HCTheme.ViewModel
{
    /// <summary>
    /// Class làm view model cho window
    /// </summary>
    public abstract class ViewModelBase : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

       
        private readonly HashSet<INotifyCollectionChanged> hooked = new HashSet<INotifyCollectionChanged>();

        /// <summary>
        /// Raise event thay đổi thuộc tính. Property có [RegistryPersist] thì tự ghi registry.
        /// </summary>
        public void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

           

        /// <summary>
        /// Gán giá trị và tự động raise event. Hỗ trợ gọi nối tiếp .With() cho các thuộc tính phụ thuộc.
        /// </summary>
        protected ViewModelBase SetProperty<T>(ref T property, T value, [CallerMemberName] string propertyName = null)
        {
            if (EqualityComparer<T>.Default.Equals(property, value)) return this;

            property = value;
            OnPropertyChanged(propertyName);
            return this;
        }

        /// <summary>
        /// Thông báo thay đổi cho các thuộc tính phụ thuộc (Fluent API)
        /// </summary>
        public ViewModelBase With(string propertyName)
        {
            OnPropertyChanged(propertyName);
            return this;
        }

        /// <summary>
        /// Raise event cho thuộc tính hiện tại và trả về đối tượng để nối tiếp
        /// </summary>
        protected ViewModelBase RaisePropertyValue([CallerMemberName] string propertyName = null)
        {
            OnPropertyChanged(propertyName);
            return this;
        }

        
       

    }
}