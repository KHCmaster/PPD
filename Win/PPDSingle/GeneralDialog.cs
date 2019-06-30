using PPDFramework;

namespace PPDSingle
{
    class GeneralDialog : GeneralDialogBase
    {
        public enum ButtonTypes
        {
            Ok,
            OkCancel,
            YesNo,
        }

        public bool OK
        {
            get { return Result == 0; }
        }

        protected override int CancelIndex
        {
            get
            {
                return 1;
            }
        }

        public GeneralDialog(PPDDevice device, PPDFramework.Resource.ResourceManager resourceManager, ISound sound, string displayText) :
            this(device, resourceManager, sound, displayText, ButtonTypes.Ok)
        { }

        public GeneralDialog(PPDDevice device, PPDFramework.Resource.ResourceManager resourceManager, ISound sound, string displayText, ButtonTypes buttons) :
            base(device, resourceManager, sound, displayText,
           buttons == ButtonTypes.Ok ? new string[] { "OK" } : (buttons == ButtonTypes.OkCancel ? new string[] { Utility.Language["OK"], Utility.Language["Cancel"] } : new string[] { Utility.Language["Yes"], Utility.Language["No"] }))
        { }
    }
}
