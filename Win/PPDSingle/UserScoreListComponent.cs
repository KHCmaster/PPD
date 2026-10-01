using PPDMath;
using PPDFramework;
using PPDShareComponent;
using System;
using System.Collections.Generic;
using System.Linq;

namespace PPDSingle
{
    class UserScoreListComponent(PPDDevice device, PPDFramework.Resource.ResourceManager resourceManager) : GameComponent(device)
    {
        const int ItemHeight = 45;

        readonly List<PlayUserIcon> list = [];
        readonly PPDFramework.Resource.ResourceManager resourceManager = resourceManager;

        public PlayUserIcon[] Players
        {
            get { return [.. list]; }
        }

        public void AddUser(UserPlayState userPlayState, bool showScore, bool showEvaluate, bool showLife)
        {
            var playUserIcon = new PlayUserIcon(device, resourceManager, userPlayState, showScore, showEvaluate, showLife)
            {
                Position = new System.Numerics.Vector2(0, ChildrenCount * ItemHeight)
            };
            this.AddChild(playUserIcon);
            list.Add(playUserIcon);
        }

        public void Retry()
        {
            for (int i = 0; i < list.Count; i++)
            {
                list[i].Retry();
            }
            list.Sort(Compare);
            for (int i = 0; i < list.Count; i++)
            {
                list[i].Position = new System.Numerics.Vector2(0, i * ItemHeight);
            }
        }

        public void DeleteUser(UserPlayState userPlayState)
        {
            PlayUserIcon found = null;
            foreach (PlayUserIcon playUserIcon in Children.Cast<PlayUserIcon>())
            {
                if (playUserIcon.UserPlayState == userPlayState)
                {
                    found = playUserIcon;
                    break;
                }
            }

            if (found != null)
            {
                this.RemoveChild(found);
                list.Remove(found);
            }
        }

        protected override void UpdateImpl()
        {
            list.Sort(Compare);
            int lastScore = -1;
            int rank = 0;
            foreach (PlayUserIcon icon in list)
            {
                if (lastScore != icon.UserPlayState.Score)
                {
                    lastScore = icon.UserPlayState.Score;
                    rank++;
                }
                icon.UserPlayState.Rank = rank;
            }

            for (int i = 0; i < list.Count; i++)
            {
                list[i].Position = new System.Numerics.Vector2(0, AnimationUtility.GetAnimationValue(list[i].Position.Y, i * ItemHeight));
            }
        }

        private int Compare(PlayUserIcon icon1, PlayUserIcon icon2)
        {
            int ret;

            if (icon1.DisplayScore == icon2.DisplayScore)
            {
                if (icon1.UserPlayState.User.IsSelf)
                {
                    return -1;
                }
                else if (icon2.UserPlayState.User.IsSelf)
                {
                    return 1;
                }
                ret = String.Compare(icon1.UserPlayState.User.AccountId, icon2.UserPlayState.User.AccountId);
            }
            else
            {
                ret = icon1.DisplayScore - icon2.DisplayScore;
            }
            return ret * -1;
        }
    }
}
