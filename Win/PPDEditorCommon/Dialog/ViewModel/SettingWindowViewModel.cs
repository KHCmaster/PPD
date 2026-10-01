using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.Generic;
using System.Linq;

namespace PPDEditorCommon.Dialog.ViewModel
{
    public class SettingWindowViewModel(string title) : ObservableObject
    {
        private bool isOkEnabled;
        private readonly List<SettingViewModelBase> settings = [];

        public object this[string key]
        {
            get
            {
                return settings.First(s => s.Name == key).ValidatedValue;
            }
        }

        public KeyValuePair<object, object>[] SettingPairs
        {
            get
            {
                return [.. settings.Select(s => new KeyValuePair<object, object>(s.Name, s.ValidatedValue))];

            }
        }

        public object[] Settings
        {
            get;
            private set;
        }

        public string Title
        {
            get;
            private set;
        } = title;

        public bool IsOkEnabled
        {
            get { return isOkEnabled; }
            set
            {
                if (isOkEnabled != value)
                {
                    isOkEnabled = value;
                    OnPropertyChanged("IsOkEnabled");
                }
            }
        }

        public void Initialize()
        {
            Settings = [.. settings];
            foreach (var setting in settings)
            {
                setting.PropertyChanged += Setting_PropertyChanged;
            }
            IsOkEnabled = true;
        }

        public void AddSetting(SettingBase setting)
        {
            if (setting == null)
            {
                return;
            }

            SettingViewModelBase settingViewModel = null;
            if (setting is BoolSetting setting1)
            {
                settingViewModel = new BoolSettingViewModel(setting1);
            }
            else if (setting is DoubleSetting setting2)
            {
                settingViewModel = new DoubleSettingViewModel(setting2);
            }
            else if (setting is EnumerableSetting setting3)
            {
                settingViewModel = new EnumerableSettingViewModel(setting3);
            }
            else if (setting is FloatSetting setting4)
            {
                settingViewModel = new FloatSettingViewModel(setting4);
            }
            else if (setting is IntSetting setting5)
            {
                settingViewModel = new IntSettingViewModel(setting5);
            }
            else if (setting is StringSetting setting6)
            {
                settingViewModel = new StringSettingViewModel(setting6);
            }
            if (settingViewModel == null)
            {
                return;
            }

            settings.Add(settingViewModel);
        }

        void Setting_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "IsValidated")
            {
                IsOkEnabled = settings.All(s => s.IsValidated);
            }
        }
    }
}
