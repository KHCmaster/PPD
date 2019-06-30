using PPDFramework.PPDStructure.EVDData;

namespace PPDCore
{
    public class MainGameConfigBase
    {
        private static MainGameConfigBase _default = new MainGameConfigBase();

        public static MainGameConfigBase Default
        {
            get
            {
                return _default;
            }
        }

        public virtual float ScoreScale
        {
            get
            {
                return 1;
            }
        }

        public virtual bool Auto
        {
            get
            {
                return false;
            }
        }

        public virtual DisplayState DisplayState
        {
            get
            {
                return DisplayState.Normal;
            }
        }

        public virtual float SpeedScale
        {
            get
            {
                return 1;
            }
        }

        public virtual float ComboScale
        {
            get
            {
                return 1;
            }
        }
    }
}
