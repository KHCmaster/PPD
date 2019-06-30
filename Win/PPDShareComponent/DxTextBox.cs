using PPDFramework;
using PPDFramework.Shaders;
using SharpDX;
using System;

namespace PPDShareComponent
{
    public class DxTextBox : FocusableGameComponent
    {
        public event EventHandler TextChanged;
        [Flags]
        public enum DrawingMode
        {
            None = 0,
            DrawBack = 1,
            DrawBorder = 2,
            DrawCaret = 4,
            DrawSelection = 8,
            DrawAll = 15
        }

        const int thickness = 2;
        const int caretMargin = 4;
        const int caretWidth = 3;

        string _lastText;
        string text = "";

        bool enabled;
        DrawingMode drawMode;
        TextureString stringObj;
        RectangleComponent back;
        RectangleComponent selection;
        LineRectangleComponent border;
        RectangleComponent caret;
        IGameHost host;
        int count;
        int scrollIndex;
        int lastCaretIndex;
        TextBoxSelection lastSelection;

        public DxTextBox(PPDDevice device, IGameHost host, PPDFramework.Resource.ResourceManager resourceManager) : base(device)
        {
            this.host = host;
            back = new RectangleComponent(device, resourceManager, PPDColors.White);
            selection = new RectangleComponent(device, resourceManager, PPDColors.Active);
            border = new LineRectangleComponent(device, resourceManager, PPDColors.Selection);
            caret = new RectangleComponent(device, resourceManager, PPDColors.Black);
            caret.CanDraw += (o, c, d, ci) =>
            {
                return count >= 30 && Focused;
            };
            stringObj = new TextureString(device, "", 14, PPDColors.Black);
            TextBoxWidth = 150;
            MaxTextLength = int.MaxValue;
            MaxWidth = int.MaxValue;
            DrawMode = DrawingMode.DrawAll;
            DrawOnlyFocus = true;
            GotFocused += DxTextBox_GotFocused;
            LostFocused += DxTextBox_LostFocused;
            Inputed += DxTextBox_Inputed;
            host.IMEStarted += host_IMEStarted;
            host.TextBoxEnabledChanged += host_TextBoxEnabledChanged;
            this.AddChild(border);
            this.AddChild(caret);
            this.AddChild(stringObj);
            this.AddChild(selection);
            this.AddChild(back);
        }

        public bool ManuallyClosed
        {
            get;
            private set;
        }

        void DxTextBox_Inputed(IFocusable sender, InputEventArgs args)
        {
            if (args.InputInfo.IsPressed(ButtonType.Start))
            {
                ManuallyClosed = true;
                host.TextBoxEnabled = false;
            }
        }

        void host_TextBoxEnabledChanged(object sender, EventArgs e)
        {
            if (!host.TextBoxEnabled && FocusManager != null && FocusManager.CurrentFocusObject == this)
            {
                FocusManager.RemoveFocus();
            }
        }

        void host_IMEStarted(object sender, EventArgs e)
        {
            var p1 = stringObj.GetWidthToIndex(host.TextBoxSelection.SelectionStart);
            host.TextBoxLocation = new Vector2(stringObj.ScreenPos.X + p1, stringObj.ScreenPos.Y);
            host.TextBoxFontSize = (int)PPDSetting.Setting.GetAdjustedFontSize(stringObj.CharacterHeight);
        }

        void DxTextBox_GotFocused(IFocusable sender, FocusEventArgs args)
        {
            host.TextBoxSelection = new TextBoxSelection(0, text.Length);
            host.TextBoxCaretIndex = text.Length;
            Enabled = true;
        }

        void DxTextBox_LostFocused(IFocusable sender, FocusEventArgs args)
        {
            Enabled = false;
        }

        protected override void UpdateImpl()
        {
            if (text != host.TextBoxText)
            {
                if (host.TextBoxText.Length >= MaxTextLength)
                {
                    host.TextBoxText = host.TextBoxText.Substring(0, MaxTextLength);
                    text = host.TextBoxText;
                }
                OnTextChanged();
                _lastText = text;
                text = host.TextBoxText;
            }

            if (lastCaretIndex != host.TextBoxCaretIndex || lastSelection != host.TextBoxSelection || _lastText != text)
            {
                if (scrollIndex >= host.TextBoxCaretIndex)
                {
                    do
                    {
                        scrollIndex--;
                    } while (scrollIndex >= text.Length);

                    if (scrollIndex < 0)
                    {
                        scrollIndex = 0;
                    }

                    stringObj.Text = text.Substring(scrollIndex);
                    stringObj.Update();
                }
                else
                {
                    float tempWidth = 0;
                    scrollIndex--;
                    do
                    {
                        scrollIndex++;
                        stringObj.Text = text.Substring(scrollIndex);
                        stringObj.Update();
                        tempWidth = stringObj.GetWidthToIndex(host.TextBoxCaretIndex - scrollIndex);
                    } while (tempWidth >= TextBoxWidth);
                }

                _lastText = text;
                lastSelection = host.TextBoxSelection;
                lastCaretIndex = host.TextBoxCaretIndex;
            }

            float p1 = host.TextBoxSelection.SelectionStart < scrollIndex ? stringObj.GetWidthToIndex(0) : stringObj.GetWidthToIndex(host.TextBoxSelection.SelectionStart - scrollIndex),
                p2 = host.TextBoxSelection.SelectionEnd - scrollIndex >= stringObj.Text.Length ? stringObj.GetWidthToIndex(stringObj.Text.Length) : stringObj.GetWidthToIndex(host.TextBoxSelection.SelectionEnd - scrollIndex);
            var width = stringObj.GetWidthToIndex(host.TextBoxCaretIndex - scrollIndex);
            caret.Position = new Vector2(width, 0);
            caret.RectangleWidth = caretWidth;
            caret.RectangleHeight = TextBoxHeight + caretMargin;
            selection.Position = new Vector2(p1, 0);
            selection.RectangleWidth = p2 - p1;
            selection.RectangleHeight = TextBoxHeight + caretMargin;
            back.RectangleWidth = Math.Max(TextBoxWidth, stringObj.JustWidth);
            back.RectangleHeight = stringObj.CharacterHeight + caretMargin;
            border.Position = new Vector2(-thickness, -thickness);
            border.BorderThickness = thickness;
            border.RectangleWidth = Math.Max(TextBoxWidth, stringObj.JustWidth);
            border.RectangleHeight = stringObj.CharacterHeight + caretMargin + thickness;
            count++;
            if (count >= 60) count = 0;
        }

        protected override bool OnCanUpdate()
        {
            return !(PPDSetting.Setting.TextBoxDisabled || Disposed || Hidden || (DrawOnlyFocus && !Focused));
        }

        protected override void DrawImpl(AlphaBlendContext alphaBlendContext)
        {
            //host.SetClipping((int)this.ScreenPos.X - thickness, (int)this.ScreenPos.Y - thickness, (int)TextBoxWidth + caretMargin + 2 * thickness, (int)TextBoxHeight + 2 * thickness + caretMargin);
            //host.RestoreClipping();
        }

        protected override bool OnCanDraw(AlphaBlendContext alphaBlendContext, int depth, int childIndex)
        {
            return !(PPDSetting.Setting.TextBoxDisabled || Hidden || (DrawOnlyFocus && !Focused));
        }

        protected void OnTextChanged()
        {
            if (TextChanged != null)
            {
                TextChanged.Invoke(this, EventArgs.Empty);
            }
        }

        public bool Enabled
        {
            get
            {
                return enabled;
            }
            set
            {
                bool last = enabled;
                if (last != value)
                {
                    enabled = value;
                    host.TextBoxEnabled = value;
                    if (enabled)
                    {
                        host.TextBoxEnabled = true;
                    }
                    else
                    {
                        host.TextBoxEnabled = false;
                    }
                }
            }
        }

        public Color4 TextColor
        {
            get
            {
                return stringObj.Color;
            }
            set
            {
                stringObj.Color = value;
            }
        }

        public Color4 CaretColor
        {
            get { return caret.Color; }
            set
            {
                caret.Color = value;
            }
        }

        public int MaxTextLength
        {
            get;
            set;
        }

        public int MaxWidth
        {
            get;
            set;
        }

        public string Text
        {
            get
            {
                return host.TextBoxText;
            }
            set
            {
                text = value;
                host.TextBoxText = value;
            }
        }

        protected override void DisposeResource()
        {
            host.IMEStarted -= host_IMEStarted;
            host.TextBoxEnabledChanged -= host_TextBoxEnabledChanged;
        }

        public int TextBoxWidth
        {
            get;
            set;
        }

        public int TextBoxHeight
        {
            get
            {
                return stringObj.CharacterHeight;
            }
            set
            {
                stringObj.CharacterHeight = value;
            }
        }

        public bool DrawOnlyFocus
        {
            get;
            set;
        }

        public DrawingMode DrawMode
        {
            get { return drawMode; }
            set
            {
                if (drawMode != value)
                {
                    drawMode = value;
                    back.Hidden = !drawMode.HasFlag(DrawingMode.DrawBack);
                    selection.Hidden = !drawMode.HasFlag(DrawingMode.DrawSelection);
                    caret.Hidden = !drawMode.HasFlag(DrawingMode.DrawCaret);
                    border.Hidden = !drawMode.HasFlag(DrawingMode.DrawBorder);
                }
            }
        }
    }
}
