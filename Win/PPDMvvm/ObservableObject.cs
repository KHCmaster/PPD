using System.ComponentModel;

namespace PPDMvvm
{
    /// <summary>
    /// プロパティの変更を通知するオブジェクトの基底クラスです。
    /// </summary>
    public class ObservableObject : INotifyPropertyChanged
    {
        /// <summary>
        /// プロパティが変更されたときに発生します。
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// プロパティの変更を通知します。
        /// </summary>
        /// <param name="propertyName">プロパティ名。</param>
        protected virtual void RaisePropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
