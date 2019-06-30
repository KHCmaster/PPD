using PPDFramework;
using PPDShareComponent;
using SharpDX;
using System;

namespace PPDMulti
{
    class ItemOverrayComponent : GameComponent
    {
        public event Action ItemSet;

        PPDFramework.Resource.ResourceManager resourceManager;
        ItemManagerComponent itemManagerComponent;

        public ItemOverrayComponent(PPDDevice device, PPDFramework.Resource.ResourceManager resourceManager, ItemManagerComponent itemManagerComponent) : base(device)
        {
            this.resourceManager = resourceManager;
            this.itemManagerComponent = itemManagerComponent;
        }

        public void AddItem(Vector2 pos)
        {
            if (itemManagerComponent.ItemCount + this.ChildrenCount < itemManagerComponent.MaxItemCount)
            {
                this.AddChild(new PictureObject(device, resourceManager, Utility.Path.Combine("item", "something.png"))
                {
                    Position = pos
                });
            }
        }

        protected override void UpdateImpl()
        {
            for (int i = ChildrenCount - 1; i >= 0; i--)
            {
                GameComponent gc = this[i];
                Vector2 lastPos = gc.Position;
                gc.Position = AnimationUtility.GetAnimationPosition(gc.Position, itemManagerComponent.GetItemPosCand(i));
                if (lastPos == gc.Position)
                {
                    this.RemoveChild(gc);
                    if (ItemSet != null)
                    {
                        ItemSet.Invoke();
                    }
                }
            }
        }
    }
}
