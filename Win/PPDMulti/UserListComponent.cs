using PPDFramework;
using PPDMulti.Model;
using PPDShareComponent;

namespace PPDMulti
{
    class UserListComponent : GameComponent
    {
        PPDFramework.Resource.ResourceManager resourceManager;
        int nameAnimCount;
        float nameAlpha = 1;
        float idAlpha;
        bool isNameVisible = true;

        public float NameAlpha
        {
            get { return nameAlpha; }
        }

        public float IdAlpha
        {
            get { return idAlpha; }
        }

        public bool IsNameVisible
        {
            get { return isNameVisible; }
        }

        private float CurrentAlpha
        {
            get
            {
                return isNameVisible ? nameAlpha : idAlpha;
            }
            set
            {
                if (isNameVisible)
                {
                    nameAlpha = value;
                }
                else
                {
                    idAlpha = value;
                }
            }
        }

        public UserListComponent(PPDDevice device, PPDFramework.Resource.ResourceManager resourceManager) : base(device)
        {
            this.resourceManager = resourceManager;
        }

        public void AddUser(User user)
        {
            var x = 210 * (this.ChildrenCount % 2);
            var y = this.ChildrenCount / 2 * 50;
            this.AddChild(new MenuUserIcon(device, resourceManager, this, user)
            {
                Position = new SharpDX.Vector2(x, y),
                Alpha = 0
            });
        }

        public void RemoveUser(User user)
        {
            MenuUserIcon found = null;
            foreach (MenuUserIcon menuUser in this.Children)
            {
                if (menuUser.User == user)
                {
                    found = menuUser;
                    break;
                }
            }

            if (found != null)
            {
                this.RemoveChild(found);
            }
        }

        protected override void UpdateImpl()
        {
            int iter = 0;
            foreach (GameComponent gc in Children)
            {
                gc.Alpha = AnimationUtility.IncreaseAlpha(gc.Alpha);
                var x = AnimationUtility.GetAnimationValue(gc.Position.X, 210 * (iter % 2));
                var y = AnimationUtility.GetAnimationValue(gc.Position.Y, iter / 2 * 50);
                gc.Position = new SharpDX.Vector2(x, y);
                iter++;
            }
            if (nameAnimCount >= 0)
            {
                if (CurrentAlpha > 0)
                {
                    nameAnimCount++;
                    if (nameAnimCount > 300)
                    {
                        CurrentAlpha = AnimationUtility.DecreaseAlpha(CurrentAlpha);
                        if (CurrentAlpha == 0)
                        {
                            nameAnimCount = -1;
                            isNameVisible = !isNameVisible;
                        }
                    }
                }
            }
            else
            {
                CurrentAlpha = AnimationUtility.IncreaseAlpha(CurrentAlpha);
                if (CurrentAlpha == 1)
                {
                    nameAnimCount = 0;
                }
            }
        }
    }
}
