namespace PPDEditorCommon.Dialog.ViewModel
{
    class StringSettingViewModel : TextSettingViewModel
    {
        public override object ValidatedValue
        {
            get { return Text; }
        }

        public StringSettingViewModel(StringSetting setting)
            : base(setting)
        {
            Text = setting.DefaultValue;
        }

        protected override void Validate()
        {
            if (Text.Length >= GetSetting<StringSetting>().MaxLength)
            {
                IsValidated = false;
                return;
            }
            IsValidated = true;
        }
    }
}
