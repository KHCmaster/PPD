using PPDFramework;

namespace PPDSingle
{
    class SuperAutoDialog : GeneralDialogBase
    {
        public bool OK
        {
            get { return Result >= 0; }
        }

        public SuperAutoDialog(PPDDevice device, PPDFramework.Resource.ResourceManager resourceManager, ISound sound, string displayText) :
            base(device, resourceManager, sound, displayText,
            new string[] {
                Utility.Language["SuperAuto1"],
                Utility.Language["SuperAuto2"],
                Utility.Language["SuperAuto3"],
                Utility.Language["SuperAuto4"]})
        { }
    }
}
