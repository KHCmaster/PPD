using GalaSoft.MvvmLight;
using System.Collections.Generic;
using System.Linq;

namespace PPDEditorCommon.Dialog.ViewModel
{
    public class SettingWindowViewModel : ViewModelBase
    {
        private bool isOkEnabled;
        private List<SettingViewModelBase> settings;

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
                return settings.Select(s => new KeyValuePair<object, object>(s.Name, s.ValidatedValue)).ToArray();

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
        }

        public bool IsOkEnabled
        {
            get { return isOkEnabled; }
            set
            {
                if (isOkEnabled != value)
                {
                    isOkEnabled = value;
                    RaisePropertyChanged("IsOkEnabled");
                }
            }
        }

        public SettingWindowViewModel(string title)
        {
            Title = title;
            settings = new List<SettingViewModelBase>();
        }

        public void Initialize()
        {
            Settings = settings.ToArray();
            foreach (var setting in settings)
            {
                setting.PropertyChanged += setting_PropertyChanged;
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
            if (setting is BoolSetting)
            {
                settingViewModel = new BoolSettingViewModel((BoolSetting)setting);
            }
            else if (setting is DoubleSetting)
            {
                settingViewModel = new DoubleSettingViewModel((DoubleSetting)setting);
            }
            else if (setting is EnumerableSetting)
            {
                settingViewModel = new EnumerableSettingViewModel((EnumerableSetting)setting);
            }
            else if (setting is FloatSetting)
            {
                settingViewModel = new FloatSettingViewModel((FloatSetting)setting);
            }
            else if (setting is IntSetting)
            {
                settingViewModel = new IntSettingViewModel((IntSetting)setting);
            }
            else if (setting is StringSetting)
            {
                settingViewModel = new StringSettingViewModel((StringSetting)setting);
            }
            if (settingViewModel == null)
            {
                return;
            }

            settings.Add(settingViewModel);
        }

        void setting_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "IsValidated")
            {
                IsOkEnabled = settings.All(s => s.IsValidated);
            }
        }
    }
}
