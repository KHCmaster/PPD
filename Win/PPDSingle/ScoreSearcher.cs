using PPDFramework;
using PPDShareComponent;
using SharpDX;
using System;

namespace PPDSingle
{
    class ScoreSearcher : FocusableGameComponent
    {
        DxTextBox textBox;
        PictureObject back;
        LineRectangleComponent border;
        SongSelectFilter filter;

        SongInfoFinder sif;
        bool mustcheck;

        TextureString[] results;

        int selection = -1;

        public bool Selected
        {
            get;
            private set;
        }

        public ScoreSearcher(PPDDevice device, PPDFramework.Resource.ResourceManager resourceManager, DxTextBox textBox, SongSelectFilter filter) : base(device)
        {
            this.textBox = textBox;
            this.filter = filter;

            back = new PictureObject(device, resourceManager, Utility.Path.Combine("searchback.png"), true)
            {
                Position = new Vector2(400, 234)
            };

            sif = new SongInfoFinder(filter);

            border = new LineRectangleComponent(device, resourceManager, PPDColors.Selection)
            {
                BorderThickness = 3,
                Hidden = true
            };
            this.AddChild(border);
            results = new TextureString[12];
            for (int i = 0; i < results.Length; i++)
            {
                results[i] = new TextureString(device, "", 20, 300, PPDColors.Black)
                {
                    Position = new Vector2(240, 120 + 26 * i)
                };
                this.AddChild(results[i]);
            }


            Inputed += ScoreSearcher_Inputed;
            GotFocused += ScoreSearcher_GotFocused;

            this.AddChild(back);
        }

        void ScoreSearcher_GotFocused(IFocusable sender, FocusEventArgs args)
        {
            var type = args.FocusObject.GetType();
            if (type == typeof(MenuSelectSong))
            {
                Selected = false;
                sif.Name = "";
                sif.Find();
                selection = -1;
                textBox.Text = "";
                FocusTextBox();
                border.Hidden = true;
            }
            else if (type == typeof(DxTextBox))
            {
                textBox.TextChanged -= textBox_TextChanged;
                textBox.DrawMode = DxTextBox.DrawingMode.None;
                if (sif.Finished && sif.Result.Count > 0)
                {
                    selection = 0;
                }
                else
                {
                    selection = -1;
                }
                UpdateBorder();
            }
        }

        private void FocusTextBox()
        {
            textBox.DrawMode = DxTextBox.DrawingMode.DrawCaret | DxTextBox.DrawingMode.DrawSelection;
            textBox.DrawOnlyFocus = false;
            textBox.Position = new Vector2(240, 65);
            textBox.TextBoxWidth = 300;
            textBox.TextBoxHeight = 20;
            textBox.TextChanged += textBox_TextChanged;
            FocusManager.Focus(textBox);
        }

        void textBox_TextChanged(object sender, EventArgs e)
        {
            mustcheck = true;
        }

        void ScoreSearcher_Inputed(IFocusable sender, InputEventArgs args)
        {
            if (args.InputInfo.IsPressed(ButtonType.Cross))
            {
                textBox.DrawOnlyFocus = true;
                FocusManager.RemoveFocus();
            }
            else if (args.InputInfo.IsPressed(ButtonType.Start))
            {
                selection = -1;
                FocusTextBox();
                UpdateBorder();
            }
            else if (args.InputInfo.IsPressed(ButtonType.Circle))
            {
                Selected = true;
                textBox.DrawOnlyFocus = true;
                FocusManager.RemoveFocus();
            }
            else if (args.InputInfo.IsPressed(ButtonType.Up))
            {
                if (sif.Finished && sif.Result.Count > 0)
                {
                    results[selection].AllowScroll = false;
                    selection--;
                    if (selection < 0)
                    {
                        selection = Math.Min(sif.Result.Count, results.Length) - 1;
                    }
                    results[selection].AllowScroll = true;
                }
                UpdateBorder();
            }
            else if (args.InputInfo.IsPressed(ButtonType.Down))
            {
                if (sif.Finished && sif.Result.Count > 0)
                {
                    results[selection].AllowScroll = false;
                    selection++;
                    if (selection >= Math.Min(sif.Result.Count, results.Length))
                    {
                        selection = 0;
                    }
                    results[selection].AllowScroll = true;
                }
                UpdateBorder();
            }
        }

        private void UpdateBorder()
        {
            if (selection < 0)
            {
                border.Hidden = true;
                return;
            }
            border.Position = results[selection].Position - new Vector2(2, 2);
            border.RectangleHeight = results[selection].CharacterHeight + 1;
            border.RectangleWidth = results[selection].JustWidth + 4;
            border.Hidden = false;
        }

        protected override void UpdateImpl()
        {
            if (sif.Finished)
            {
                for (int i = 0; i < results.Length; i++)
                {
                    if (i < sif.Result.Count)
                    {
                        results[i].Text = sif.Result[i].DirectoryName;
                    }
                    else
                    {
                        results[i].Text = "";
                    }
                }
            }
            if (mustcheck)
            {
                sif.Name = textBox.Text;
                mustcheck &= !sif.Find();
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

        public SongInformation SelectedSongInformation
        {
            get
            {
                if (selection < 0 || selection >= sif.Result.Count)
                {
                    return null;
                }
                else
                {
                    return sif.Result[selection];
                }
            }
        }
    }
}
