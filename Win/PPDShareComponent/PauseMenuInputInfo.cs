using PPDFramework;
using System;

namespace PPDShareComponent
{
    class PauseMenuInputInfo : InputInfoBase
    {
        private InputInfoBase baseInputInfo;

        public PauseMenuInputInfo(InputInfoBase inputInfo) :
            base(inputInfo.Accurate)
        {
            baseInputInfo = inputInfo;
        }

        /// <summary>
        /// 押されているフレーム数を取得します。
        /// </summary>
        /// <param name="buttonType"></param>
        /// <returns></returns>
        public override int GetPressingFrame(ButtonType buttonType)
        {
            return baseInputInfo.GetPressingFrame(buttonType);
        }

        /// <summary>
        /// 押されたかどうかを取得します。
        /// </summary>
        /// <param name="buttonType"></param>
        /// <returns></returns>
        public override bool IsPressed(ButtonType buttonType)
        {
            if (buttonType == ButtonType.Right || buttonType == ButtonType.Left ||
                buttonType == ButtonType.R || buttonType == ButtonType.L)
            {
                var pressCount = baseInputInfo.GetPressingFrame(buttonType);
                if (pressCount % 5 == 4 && pressCount > 20)
                {
                    return true;
                }
            }
            return baseInputInfo.IsPressed(buttonType);
        }

        /// <summary>
        /// 押された長さを取得します。
        /// </summary>
        /// <returns></returns>
        public int GetPressingInterval(ButtonType buttonType)
        {
            if (buttonType == ButtonType.Right || buttonType == ButtonType.Left ||
                buttonType == ButtonType.R || buttonType == ButtonType.L)
            {
                var pressCount = baseInputInfo.GetPressingFrame(buttonType);
                if (pressCount % 5 == 4 && pressCount > 20)
                {
                    return Math.Max(1, pressCount / 20);
                }
            }
            return 1;
        }

        /// <summary>
        /// 離されたかどうかを取得します。
        /// </summary>
        /// <param name="buttonType"></param>
        /// <returns></returns>
        public override bool IsReleased(ButtonType buttonType)
        {
            return baseInputInfo.IsReleased(buttonType);
        }
    }
}
