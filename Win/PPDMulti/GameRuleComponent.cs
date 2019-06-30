using PPDFramework;
using PPDMultiCommon.Data;
using PPDMultiCommon.Model;
using PPDShareComponent;
using SharpDX;
using System;

namespace PPDMulti
{
    class GameRuleComponent : FocusableGameComponent
    {
        public event EventHandler RuleChanged;

        PPDFramework.Resource.ResourceManager resourceManager;
        ISound sound;
        PictureObject back;

        SpriteObject ruleSprite;
        SpriteObject itemSupplyTypeSprite;
        GridSelection gridSelection;
        SelectableComponent[] selectList;

        ListBoxComponent resultSort;
        CheckBoxComponent itemAvailable;
        RadioBoxComponent comboSupply;
        RadioBoxComponent rankSupply;
        ListBoxComponent comboSupplyCount;
        ListBoxComponent worstSupplyCount;
        ListBoxComponent maxItemCount;
        ButtonComponent okButton;
        ButtonComponent cancelButton;

        GameRule gameRule = new GameRule();

        private SelectableComponent CurrentSelectedComponent
        {
            get
            {
                return selectList[gridSelection.Current];
            }
        }

        public GameRule GameRule
        {
            get
            {
                return gameRule;
            }
        }

        public GameRuleComponent(PPDDevice device, PPDFramework.Resource.ResourceManager resourceManager, ISound sound) : base(device)
        {
            this.resourceManager = resourceManager;
            this.sound = sound;

            this.AddChild(back = new PictureObject(device, resourceManager, Utility.Path.Combine("dialog_back.png")));
            this.AddChild(new RectangleComponent(device, resourceManager, PPDColors.Black)
            {
                RectangleHeight = 450,
                RectangleWidth = 800,
                Alpha = 0.75f
            });

            back.AddChild(ruleSprite = new SpriteObject(device));
            ruleSprite.AddChild(itemSupplyTypeSprite = new SpriteObject(device));
            back.AddChild(new TextureString(device, Utility.Language["ChangeRule"], 30, PPDColors.White)
            {
                Position = new Vector2(35, 30)
            });

            ruleSprite.AddChild(resultSort = new ListBoxComponent(device, resourceManager, Utility.Path, Utility.Language["RankOrder"], Utility.Language["Score"], Utility.Language["Accuracy"])
            {
                Position = new SharpDX.Vector2(80, 80),
                Selected = true
            });
            ruleSprite.AddChild(itemAvailable = new CheckBoxComponent(device, resourceManager, Utility.Path, Utility.Language["ItemAvailable"])
            {
                Position = new SharpDX.Vector2(50, 120)
            });
            ruleSprite.AddChild(okButton = new ButtonComponent(device, resourceManager, Utility.Path, Utility.Language["OK"])
            {
                Position = new Vector2(270, 380)
            });
            ruleSprite.AddChild(cancelButton = new ButtonComponent(device, resourceManager, Utility.Path, Utility.Language["Cancel"])
            {
                Position = new Vector2(470, 380)
            });
            ruleSprite.AddChild(maxItemCount = new ListBoxComponent(device, resourceManager, Utility.Path, Utility.Language["MaxItemCount"], 1, 2, 3, 4, 5, 6)
            {
                Position = new Vector2(110, 160)
            });
            itemSupplyTypeSprite.AddChild(comboSupply = new RadioBoxComponent(device, resourceManager, Utility.Path, Utility.Language["DependentOnComboOrWorst"])
            {
                Position = new Vector2(80, 200)
            });
            ruleSprite.AddChild(comboSupplyCount = new ListBoxComponent(device, resourceManager, Utility.Path, Utility.Language["ItemPerCombo"], 50, 60, 70, 80, 90, 100)
            {
                Position = new Vector2(130, 240)
            });
            ruleSprite.AddChild(worstSupplyCount = new ListBoxComponent(device, resourceManager, Utility.Path, Utility.Language["ItemPerWorst"], 50, 60, 70, 80, 90, 100)
            {
                Position = new Vector2(130, 280)
            });
            itemSupplyTypeSprite.AddChild(rankSupply = new RadioBoxComponent(device, resourceManager, Utility.Path, Utility.Language["DependentOnRank"])
            {
                Position = new Vector2(80, 320)
            });

            selectList = new SelectableComponent[]{
                resultSort,
                itemAvailable,
                maxItemCount,
                comboSupply,
                comboSupplyCount,
                worstSupplyCount,
                rankSupply,
                okButton,
                cancelButton
            };
            gridSelection = new GridSelection();
            foreach (SelectableComponent comp in selectList)
            {
                gridSelection.Add(comp.Position);
            }
            resultSort.SelectedItem = Utility.Language[gameRule.ResultSortType.ToString()];
            itemAvailable.Checked = gameRule.ItemAvailable;
            comboSupply.Checked = gameRule.ItemSupplyType == ItemSupplyType.ComboWorstCount;
            rankSupply.Checked = gameRule.ItemSupplyType == ItemSupplyType.Rank;
            comboSupplyCount.SelectedItem = gameRule.ItemSupplyComboCount;
            worstSupplyCount.SelectedItem = gameRule.ItemSupplyWorstCount;
            maxItemCount.SelectedItem = gameRule.MaxItemCount;

            Inputed += ItemSettingComponent_Inputed;
            GotFocused += ItemSettingComponent_GotFocused;
        }

        void ItemSettingComponent_GotFocused(IFocusable sender, FocusEventArgs args)
        {
            back.Position = new SharpDX.Vector2(0, 50);
            Alpha = 0;
        }

        void ItemSettingComponent_Inputed(IFocusable sender, InputEventArgs args)
        {
            if (args.InputInfo.IsPressed(ButtonType.Cross))
            {
                sound.Play(PPDSetting.DefaultSounds[2], -1000);
                FocusManager.RemoveFocus();
            }
            else if (args.InputInfo.IsPressed(ButtonType.Down))
            {
                if (!CurrentSelectedComponent.Down())
                {
                    CurrentSelectedComponent.Selected = false;
                    gridSelection.Down();
                    CurrentSelectedComponent.Selected = true;
                    sound.Play(PPDSetting.DefaultSounds[0], -1000);
                }
                else
                {
                    sound.Play(PPDSetting.DefaultSounds[3], -1000);
                }
            }
            else if (args.InputInfo.IsPressed(ButtonType.Up))
            {
                if (!CurrentSelectedComponent.Up())
                {
                    CurrentSelectedComponent.Selected = false;
                    gridSelection.Up();
                    CurrentSelectedComponent.Selected = true;
                    sound.Play(PPDSetting.DefaultSounds[0], -1000);
                }
                else
                {
                    sound.Play(PPDSetting.DefaultSounds[3], -1000);
                }
            }
            else if (args.InputInfo.IsPressed(ButtonType.Left))
            {
                if (!CurrentSelectedComponent.Left())
                {
                    CurrentSelectedComponent.Selected = false;
                    gridSelection.Left();
                    CurrentSelectedComponent.Selected = true;
                    sound.Play(PPDSetting.DefaultSounds[0], -1000);
                }
                else
                {
                    sound.Play(PPDSetting.DefaultSounds[3], -1000);
                }
            }
            else if (args.InputInfo.IsPressed(ButtonType.Right))
            {
                if (!CurrentSelectedComponent.Right())
                {
                    CurrentSelectedComponent.Selected = false;
                    gridSelection.Right();
                    CurrentSelectedComponent.Selected = true;
                    sound.Play(PPDSetting.DefaultSounds[0], -1000);
                }
                else
                {
                    sound.Play(PPDSetting.DefaultSounds[3], -1000);
                }
            }
            else if (args.InputInfo.IsPressed(ButtonType.Circle))
            {
                if (CurrentSelectedComponent == okButton)
                {
                    SetRule();
                    if (RuleChanged != null)
                    {
                        RuleChanged.Invoke(this, EventArgs.Empty);
                    }
                    sound.Play(PPDSetting.DefaultSounds[1], -1000);
                }
                else if (CurrentSelectedComponent == cancelButton)
                {
                    FocusManager.RemoveFocus();
                }
                else
                {
                    CurrentSelectedComponent.Checked = !CurrentSelectedComponent.Checked;
                    sound.Play(PPDSetting.DefaultSounds[3], -1000);
                }
            }
        }

        private void SetRule()
        {
            gameRule.ResultSortType = (string)resultSort.SelectedItem == Utility.Language[ResultSortType.Score.ToString()] ? ResultSortType.Score : ResultSortType.Accuracy;
            gameRule.ItemAvailable = itemAvailable.Checked;
            gameRule.ItemSupplyType = comboSupply.Checked ? ItemSupplyType.ComboWorstCount : ItemSupplyType.Rank;
            gameRule.MaxItemCount = (int)maxItemCount.SelectedItem;
            gameRule.ItemSupplyComboCount = (int)comboSupplyCount.SelectedItem;
            gameRule.ItemSupplyWorstCount = (int)worstSupplyCount.SelectedItem;
        }

        protected override void UpdateImpl()
        {
            if (OverFocused)
            {
                back.Position = new SharpDX.Vector2(0, AnimationUtility.GetAnimationValue(back.Position.Y, 0));
                Alpha = AnimationUtility.IncreaseAlpha(Alpha);
            }
            else
            {
                back.Position = new SharpDX.Vector2(0, AnimationUtility.GetAnimationValue(back.Position.Y, 50));
                Alpha = AnimationUtility.DecreaseAlpha(Alpha);
            }
        }
    }
}