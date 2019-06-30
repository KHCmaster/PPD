using PPDFramework.Texture;
using PPDFramework.Vertex;
using SharpDX;

namespace PPDFramework.Resource
{
    /// <summary>
    /// 画像リソースのクラスです。
    /// </summary>
    public abstract class ImageResourceBase : ResourceBase
    {
        /// <summary>
        /// テクスチャを取得します。
        /// </summary>
        public abstract TextureBase Texture { get; }

        /// <summary>
        /// 幅を取得します。
        /// </summary>
        public abstract float Width { get; }

        /// <summary>
        /// 高さを取得します。
        /// </summary>
        public abstract float Height { get; }

        /// <summary>
        /// UV座標を取得します。
        /// </summary>
        public abstract Vector2 UV { get; }

        /// <summary>
        /// UVサイズを取得します。
        /// </summary>
        public abstract Vector2 UVSize { get; }

        /// <summary>
        /// 実際のUVサイズを取得します。
        /// </summary>
        public abstract Vector2 ActualUVSize { get; }

        /// <summary>
        /// 頂点情報を取得します。
        /// </summary>
        public abstract VertexInfo Vertex { get; }

        /// <summary>
        /// ファイル名を取得します。
        /// </summary>
        public abstract string FileName { get; }

        /// <summary>
        /// 半ピクセルを取得します。
        /// </summary>
        public abstract Vector2 HalfPixel { get; }

        /// <summary>
        /// 実際のUVを取得します。
        /// </summary>
        /// <param name="uv"></param>
        /// <returns></returns>
        public Vector2 GetActualUV(Vector2 uv)
        {
            return UV + ActualUVSize * uv;
        }

        /// <summary>
        /// 実際のUVを取得します。
        /// </summary>
        /// <param name="uv"></param>
        /// <returns></returns>
        public Vector2 GetActualUVWithHalfPixel(Vector2 uv)
        {
            var rect = GetActualUVRectangle(0, 0, 1, 1);
            return new Vector2(rect.Left + rect.Width * uv.X, rect.Top + rect.Height * uv.Y);
        }

        /// <summary>
        /// 実際UV矩形を取得します。
        /// </summary>
        /// <param name="left"></param>
        /// <param name="top"></param>
        /// <param name="right"></param>
        /// <param name="bottom"></param>
        /// <returns></returns>
        public RectangleF GetActualUVRectangle(float left, float top, float right, float bottom)
        {
            var topLeft = GetActualUV(new Vector2(left, top));
            var bottomRight = GetActualUV(new Vector2(right, bottom));
            var halfPixel = HalfPixel;
            return new RectangleF(topLeft.X + halfPixel.X, topLeft.Y + halfPixel.Y,
                bottomRight.X - topLeft.X - halfPixel.X,
                bottomRight.Y - topLeft.Y - halfPixel.Y);

        }
    }
}
