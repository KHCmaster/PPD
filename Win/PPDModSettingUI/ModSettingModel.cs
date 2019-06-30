using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using PPDConfiguration;
using PPDFramework.Mod;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Input;

namespace PPDModSettingUI
{
    public class ModSettingModel : ViewModelBase
    {
        private ModSetting modSetting;
        private LanguageReader language;

        private string name;
        private string description;
        private string defaultValue;
        private string otherValues;
        private string newValue;
        private object[] availableValues;
        private int selectedIndex;
        private bool isOkEnabled;
        private ICommand setDefaultCommand;

        public string Name
        {
            get { return name; }
            private set
            {
                if (name != value)
                {
                    name = value;
                    RaisePropertyChanged("Name");
                }
            }
        }

        public string Description
        {
            get { return description; }
            private set
            {
                if (description != value)
                {
                    description = value;
                    RaisePropertyChanged("Description");
                }
            }
        }

        public string DefaultValue
        {
            get { return defaultValue; }
            private set
            {
                if (defaultValue != value)
                {
                    defaultValue = value;
                    RaisePropertyChanged("DefaultValue");
                }
            }
        }

        public string OtherValues
        {
            get { return otherValues; }
            private set
            {
                if (otherValues != value)
                {
                    otherValues = value;
                    RaisePropertyChanged("OtherValues");
                }
            }
        }

        public string NewValue
        {
            get { return newValue; }
            set
            {
                if (newValue != value)
                {
                    newValue = value;
                    RaisePropertyChanged("NewValue");
                    Validate();
                }
            }
        }

        public bool IsOkEnabled
        {
            get { return isOkEnabled; }
            private set
            {
                if (isOkEnabled != value)
                {
                    isOkEnabled = value;
                    RaisePropertyChanged("IsOkEnabled");
                }
            }
        }

        public object[] AvailableValues
        {
            get { return availableValues; }
            private set
            {
                if (availableValues != value)
                {
                    availableValues = value;
                    RaisePropertyChanged("AvailableValues");
                }
            }
        }

        public int SelectedIndex
        {
            get { return selectedIndex; }
            set
            {
                if (selectedIndex != value)
                {
                    selectedIndex = value;
                    RaisePropertyChanged("SelectedIndex");
                    UpdateNewValueBySelectedIndex();
                }
            }
        }

        public ICommand SetDefaultCommand
        {
            get
            {
                return setDefaultCommand ?? (setDefaultCommand = new RelayCommand(
                    SetDefaultCommand_Execute));
            }
        }

        public string ModSettingText
        {
            get
            {
                return language["ModSetting"];
            }
        }

        public string DefaultValueText
        {
            get
            {
                return language["DefaultValue"];
            }
        }

        public string SetDefaultValueText
        {
            get
            {
                return language["SetDefaultValue"];
            }
        }

        public string OtherValuesText
        {
            get
            {
                return language["OtherValues"];
            }
        }

        public string NewValueText
        {
            get
            {
                return language["NewValue"];
            }
        }

        public ModSettingModel(ModSetting modSetting, string currentValue, LanguageReader language)
        {
            this.language = language;
            this.modSetting = modSetting;
            Name = modSetting.Name;
            Description = modSetting.Description;
            NewValue = currentValue;
            AvailableValues = modSetting.AvailableValues;
            if (AvailableValues != null)
            {
                UpdateSelectedIndexByNewValue();
            }

            if (modSetting is Int32ModSetting)
            {
                var int32ModSetting = (Int32ModSetting)modSetting;
                var otherValues = GetOtherValues(int32ModSetting);
                if (!String.IsNullOrEmpty(otherValues))
                {
                    OtherValues = otherValues;
                }
            }
            else if (modSetting is FloatModSetting)
            {
                var floatModSetting = (FloatModSetting)modSetting;
                var otherValues = GetOtherValues(floatModSetting);
                if (!String.IsNullOrEmpty(otherValues))
                {
                    OtherValues = otherValues;
                }
            }
            else if (modSetting is DoubleModSetting)
            {
                var doubleModSetting = (DoubleModSetting)modSetting;
                var otherValues = GetOtherValues(doubleModSetting);
                if (!String.IsNullOrEmpty(otherValues))
                {
                    OtherValues = otherValues;
                }
            }
            else if (modSetting is StringModSetting)
            {
                var stringModSetting = (StringModSetting)modSetting;
                if (!stringModSetting.IsMaxLengthClassMaxLength)
                {
                    OtherValues = String.Format("{0}:{1}}", language["MaxLength"], stringModSetting.MaxLength);
                }
            }
            DefaultValue = modSetting.GetStringValue(modSetting.Default);
        }

        private void UpdateNewValueBySelectedIndex()
        {
            if (SelectedIndex >= 0)
            {
                NewValue = modSetting.GetStringValue(AvailableValues[SelectedIndex]);
            }
        }

        private void UpdateSelectedIndexByNewValue()
        {
            if (modSetting.Validate(NewValue, out object val))
            {
                SelectedIndex = Array.IndexOf(AvailableValues, val);
            }
        }

        private string GetOtherValues<T>(NumberModSetting<T> modSetting)
            where T : struct
        {
            var otherValues = new List<string>();
            if (!modSetting.IsMaxClassMaxValue)
            {
                otherValues.Add(String.Format("{0}:{1}", language["MaxValue"], modSetting.MaxValue));
            }
            if (!modSetting.IsMinClassMinValue)
            {
                otherValues.Add(String.Format("{0}:{1}", language["MinValue"], modSetting.MinValue));
            }
            switch (otherValues.Count)
            {
                case 0:
                    return "";
                case 1:
                    return otherValues[0];
                default:
                    return otherValues.Aggregate((s1, s2) => String.Format("{0}, {1}", s1, s2));
            }
        }

        private void SetDefaultCommand_Execute()
        {
            NewValue = modSetting.GetStringValue(modSetting.Default);
            if (AvailableValues != null)
            {
                UpdateSelectedIndexByNewValue();
            }
        }

        private void Validate()
        {
            IsOkEnabled = modSetting.Validate(NewValue, out object val);
        }
    }
}
