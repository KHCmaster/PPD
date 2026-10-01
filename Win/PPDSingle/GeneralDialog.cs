using PPDFramework;

namespace PPDSingle
{
    class GeneralDialog(PPDDevice device, PPDFramework.Resource.ResourceManager resourceManager, ISound sound, string displayText, GeneralDialog.ButtonTypes buttons) : GeneralDialogBase(device, resourceManager, sound, displayText,
       buttons == ButtonTypes.Ok ? ["OK"] : (buttons == ButtonTypes.OkCancel ? new string[] { Utility.Language["OK"], Utility.Language["Cancel"] } : [Utility.Language["Yes"], Utility.Language["No"]]))
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
    }
}
