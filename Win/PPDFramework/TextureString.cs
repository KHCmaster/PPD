using PPDFramework.RenderMask;
using PPDFramework.Shaders;
using SharpDX;
using System;

namespace PPDFramework
{
    /// <summary>
    /// Sprite非使用の文字列描画クラス
    /// </summary>
    public class TextureString : GameComponent
    {
        const int ScrollSpeed = 3;

        string text = "";
        int characterHeight;
        Size2F size;
        string facename;
        bool multiline;
        Color4 color;
        TextureChar[] chars = new TextureChar[0];

        TextureChar sparechar;

        float justwidth;
        float maxwidth;
        float width;
        bool allowScroll;
        bool scrollable;
        int scrollCount;
        int wait;

        bool textChanged;
        bool colorChanged;
        bool allowScrollChanged;
        bool hidden;
        Border border;
        BorderMask borderMask;
        Alignment alignment;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="device"></param>
        /// <param name="text">文字列</param>
        /// <param name="height">フォントサイズ</param>
        /// <param name="color">色</param>
        public TextureString(PPDDevice device, string text, int height, Color4 color) : base(device)
        {
            this.text = text;
            this.characterHeight = height;
            this.facename = PPDSetting.Setting.FontName;
            this.color = color;
            InnerStruct();
        }

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="device"></param>
        /// <param name="text">文字列</param>
        /// <param name="height">フォントサイズ</param>
        /// <param name="center">センタリング</param>
        /// <param name="color">色</param>
        public TextureString(PPDDevice device, string text, int height, bool center, Color4 color) : base(device)
        {
            this.text = text;
            this.characterHeight = height;
            this.facename = PPDSetting.Setting.FontName;
            this.color = color;
            this.alignment = Alignment.Center;
            InnerStruct();
        }

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="device"></param>
        /// <param name="text">文字列</param>
        /// <param name="height">フォントサイズ</param>
        /// <param name="maxwidth">最大幅</param>
        /// <param name="color">色</param>
        public TextureString(PPDDevice device, string text, int height, int maxwidth, Color4 color) : base(device)
        {
            this.text = text;
            this.characterHeight = height;
            this.facename = PPDSetting.Setting.FontName;
            this.color = color;
            this.maxwidth = maxwidth;
            this.scrollable = true;
            sparechar = new TextureChar(device, '…', 0, 0, this.characterHeight, facename, color);
            InnerStruct();
        }

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="device"></param>
        /// <param name="text">テキスト</param>
        /// <param name="characterHeight">フォントサイズ</param>
        /// <param name="height">幅</param>
        /// <param name="width">高さ</param>
        /// <param name="multiLine">マルチラインか</param>
        /// <param name="color">色</param>
        public TextureString(PPDDevice device, string text, int characterHeight, int width, int height, bool multiLine, Color4 color) : base(device)
        {
            this.text = text;
            this.characterHeight = characterHeight;
            this.facename = PPDSetting.Setting.FontName;
            this.color = color;
            size = new Size2F(width, height);
            this.multiline = multiLine;
            sparechar = new TextureChar(device, '…', 0, 0, this.characterHeight, facename, color);
            InnerStruct();
        }

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="device"></param>
        /// <param name="text">テキスト</param>
        /// <param name="characterHeight">フォントサイズ</param>
        /// <param name="height">幅</param>
        /// <param name="width">高さ</param>
        /// <param name="multiLine">マルチラインか</param>
        /// <param name="center">センタリングするかどうか</param>
        /// <param name="color">色</param>
        public TextureString(PPDDevice device, string text, int characterHeight, int width, int height, bool multiLine, bool center, Color4 color) : base(device)
        {
            this.text = text;
            this.characterHeight = characterHeight;
            this.facename = PPDSetting.Setting.FontName;
            this.color = color;
            size = new Size2F(width, height);
            this.multiline = multiLine;
            this.alignment = Alignment.Center;
            sparechar = new TextureChar(device, '…', 0, 0, this.characterHeight, facename, color);
            InnerStruct();
        }

        /// <summary>
        /// デストラクタ
        /// </summary>
        ~TextureString()
        {
            Dispose();
        }

        private void InnerStruct()
        {
            characterHeight = characterHeight <= 0 ? 10 : characterHeight;
            CreateText();
            if (scrollable)
            {
                MeasureJustWidth();
                allowScroll |= Width > maxwidth;
            }
            ChangePositionImpl();
        }

        /// <summary>
        /// 更新します。
        /// </summary>
        protected override void UpdateImpl()
        {
            if (scrollable && allowScroll && width > maxwidth)
            {
                wait++;
                if (wait >= 600)
                {
                    scrollCount += ScrollSpeed;
                    if (scrollCount >= this.width)
                    {
                        scrollCount = 0;
                        wait = 0;
                        ChangePositionImpl();
                    }
                }
            }
            ChangePosition();
            colorChanged = textChanged = allowScrollChanged = false;
        }

        private void CreateText()
        {
            foreach (TextureChar c in chars)
            {
                c.Dispose();
            }
            chars = new TextureChar[text.Length];
            width = 0;
            for (int i = 0; i < chars.Length; i++)
            {
                chars[i] = new TextureChar(device, text[i], 0, 0, characterHeight * Math.Max(Scale.X, Scale.Y), facename, color);
                width += chars[i].Width;
            }
            if (scrollable)
            {
                MeasureJustWidth();
            }
        }

        private void ChangePosition()
        {
            if (!textChanged && !allowScrollChanged && !(scrollable && allowScroll))
            {
                return;
            }
            ChangePositionImpl();
        }

        private void ChangePositionImpl()
        {
            if (multiline)
            {
                float tempx = 0, tempy = 0;
                int startIndex = 0;
                int iter = 0;
                foreach (TextureChar c in chars)
                {
                    c.Position = new Vector2(tempx, tempy);
                    tempx += c.Width;
                    if (tempx >= size.Width || c.Char == '\n')
                    {
                        if (alignment == Alignment.Center)
                        {
                            for (int i = startIndex; i <= iter; i++)
                            {
                                chars[i].Position = new Vector2((size.Width - tempx) / 2 + chars[i].Position.X, chars[i].Position.Y);
                            }
                            startIndex = iter + 1;
                        }
                        tempy += (int)(characterHeight * 1.3f);
                        tempx = 0;
                    }
                    iter++;
                }
                if (alignment == Alignment.Center)
                {
                    for (int i = startIndex; i < Math.Min(iter, chars.Length); i++)
                    {
                        chars[i].Position = new Vector2((size.Width - tempx) / 2 + chars[i].Position.X, chars[i].Position.Y);
                    }
                    startIndex = iter + 1;
                }

                MultiLineHeight = tempy + (int)(characterHeight * 1.3f);
            }
            else
            {
                var offset = 0f;
                switch (alignment)
                {
                    case Alignment.Center:
                        offset = -JustWidth / 2;
                        break;
                    case Alignment.Right:
                        offset = -JustWidth;
                        break;
                }
                float tempx = -scrollCount + offset;
                foreach (TextureChar c in chars)
                {
                    c.Position = new Vector2(tempx, 0);
                    tempx += c.Width;
                }
            }
        }

        /// <summary>
        /// 描画します。
        /// </summary>
        /// <param name="alphaBlendContext"></param>
        protected override void DrawImpl(AlphaBlendContext alphaBlendContext)
        {
            var cloneContext = device.GetModule<AlphaBlendContextCache>().Clone(alphaBlendContext);
            cloneContext.Overlay = color;
            if (scrollable && Width > maxwidth)
            {
                if (allowScroll)
                {
                    if (wait < 600)
                    {
                        DrawWithSpare(cloneContext);
                    }
                    else
                    {
                        for (int i = 0; i < chars.Length; i++)
                        {
                            float destx = chars[i].Position.X + chars[i].Width;
                            if (destx <= 0)
                            {
                                continue;
                            }
                            if (chars[i].Position.X > maxwidth)
                            {
                                break;
                            }
                            if (chars[i].Position.X < 0)
                            {
                                if (destx >= maxwidth)
                                {
                                    chars[i].Draw(cloneContext, -chars[i].Position.X, chars[i].Width - (destx - maxwidth));
                                }
                                else
                                {
                                    chars[i].Draw(cloneContext, -chars[i].Position.X, chars[i].Width);
                                }
                            }
                            else
                            {
                                if (destx >= maxwidth)
                                {
                                    chars[i].Draw(cloneContext, 0, chars[i].Width - (destx - maxwidth));
                                }
                                else
                                {
                                    chars[i].Draw(cloneContext);
                                }
                            }
                        }
                    }
                }
                else
                {
                    DrawWithSpare(cloneContext);
                }
            }
            else if (multiline)
            {
                for (int i = 0; i < chars.Length; i++)
                {
                    if (chars[i].Position.Y + characterHeight >= size.Height)
                    {
                        break;
                    }

                    if (chars[i].Char != '\n')
                    {
                        chars[i].Draw(cloneContext);
                    }
                }
            }
            else
            {
                for (int i = 0; i < chars.Length; i++)
                {
                    chars[i].Draw(cloneContext);
                }
            }
        }

        /// <summary>
        /// 描画可能かどうかを返します。
        /// </summary>
        /// <param name="alphaBlendContext"></param>
        /// <param name="depth"></param>
        /// <param name="childIndex"></param>
        /// <returns></returns>
        protected override bool OnCanDraw(AlphaBlendContext alphaBlendContext, int depth, int childIndex)
        {
            return chars != null && chars.Length > 0;
        }

        private void DrawWithSpare(AlphaBlendContext alphaBlendContext)
        {
            float sum = 0;
            for (int i = 0; i < chars.Length; i++)
            {
                if (chars[i].Width + sum + sparechar.Width <= maxwidth)
                {
                    sum += chars[i].Width;
                    chars[i].Draw(alphaBlendContext);
                }
                else
                {
                    sparechar.Position = chars[i].Position;
                    sparechar.Draw(alphaBlendContext);
                    break;
                }
            }
        }

        private void MeasureJustWidth()
        {
            float sum = 0;
            for (int i = 0; i < chars.Length; i++)
            {
                if (chars[i].Width + sum <= maxwidth)
                {
                    sum += chars[i].Width;
                }
                else
                {
                    sum += sparechar.Width;
                    break;
                }
            }
            justwidth = sum;
        }

        /// <summary>
        /// リソースを開放します
        /// </summary>
        protected override void DisposeResource()
        {
            if (chars != null)
            {
                foreach (TextureChar c in chars)
                {
                    if (c != null)
                    {
                        c.Dispose();
                    }
                }
                chars = null;
            }

            if (sparechar != null)
            {
                sparechar.Dispose();
                sparechar = null;
            }
        }

        /// <summary>
        /// マルチライン時の高さを取得します
        /// /// </summary>
        public float MultiLineHeight
        {
            get;
            private set;
        }

        /// <summary>
        /// アルファ
        /// </summary>
        public override float Alpha
        {
            get
            {
                return color.Alpha;
            }
            set
            {
                if (color.Alpha != value)
                {
                    colorChanged = true;
                    color.Alpha = value;
                }
            }
        }

        /// <summary>
        /// 色
        /// </summary>
        public Color4 Color
        {
            get
            {
                return color;
            }
            set
            {
                if (color != value)
                {
                    colorChanged = true;
                    color = value;
                }
            }
        }

        /// <summary>
        /// 描画するかどうか
        /// </summary>
        public override bool Hidden
        {
            get
            {
                return hidden;
            }
            set
            {
                hidden = value;
                foreach (TextureChar c in chars)
                {
                    c.Hidden = value;
                }
                if (sparechar != null)
                {
                    sparechar.Hidden = value;
                }
            }
        }

        /// <summary>
        /// ボーダー
        /// </summary>
        public Border Border
        {
            get
            {
                return border;
            }
            set
            {
                if (border != value)
                {
                    if (border != null)
                    {
                        renderMasks.Remove(borderMask);
                    }
                    border = value;
                    if (border != null)
                    {
                        renderMasks.Add(borderMask = new BorderMask(border));
                    }
                }
            }
        }

        /// <summary>
        /// テキスト
        /// </summary>
        public string Text
        {
            get
            {
                return text;
            }
            set
            {
                if (text != value)
                {
                    textChanged = true;
                    text = value;
                    CreateText();
                }
            }
        }

        /// <summary>
        /// 高さ
        /// </summary>
        public int CharacterHeight
        {
            get
            {
                return characterHeight;
            }
            set
            {
                if (characterHeight != value)
                {
                    characterHeight = value;
                    CreateText();
                }
            }
        }

        /// <summary>
        /// 幅
        /// </summary>
        public override float Width
        {
            get
            {
                return Math.Max(width, base.Width);
            }
        }

        /// <summary>
        /// 実際の幅
        /// </summary>
        public float JustWidth
        {
            get
            {
                if (scrollable)
                {
                    if (allowScroll)
                    {
                        return justwidth;
                    }
                    return Math.Min(width, maxwidth);
                }
                return width;
            }
        }

        /// <summary>
        /// あるインデックスまでの幅を取得します
        /// </summary>
        /// <param name="index">インデックス</param>
        /// <returns>幅</returns>
        public float GetWidthToIndex(int index)
        {
            if (index < 0) return 0;
            float sum = 0;
            var min = Math.Min(index, chars.Length);
            for (int i = 0; i < min; i++)
            {
                sum += chars[i].Width;
            }
            return sum;
        }

        /// <summary>
        /// 最大の幅を取得します
        /// </summary>
        public float MaxWidth
        {
            get
            {
                if (scrollable)
                {
                    return maxwidth;
                }
                else
                {
                    return Width;
                }
            }
        }

        /// <summary>
        /// マルチラインか
        /// </summary>
        public bool MultiLine
        {
            get
            {
                return multiline;
            }
            set
            {
                multiline = value;
            }
        }

        /// <summary>
        /// スクロールをするか
        /// </summary>
        public bool AllowScroll
        {
            get
            {
                return allowScroll;
            }
            set
            {
                allowScroll = value;
                scrollCount = 0;
                wait = 500;
                allowScrollChanged = true;
            }
        }

        /// <summary>
        /// 高さを取得します
        /// </summary>
        public override float Height
        {
            get
            {
                if (!multiline)
                {
                    return characterHeight;
                }
                return MultiLineHeight;
            }
        }

        /// <summary>
        /// スケールを取得、設定します。
        /// </summary>
        public override Vector2 Scale
        {
            get
            {
                return base.Scale;
            }
            set
            {
                if (base.Scale != value)
                {
                    base.Scale = value;
                    CreateText();
                }
            }
        }

        /// <summary>
        /// 衝突判定を行います
        /// </summary>
        /// <param name="vec"></param>
        /// <returns></returns>
        public override bool HitTest(Vector2 vec)
        {
            if (alignment == Alignment.Center)
            {
                return base.HitTest(new Vector2(vec.X + width / 2, vec.Y));
            }
            else
            {
                return base.HitTest(vec);
            }
        }

        /// <summary>
        /// アラインメントを設定します。
        /// </summary>
        public Alignment Alignment
        {
            get
            {
                return alignment;
            }
            set
            {
                if (alignment != value)
                {
                    alignment = value;
                    textChanged = true;
                }
            }
        }
    }
}
