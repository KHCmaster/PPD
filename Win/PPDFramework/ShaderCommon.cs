using PPDFramework.Vertex;
using SharpDX;

namespace PPDFramework
{
    /// <summary>
    /// シェーダーを使う上で共通の部分です。
    /// </summary>
    public class ShaderCommon : DisposableComponent
    {
        VertexManager vertexManager;
        VertexInfo vertices;

        /// <summary>
        /// スクリーンサイズのVertexを取得します。
        /// </summary>
        public VertexInfo ScreenVertex
        {
            get { return vertices; }
        }

        /// <summary>
        /// コンストラクタです。
        /// </summary>
        /// <param name="device"></param>
        public ShaderCommon(PPDDevice device)
        {
            vertexManager = new VertexManager(device);
        }

        /// <summary>
        /// スクリーン頂点を初期化します。
        /// </summary>
        public void InitializeScreenVertex(PPDDevice device)
        {
            vertices = CreateVertex(4);
            var width = device.Width;
            var height = device.Height;
            float textureWidth = width;
            float textureHeight = height;
            float x1 = 0.5f / textureWidth, y1 = 0.5f / textureHeight;
            float x2 = (width + 0.5f) / textureWidth, y2 = (height + 0.5f) / textureHeight;
            vertices.Write(new[] {
                new ColoredTexturedVertex(new Vector3(0, 0, 0.5f), new Vector2(x1, y1)),
                new ColoredTexturedVertex(new Vector3(width, 0, 0.5f), new Vector2(x2, y1)),
                new ColoredTexturedVertex(new Vector3(0, height, 0.5f), new Vector2(x1, y2)),
                new ColoredTexturedVertex(new Vector3(width, height, 0.5f), new Vector2(x2, y2))
            });
        }

        /// <summary>
        /// 頂点を作成します。
        /// </summary>
        /// <param name="count"></param>
        /// <returns></returns>
        public VertexInfo CreateVertex(int count)
        {
            return vertexManager.Allocate(count);
        }

        /// <summary>
        /// 破棄します。
        /// </summary>
        protected override void DisposeResource()
        {
            if (vertexManager != null)
            {
                vertexManager.Dispose();
                vertexManager = null;
            }
        }
    }
}
