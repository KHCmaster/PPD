using PPDFramework;

namespace PPDSingle
{
    class SuperAutoDialog(PPDDevice device, PPDFramework.Resource.ResourceManager resourceManager, ISound sound, string displayText) : GeneralDialogBase(device, resourceManager, sound, displayText,
        [
                Utility.Language["SuperAuto1"],
                Utility.Language["SuperAuto2"],
                Utility.Language["SuperAuto3"],
                Utility.Language["SuperAuto4"]])
    {
        public bool OK
        {
            get { return Result >= 0; }
        }
    }
}
