using PPDFramework;
using PPDFramework.Mod;
using PPDFramework.Web;
using PPDShareComponent;
using SharpDX;
using System;
using System.Linq;

namespace PPDSingle
{
    class LeftMenu : FocusableGameComponent
    {
        enum SelectionMode
        {
            UpdateScore = 0,
            Filter,
            ScoreManage,
            PlayRecord,
            Replay,
            RandomSelect,
            ItemList,
            UpdateScoreDB,
            Mod,
            Finish,
            MaxCount,
        }

        public event Action<RandomSelectType> RandomSelected;
        public event Action FilterChanged;

        IGameHost gameHost;
        PPDFramework.Resource.ResourceManager resourceManager;
        ISound sound;
        DxTextBox textBox;
        PictureObject back;
        ScoreManager sm;
        PlayRecord pr;
        SelectSongManager ssm;
        ModPanel mp;
        FilterControl fc;
        UpdateScoreControl usc;
        ItemListComponent ilc;
        ReplayListComponent rlc;

        PictureObject scoreUpdateCountBack;
        NumberPictureObject scoreUpdateCount;
        PictureObject modUpdateCountBack;
        NumberPictureObject modUpdateCount;
        TextureString updateScore;
        TextureString filter;
        MultiSelectableText randomSelect;
        TextureString scoreManage;
        TextureString playRecord;
        TextureString replay;
        TextureString itemList;
        TextureString updateScoreDB;
        TextureString mod;
        TextureString finish;

        EffectObject select;
        const int selectX = 15;
        const int selectDiffY = 10;

        SelectionMode selection = SelectionMode.Filter;
        bool playRecordAvailable;
        bool webScoreUpdated;
        bool modInitialized;

        int width;
        bool initialized;
        public LeftMenu(PPDDevice device, IGameHost gameHost, PPDFramework.Resource.ResourceManager resourceManager, DxTextBox textBox, SelectSongManager ssm, ISound sound) : base(device)
        {
            this.gameHost = gameHost;
            this.resourceManager = resourceManager;
            this.sound = sound;
            this.textBox = textBox;
            this.ssm = ssm;

            scoreUpdateCountBack = new PictureObject(device, resourceManager, Utility.Path.Combine("update_count_back.png"))
            {
                Position = new Vector2(20, 90),
                Hidden = true,
                Alpha = 0.75f
            };
            scoreUpdateCountBack.AddChild(scoreUpdateCount = new NumberPictureObject(device, resourceManager, Utility.Path.Combine("result", "scoresmall.png"))
            {
                Position = new Vector2(17, 5),
                Alignment = PPDFramework.Alignment.Center,
                MaxDigit = -1,
                Scale = new Vector2(0.75f)
            });
            modUpdateCountBack = new PictureObject(device, resourceManager, Utility.Path.Combine("update_count_back.png"))
            {
                Position = new Vector2(20, 330),
                Hidden = true,
                Alpha = 0.75f
            };
            modUpdateCountBack.AddChild(modUpdateCount = new NumberPictureObject(device, resourceManager, Utility.Path.Combine("result", "scoresmall.png"))
            {
                Position = new Vector2(17, 5),
                Alignment = PPDFramework.Alignment.Center,
                MaxDigit = -1,
                Scale = new Vector2(0.75f)
            });
            WebSongInformationManager.Instance.Updated += Instance_Updated;
            WebSongInformationManager.Instance.Update(true);

            this.Inputed += LeftMenu_Inputed;
            this.GotFocused += LeftMenu_GotFocused;
        }

        private bool PlayRecordAvailable
        {
            get
            {
                return playRecordAvailable;
            }
            set
            {
                playRecordAvailable = value;
                if (playRecordAvailable)
                {
                    playRecord.Color = PPDColors.White;
                }
                else
                {
                    playRecord.Color = PPDColors.Gray;
                    if (selection == SelectionMode.PlayRecord)
                    {
                        selection = SelectionMode.ScoreManage;
                    }
                }
            }
        }

        public bool ShouldFinish
        {
            get;
            private set;
        }

        public SongInformation SelectedSongInformation
        {
            get
            {
                return ssm.SelectedSongInformation.SongInfo;
            }
        }

        public PPDGameUtility LastGameUtility
        {
            get;
            set;
        }

        public ItemInfo UseItem
        {
            get
            {
                return ilc?.UseItem;
            }
        }

        void LeftMenu_GotFocused(IFocusable sender, FocusEventArgs args)
        {
            if (!initialized)
            {
                back = new PictureObject(device, resourceManager, Utility.Path.Combine("leftmenu.png"));
                width = (int)back.Width;
                this.Position = new Vector2(-width, 0);

                sm = new ScoreManager(device, resourceManager, textBox, sound, ssm.Filter);
                pr = new PlayRecord(device, resourceManager, sound);
                mp = new ModPanel(device, gameHost, resourceManager, sound);
                mp.LostFocused += mp_LostFocused;
                fc = new FilterControl(device, resourceManager, sound, ssm.Filter);
                fc.Changed += fc_Changed;
                usc = new UpdateScoreControl(device, resourceManager, sound);
                usc.LostFocused += usc_LostFocused;
                ilc = new ItemListComponent(device, gameHost, resourceManager, sound);
                rlc = new ReplayListComponent(device, gameHost, resourceManager, sound);

                updateScore = new TextureString(device, Utility.Language["UpdateScore"], 16, PPDColors.White)
                {
                    Position = new Vector2(30, 100)
                };
                filter = new TextureString(device, Utility.Language["Filter"], 16, PPDColors.White)
                {
                    Position = new Vector2(30, 130)
                };
                scoreManage = new TextureString(device, Utility.Language["ScoreManager"], 16, PPDColors.White)
                {
                    Position = new Vector2(30, 160)
                };
                playRecord = new TextureString(device, Utility.Language["PlayRecord"], 16, PPDColors.White)
                {
                    Position = new Vector2(30, 190)
                };
                replay = new TextureString(device, Utility.Language["Replay"], 16, PPDColors.White)
                {
                    Position = new Vector2(30, 220)
                };
                randomSelect = new MultiSelectableText(device, gameHost, resourceManager, Utility.Language["Random"],
                    new string[] { Utility.Language["InAll"], Utility.Language["InList"] }, 16)
                {
                    Position = new Vector2(30, 250)
                };
                randomSelect.Selected += randomSelect_Selected;
                itemList = new TextureString(device, Utility.Language["ItemList"], 16, PPDColors.White)
                {
                    Position = new Vector2(30, 280)
                };
                updateScoreDB = new TextureString(device, Utility.Language["UpdateDB"], 16, PPDColors.White)
                {
                    Position = new Vector2(30, 310)
                };
                mod = new TextureString(device, Utility.Language["Mod"], 16, PPDColors.White)
                {
                    Position = new Vector2(30, 340)
                };
                finish = new TextureString(device, Utility.Language["Exit"], 16, PPDColors.White)
                {
                    Position = new Vector2(30, 370)
                };

                select = new EffectObject(device, resourceManager, Utility.Path.Combine("greenflare.etd"))
                {
                    Position = new Vector2(15, scoreManage.Position.Y + selectDiffY)
                };
                select.PlayType = Effect2D.EffectManager.PlayType.ReverseLoop;
                select.Play();
                select.Alignment = EffectObject.EffectAlignment.Center;
                select.Scale = new Vector2(0.4f, 0.4f);

                this.AddChild(usc);
                this.AddChild(sm);
                this.AddChild(pr);
                this.AddChild(mp);
                this.AddChild(fc);
                this.AddChild(ilc);
                this.AddChild(rlc);
                this.AddChild(scoreUpdateCountBack);
                this.AddChild(modUpdateCountBack);
                this.AddChild(updateScore);
                this.AddChild(filter);
                this.AddChild(randomSelect);
                this.AddChild(scoreManage);
                this.AddChild(playRecord);
                this.AddChild(replay);
                this.AddChild(itemList);
                this.AddChild(updateScoreDB);
                this.AddChild(mod);
                this.AddChild(finish);
                this.AddChild(select);
                this.AddChild(back);
                initialized = true;
            }
            PlayRecordAvailable = ssm.SelectedSongInformation != null && ssm.SelectedSongInformation.SongInfo != null && !ssm.SelectedSongInformation.IsFolder;
        }

        void LeftMenu_Inputed(IFocusable sender, InputEventArgs args)
        {
            if (args.InputInfo.IsPressed(ButtonType.Circle))
            {
                switch (selection)
                {
                    case SelectionMode.UpdateScore:
                        FocusManager.Focus(usc);
                        break;
                    case SelectionMode.Filter:
                        FocusManager.Focus(fc);
                        break;
                    case SelectionMode.ScoreManage:
                        FocusManager.Focus(sm);
                        break;
                    case SelectionMode.PlayRecord:
                        FocusManager.Focus(pr);
                        break;
                    case SelectionMode.Replay:
                        FocusManager.Focus(rlc);
                        break;
                    case SelectionMode.ItemList:
                        FocusManager.Focus(ilc);
                        break;
                    case SelectionMode.UpdateScoreDB:
                        SongInformation.Update();
                        FocusManager.RemoveFocus();
                        break;
                    case SelectionMode.Mod:
                        FocusManager.Focus(mp);
                        break;
                    case SelectionMode.Finish:
                        ShouldFinish = true;
                        FocusManager.RemoveFocus();
                        break;
                    case SelectionMode.RandomSelect:
                        FocusManager.Focus(randomSelect);
                        break;
                }
                if (!ShouldFinish)
                {
                    sound.Play(PPDSetting.DefaultSounds[1], -1000);
                }
            }
            else if (args.InputInfo.IsPressed(ButtonType.Cross))
            {
                FocusManager.RemoveFocus();
            }
            else if (args.InputInfo.IsPressed(ButtonType.Square))
            {
                FocusManager.RemoveFocus();
            }
            else if (args.InputInfo.IsPressed(ButtonType.Up))
            {
                selection--;
                if (selection < 0)
                {
                    selection = SelectionMode.MaxCount - 1;
                }
                if (selection == SelectionMode.PlayRecord && !PlayRecordAvailable)
                {
                    selection--;
                }
                sound.Play(PPDSetting.DefaultSounds[0], -1000);
            }
            else if (args.InputInfo.IsPressed(ButtonType.Down))
            {
                selection++;
                if (selection >= SelectionMode.MaxCount)
                {
                    selection = 0;
                }
                if (selection == SelectionMode.PlayRecord && !PlayRecordAvailable)
                {
                    selection++;
                }
                sound.Play(PPDSetting.DefaultSounds[0], -1000);
            }

            float selectY = selectDiffY;
            switch (selection)
            {
                case SelectionMode.UpdateScore:
                    selectY += updateScore.Position.Y;
                    break;
                case SelectionMode.Filter:
                    selectY += filter.Position.Y;
                    break;
                case SelectionMode.ScoreManage:
                    selectY += scoreManage.Position.Y;
                    break;
                case SelectionMode.PlayRecord:
                    selectY += playRecord.Position.Y;
                    break;
                case SelectionMode.Replay:
                    selectY += replay.Position.Y;
                    break;
                case SelectionMode.ItemList:
                    selectY += itemList.Position.Y;
                    break;
                case SelectionMode.UpdateScoreDB:
                    selectY += updateScoreDB.Position.Y;
                    break;
                case SelectionMode.Mod:
                    selectY += mod.Position.Y;
                    break;
                case SelectionMode.Finish:
                    selectY += finish.Position.Y;
                    break;
                case SelectionMode.RandomSelect:
                    selectY += randomSelect.Position.Y;
                    break;
            }
            select.Position = new Vector2(select.Position.X, selectY);
        }

        void fc_Changed()
        {
            OnFilterChanged();
        }

        private void OnFilterChanged()
        {
            FilterChanged?.Invoke();
            sm.FilterChanged();
        }

        private void OnRandomSelected(RandomSelectType randomSelectType)
        {
            if (RandomSelected != null)
            {
                RandomSelected.Invoke(randomSelectType);
            }
        }

        void Instance_Updated()
        {
            webScoreUpdated = true;
        }

        void usc_LostFocused(IFocusable sender, FocusEventArgs args)
        {
            SongInformation.Update(usc.UpdatedSongInformations);
            UpdateUpdatableCount();
        }

        private void UpdateUpdatableCount()
        {
            WebSongInformationManager.Instance.Update(true);
            var updateCount = WebSongInformationManager.Instance.UpdatableCount;
            if (updateCount > 0)
            {
                scoreUpdateCount.Value = (uint)updateCount;
                scoreUpdateCountBack.Hidden = false;
            }
            else
            {
                scoreUpdateCountBack.Hidden = true;
            }
        }

        protected override void UpdateImpl()
        {
            if (OverFocused)
            {
                Position = new Vector2(Position.X - (0 + Position.X) * 0.3f, 0);
            }
            else
            {
                Position = new Vector2(Position.X - (width + Position.X) * 0.3f, 0);
            }

            if (webScoreUpdated)
            {
                UpdateUpdatableCount();
                webScoreUpdated = false;
            }

            if (ModManager.Instance.Initialized && !modInitialized)
            {
                modInitialized = true;
                CheckUpdatableMod();
            }
        }

        void mp_LostFocused(IFocusable sender, FocusEventArgs args)
        {
            CheckUpdatableMod();
        }

        void randomSelect_Selected()
        {
            OnRandomSelected((RandomSelectType)randomSelect.SelectedIndex);
            FocusManager.RemoveFocus();
        }

        private void CheckUpdatableMod()
        {
            if (!ModManager.Instance.Initialized)
            {
                return;
            }

            var updatableCount = ModManager.Instance.Root.Descendants().OfType<ModInfo>().Count(m => m.CanUpdate);
            if (updatableCount > 0)
            {
                modUpdateCount.Value = (uint)updatableCount;
                modUpdateCountBack.Hidden = false;
            }
            else
            {
                modUpdateCountBack.Hidden = true;
            }
        }

        protected override void DisposeResource()
        {
            base.DisposeResource();
            WebSongInformationManager.Instance.Updated -= Instance_Updated;
        }
    }
}
