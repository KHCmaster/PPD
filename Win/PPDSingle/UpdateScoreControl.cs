using PPDFramework;
using PPDFramework.Web;
using SharpDX;
using System;
using System.Collections.Generic;

namespace PPDSingle
{
    class UpdateScoreControl : FocusableGameComponent
    {
        const int MaxDisplayCount = 13;
        const int scrollBarStartY = 80;
        const int scrollBarMaxHeight = 330;

        PPDFramework.Resource.ResourceManager resourceManager;
        ISound sound;
        PictureObject back;
        SpriteObject mainSprite;
        RectangleComponent scrollBar;

        int scrollIndex;
        int updateIndex;
        bool updating;
        List<bool> updateSucess;
        List<SongInformation> updatedSongInformations;

        public SongInformation[] UpdatedSongInformations
        {
            get
            {
                if (updatedSongInformations != null)
                {
                    return updatedSongInformations.ToArray();
                }
                return new SongInformation[0];
            }
        }

        public UpdateScoreControl(PPDDevice device, PPDFramework.Resource.ResourceManager resourceManager, ISound sound) : base(device)
        {
            this.resourceManager = resourceManager;
            this.sound = sound;

            mainSprite = new SpriteObject(device)
            {
                Position = new Vector2(40, 80)
            };
            this.AddChild(scrollBar = new RectangleComponent(device, resourceManager, PPDColors.White)
            {
                Position = new Vector2(755, 80),
                RectangleHeight = 330,
                RectangleWidth = 5
            });
            this.AddChild(mainSprite);
            this.AddChild(back = new PictureObject(device, resourceManager, Utility.Path.Combine("dialog_back.png")));
            this.AddChild(new RectangleComponent(device, resourceManager, PPDColors.Black)
            {
                RectangleHeight = 450,
                RectangleWidth = 800,
                Alpha = 0.65f
            });
            back.AddChild(new TextureString(device, Utility.Language["UpdateScore"], 30, PPDColors.White)
            {
                Position = new Vector2(35, 30)
            });
            GotFocused += UpdateScoreControl_GotFocused;
            Inputed += UpdateScoreControl_Inputed;
        }

        void UpdateScoreControl_Inputed(IFocusable sender, InputEventArgs args)
        {
            if (updating)
            {
                return;
            }
            if (args.InputInfo.IsPressed(ButtonType.Cross))
            {
                FocusManager.RemoveFocus();
                sound.Play(PPDSetting.DefaultSounds[2], -1000);
                return;
            }
            else if (args.InputInfo.IsPressed(ButtonType.Circle))
            {
                BeginUpdate();
            }
            else if (args.InputInfo.IsPressed(ButtonType.Down))
            {
                scrollIndex++;
                if (scrollIndex >= mainSprite.ChildrenCount - MaxDisplayCount)
                {
                    scrollIndex = 0;
                }
                sound.Play(PPDSetting.DefaultSounds[0], -1000);
                AdjustScrollBar();
            }
            else if (args.InputInfo.IsPressed(ButtonType.Up))
            {
                scrollIndex--;
                if (scrollIndex < 0)
                {
                    scrollIndex = Math.Max(0, mainSprite.ChildrenCount - MaxDisplayCount - 1);
                }
                sound.Play(PPDSetting.DefaultSounds[0], -1000);
                AdjustScrollBar();
            }
        }

        private void BeginUpdate()
        {
            updating = true;
            updateIndex = 0;
            updateSucess = new List<bool>();
            updatedSongInformations = new List<SongInformation>();
            WebSongInformationManager.Instance.ScoreUpdated += Instance_ScoreUpdated;
            WebSongInformationManager.Instance.ScoreUpdateFailed += Instance_ScoreUpdateFailed;
            WebSongInformationManager.Instance.UpdateScore();
        }

        void Instance_ScoreUpdateFailed(int obj, SongInformation songInfo)
        {
            lock (this)
            {
                updateSucess.Add(false);
                updateIndex = obj;
            }
        }

        void Instance_ScoreUpdated(int obj, SongInformation songInfo)
        {
            lock (this)
            {
                updateSucess.Add(true);
                updatedSongInformations.Add(songInfo);
                updateIndex = obj;
            }
        }

        void UpdateScoreControl_GotFocused(IFocusable sender, FocusEventArgs args)
        {
            if (WebSongInformationManager.Instance.UpdatableCount == 0)
            {
                return;
            }

            mainSprite.ClearChildren();
            scrollIndex = 0;
            int iter = 0;
            foreach (WebSongInformation info in WebSongInformationManager.Instance.UpdatableInformations)
            {
                mainSprite.AddChild(new UpdatableScoreGameCompoennt(device, resourceManager, info)
                {
                    Position = new Vector2(0, 26 * iter)
                });
                iter++;
            }
            AdjustScrollBar();
        }

        private void AdjustScrollBar()
        {
            for (int i = 0; i < mainSprite.ChildrenCount; i++)
            {
                mainSprite[i].Position = new Vector2(0, 26 * (i - scrollIndex));
                mainSprite[i].Hidden = i < scrollIndex || scrollIndex + MaxDisplayCount <= i;
            }

            scrollBar.RectangleHeight = mainSprite.ChildrenCount <= MaxDisplayCount ? scrollBarMaxHeight : scrollBarMaxHeight * MaxDisplayCount / mainSprite.ChildrenCount;
            scrollBar.Position = new Vector2(scrollBar.Position.X, mainSprite.ChildrenCount <= MaxDisplayCount ? scrollBarStartY : scrollBarStartY + scrollBarMaxHeight * scrollIndex / mainSprite.ChildrenCount);
        }

        private void CheckUpdateIndex()
        {
            lock (this)
            {
                if (updateIndex < scrollIndex)
                {
                    scrollIndex = updateIndex;
                }
                if (updateIndex + 1 - scrollIndex > MaxDisplayCount)
                {
                    scrollIndex = Math.Min(mainSprite.ChildrenCount - MaxDisplayCount, updateIndex - MaxDisplayCount + 1);
                }
                AdjustScrollBar();
                for (int i = 0; i < updateIndex; i++)
                {
                    var scoreComponent = mainSprite.GetChildAt(i) as UpdatableScoreGameCompoennt;
                    scoreComponent.State = updateSucess[i] ? UpdatableScoreGameCompoennt.UpdateState.Updated : UpdatableScoreGameCompoennt.UpdateState.UpdateFailed;
                }
                if (updateIndex >= mainSprite.ChildrenCount)
                {
                    updating = false;
                    FocusManager.RemoveFocus();
                    return;
                }
                (mainSprite.GetChildAt(updateIndex) as UpdatableScoreGameCompoennt).State = UpdatableScoreGameCompoennt.UpdateState.Updating;
            }
        }

        protected override void UpdateImpl()
        {
            if (updating)
            {
                CheckUpdateIndex();
            }
        }

        protected override bool OnCanUpdate()
        {
            return !Hidden && OverFocused;
        }

        protected override bool OnCanDraw(PPDFramework.Shaders.AlphaBlendContext alphaBlendContext, int depth, int childIndex)
        {
            return OverFocused;
        }

        public class UpdatableScoreGameCompoennt : GameComponent
        {
            PPDFramework.Resource.ResourceManager resourceManager;
            WebSongInformation songInformation;
            UpdateState state = UpdateState.NotYet;
            TextureString stateText;
            TextureString songInfoText;

            public enum UpdateState
            {
                NotYet,
                Updating,
                Updated,
                UpdateFailed,
            }

            public WebSongInformation SongInformation
            {
                get
                {
                    return songInformation;
                }
            }

            public UpdateState State
            {
                get
                {
                    return state;
                }
                set
                {
                    if (state != value)
                    {
                        state = value;
                        UpdateStateText();
                    }
                }
            }

            public UpdatableScoreGameCompoennt(PPDDevice device, PPDFramework.Resource.ResourceManager resourceManager, WebSongInformation songInformation) : base(device)
            {
                this.resourceManager = resourceManager;
                this.songInformation = songInformation;
                this.AddChild(new StackObject(device,
                    songInfoText = new TextureString(device, songInformation.Title, 18, PPDColors.White),
                    new SpaceObject(device, 5, 1),
                    stateText = new TextureString(device, "", 18, PPDColors.White))
                {
                    IsHorizontal = true
                });
                UpdateStateText();
            }

            private void UpdateStateText()
            {
                string text = "";
                switch (state)
                {
                    case UpdateState.NotYet:
                        text = Utility.Language["NotUpdated"];
                        break;
                    case UpdateState.Updating:
                        text = Utility.Language["Updating"];
                        break;
                    case UpdateState.Updated:
                        text = Utility.Language["Updated"];
                        break;
                    case UpdateState.UpdateFailed:
                        text = Utility.Language["UpdateFailed"];
                        break;
                }
                stateText.Text = String.Format("[{0}]", text);
            }
        }
    }
}
