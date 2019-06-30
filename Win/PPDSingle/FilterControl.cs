using PPDFramework;
using PPDShareComponent;
using SharpDX;
using System;

namespace PPDSingle
{
    class FilterControl : FocusableGameComponent
    {
        private const int Margin = 20;
        ISound sound;
        SongSelectFilter filter;

        RadioBoxComponent nameRadio;
        RadioBoxComponent timeRadio;
        RadioBoxComponent updateDateRadio;
        RadioBoxComponent bpmRadio;
        RadioBoxComponent authorRadio;

        RadioBoxComponent ascRadio;
        RadioBoxComponent descRadio;

        CheckBoxComponent easyCheck;
        CheckBoxComponent normalCheck;
        CheckBoxComponent hardCheck;
        CheckBoxComponent extremeCheck;
        CheckBoxComponent normalScoreCheck;
        CheckBoxComponent acScoreCheck;
        CheckBoxComponent acftScoreCheck;

        ButtonComponent setDefaultButton;

        GridSelection selection;

        SelectableComponent[] selectList;

        bool initialized;

        public event Action Changed;

        public SelectableComponent CurrentSelectedComponent
        {
            get
            {
                return selectList[selection.Current];
            }
            set
            {
                selection.SetAt(value.Position);
            }
        }

        public SongSelectFilter.SortField SortField
        {
            get
            {
                if (nameRadio.Checked)
                {
                    return SongSelectFilter.SortField.Name;
                }
                else if (timeRadio.Checked)
                {
                    return SongSelectFilter.SortField.Time;
                }
                else if (updateDateRadio.Checked)
                {
                    return SongSelectFilter.SortField.UpdateDate;
                }
                else if (bpmRadio.Checked)
                {
                    return SongSelectFilter.SortField.BPM;
                }
                else if (authorRadio.Checked)
                {
                    return SongSelectFilter.SortField.Author;
                }
                return SongSelectFilter.SortField.Name;
            }
            set
            {
                switch (value)
                {
                    case SongSelectFilter.SortField.Name:
                        nameRadio.Checked = true;
                        break;
                    case SongSelectFilter.SortField.Time:
                        timeRadio.Checked = true;
                        break;
                    case SongSelectFilter.SortField.UpdateDate:
                        updateDateRadio.Checked = true;
                        break;
                    case SongSelectFilter.SortField.BPM:
                        bpmRadio.Checked = true;
                        break;
                    case SongSelectFilter.SortField.Author:
                        authorRadio.Checked = true;
                        break;
                }
            }
        }

        public bool Desc
        {
            get
            {
                return descRadio.Checked;
            }
            set
            {
                descRadio.Checked = value;
                ascRadio.Checked = !value;
            }
        }

        public SongInformation.AvailableDifficulty Difficulty
        {
            get
            {
                SongInformation.AvailableDifficulty ret = SongInformation.AvailableDifficulty.None;
                if (easyCheck.Checked)
                {
                    ret |= SongInformation.AvailableDifficulty.Easy;
                }
                if (normalCheck.Checked)
                {
                    ret |= SongInformation.AvailableDifficulty.Normal;
                }
                if (hardCheck.Checked)
                {
                    ret |= SongInformation.AvailableDifficulty.Hard;
                }
                if (extremeCheck.Checked)
                {
                    ret |= SongInformation.AvailableDifficulty.Extreme;
                }
                return ret;
            }
            set
            {
                easyCheck.Checked = value.HasFlag(SongInformation.AvailableDifficulty.Easy);
                normalCheck.Checked = value.HasFlag(SongInformation.AvailableDifficulty.Normal);
                hardCheck.Checked = value.HasFlag(SongInformation.AvailableDifficulty.Hard);
                extremeCheck.Checked = value.HasFlag(SongInformation.AvailableDifficulty.Extreme);
            }
        }

        public SongSelectFilter.ScoreType ScoreType
        {
            get
            {
                SongSelectFilter.ScoreType ret = SongSelectFilter.ScoreType.None;
                if (normalScoreCheck.Checked)
                {
                    ret |= SongSelectFilter.ScoreType.Normal;
                }
                if (acScoreCheck.Checked)
                {
                    ret |= SongSelectFilter.ScoreType.AC;
                }
                if (acftScoreCheck.Checked)
                {
                    ret |= SongSelectFilter.ScoreType.ACFT;
                }
                return ret;
            }
            set
            {
                normalScoreCheck.Checked = value.HasFlag(SongSelectFilter.ScoreType.Normal);
                acScoreCheck.Checked = value.HasFlag(SongSelectFilter.ScoreType.AC);
                acftScoreCheck.Checked = value.HasFlag(SongSelectFilter.ScoreType.ACFT);
            }
        }

        public FilterControl(PPDDevice device, PPDFramework.Resource.ResourceManager resourceManager, ISound sound, SongSelectFilter filter) : base(device)
        {
            this.sound = sound;
            this.filter = filter;
            selection = new GridSelection();

            PictureObject back;

            var mainSprite = new SpriteObject(device);
            mainSprite.AddChild(new TextureString(device, Utility.Language["SortField"], 22, PPDColors.White)
            {
                Position = new Vector2(50, 80)
            });
            var sortFieldSprite = new SpriteObject(device);
            this.AddChild(sortFieldSprite);
            sortFieldSprite.AddChild(nameRadio = new RadioBoxComponent(device, resourceManager, Utility.Path, Utility.Language["Name"])
            {
                Position = new Vector2(80, 120),
                Selected = true
            });
            nameRadio.Update();
            sortFieldSprite.AddChild(timeRadio = new RadioBoxComponent(device, resourceManager, Utility.Path, Utility.Language["Time"])
            {
                Position = new Vector2(nameRadio.Position.X + nameRadio.Width + Margin, 120)
            });
            timeRadio.Update();
            sortFieldSprite.AddChild(updateDateRadio = new RadioBoxComponent(device, resourceManager, Utility.Path, Utility.Language["UpdateDate"])
            {
                Position = new Vector2(timeRadio.Position.X + timeRadio.Width + Margin, 120)
            });
            updateDateRadio.Update();
            sortFieldSprite.AddChild(bpmRadio = new RadioBoxComponent(device, resourceManager, Utility.Path, Utility.Language["BPM"])
            {
                Position = new Vector2(updateDateRadio.Position.X + updateDateRadio.Width + Margin, 120)
            });
            bpmRadio.Update();
            sortFieldSprite.AddChild(authorRadio = new RadioBoxComponent(device, resourceManager, Utility.Path, Utility.Language["Author"])
            {
                Position = new Vector2(bpmRadio.Position.X + bpmRadio.Width + Margin, 120)
            });

            mainSprite.AddChild(new TextureString(device, Utility.Language["SortOrder"], 22, PPDColors.White)
            {
                Position = new Vector2(50, 160)
            });
            var sortOrderSprite = new SpriteObject(device);
            this.AddChild(sortOrderSprite);
            sortOrderSprite.AddChild(ascRadio = new RadioBoxComponent(device, resourceManager, Utility.Path, Utility.Language["Ascendant"])
            {
                Position = new Vector2(80, 200)
            });
            nameRadio.Update();
            sortOrderSprite.AddChild(descRadio = new RadioBoxComponent(device, resourceManager, Utility.Path, Utility.Language["Descendant"])
            {
                Position = new Vector2(ascRadio.Position.X + ascRadio.Width + Margin, 200)
            });

            mainSprite.AddChild(new TextureString(device, Utility.Language["Type"], 22, PPDColors.White)
            {
                Position = new Vector2(50, 240)
            });
            mainSprite.AddChild(easyCheck = new CheckBoxComponent(device, resourceManager, Utility.Path, "EASY")
            {
                Position = new Vector2(80, 280)
            });
            easyCheck.Update();
            mainSprite.AddChild(normalCheck = new CheckBoxComponent(device, resourceManager, Utility.Path, "NORMAL")
            {
                Position = new Vector2(easyCheck.Position.X + easyCheck.Width + Margin, 280)
            });
            normalCheck.Update();
            mainSprite.AddChild(hardCheck = new CheckBoxComponent(device, resourceManager, Utility.Path, "HARD")
            {
                Position = new Vector2(normalCheck.Position.X + normalCheck.Width + Margin, 280)
            });
            hardCheck.Update();
            mainSprite.AddChild(extremeCheck = new CheckBoxComponent(device, resourceManager, Utility.Path, "EXTREME")
            {
                Position = new Vector2(hardCheck.Position.X + hardCheck.Width + Margin, 280)
            });
            mainSprite.AddChild(normalScoreCheck = new CheckBoxComponent(device, resourceManager, Utility.Path, Utility.Language["NormalScore"])
            {
                Position = new Vector2(80, 320)
            });
            normalScoreCheck.Update();
            mainSprite.AddChild(acScoreCheck = new CheckBoxComponent(device, resourceManager, Utility.Path, Utility.Language["ACScore"])
            {
                Position = new Vector2(normalScoreCheck.Position.X + normalScoreCheck.Width + Margin, 320)
            });
            acScoreCheck.Update();
            mainSprite.AddChild(acftScoreCheck = new CheckBoxComponent(device, resourceManager, Utility.Path, Utility.Language["ACFTScore"])
            {
                Position = new Vector2(acScoreCheck.Position.X + acScoreCheck.Width + Margin, 320)
            });

            mainSprite.AddChild(setDefaultButton = new ButtonComponent(device, resourceManager, Utility.Path, Utility.Language["SetDefault"])
            {
                Position = new Vector2(300, 370)
            });

            selectList = new SelectableComponent[]{
                nameRadio,
                timeRadio,
                updateDateRadio,
                bpmRadio,
                authorRadio,
                ascRadio,
                descRadio,
                easyCheck,
                normalCheck,
                hardCheck,
                extremeCheck,
                normalScoreCheck,
                acScoreCheck,
                acftScoreCheck,
                setDefaultButton
            };
            foreach (SelectableComponent comp in selectList)
            {
                selection.Add(comp.Position);
            }

            this.AddChild(mainSprite);
            this.AddChild(back = new PictureObject(device, resourceManager, Utility.Path.Combine("dialog_back.png")));
            this.AddChild(new RectangleComponent(device, resourceManager, PPDColors.Black)
            {
                RectangleHeight = 450,
                RectangleWidth = 800,
                Alpha = 0.65f
            });
            back.AddChild(new TextureString(device, Utility.Language["Filter"], 30, PPDColors.White)
            {
                Position = new Vector2(35, 30)
            });

            Inputed += FilterControl_Inputed;
            GotFocused += FilterControl_GotFocused;
            LostFocused += FilterControl_LostFocused;
        }

        void FilterControl_LostFocused(IFocusable sender, FocusEventArgs args)
        {
            bool changed = false;
            if (filter.Difficulty != Difficulty)
            {
                filter.Difficulty = Difficulty;
                changed = true;
            }
            if (filter.Field != SortField)
            {
                filter.Field = SortField;
                changed = true;
            }
            if (filter.Desc != Desc)
            {
                filter.Desc = Desc;
                changed = true;
            }
            if (filter.Type != ScoreType)
            {
                filter.Type = ScoreType;
                changed = true;
            }

            if (changed)
            {
                Changed?.Invoke();
            }
        }

        void FilterControl_GotFocused(IFocusable sender, FocusEventArgs args)
        {
            if (!initialized)
            {
                Initialize();
                initialized = true;
            }
        }

        void FilterControl_Inputed(IFocusable sender, InputEventArgs args)
        {
            if (args.InputInfo.IsPressed(ButtonType.Cross))
            {
                FocusManager.RemoveFocus();
                sound.Play(PPDSetting.DefaultSounds[2], -1000);
            }
            else if (args.InputInfo.IsPressed(ButtonType.Circle))
            {
                if (CurrentSelectedComponent == setDefaultButton)
                {
                    SetFlagDefault();
                    sound.Play(PPDSetting.DefaultSounds[1], -1000);
                }
                else
                {
                    CurrentSelectedComponent.Checked = !CurrentSelectedComponent.Checked;
                    sound.Play(PPDSetting.DefaultSounds[3], -1000);
                }
            }
            else if (args.InputInfo.IsPressed(ButtonType.Down))
            {
                CurrentSelectedComponent.Selected = false;
                selection.Down();
                CurrentSelectedComponent.Selected = true;
                sound.Play(PPDSetting.DefaultSounds[0], -1000);
            }
            else if (args.InputInfo.IsPressed(ButtonType.Up))
            {
                CurrentSelectedComponent.Selected = false;
                selection.Up();
                CurrentSelectedComponent.Selected = true;
                sound.Play(PPDSetting.DefaultSounds[0], -1000);
            }
            else if (args.InputInfo.IsPressed(ButtonType.Left))
            {
                CurrentSelectedComponent.Selected = false;
                selection.Left();
                CurrentSelectedComponent.Selected = true;
                sound.Play(PPDSetting.DefaultSounds[0], -1000);
            }
            else if (args.InputInfo.IsPressed(ButtonType.Right))
            {
                CurrentSelectedComponent.Selected = false;
                selection.Right();
                CurrentSelectedComponent.Selected = true;
                sound.Play(PPDSetting.DefaultSounds[0], -1000);
            }
        }

        private void SetFlagDefault()
        {
            nameRadio.Checked = ascRadio.Checked = easyCheck.Checked = normalCheck.Checked =
                hardCheck.Checked = extremeCheck.Checked = normalScoreCheck.Checked = acScoreCheck.Checked = acftScoreCheck.Checked = true;
        }

        private void Initialize()
        {
            SortField = filter.Field;
            Desc = filter.Desc;
            Difficulty = filter.Difficulty;
            ScoreType = filter.Type;
        }

        protected override bool OnCanUpdate()
        {
            return !Hidden && OverFocused;
        }

        protected override bool OnCanDraw(PPDFramework.Shaders.AlphaBlendContext alphaBlendContext, int depth, int childIndex)
        {
            return OverFocused;
        }
    }
}
