using System;

namespace PPDEditorCommon.Dialog.ViewModel
{
    class FloatSettingViewModel : TextSettingViewModel
    {
        public override object ValidatedValue
        {
            get { return GetValue(); }
        }

        public FloatSettingViewModel(FloatSetting setting)
            : base(setting)
        {
            Text = setting.DefaultValue.ToString();
        }

        protected override void Validate()
        {
            if (!float.TryParse(Text, out float val))
            {
                IsValidated = false;
                ValidateErrorText = "Not Proper Number";
                return;
            }
            var setting = GetSetting<FloatSetting>();
            if (val < setting.MinValue || val > setting.MaxValue)
            {
                IsValidated = false;
                ValidateErrorText = String.Format("{0}~{1}", setting.MinValue, setting.MaxValue);
                return;
            }
            IsValidated = true;
        }

        private float GetValue()
        {
            float.TryParse(Text, out float val);
            return val;
        }
    }
}
