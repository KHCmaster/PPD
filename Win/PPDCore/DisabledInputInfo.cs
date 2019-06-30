using PPDCoreModel;
using PPDCoreModel.Data;
using PPDFramework;

namespace PPDCore
{
    class DisabledInputInfo : InputInfoBase
    {
        private InputInfoBase inputInfo;
        private InputManager inputManager;

        public DisabledInputInfo(InputInfoBase inputInfo, InputManager inputManager)
            : base(inputInfo.Accurate)
        {
            this.inputInfo = inputInfo;
            this.inputManager = inputManager;
        }

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
