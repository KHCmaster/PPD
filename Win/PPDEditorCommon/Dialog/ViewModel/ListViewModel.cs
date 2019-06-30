using GalaSoft.MvvmLight;

namespace PPDEditorCommon.Dialog.ViewModel
{
    public class ListViewModel<T> : ObservableObject where T : class
    {
        private string text;
        private T value;
        private bool isSelected;

        public T Value
        {
            get { return value; }
            set
            {
                bool changed = false;
                if (this.value == null)
                {
                    if (value != null)
                    {
                        this.value = value;
                        changed = true;
                    }
                }
                else
                {
                    if (!this.value.Equals(value))
                    {
                        this.value = value;
                        changed = true;
                    }
                }
                if (changed)
                {
                    RaisePropertyChanged("Value");
                }
            }
        }

        public string Text
        {
            get { return text; }
            set
            {
                if (text != value)
                {
                    text = value;
                    RaisePropertyChanged("Text");
                }
            }
        }

        public bool IsSelected
        {
            get { return isSelected; }
            set
            {
                if (isSelected != value)
                {
                    isSelected = value;
                    RaisePropertyChanged("IsSelected");
                }
            }
        }
    }
}
