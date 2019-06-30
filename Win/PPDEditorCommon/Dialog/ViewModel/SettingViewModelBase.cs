using GalaSoft.MvvmLight;

namespace PPDEditorCommon.Dialog.ViewModel
{
    abstract class SettingViewModelBase : ObservableObject
    {
        private bool isValidated;
        private string validateErrorText;
        protected SettingBase setting;

        public string Name
        {
            get
            {
                return setting.Name;
            }
        }

        public string Description
        {
            get
            {
                return setting.Description;
            }
        }

        public bool IsValidated
        {
            get { return isValidated; }
            protected set
            {
                if (isValidated != value)
                {
                    isValidated = value;
                    RaisePropertyChanged("IsValidated");
                    if (isValidated)
                    {
                        ValidateErrorText = null;
                    }
                }
            }
        }

        public string ValidateErrorText
        {
            get { return validateErrorText; }
            protected set
            {
                if (validateErrorText != value)
                {
                    validateErrorText = value;
                    RaisePropertyChanged("ValidateErrorText");
                }
            }
        }

        public abstract object ValidatedValue
        {
            get;
        }

        protected SettingViewModelBase(SettingBase setting)
        {
            this.setting = setting;
            IsValidated = true;
        }

        public T GetSetting<T>() where T : SettingBase
        {
            return (T)setting;
        }
    }
}
