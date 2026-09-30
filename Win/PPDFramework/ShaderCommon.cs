using PPDFramework.Vertex;
using SharpDX;
using System.Numerics;
using PPDMath;

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
            vertices.Write(new[] {
                new ColoredTexturedVertex(new Vector3(0, 0, 0.5f), new Vector2(0, 0)),
                new ColoredTexturedVertex(new Vector3(width, 0, 0.5f), new Vector2(1, 0)),
                new ColoredTexturedVertex(new Vector3(0, height, 0.5f), new Vector2(0, 1)),
                new ColoredTexturedVertex(new Vector3(width, height, 0.5f), new Vector2(1, 1))
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
