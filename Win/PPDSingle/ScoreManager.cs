using PPDFramework;
using PPDShareComponent;
using SharpDX;
using System;
using System.Collections.Generic;

namespace PPDSingle
{
    class ScoreManager : FocusableGameComponent
    {
        private string createlinkmenu;
        private string createfoldermenu;
        private string cutmenu;
        private string copymenu;
        private string pastemenu;
        private string deletemenu;
        private string renamemenu;

        enum FocusPanel
        {
            Left = 0,
            Right = 1
        }

        struct TreeInfo
        {
            public int Selection;
            public int Scroll;
        }

        FocusPanel focusPanel = FocusPanel.Left;

        ISound sound;
        DxTextBox textBox;
        SongSelectFilter filter;

        PictureObject back;
        RectangleComponent black;
        PictureObject folder;
        PictureObject score;
        PictureObject Lw;
        PictureObject Rw;
        PictureObject Lb;
        PictureObject Rb;
        RectangleComponent songInfoSelection;
        RectangleComponent logicInfoSelection;

        const int siStartX = 35;
        const int siStartY = 85;

        const int lfStartX = 400;
        const int lfStartY = 85;

        const int gapX = 30;
        const int gapY = 4;
        const int maxwidth = 350;

        const int maxHeightNumber = 17;
        TreeInfo leftInfo;
        TreeInfo rightInfo;

        bool first = true;
        TreeView siTreeView;
        TreeView lfTreeView;

        ContextMenu cm;

        LogicFolderTreeViewItem cutOrCopylftvi;
        bool cut;

        bool swap;
        LogicFolderTreeViewItem swaplftvi;

        public ScoreManager(PPDDevice device, PPDFramework.Resource.ResourceManager resourceManager, DxTextBox textBox, ISound sound, SongSelectFilter filter) : base(device)
        {
            this.sound = sound;
            this.textBox = textBox;
            this.filter = filter;

            back = new PictureObject(device, resourceManager, Utility.Path.Combine("scoremanager", "back.png"));

            folder = new PictureObject(device, resourceManager, Utility.Path.Combine("scoremanager", "folder.png"));

            score = new PictureObject(device, resourceManager, Utility.Path.Combine("scoremanager", "score.png"));

            Lw = new PictureObject(device, resourceManager, Utility.Path.Combine("scoremanager", "lw.png"))
            {
                Position = new Vector2(34, 57)
            };

            Lb = new PictureObject(device, resourceManager, Utility.Path.Combine("scoremanager", "lb.png"))
            {
                Position = new Vector2(34, 57)
            };

            Rw = new PictureObject(device, resourceManager, Utility.Path.Combine("scoremanager", "rw.png"))
            {
                Position = new Vector2(720, 57)
            };

            Rb = new PictureObject(device, resourceManager, Utility.Path.Combine("scoremanager", "rb.png"))
            {
                Position = new Vector2(720, 57)
            };

            songInfoSelection = new RectangleComponent(device, resourceManager, PPDColors.White);
            logicInfoSelection = new RectangleComponent(device, resourceManager, PPDColors.White);

            black = new RectangleComponent(device, resourceManager, PPDColors.Black)
            {
                RectangleHeight = 450,
                RectangleWidth = 800,
                Alpha = 0.65f
            };

            this.AddChild(new TextureString(device, Utility.Language["ScoreManager"], 24, PPDColors.White)
            {
                Position = new Vector2(32, 25)
            });
            this.AddChild(new TextureString(device, Utility.Language["Move"], 16, PPDColors.White)
            {
                Position = new Vector2(70, 406)
            });
            this.AddChild(new TextureString(device, Utility.Language["SortInHolding"], 16, PPDColors.White)
            {
                Position = new Vector2(190, 406)
            });
            this.AddChild(new TextureString(device, Utility.Language["Menu"], 16, PPDColors.White)
            {
                Position = new Vector2(430, 406)
            });
            this.AddChild(new TextureString(device, Utility.Language["FolderExpandCollapse"], 16, PPDColors.White)
            {
                Position = new Vector2(545, 406)
            });
            this.AddChild(new TextureString(device, Utility.Language["Back"], 16, PPDColors.White)
            {
                Position = new Vector2(707, 406)
            });
            this.AddChild(back);
            this.AddChild(black);

            createlinkmenu = Utility.Language["CreateLink"];
            createfoldermenu = Utility.Language["CreateFolder"];
            cutmenu = Utility.Language["Cut"];
            copymenu = Utility.Language["Copy"];
            pastemenu = Utility.Language["Paste"];
            deletemenu = Utility.Language["Delete"];
            renamemenu = Utility.Language["Rename"];

            cm = new ContextMenu(device, resourceManager, sound);
            cm.Selected += cm_Selected;

            Inputed += ScoreManager_Inputed;

            SongInformation.Updated += SongInformation_Updated;
        }

        public void FilterChanged()
        {
            CreateInformation();
        }

        void SongInformation_Updated(object sender, EventArgs e)
        {
            CreateInformation();
        }

        void cm_Selected(object sender, EventArgs e)
        {
            if (sender is MenuItem menuItem)
            {
                if (menuItem.Name == createlinkmenu)
                {
                    CreateLink();
                }
                else if (menuItem.Name == createfoldermenu)
                {
                    CreateFolder();
                }
                else if (menuItem.Name == deletemenu)
                {
                    Delete();
                }
                else if (menuItem.Name == renamemenu)
                {
                    Rename();
                }
                else if (menuItem.Name == cutmenu)
                {
                    Cut();
                }
                else if (menuItem.Name == copymenu)
                {
                    Copy();
                }
                else if (menuItem.Name == pastemenu)
                {
                    Paste();
                }
            }
        }

        private void CreateLink()
        {
            if (siTreeView.SelectedItem == null) return;
            bool nodata = lfTreeView.ItemCount == 0;
            var lftvi = (lfTreeView.SelectedItem ?? lfTreeView.Root) as LogicFolderTreeViewItem;
            if (!lftvi.LogicFolderInfo.IsFolder)
            {
                lftvi = lftvi.Parent as LogicFolderTreeViewItem;
            }
            var sitvi = siTreeView.SelectedItem as SongInfoTreeViewItem;

            if (sitvi.SongInformation.IsPPDSong)
            {
                var newinfo = lftvi.LogicFolderInfo.AddScore(sitvi.SongInformation);
                CreateAndAdd(newinfo, lftvi);
            }
            else
            {
                var folder = lftvi.LogicFolderInfo.AddFolder(sitvi.SongInformation.DirectoryName);
                var newlftvi = CreateAndAdd(folder, lftvi);
                RecursiveCopy(sitvi, newlftvi);
                lftvi.Expand();
            }
            lftvi.Expand();
            lftvi.Sort();
            if (nodata)
            {
                lfTreeView.Select(lfTreeView.Items[0]);
            }
        }

        private void RecursiveCopy(SongInfoTreeViewItem sitvi, LogicFolderTreeViewItem lftvi)
        {
            foreach (SongInfoTreeViewItem childsitvi in sitvi.Items)
            {
                if (!childsitvi.SongInformation.IsPPDSong)
                {
                    var folder = lftvi.LogicFolderInfo.AddFolder(childsitvi.SongInformation.DirectoryName);
                    var newlftvi = CreateAndAdd(folder, lftvi);
                    RecursiveCopy(childsitvi, newlftvi);
                }
                else
                {
                    var newinfo = lftvi.LogicFolderInfo.AddScore(childsitvi.SongInformation);
                    CreateAndAdd(newinfo, lftvi);
                }
            }
            lftvi.Sort();
        }

        private void CreateFolder()
        {
            if (siTreeView.SelectedItem == null) return;
            bool nodata = lfTreeView.ItemCount == 0;
            var lftvi = (lfTreeView.SelectedItem ?? lfTreeView.Root) as LogicFolderTreeViewItem;
            if (!lftvi.LogicFolderInfo.IsFolder)
            {
                lftvi = lftvi.Parent as LogicFolderTreeViewItem;
            }
            string foldername = Utility.Language["NewFolder"];
            var folder = lftvi.LogicFolderInfo.AddFolder(foldername);
            var newlftvi = CreateAndAdd(folder, lftvi);
            lftvi.Expand();
            lftvi.Sort();
            if (nodata)
            {
                lfTreeView.Select(lfTreeView.Items[0]);
            }
            LFScroll();

            textBox.LostFocused += CreateFolderEvent;
            ShowTextBox(newlftvi);
        }

        void CreateFolderEvent(IFocusable sender, FocusEventArgs args)
        {
            textBox.LostFocused -= CreateFolderEvent;
            if (textBox.Text != string.Empty)
            {
                if (lfTreeView.SelectedItem is LogicFolderTreeViewItem lftvi)
                {
                    string newText = this.textBox.Text;
                    lftvi.LogicFolderInfo.Rename(newText);
                    lftvi.TextureString.Text = newText;
                    if (lftvi.Parent != null)
                    {
                        lftvi.Parent.Sort();
                    }
                }
            }
        }

        private void Delete()
        {
            var lftvi = lfTreeView.SelectedItem as LogicFolderTreeViewItem;
            if (lftvi == null) return;

            lftvi.Remove();
            lftvi.LogicFolderInfo.Remove();
        }

        private void Rename()
        {
            var lftvi = lfTreeView.SelectedItem as LogicFolderTreeViewItem;
            if (lftvi == null) return;

            textBox.LostFocused += RenameEvent;
            ShowTextBox(lftvi);
        }

        private void Cut()
        {
            if (lfTreeView.SelectedItem == null) return;
            cutOrCopylftvi = lfTreeView.SelectedItem as LogicFolderTreeViewItem;
            cut = true;
        }

        private void Copy()
        {
            if (lfTreeView.SelectedItem == null) return;
            cutOrCopylftvi = lfTreeView.SelectedItem as LogicFolderTreeViewItem;
            cut = false;
        }

        private void Paste()
        {
            var target = (lfTreeView.SelectedItem ?? lfTreeView.Root) as LogicFolderTreeViewItem;
            if (!target.LogicFolderInfo.IsFolder)
            {
                target = target.Parent as LogicFolderTreeViewItem;
            }
            if (cutOrCopylftvi != null && target != null && !cutOrCopylftvi.LogicFolderInfo.ContainAsChildrenOrSelf(target.LogicFolderInfo))
            {
                if (cut)
                {
                    if (target != cutOrCopylftvi.Parent)
                    {
                        lfTreeView.Select(Copy(cutOrCopylftvi, target));
                        cutOrCopylftvi.Remove();
                        cutOrCopylftvi.LogicFolderInfo.Remove();
                        cutOrCopylftvi = null;
                        target.Expand();
                    }
                }
                else
                {
                    lfTreeView.Select(Copy(cutOrCopylftvi, target));
                    target.Expand();
                }
            }
        }


        private LogicFolderTreeViewItem Copy(LogicFolderTreeViewItem src, LogicFolderTreeViewItem dest)
        {
            if (src.LogicFolderInfo.IsFolder)
            {
                var newFolder = dest.LogicFolderInfo.AddFolder(src.LogicFolderInfo.Name);
                var newlftvi = CreateAndAdd(newFolder, dest);
                dest.Sort();
                foreach (LogicFolderTreeViewItem childlftvi in src.Items)
                {
                    Copy(childlftvi, newlftvi);
                }
                return newlftvi;
            }
            else
            {
                var newScore = dest.LogicFolderInfo.AddScore(SongInformation.FindSongInformationByID(src.LogicFolderInfo.ScoreID), src.LogicFolderInfo.Name);
                var newlftvi = CreateAndAdd(newScore, dest);
                dest.Sort();
                return newlftvi;
            }
        }

        private bool ChangeIndex(LogicFolderTreeViewItem swaplftvi, bool isUp)
        {
            if (swaplftvi == null || swaplftvi.Parent == null) return false;
            var index = swaplftvi.Parent.Items.IndexOf(swaplftvi);
            if (isUp)
            {
                if (index > 0)
                {
                    swaplftvi.Parent.Items[index] = swaplftvi.Parent.Items[index - 1];
                    swaplftvi.Parent.Items[index - 1] = swaplftvi;
                    return true;
                }
            }
            else
            {
                if (index < swaplftvi.Parent.ItemCount - 1)
                {
                    swaplftvi.Parent.Items[index] = swaplftvi.Parent.Items[index + 1];
                    swaplftvi.Parent.Items[index + 1] = swaplftvi;
                    return true;
                }
            }
            return false;
        }

        private void ChangeIndex(LogicFolderTreeViewItem swap)
        {
            if (swap == null || swap == lfTreeView.Root) return;
            var parent = swap.Parent as LogicFolderTreeViewItem;
            var currentIndex = parent.Items.IndexOf(swap);
            swap.LogicFolderInfo.ChangeIndex(currentIndex);
            parent.Sort();
            lfTreeView.Select(swap);
        }

        private void ShowTextBox(LogicFolderTreeViewItem lftvi)
        {
            textBox.DrawOnlyFocus = true;
            textBox.DrawMode = DxTextBox.DrawingMode.DrawAll;
            textBox.Text = lftvi.TextureString.Text;
            textBox.Position = new Vector2(lftvi.TextureString.Position.X, lftvi.TextureString.Position.Y);
            textBox.MaxWidth = textBox.TextBoxWidth = Math.Max(300, (int)lftvi.TextureString.MaxWidth);
            textBox.TextBoxHeight = 14;
            FocusManager.Focus(textBox);
        }

        void RenameEvent(IFocusable sender, FocusEventArgs args)
        {
            textBox.LostFocused -= RenameEvent;
            if (textBox.Text != string.Empty)
            {
                if (lfTreeView.SelectedItem is LogicFolderTreeViewItem lftvi)
                {
                    string newText = this.textBox.Text;
                    lftvi.LogicFolderInfo.Rename(newText);
                    lftvi.TextureString.Text = newText;
                    if (lftvi.Parent != null)
                    {
                        lftvi.Parent.Sort();
                    }
                }
            }
        }

        void ScoreManager_Inputed(IFocusable sender, InputEventArgs args)
        {
            if (args.InputInfo.IsPressed(ButtonType.Circle))
            {
                ProcessCircleInput();
            }
            else if (args.InputInfo.IsPressed(ButtonType.Cross))
            {
                FocusManager.RemoveFocus();
                sound.Play(PPDSetting.DefaultSounds[2], -1000);
            }
            else if (args.InputInfo.IsPressed(ButtonType.Triangle))
            {
                if (lfTreeView.SelectedItem == null)
                {
                    foreach (MenuItem menuItem in cm.Menus)
                    {
                        menuItem.Enabled = menuItem.Name == createlinkmenu || menuItem.Name == createfoldermenu || (menuItem.Name == pastemenu && cutOrCopylftvi != null);
                    }
                    cm.Position = new Vector2(500, 200);
                }
                else
                {
                    foreach (MenuItem menuItem in cm.Menus)
                    {
                        if (menuItem.Name == pastemenu && cutOrCopylftvi == null)
                        {
                            menuItem.Enabled = false;
                        }
                        else
                        {
                            menuItem.Enabled = true;
                        }
                    }
                    var lftvi = lfTreeView.SelectedItem as LogicFolderTreeViewItem;
                    var pos = new Vector2(lftvi.TextureString.Position.X, lftvi.TextureString.Position.Y + lftvi.TextureString.CharacterHeight + 20);
                    if (cm.CheckPositionX(pos) >= 0)
                    {
                        if (cm.CheckPositionY(pos) >= 0)
                        {
                        }
                        else
                        {
                            pos.Y = lftvi.TextureString.Position.Y - cm.Height;
                        }
                    }
                    else
                    {
                        if (cm.CheckPositionY(pos) >= 0)
                        {
                            pos.X = pos.X + cm.CheckPositionX(pos);
                        }
                        else
                        {
                            pos = new Vector2(pos.X + cm.CheckPositionX(pos), lftvi.TextureString.Position.Y - cm.Height);
                        }
                    }
                    cm.Position = pos;
                }
                FocusManager.Focus(cm);
            }
            else if (args.InputInfo.IsPressed(ButtonType.Square))
            {
                swap = true;
                swaplftvi = lfTreeView.SelectedItem as LogicFolderTreeViewItem;
            }
            else if (args.InputInfo.IsPressed(ButtonType.R))
            {
                focusPanel = FocusPanel.Right;
                sound.Play(PPDSetting.DefaultSounds[3], -1000);
            }
            else if (args.InputInfo.IsPressed(ButtonType.L))
            {
                focusPanel = FocusPanel.Left;
                sound.Play(PPDSetting.DefaultSounds[3], -1000);
            }
            else if (args.InputInfo.IsPressed(ButtonType.Up))
            {
                if (ProcessUpInput())
                    sound.Play(PPDSetting.DefaultSounds[0], -1000);
            }
            else if (args.InputInfo.IsPressed(ButtonType.Down))
            {
                if (ProcessDownInput())
                    sound.Play(PPDSetting.DefaultSounds[0], -1000);
            }
            else if (args.InputInfo.IsPressed(ButtonType.Left))
            {
                bool ok = false;
                for (int i = 0; i < maxHeightNumber / 2; i++)
                {
                    ok |= ProcessUpInput();
                }
                if (ok)
                    sound.Play(PPDSetting.DefaultSounds[0], -1000);
            }
            else if (args.InputInfo.IsPressed(ButtonType.Right))
            {
                bool ok = false;
                for (int i = 0; i < maxHeightNumber / 2; i++)
                {
                    ok |= ProcessDownInput();
                }
                if (ok)
                    sound.Play(PPDSetting.DefaultSounds[0], -1000);
            }

            if (args.InputInfo.IsReleased(ButtonType.Square))
            {
                if (swap)
                {
                    ChangeIndex(swaplftvi);
                    swap = false;
                    swaplftvi = null;
                }
            }
        }

        private void ProcessCircleInput()
        {
            switch (focusPanel)
            {
                case FocusPanel.Left:
                    if (siTreeView.SelectedItem != null)
                    {
                        if (siTreeView.SelectedItem is SongInfoTreeViewItem tvi && !tvi.SongInformation.IsPPDSong)
                        {
                            tvi.ExpandOrShrink();
                        }
                    }
                    break;
                case FocusPanel.Right:
                    if (lfTreeView.SelectedItem != null)
                    {
                        if (lfTreeView.SelectedItem is LogicFolderTreeViewItem tvi && tvi.LogicFolderInfo.IsFolder)
                        {
                            tvi.ExpandOrShrink();
                        }
                    }
                    break;
            }
        }

        private bool ProcessUpInput()
        {
            switch (focusPanel)
            {
                case FocusPanel.Left:
                    return siTreeView.MoveToPrevious();
                case FocusPanel.Right:
                    if (swap)
                    {
                        return ChangeIndex(swaplftvi, true);
                    }
                    else if (!lfTreeView.MoveToPrevious())
                    {
                        if (lfTreeView.SelectedItem == null)
                        {
                            return false;
                        }
                        lfTreeView.Select(null);
                    }
                    return true;
                default:
                    return false;
            }
        }

        private bool ProcessDownInput()
        {
            switch (focusPanel)
            {
                case FocusPanel.Left:
                    return siTreeView.MoveToNext();
                case FocusPanel.Right:
                    if (swap)
                    {
                        return ChangeIndex(swaplftvi, false);
                    }
                    else if (!lfTreeView.MoveToNext())
                    {
                        if (lfTreeView.SelectedItem == null)
                        {
                            return false;
                        }
                        lfTreeView.Select(null);
                    }
                    return true;
                default:
                    return false;
            }
        }

        public override void Update()
        {
            if (Disposed || Hidden || !OverFocused) return;
            base.Update();
            cm.Update();
            int iter = 0;
            if (first)
            {
                first = false;
                CreateInformation();

                cm.AddMenu(createlinkmenu);
                cm.AddMenu(createfoldermenu);
                cm.AddMenu(cutmenu);
                cm.AddMenu(copymenu);
                cm.AddMenu(pastemenu);
                cm.AddMenu(deletemenu);
                cm.AddMenu(renamemenu);
            }

            iter = -leftInfo.Scroll;
            RecursiveUpdate(siTreeView.Items, ref iter);
            if (leftInfo.Selection < 0 || leftInfo.Selection >= maxHeightNumber)
            {
                leftInfo.Scroll += leftInfo.Selection < 0 ? -1 : 1;
                iter = -leftInfo.Scroll;
                RecursiveUpdate(siTreeView.Items, ref iter);
            }

            iter = -rightInfo.Scroll;
            LFRecursiveUpdate(lfTreeView.Items, ref iter);
            if (rightInfo.Selection < 0 || rightInfo.Selection >= maxHeightNumber)
            {
                rightInfo.Scroll += rightInfo.Selection < 0 ? -1 : 1;
                iter = -rightInfo.Scroll;
                LFRecursiveUpdate(lfTreeView.Items, ref iter);
            }
        }

        private void CreateInformation()
        {
            if (siTreeView != null)
            {
                RecursiveDispose(siTreeView.Items);
            }
            if (lfTreeView != null)
            {
                LFRecursiveDispose(lfTreeView.Items);
            }

            int iter = 0;
            siTreeView = new TreeView(new SongInfoTreeViewItem());
            siTreeView.SelectionChanged += treeview_SelectionChanged;
            RecursiveCreate(filter.GetFiltered(SongInformation.Root.Children), ref iter, siTreeView.Root);
            if (siTreeView.ItemCount > 0)
            {
                siTreeView.Select(siTreeView.Items[0]);
            }

            var lfroot = new LogicFolderTreeViewItem
            {
                LogicFolderInfo = LogicFolderInfomation.Root
            };
            lfTreeView = new TreeView(lfroot);
            lfTreeView.SelectionChanged += lfTreeView_SelectionChanged;
            iter = 0;
            LFRecursiveCreate(LogicFolderInfomation.Root.Children, ref iter, lfTreeView.Root);
            if (lfTreeView.ItemCount > 0)
            {
                lfTreeView.Select(lfTreeView.Items[0]);
            }
        }

        private void LFScroll()
        {
            int iter = -rightInfo.Scroll;
            LFRecursiveUpdate(lfTreeView.Items, ref iter);
            if (rightInfo.Selection < 0 || rightInfo.Selection >= maxHeightNumber)
            {
                rightInfo.Scroll += rightInfo.Selection < 0 ? -rightInfo.Selection : rightInfo.Selection - maxHeightNumber + 1;
                iter = -rightInfo.Scroll;
                LFRecursiveUpdate(lfTreeView.Items, ref iter);
            }
        }


        #region LOGICFOLDER

        void lfTreeView_SelectionChanged(object sender, SelectionEventArgs args)
        {
            var prev = args.Prev as LogicFolderTreeViewItem;
            var news = args.New as LogicFolderTreeViewItem;
            if (prev != null)
            {
                prev.TextureString.Color = PPDColors.White;
                prev.TextureString.AllowScroll = false;
            }
            if (news != null)
            {
                news.TextureString.Color = PPDColors.Black;
                news.TextureString.AllowScroll = true;
            }
        }

        private void LFRecursiveUpdate(List<TreeViewItem> sis, ref int iter)
        {
            foreach (LogicFolderTreeViewItem child in sis)
            {
                if (child == lfTreeView.SelectedItem)
                {
                    rightInfo.Selection = iter;
                }
                child.TextureString.Position = new Vector2(lfStartX + gapX * child.LogicFolderInfo.Depth, lfStartY + (14 + gapY) * iter);
                child.TextureString.Update();
                iter++;
                if (child.LogicFolderInfo.IsFolder && child.IsExpanded)
                {
                    LFRecursiveUpdate(child.Items, ref iter);
                }
            }
        }

        private void LFRecursiveCreate(LogicFolderInfomation[] lfis, ref int iter, TreeViewItem parent)
        {
            foreach (LogicFolderInfomation child in lfis)
            {
                var tvi = CreateAndAdd(child, parent);
                iter++;
                if (child.IsFolder)
                {
                    LFRecursiveCreate(child.Children, ref iter, tvi);
                }
            }
            parent.Sort();
        }

        private LogicFolderTreeViewItem CreateAndAdd(LogicFolderInfomation child, TreeViewItem parent)
        {
            var tvi = new LogicFolderTreeViewItem
            {
                TextureString = new TextureString(device, child.Name, 14, maxwidth - gapX * child.Depth, PPDColors.White)
                {
                    AllowScroll = false
                },
                LogicFolderInfo = child
            };
            parent.Add(tvi);
            return tvi;
        }

        private bool LFRecursiveDraw(List<TreeViewItem> sis, ref int iter)
        {
            foreach (LogicFolderTreeViewItem child in sis)
            {
                TextureString ts = child.TextureString;
                if (iter >= 0)
                {
                    if (child == lfTreeView.SelectedItem)
                    {
                        logicInfoSelection.RectangleHeight = ts.CharacterHeight + gapY;
                        logicInfoSelection.RectangleWidth = ts.JustWidth;
                        logicInfoSelection.Position = ts.Position;
                        logicInfoSelection.Update();
                        logicInfoSelection.Draw();
                    }
                    ts.Draw();
                    if (!child.LogicFolderInfo.IsFolder)
                    {
                        score.Position = new Vector2(ts.Position.X - 20, ts.Position.Y - 2);
                        score.Update();
                        score.Draw();
                    }
                    else
                    {
                        folder.Position = new Vector2(ts.Position.X - 20, ts.Position.Y);
                        folder.Update();
                        folder.Draw();
                    }
                    if (child == cutOrCopylftvi)
                    {
                        /*GraphicsUtility.DrawLine(new Vector2(ts.Position.X - 20, ts.Position.Y + ts.CharacterHeight + gapY),
                            new Vector2(ts.Position.X + ts.JustWidth, ts.Position.Y + ts.CharacterHeight + gapY),
                            PPDColors.White, 0.5f, cut ? GraphicsUtility.LineMode.Dashed : GraphicsUtility.LineMode.Solid, white);*/
                    }
                }
                iter++;
                if (iter >= maxHeightNumber)
                {
                    return false;
                }
                if (child.LogicFolderInfo.IsFolder && child.IsExpanded)
                {
                    if (!LFRecursiveDraw(child.Items, ref iter))
                    {
                        return false;
                    }
                }
            }
            return true;
        }

        private void LFRecursiveDispose(List<TreeViewItem> sis)
        {
            foreach (LogicFolderTreeViewItem child in sis)
            {
                child.TextureString.Dispose();
                child.TextureString = null;
                if (child.LogicFolderInfo.IsFolder)
                {
                    LFRecursiveDispose(child.Items);
                }
            }
        }
        #endregion

        #region SONGINFO

        void treeview_SelectionChanged(object sender, SelectionEventArgs args)
        {
            var prev = args.Prev as SongInfoTreeViewItem;
            var news = args.New as SongInfoTreeViewItem;
            if (prev != null)
            {
                prev.TextureString.Color = PPDColors.White;
                prev.TextureString.AllowScroll = false;
            }
            if (news != null)
            {
                news.TextureString.Color = PPDColors.Black;
                news.TextureString.AllowScroll = true;
            }
        }

        private void RecursiveUpdate(List<TreeViewItem> sis, ref int iter)
        {
            foreach (SongInfoTreeViewItem child in sis)
            {
                if (child == siTreeView.SelectedItem)
                {
                    leftInfo.Selection = iter;
                }
                child.TextureString.Position = new Vector2(siStartX + gapX * child.SongInformation.Depth, siStartY + (14 + gapY) * iter);
                child.TextureString.Update();
                iter++;
                if (!child.SongInformation.IsPPDSong && child.IsExpanded)
                {
                    RecursiveUpdate(child.Items, ref iter);
                }
            }
        }

        private void RecursiveCreate(SongInformation[] sis, ref int iter, TreeViewItem parent)
        {
            foreach (SongInformation child in sis)
            {
                var tvi = new SongInfoTreeViewItem
                {
                    TextureString = new TextureString(device, child.DirectoryName, 14, maxwidth - gapX * child.Depth, PPDColors.White)
                    {
                        AllowScroll = false
                    },
                    SongInformation = child
                };
                parent.Add(tvi);
                iter++;
                if (!child.IsPPDSong)
                {
                    RecursiveCreate(filter.GetFiltered(child.Children), ref iter, tvi);
                }
            }
        }

        private bool RecursiveDraw(List<TreeViewItem> sis, ref int iter)
        {
            foreach (SongInfoTreeViewItem child in sis)
            {
                TextureString ts = child.TextureString;
                if (iter >= 0)
                {
                    if (child == siTreeView.SelectedItem)
                    {
                        songInfoSelection.RectangleHeight = ts.CharacterHeight + gapY;
                        songInfoSelection.RectangleWidth = ts.JustWidth;
                        songInfoSelection.Position = ts.Position;
                        songInfoSelection.Update();
                        songInfoSelection.Draw();
                    }
                    ts.Draw();
                    if (child.SongInformation.IsPPDSong)
                    {
                        score.Position = new Vector2(ts.Position.X - 20, ts.Position.Y - 2);
                        score.Update();
                        score.Draw();
                    }
                    else
                    {
                        folder.Position = new Vector2(ts.Position.X - 20, ts.Position.Y);
                        folder.Update();
                        folder.Draw();
                    }
                }
                iter++;
                if (iter >= maxHeightNumber)
                {
                    return false;
                }
                if (!child.SongInformation.IsPPDSong && child.IsExpanded)
                {
                    if (!RecursiveDraw(child.Items, ref iter))
                    {
                        return false;
                    }
                }
            }
            return true;
        }

        private void RecursiveDispose(List<TreeViewItem> sis)
        {
            foreach (SongInfoTreeViewItem child in sis)
            {
                child.TextureString.Dispose();
                child.TextureString = null;
                if (!child.SongInformation.IsPPDSong)
                {
                    RecursiveDispose(child.Items);
                }
            }
        }

        #endregion

        protected override void AfterChildenDraw(PPDFramework.Shaders.AlphaBlendContext alphaBlendContext)
        {
            base.AfterChildenDraw(alphaBlendContext);
            switch (focusPanel)
            {
                case FocusPanel.Left:
                    Lw.Draw();
                    Rb.Draw();
                    break;
                case FocusPanel.Right:
                    Lb.Draw();
                    Rw.Draw();
                    break;
            }

            int iter = -leftInfo.Scroll;
            RecursiveDraw(siTreeView.Items, ref iter);
            iter = -rightInfo.Scroll;
            LFRecursiveDraw(lfTreeView.Items, ref iter);

            cm.Draw();
        }

        protected override bool OnCanDraw(PPDFramework.Shaders.AlphaBlendContext alphaBlendContext, int depth, int childIndex)
        {
            return OverFocused;
        }

        protected override void DisposeResource()
        {
            if (siTreeView != null)
            {
                RecursiveDispose(siTreeView.Items);
            }
            if (lfTreeView != null)
            {
                LFRecursiveDispose(lfTreeView.Items);
            }
            cm.Dispose();
            SongInformation.Updated -= SongInformation_Updated;
        }

        class SongInfoComponent : GameComponent
        {
            PictureObject folder;
            PictureObject score;
            RectangleComponent selection;
            TextureString text;
            int depth;

            SongInformation songInformation;

            public SongInformation SongInformation
            {
                get { return songInformation; }
                set
                {
                    if (songInformation != value)
                    {
                        songInformation = value;
                        UpdateData();
                    }
                }
            }

            public int Depth
            {
                get { return depth; }
                set
                {
                    if (depth != value)
                    {
                        depth = value;
                        UpdateDepth();
                    }
                }
            }

            public bool Selected
            {
                get { return !selection.Hidden; }
                set
                {
                    if (selection.Hidden != value)
                    {
                        selection.Hidden = value;
                        text.Color = selection.Hidden ? PPDColors.White : PPDColors.Black;
                    }
                }
            }

            public SongInfoComponent(PPDDevice device, PPDFramework.Resource.ResourceManager resourceManager) : base(device)
            {
                folder = new PictureObject(device, resourceManager, Utility.Path.Combine("scoremanager", "folder.png"));
                score = new PictureObject(device, resourceManager, Utility.Path.Combine("scoremanager", "score.png"));
                selection = new RectangleComponent(device, resourceManager, PPDColors.White)
                {
                    Hidden = true
                };
                this.AddChild(folder);
                this.AddChild(score);
                this.AddChild(selection);
            }

            private void UpdateData()
            {
                if (songInformation == null)
                {
                    return;
                }

                folder.Hidden = songInformation.IsPPDSong;
                score.Hidden = !songInformation.IsPPDSong;
                if (text != null)
                {
                    text.Text = songInformation.DirectoryName;
                }
            }

            private void UpdateDepth()
            {
                if (text != null)
                {
                    this.RemoveChild(text);
                }
                text = new TextureString(device, "", 14, 350 - 30 * depth, PPDColors.White);
                this.InsertChild(text, 0);
            }

            protected override bool OnCanDraw(PPDFramework.Shaders.AlphaBlendContext alphaBlendContext, int depth, int childIndex)
            {
                return songInformation != null;
            }
        }
    }
}
