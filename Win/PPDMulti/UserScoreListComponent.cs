using PPDFramework;
using PPDMultiCommon.Model;
using PPDShareComponent;
using System.Linq;

namespace PPDMulti
{
    class UserScoreListComponent : GameComponent
    {
        const int ItemHeight = 45;
        const int MaxDisplayCount = 8;

        SpriteObject iconSprite;
        PPDFramework.Resource.ResourceManager resourceManager;

        public UserScoreListComponent(PPDDevice device, PPDFramework.Resource.ResourceManager resourceManager) : base(device)
        {
            this.resourceManager = resourceManager;

            this.AddChild(iconSprite = new SpriteObject(device));
        }

        public void AddUser(UserPlayState userPlayState)
        {
            var playUserIcon = new PlayUserIcon(device, resourceManager, userPlayState)
            {
                Position = new SharpDX.Vector2(0, ChildrenCount * ItemHeight)
            };
            iconSprite.AddChild(playUserIcon);
        }

        public void AddSelfUser(UserPlayState selfUserPlayState, ItemManagerComponent itemManager)
        {
            var playUserIcon = new PlayUserIcon(device, resourceManager, selfUserPlayState, itemManager)
            {
                Position = new SharpDX.Vector2(0, ChildrenCount * ItemHeight)
            };
            iconSprite.AddChild(playUserIcon);
        }

        public void AddFinish()
        {
            SortUserIcon(true);
        }

        public void DeleteUser(UserPlayState userPlayState)
        {
            PlayUserIcon found = null;
            foreach (PlayUserIcon playUserIcon in iconSprite.Children)
            {
                if (playUserIcon.UserPlayState == userPlayState)
                {
                    found = playUserIcon;
                    break;
                }
            }

            if (found != null)
            {
                iconSprite.RemoveChild(found);
            }
        }

        private void SortUserIcon(bool noAnimation)
        {
            var icons = iconSprite.Children.Cast<PlayUserIcon>().ToList();
            icons.Sort((s1, s2) =>
            {
                return s2.UserPlayState.Score - s1.UserPlayState.Score;
            });
            int lastScore = -1;
            int rank = 0;
            foreach (PlayUserIcon icon in icons)
            {
                if (lastScore != icon.UserPlayState.Score)
                {
                    lastScore = icon.UserPlayState.Score;
                    rank++;
                }
                icon.UserPlayState.Rank = rank;
            }
            icons.Sort(Compare);

            for (int i = 0; i < icons.Count; i++)
            {
                var newY = i * ItemHeight;
                icons[i].Position = new SharpDX.Vector2(0, noAnimation ? newY : AnimationUtility.GetAnimationValue(icons[i].Position.Y, newY));
            }
            var selfIndex = icons.FindIndex(icon => icon.UserPlayState.User.IsSelf);
            var y = 0;
            if (selfIndex >= 0 && selfIndex >= MaxDisplayCount)
            {
                y = -(selfIndex - MaxDisplayCount + 1) * ItemHeight;
            }
            iconSprite.Position = new SharpDX.Vector2(iconSprite.Position.X, noAnimation ? y : AnimationUtility.GetAnimationValue(iconSprite.Position.Y, y));
        }

        protected override void UpdateImpl()
        {
            SortUserIcon(false);
        }

        private int Compare(PlayUserIcon icon1, PlayUserIcon icon2)
        {
            int ret;
            if (icon1.Alpha == 0 && icon2.Alpha == 0)
            {
                return 0;
            }
            else if (icon1.Alpha == 0)
            {
                return 1;
            }
            else if (icon2.Alpha == 0)
            {
                return -1;
            }

            if (icon1.DisplayScore == icon2.DisplayScore)
            {
                ret = icon2.UserPlayState.User.ID - icon1.UserPlayState.User.ID;
            }
            else
            {
                ret = icon1.DisplayScore - icon2.DisplayScore;
            }
            return ret * -1;
        }
    }
}
