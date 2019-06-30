using PPDFramework.Texture;
using PPDFramework.Vertex;
using SharpDX;
using SharpDX.Direct3D9;
using System.IO;
using System.Windows.Forms;

namespace PPDFramework.Resource.DX9
{
    /// <summary>
    /// 画像リソースクラス
    /// </summary>
    class ImageResource : ImageResourceBase
    {
        int width;
        int height;
        string filename;
        Texture.DX9.Texture texture;
        VertexInfo vertices;
        Vector2 halfPixel;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="device"></param>
        /// <param name="filename">ファイルパス</param>
        /// <param name="pa"></param>
        public ImageResource(PPDDevice device, string filename, bool pa)
        {
            try
            {
                this.filename = filename;
                var ii = ImageInformation.FromFile(filename);
                this.width = ii.Width;
                this.height = ii.Height;
                this.halfPixel = new Vector2(0.5f / this.width, 0.5f / this.height);
                this.texture = (Texture.DX9.Texture)TextureFactoryManager.Factory.FromFile(device, filename, this.width, this.height, pa);
                CreateVertexBuffer(device);
            }
            catch
            {
                MessageBox.Show(PPDExceptionContentProvider.Provider.GetContent(PPDExceptionType.ImageReadError) + System.Environment.NewLine + filename);
            }
        }

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="device"></param>
        /// <param name="stream"></param>
        /// <param name="pa"></param>
        public ImageResource(PPDDevice device, Stream stream, bool pa)
        {
            try
            {
                var ii = ImageInformation.FromStream(stream);
                this.width = ii.Width;
                this.height = ii.Height;
                this.halfPixel = new Vector2(0.5f / this.width, 0.5f / this.height);
                this.texture = (Texture.DX9.Texture)TextureFactoryManager.Factory.FromStream(device, stream, this.width, this.height, pa);
                CreateVertexBuffer(device);
            }
            catch
            {
                MessageBox.Show(PPDExceptionContentProvider.Provider.GetContent(PPDExceptionType.ImageReadError));
            }
        }

        private void CreateVertexBuffer(PPDDevice device)
        {
            vertices = device.GetModule<ShaderCommon>().CreateVertex(4);
            var rect = GetActualUVRectangle(0, 0, 1, 1);
            vertices.Write(new[] {
                new ColoredTexturedVertex(new Vector3(0, 0, 0.5f), rect.TopLeft),
                new ColoredTexturedVertex(new Vector3(Width, 0, 0.5f), rect.TopRight),
                new ColoredTexturedVertex(new Vector3(0, Height, 0.5f), rect.BottomLeft),
                new ColoredTexturedVertex(new Vector3(Width, Height, 0.5f), rect.BottomRight)
            });
        }

        /// <summary>
        /// デストラクタ
        /// </summary>
        ~ImageResource()
        {
            Dispose();
        }

        /// <summary>
        /// 幅
        /// </summary>
        public override float Width
        {
            get
            {
                return width;
            }
        }

        /// <summary>
        /// 高さ
        /// </summary>
        public override float Height
        {
            get
            {
                return height;
            }
        }

        /// <summary>
        /// UV座標を取得します。
        /// </summary>
        public override Vector2 UV
        {
            get
            {
                return Vector2.Zero;
            }
        }

        /// <summary>
        /// UVサイズを取得します。
        /// </summary>
        public override Vector2 UVSize
        {
            get
            {
                return Vector2.One;
            }
        }

        /// <summary>
        /// 実際のUVサイズを取得します。
        /// </summary>
        public override Vector2 ActualUVSize
        {
            get
            {
                return Vector2.One;
            }
        }

        /// <summary>
        /// テクスチャ
        /// </summary>
        public override TextureBase Texture
        {
            get
            {
                return texture;
            }
        }

        /// <summary>
        /// 頂点データ
        /// </summary>
        public override VertexInfo Vertex
        {
            get
            {
                return vertices;
            }
        }

        /// <summary>
        /// ファイル名を取得します
        /// </summary>
        public override string FileName
        {
            get { return filename; }
        }

        /// <summary>
        /// 半ピクセルを取得します。
        /// </summary>
        public override Vector2 HalfPixel
        {
            get { return halfPixel; }
        }

        /// <summary>
        /// リソースを処分します
        /// </summary>
        protected override void DisposeResource()
        {
            if (texture != null)
            {
                texture.Dispose();
                texture = null;
            }
            if (vertices != null)
            {
                vertices.Dispose();
                vertices = null;
            }
        }
    }
}
