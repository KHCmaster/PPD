using System.Collections.Generic;
using System.Linq;

namespace PPDEditorCommon.Dialog.ViewModel
{
    class EnumerableSettingViewModel : SettingViewModelBase
    {
        private object selectedItem;

        public IEnumerable<object> Items
        {
            get;
            private set;
        }

        public object SelectedItem
        {
            get { return selectedItem; }
            set
            {
                if (selectedItem != value)
                {
                    selectedItem = value;
                    RaisePropertyChanged("SelectedItem");
                }
            }
        }

        public override object ValidatedValue
        {
            get { return selectedItem; }
        }

        public EnumerableSettingViewModel(EnumerableSetting setting) :
            base(setting)
        {
            Items = setting.Enumerable;
            if (setting.Enumerable.Contains(setting.DefaultValue))
            {
                selectedItem = setting.DefaultValue;
            }
            else if (Items.Count() > 0)
            {
                selectedItem = Items.First();
            }
        }
    }
}
