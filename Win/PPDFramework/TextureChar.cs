using PPDFramework.Chars;
using PPDFramework.Shaders;
using PPDFramework.Vertex;
using SharpDX;

namespace PPDFramework
{
    /// <summary>
    /// Sprite非使用の文字描画クラス
    /// </summary>
    public class TextureChar : DrawableGameComponent
    {
        Color4 color;
        CharCacheInfo cci;
        char c;

        PPDDevice device;
        string facename;
        bool isSpace;
        float widthScale = 1;
        VertexInfo vertices;
        float prevStartX;
        float prevEndX;
        Vector2 prevPosition;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="device"></param>
        /// <param name="c">文字</param>
        /// <param name="x">x</param>
        /// <param name="y">y</param>
        /// <param name="fontsize">フォントサイズ</param>
        /// <param name="facename">フォント名</param>
        /// <param name="color">色</param>
        public TextureChar(PPDDevice device, char c, float x, float y, float fontsize, string facename, Color4 color)
        {
            this.device = device;
            Position = new Vector2(x, y);
            this.c = c;
            this.color = color;
            fontsize = PPDSetting.Setting.GetAdjustedFontSize(fontsize);
            this.facename = facename;
            isSpace = CheckSpace();
            cci = device.GetModule<CharCacheManager>().Get(fontsize, facename, this.c);
            vertices = device.GetModule<ShaderCommon>().CreateVertex(4);
        }

        private bool CheckSpace()
        {
            switch (c)
            {
                case '\u0020'://（半角スペース）
                    c = 'x';
                    break;
                case '\u00A0':// ノーブレークスペース
                    c = 'x';
                    break;
                case '\u2002':// enスペース
                    c = 'n';
                    break;
                case '\u2003':// emスペース
                    c = 'M';
                    break;
                case '\u2004':// （emの1/3幅のスペース）
                    c = 'M';
                    widthScale = 1 / 3f;
                    break;
                case '\u2005':// （emの1/4幅のスペース）
                    c = 'M';
                    widthScale = 1 / 4f;
                    break;
                case '\u2009':// （emの1/5幅のスペース）
                    c = 'M';
                    widthScale = 1 / 5f;
                    break;
                case '\u2006':// （emの1/6幅のスペース）
                    c = 'M';
                    widthScale = 1 / 6f;
                    break;
                case '\u2007':// （等幅フォントの半分の幅）
                    c = 'あ';
                    widthScale = 1 / 2f;
                    break;
                case '\u2008':// 約物スペース
                    break;
                case '\u200A':// ヘアスペース
                    break;
                case '\u200B':// 幅なしスペース（幅が0）
                    widthScale = 0;
                    break;
                case '\u3000':// 全角スペース
                    c = 'あ';
                    break;
                case '\uFEFF':// 幅なし改行なしスペース
                    widthScale = 0;
                    break;
                default:
                    return false;
            }
            return true;
        }

        /// <summary>
        /// 描画します
        /// </summary>
        public void Draw(AlphaBlendContext alphaBlendContext)
        {
            if (Hidden || isSpace)
            {
                return;
            }
            Draw(alphaBlendContext, 0, cci.Width);
        }

        /// <summary>
        /// 描画します
        /// </summary>
        /// <param name="alphaBlendContext">コンテキスト</param>
        /// <param name="startX">表示をはじめるピクセルのx座標(0からwidth)</param>
        /// <param name="endX">表示をおえるピクセルのx座標(0からwidthかつsx以上)</param>
        public void Draw(AlphaBlendContext alphaBlendContext, float startX, float endX)
        {
            if (Hidden || isSpace)
            {
                return;
            }

            if (prevPosition != Position || prevStartX != startX || prevEndX != endX)
            {
                prevPosition = Position;
                prevStartX = startX;
                prevEndX = endX;
                float posX0 = Position.X + startX, posX1 = Position.X + endX,
                    posY0 = Position.Y, posY1 = Position.Y + cci.Height;
                var rect = cci.GetActualUVRectangle(startX / (float)cci.Width, 0, endX / (float)cci.Width, 1);
                vertices.Write(new[] {
                    new ColoredTexturedVertex(new Vector3(posX0, posY0, 0.5f), rect.TopLeft),
                    new ColoredTexturedVertex(new Vector3(posX1, posY0, 0.5f), rect.TopRight),
                    new ColoredTexturedVertex(new Vector3(posX0, posY1, 0.5f), rect.BottomLeft),
                    new ColoredTexturedVertex(new Vector3(posX1, posY1, 0.5f), rect.BottomRight)
                });
            }
            alphaBlendContext.Texture = cci.Texture;
            alphaBlendContext.Vertex = vertices;
            device.GetModule<AlphaBlend>().Draw(device, alphaBlendContext);
        }

        /// <summary>
        /// リソースを破棄します
        /// </summary>
        protected override void DisposeResource()
        {
            if (cci != null)
            {
                cci.Decrement();
                cci = null;
            }
            if (vertices != null)
            {
                vertices.Dispose();
                vertices = null;
            }
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
                color.Alpha = value;
            }
        }

        /// <summary>
        /// 幅
        /// </summary>
        public float Width
        {
            get
            {
                return cci.Width * widthScale;
            }
        }

        /// <summary>
        /// 高さ
        /// </summary>
        public float Height
        {
            get
            {
                return cci.Height;
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
                color = value;
            }
        }

        /// <summary>
        /// 文字
        /// </summary>
        public char Char
        {
            get
            {
                return c;
            }
        }
    }
}
