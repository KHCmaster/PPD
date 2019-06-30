using GalaSoft.MvvmLight;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace PPDEditorCommon.Dialog.ViewModel
{
    public class TreeNodeViewModel<T> : ObservableObject where T : class
    {
        private string text;
        private T value;
        private bool isExpanded;
        private bool isSelected;

        public ObservableCollection<TreeNodeViewModel<T>> Children
        {
            get;
            private set;
        }

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

        public bool IsExpanded
        {
            get { return isExpanded; }
            set
            {
                if (isExpanded != value)
                {
                    isExpanded = value;
                    RaisePropertyChanged("IsExpanded");
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

        public T[] Values
        {
            get
            {
                var ret = new List<T>();
                if (Value != null)
                {
                    ret.Add(Value);
                }
                foreach (var child in Children)
                {
                    ret.AddRange(child.Values);
                }
                return ret.ToArray();
            }
        }

        public TreeNodeViewModel()
        {
            Children = new ObservableCollection<TreeNodeViewModel<T>>();
        }
    }
}
