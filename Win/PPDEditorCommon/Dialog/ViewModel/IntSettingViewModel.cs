using System;

namespace PPDEditorCommon.Dialog.ViewModel
{
    class IntSettingViewModel : TextSettingViewModel
    {
        public override object ValidatedValue
        {
            get { return GetValue(); }
        }

        public IntSettingViewModel(IntSetting setting)
            : base(setting)
        {
            Text = setting.DefaultValue.ToString();
        }

        protected override void Validate()
        {
            if (!int.TryParse(Text, out int val))
            {
                IsValidated = false;
                ValidateErrorText = "Not Proper Number";
                return;
            }
            var setting = GetSetting<IntSetting>();
            if (val < setting.MinValue || val > setting.MaxValue)
            {
                IsValidated = false;
                ValidateErrorText = String.Format("{0}~{1}", setting.MinValue, setting.MaxValue);
                return;
            }
            IsValidated = true;
        }

        private int GetValue()
        {
            int.TryParse(Text, out int val);
            return val;
        }
    }
}
