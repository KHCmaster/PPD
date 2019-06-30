using PPDCore;
using PPDMultiCommon.Data;
using PPDShareComponent;

namespace PPDMulti
{
    class MainGameConfig : MainGameConfigBase
    {
        ItemManagerComponent itemManager;
        float speedScale = 1;


        public MainGameConfig(ItemManagerComponent itemManager)
        {
            this.itemManager = itemManager;
        }

        public override bool Auto
        {
            get
            {
                return itemManager.ContainEffect(ItemType.Auto);
            }
        }

        public override PPDFramework.PPDStructure.EVDData.DisplayState DisplayState
        {
            get
            {
                if (itemManager.ContainEffect(ItemType.Hidden))
                {
                    return PPDFramework.PPDStructure.EVDData.DisplayState.HiddenColor;
                }
                else if (itemManager.ContainEffect(ItemType.Sudden))
                {
                    return PPDFramework.PPDStructure.EVDData.DisplayState.SuddenColor;
                }
                else
                {
                    return PPDFramework.PPDStructure.EVDData.DisplayState.Normal;
                }
            }
        }

        public override float ScoreScale
        {
            get
            {
                float scale = 1;
                foreach (ItemEffect effect in itemManager.Effects)
                {
                    switch (effect.ItemType)
                    {
                        case ItemType.HalfScore:
                            scale *= 0.5f;
                            break;
                        case ItemType.DoubleScore:
                            scale *= 2;
                            break;
                        case ItemType.TripleScore:
                            scale *= 3;
                            break;
                    }
                }

                return scale;
            }
        }

        public override float SpeedScale
        {
            get
            {
                return speedScale;
            }
        }

        public override float ComboScale
        {
            get
            {
                float scale = 1;
                foreach (ItemEffect effect in itemManager.Effects)
                {
                    switch (effect.ItemType)
                    {
                        case ItemType.DoubleCombo:
                            scale *= 2;
                            break;
                        case ItemType.TripleCombo:
                            scale *= 3;
                            break;
                    }
                }

                return scale;
            }
        }

        public void Update()
        {
            float nextScale = 1;
            foreach (ItemEffect effect in itemManager.Effects)
            {
                if (effect.ItemType == ItemType.DoubleBPM)
                {
                    nextScale *= 2 * ((DoubleBPMEffect)effect).EffectLevel;
                }
            }
            speedScale = AnimationUtility.GetAnimationValue(speedScale, nextScale);
        }
    }
}
