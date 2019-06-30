using System;

namespace PPDEditorCommon.Dialog.ViewModel
{
    class DoubleSettingViewModel : TextSettingViewModel
    {
        public override object ValidatedValue
        {
            get { return GetValue(); }
        }

        public DoubleSettingViewModel(DoubleSetting setting)
            : base(setting)
        {
            Text = setting.DefaultValue.ToString();
        }

        protected override void Validate()
        {
            if (!double.TryParse(Text, out double val))
            {
                IsValidated = false;
                ValidateErrorText = "Not Proper Number";
                return;
            }
            var setting = GetSetting<DoubleSetting>();
            if (val < setting.MinValue || val > setting.MaxValue)
            {
                IsValidated = false;
                ValidateErrorText = String.Format("{0}~{1}", setting.MinValue, setting.MaxValue);
                return;
            }
            IsValidated = true;
        }

        private double GetValue()
        {
            double.TryParse(Text, out double val);
            return val;
        }
    }
}
