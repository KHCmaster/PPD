namespace PPDEditorCommon.Dialog.ViewModel
{
    class BoolSettingViewModel : SettingViewModelBase
    {
        private bool value;

        public bool Value
        {
            get { return value; }
            set
            {
                if (this.value != value)
                {
                    this.value = value;
                    RaisePropertyChanged("Value");
                }
            }
        }

        public override object ValidatedValue
        {
            get { return value; }
        }

        public BoolSettingViewModel(BoolSetting setting)
            : base(setting)
        {
            Value = setting.DefaultValue;
        }
    }
}
