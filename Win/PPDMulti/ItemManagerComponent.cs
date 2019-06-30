using PPDFramework;
using PPDMultiCommon.Data;
using PPDMultiCommon.Model;
using PPDShareComponent;
using SharpDX;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace PPDMulti
{
    class ItemManagerComponent : GameComponent
    {
        public event Action<ItemEffect> EffectAdded;
        public event Action<ItemEffect> EffectRemoved;

        PPDFramework.Resource.ResourceManager resourceManager;

        List<ItemType> queue;
        List<ItemEffect> effects;
        int[] itemWeight;

        SpriteObject itemSprite;

        Stopwatch stopwatch;

        private int maxItemCount;
        public int MaxItemCount
        {
            get
            {
                return maxItemCount;
            }
            set
            {
                if (maxItemCount != value)
                {
                    maxItemCount = value;
                    ChangeLockPictureVisibility();
                }
            }
        }

        PictureObject[] lockPicture;

        public ItemManagerComponent(PPDDevice device, PPDFramework.Resource.ResourceManager resourceManager, GameRule gameRule) : base(device)
        {
            this.resourceManager = resourceManager;
            stopwatch = new Stopwatch();
            stopwatch.Start();

            queue = new List<ItemType>();
            effects = new List<ItemEffect>();

            var itemTypeArray = (ItemType[])Enum.GetValues(typeof(ItemType));

            itemWeight = new int[itemTypeArray.Length - 1];
            int sum = 0;
            for (int i = 1; i < itemTypeArray.Length; i++)
            {
                int weight = 0;
                switch ((ItemType)i)
                {
                    case ItemType.Auto:
                        weight = 1;
                        break;
                    case ItemType.Barrier:
                        weight = 50;
                        break;
                    case ItemType.DoubleBPM:
                        weight = 20;
                        break;
                    case ItemType.DoubleCombo:
                        weight = 100;
                        break;
                    case ItemType.DoubleScore:
                        weight = 100;
                        break;
                    case ItemType.FogFilter:
                        weight = 20;
                        break;
                    case ItemType.HalfScore:
                        weight = 20;
                        break;
                    case ItemType.HatenaBox:
                        weight = 10;
                        break;
                    case ItemType.Hidden:
                        weight = 20;
                        break;
                    case ItemType.StripeFilter:
                        weight = 20;
                        break;
                    case ItemType.Sudden:
                        weight = 20;
                        break;
                    case ItemType.TripleCombo:
                        weight = 20;
                        break;
                    case ItemType.TripleScore:
                        weight = 20;
                        break;
                    case ItemType.Stealth:
                        weight = 50;
                        break;
                    case ItemType.PingPong:
                        weight = 20;
                        break;
                }

                sum += weight;
                itemWeight[i - 1] = sum;
            }

            this.AddChild(new PictureObject(device, resourceManager, Utility.Path.Combine("item", "frame.png")));
            lockPicture = new PictureObject[6];
            for (int i = 0; i < 6; i++)
            {
                this.AddChild(lockPicture[i] = new PictureObject(device, resourceManager, Utility.Path.Combine("item", "lock.png"))
                {
                    Position = new Vector2(3 + i * 19, 3)
                });
            }
            ChangeLockPictureVisibility();
            this.AddChild(itemSprite = new SpriteObject(device));

            MaxItemCount = gameRule.MaxItemCount;
        }

        private void ChangeLockPictureVisibility()
        {
            for (int i = 0; i < 6; i++)
            {
                lockPicture[i].Hidden = i < maxItemCount;
            }
        }

        public int ItemCount
        {
            get
            {
                return queue.Count;
            }
        }

        public Vector2 GetItemPosCand(int index)
        {
            return this.Position + new Vector2(3 + (queue.Count + index) * 19, 3);
        }

        public ItemEffect GetEffect(ItemType itemType)
        {
            return effects.FirstOrDefault(e => e.ItemType == itemType);
        }

        public void AddItem()
        {
            if (queue.Count < MaxItemCount)
            {
                var r = new Random();
                var rand = r.Next(0, itemWeight[itemWeight.Length - 1] + 1);

                int index = -1;
                for (int i = 0; i < itemWeight.Length; i++)
                {
                    if (rand <= itemWeight[i])
                    {
                        index = i;
                        break;
                    }
                }

                if (index >= 0)
                {
                    itemSprite.AddChild(new PictureObject(device, resourceManager, Utility.Path.Combine("item", String.Format("{0}.png", (ItemType)(index + 1))), true)
                    {
                        Position = new Vector2(11 + queue.Count * 19, 11),
                        Scale = new Vector2(0),
                        Alpha = 0
                    });
                    queue.Add((ItemType)(index + 1));
                }
                else
                {
                    // error
                    Console.WriteLine("Error");
                }
            }
        }

        public bool CanUse
        {
            get
            {
                return queue.Count > 0;
            }
        }

        public ItemType Use()
        {
            if (queue.Count > 0)
            {
                ItemType itemType = queue[0];
                itemSprite.RemoveChild(itemSprite[0]);
                queue.RemoveAt(0);
                return itemType;
            }
            else
            {
                return ItemType.None;
            }
        }

        public IEnumerable<ItemUseEventArgs> EnumerateItem()
        {
            for (int i = queue.Count - 1; i >= 0; i--)
            {
                if ((itemSprite[i] as PictureObject).Scale.X == 1)
                {
                    var args = new ItemUseEventArgs(queue[i]);
                    yield return args;

                    if (args.Use)
                    {
                        itemSprite.RemoveChild(itemSprite[i]);
                        queue.RemoveAt(i);
                    }
                }
            }
        }

        public void AddEffect(ItemEffect effect)
        {
            effects.Add(effect);
            effect.StartTime = stopwatch.ElapsedMilliseconds;
            if (EffectAdded != null)
            {
                EffectAdded.Invoke(effect);
            }
        }

        public void AddOthersEffect(ItemEffect effect)
        {
            effects.Add(effect);
            effect.StartTime = stopwatch.ElapsedMilliseconds;
        }

        protected override void UpdateImpl()
        {
            long time = stopwatch.ElapsedMilliseconds;
            for (int i = effects.Count - 1; i >= 0; i--)
            {
                if (time >= effects[i].EndTime)
                {
                    ItemEffect effect = effects[i];
                    effects[i].RemoveEffect();
                    effects.RemoveAt(i);
                    if (EffectRemoved != null)
                    {
                        EffectRemoved.Invoke(effect);
                    }
                }
            }

            for (int i = 0; i < itemSprite.ChildrenCount; i++)
            {
                var pic = itemSprite[i] as PictureObject;
                itemSprite[i].Position = new SharpDX.Vector2(AnimationUtility.GetAnimationValue(itemSprite[i].Position.X, 11 + i * 19), itemSprite[i].Position.Y);
                itemSprite[i].Alpha = AnimationUtility.IncreaseAlpha(itemSprite[i].Alpha);
                pic.Scale = new Vector2(AnimationUtility.IncreaseAlpha(pic.Scale.X));
            }
        }

        public IEnumerable<ItemEffect> Effects
        {
            get
            {
                return effects;
            }
        }

        public bool ContainEffect(ItemType itemType)
        {
            foreach (ItemEffect effect in effects)
            {
                if (effect.ItemType == itemType)
                {
                    return true;
                }
            }
            return false;
        }
    }

    abstract class ItemEffect
    {
        public const int DefaultEffectTime = 15000;
        private long startTime;
        private int addedEffectTime;

        public event EventHandler Removed;

        protected ItemEffect(ItemType itemType)
        {
            ItemType = itemType;
        }

        public virtual void RemoveEffect()
        {
            Removed?.Invoke(this, EventArgs.Empty);
        }

        public long StartTime
        {
            get { return startTime; }
            set
            {
                startTime = value;
                EndTime = startTime + EffectTime;
            }
        }

        public long EndTime
        {
            get;
            private set;
        }

        protected void AddEffectTime(int ms)
        {
            addedEffectTime += ms;
            EndTime = startTime + addedEffectTime;
        }

        public virtual void AddEffect()
        {

        }

        public ItemType ItemType
        {
            get;
            private set;
        }

        protected virtual int EffectTime
        {
            get
            {
                return DefaultEffectTime;
            }
        }
    }

    class DoubleScoreEffect : ItemEffect
    {
        public DoubleScoreEffect() :
            base(ItemType.DoubleScore)
        {
        }
    }

    class DoubleComboEffect : ItemEffect
    {
        public DoubleComboEffect()
            : base(ItemType.DoubleCombo)
        {
        }

        protected override int EffectTime
        {
            get
            {
                return 5000;
            }
        }
    }

    class DoubleBPMEffect : ItemEffect
    {
        public const int MaxEffectLevel = 2;

        public int EffectLevel
        {
            get;
            private set;
        }

        public DoubleBPMEffect()
            : base(ItemType.DoubleBPM)
        {
            EffectLevel = 1;
        }

        public override void AddEffect()
        {
            if (EffectLevel < MaxEffectLevel)
            {
                EffectLevel++;
            }
            else
            {
                AddEffectTime(DefaultEffectTime / MaxEffectLevel);
            }
            base.AddEffect();
        }
    }

    class TripleScoreEffect : ItemEffect
    {
        public TripleScoreEffect()
            : base(ItemType.TripleScore)
        {
        }
    }

    class TripleComboEffect : ItemEffect
    {
        public TripleComboEffect()
            : base(ItemType.TripleCombo)
        {
        }

        protected override int EffectTime
        {
            get
            {
                return 5000;
            }
        }
    }

    class HalfScoreEffect : ItemEffect
    {
        public HalfScoreEffect()
            : base(ItemType.HalfScore)
        {
        }
    }

    class AutoEffect : ItemEffect
    {
        public AutoEffect() :
            base(ItemType.Auto)
        {
        }

        protected override int EffectTime
        {
            get
            {
                return 30000;
            }
        }
    }

    class SuddenEffect : ItemEffect
    {
        public SuddenEffect()
            : base(ItemType.Sudden)
        {
        }
    }

    class HiddenEffect : ItemEffect
    {
        public HiddenEffect() :
            base(ItemType.Hidden)
        {
        }
    }

    class BarrierEffect : ItemEffect
    {
        public BarrierEffect()
            : base(ItemType.Barrier)
        {
        }
    }

    class StripeFilterEffect : ItemEffect
    {
        public const int MaxEffectLevel = 2;

        private StripeEffectComponent component;

        public int EffectLevel
        {
            get;
            private set;
        }

        public StripeFilterEffect(StripeEffectComponent component)
            : base(ItemType.StripeFilter)
        {
            this.component = component;
            component.Effect = this;
            EffectLevel = 1;
        }

        public override void RemoveEffect()
        {
            component.EffectExpired = true;
            base.RemoveEffect();
        }

        public override void AddEffect()
        {
            if (EffectLevel < MaxEffectLevel)
            {
                EffectLevel++;
            }
            else
            {
                AddEffectTime(DefaultEffectTime / MaxEffectLevel);
            }
            base.AddEffect();
        }
    }

    class FogFilterEffect : ItemEffect
    {
        public const int MaxEffectLevel = 2;

        private FogEffectComponent component;

        public int EffectLevel
        {
            get;
            private set;
        }

        public FogFilterEffect(FogEffectComponent component)
            : base(ItemType.FogFilter)
        {
            this.component = component;
            component.Effect = this;
            EffectLevel = 1;
        }

        public override void RemoveEffect()
        {
            component.EffectExpired = true;
            base.RemoveEffect();
        }

        public override void AddEffect()
        {
            if (EffectLevel < MaxEffectLevel)
            {
                EffectLevel++;
            }
            else
            {
                AddEffectTime(DefaultEffectTime / MaxEffectLevel);
            }
            base.AddEffect();
        }
    }

    class PingPongEffect : ItemEffect
    {
        private PingPongEffectComponent component;

        public PingPongEffect(PingPongEffectComponent component) :
            base(ItemType.PingPong)
        {
            this.component = component;
            component.Effect = this;
        }

        public override void RemoveEffect()
        {
            component.EffectExpired = true;
            base.RemoveEffect();
        }

        public override void AddEffect()
        {
            AddEffectTime(DefaultEffectTime);
            base.AddEffect();
        }
    }

    class StealthEffect : ItemEffect
    {
        public StealthEffect()
            : base(ItemType.Stealth)
        {
        }
    }

    class ItemUseEventArgs
    {
        public ItemUseEventArgs(ItemType itemType)
        {
            ItemType = itemType;
        }

        public ItemType ItemType
        {
            get;
            private set;
        }

        public bool Use
        {
            get;
            set;
        }
    }
}
