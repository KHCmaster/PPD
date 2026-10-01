using PPDCoreModel;
using PPDCoreModel.Data;
using PPDFramework;

namespace PPDCore
{
    class DisabledInputInfo(InputInfoBase inputInfo, InputManager inputManager) : InputInfoBase(inputInfo.Accurate)
    {
        private readonly InputInfoBase inputInfo = inputInfo;
        private readonly InputManager inputManager = inputManager;

        private bool IsNormalButton(ButtonType buttonType)
        {
            return buttonType >= ButtonType.Square && buttonType <= ButtonType.L;
        }

        public override bool IsPressed(ButtonType buttonType)
        {
            if (IsNormalButton(buttonType) && inputManager.IsDisabled((MarkType)buttonType))
            {
                return false;
            }
            return inputInfo.IsPressed(buttonType);
        }

        public override bool IsReleased(ButtonType buttonType)
        {
            if (IsNormalButton(buttonType) && inputManager.IsDisabled((MarkType)buttonType))
            {
                return false;
            }
            return inputInfo.IsReleased(buttonType);
        }

        public override int GetPressingFrame(ButtonType buttonType)
        {
            if (IsNormalButton(buttonType) && inputManager.IsDisabled((MarkType)buttonType))
            {
                return 0;
            }
            return inputInfo.GetPressingFrame(buttonType);
        }
    }
}
