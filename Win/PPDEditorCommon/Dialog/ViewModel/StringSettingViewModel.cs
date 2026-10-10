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
            // 最大長ちょうどの文字列も使える（元は >= で、最大長の文字列が通らなかった）
            if (Text.Length > GetSetting<StringSetting>().MaxLength)
            {
                IsValidated = false;
                return;
            }
            IsValidated = true;
        }
    }
}
