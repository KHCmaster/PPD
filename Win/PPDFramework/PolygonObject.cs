using PPDFramework.Resource;
using PPDFramework.Shaders;
using System.Numerics;
using PPDMath;

namespace PPDFramework
{
    /// <summary>
    /// 頂点を指定して画像を描画するコンポーネントです。
    /// </summary>
    public class PolygonObject : GameComponent
    {
        ImageResourceBase imageResource;
        Vertex.VertexInfo vertex;

        /// <summary>
        /// コンストラクタです。
        /// </summary>
        /// <param name="device">デバイス。</param>
        /// <param name="imageResource">描画する画像。</param>
        /// <param name="vertex">頂点。</param>
        public PolygonObject(PPDDevice device, ImageResourceBase imageResource, Vertex.VertexInfo vertex) : base(device)
        {
            this.imageResource = imageResource;
            this.vertex = vertex;
        }

        /// <summary>
        /// テクスチャ座標を、画像（スプライト）の実際のテクスチャ座標に変換します。
        /// </summary>
        /// <param name="textureCoorinates">画像の中のテクスチャ座標（0～1）。</param>
        /// <returns>実際のテクスチャ座標。</returns>
        public Vector2 GetActualUV(Vector2 textureCoorinates)
        {
            if (imageResource == null)
            {
                return textureCoorinates;
            }
            return imageResource.GetActualUVWithHalfPixel(textureCoorinates);
        }

        /// <summary>
        /// 描画します。
        /// </summary>
        /// <param name="alphaBlendContext">アルファブレンドのコンテキスト。</param>
        protected override void DrawImpl(AlphaBlendContext alphaBlendContext)
        {
            alphaBlendContext.Texture = this.imageResource.Texture;
            alphaBlendContext.Vertex = vertex;
            device.GetModule<AlphaBlend>().Draw(device, alphaBlendContext, PrimitiveType, PrimitiveCount, StartIndex, VertexCount);
        }

        /// <summary>
        /// 描画する画像を取得します。
        /// </summary>
        public ImageResourceBase ImageResource
        {
            get { return imageResource; }
        }

        /// <summary>
        /// プリミティブの種類を取得、設定します。
        /// </summary>
        public PrimitiveType PrimitiveType
        {
            get;
            set;
        }

        /// <summary>
        /// プリミティブの数を取得、設定します。
        /// </summary>
        public int PrimitiveCount
        {
            get;
            set;
        }

        /// <summary>
        /// 描画を始める頂点の位置を取得、設定します。
        /// </summary>
        public int StartIndex
        {
            get;
            set;
        }

        /// <summary>
        /// 頂点の数を取得、設定します。
        /// </summary>
        public int VertexCount
        {
            get;
            set;
        }
    }
}
