using PPDFramework.Resource;
using PPDFramework.Shaders;
using PPDFramework.Vertex;
using SharpDX;
using System;

namespace PPDFramework
{
    /// <summary>
    /// 画像表示クラス
    /// </summary>
    public class PictureObject : GameComponent
    {
        private RectangleF rec;
        private ImageResourceBase imageResource;
        private VertexInfo vertices;
        private Color4[] edgeColors;
        private bool center;
        private bool useCustomVertices;

        /// <summary>
        /// 画像のリソース
        /// </summary>
        public ImageResourceBase ImageResource
        {
            get
            {
                return imageResource;
            }
        }

        /// <summary>
        /// 幅
        /// </summary>
        public override float Width
        {
            get
            {
                return Math.Max(imageResource.Width, base.Width);
            }
        }

        /// <summary>
        /// 高さ
        /// </summary>
        public override float Height
        {
            get
            {
                return Math.Max(imageResource.Height, base.Height);
            }
        }

        /// <summary>
        /// 矩形
        /// </summary>
        public RectangleF Rectangle
        {
            get
            {
                return rec;
            }
            set
            {
                if (rec != value)
                {
                    rec = value;
                    UpdateVertex();
                }
            }
        }

        /// <summary>
        /// 四方の色を取得、設定します。
        /// </summary>
        public Color4[] EdgeColors
        {
            get { return edgeColors; }
            set
            {
                if (edgeColors != value)
                {
                    edgeColors = value;
                    UpdateVertex();
                }
            }
        }

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="device"></param>
        /// <param name="filename">ファイルパス</param>
        /// <param name="resourceManager">リソースマネージャー</param>
        public PictureObject(PPDDevice device, Resource.ResourceManager resourceManager, PathObject filename)
            : this(device, resourceManager, filename, false)
        {
        }

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="device"></param>
        /// <param name="filename">ファイルパス</param>
        /// <param name="center">センタリングするか</param>
        /// <param name="resourceManager">リソースマネージャー</param>
        public PictureObject(PPDDevice device, Resource.ResourceManager resourceManager, PathObject filename, bool center) : base(device)
        {
            imageResource = resourceManager.GetResource<ImageResourceBase>(filename);
            if (imageResource == null)
            {
                imageResource = (ImageResourceBase)resourceManager.Add(filename, ImageResourceFactoryManager.Factory.Create(device, filename, false));
            }
            if (center)
            {
                RotationCenter = ScaleCenter = new Vector2(imageResource.Width / 2, imageResource.Height / 2);
                Offset = new Vector2(-imageResource.Width / 2, -imageResource.Height / 2);
            }
            this.center = center;
            rec = new RectangleF(0, 0, imageResource.Width, imageResource.Height);
        }

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="device"></param>
        /// <param name="imageResource"></param>
        /// <param name="center"></param>
        public PictureObject(PPDDevice device, ImageResourceBase imageResource, bool center) : base(device)
        {
            this.imageResource = imageResource;
            if (center)
            {
                RotationCenter = ScaleCenter = new Vector2(imageResource.Width / 2, imageResource.Height / 2);
                Offset = new Vector2(-imageResource.Width / 2, -imageResource.Height / 2);
            }
            this.center = center;
            rec = new RectangleF(0, 0, imageResource.Width, imageResource.Height);
        }

        /// <summary>
        /// 描画します
        /// </summary>
        /// <param name="alphaBlendContext"></param>
        protected override void DrawImpl(AlphaBlendContext alphaBlendContext)
        {
            alphaBlendContext.Texture = imageResource.Texture;
            alphaBlendContext.Vertex = useCustomVertices ? vertices : imageResource.Vertex;
            device.GetModule<AlphaBlend>().Draw(device, alphaBlendContext);
        }

        private void UpdateVertex()
        {
            useCustomVertices = rec.X != 0 || rec.Y != 0 || rec.Width != imageResource.Width || rec.Height != imageResource.Height || (edgeColors != null && edgeColors.Length == 4);
            if (useCustomVertices)
            {
                if (vertices == null)
                {
                    vertices = device.GetModule<ShaderCommon>().CreateVertex(4);
                }
                var textureSize = new Size2F(imageResource.Width, imageResource.Height);
                var colors = edgeColors != null && edgeColors.Length == 4 ? edgeColors : new[] { PPDColors.White, PPDColors.White, PPDColors.White, PPDColors.White };
                var rect = imageResource.GetActualUVRectangle(
                    rec.X / textureSize.Width,
                    rec.Y / textureSize.Height,
                    (rec.X + rec.Width) / textureSize.Width,
                    (rec.Y + rec.Height) / textureSize.Height);
                vertices.Write(new[] {
                    new ColoredTexturedVertex(new Vector3(0, 0, 0.5f), colors[0], rect.TopLeft),
                    new ColoredTexturedVertex(new Vector3(rec.Width, 0, 0.5f), colors[1], rect.TopRight),
                    new ColoredTexturedVertex(new Vector3(0, rec.Height, 0.5f), colors[2], rect.BottomLeft),
                    new ColoredTexturedVertex(new Vector3(rec.Width, rec.Height, 0.5f), colors[3], rect.BottomRight)
                });
            }
        }

        /// <summary>
        /// 衝突判定を行います
        /// </summary>
        /// <param name="vec"></param>
        /// <returns></returns>
        public override bool HitTest(Vector2 vec)
        {
            if (!center)
            {
                return base.HitTest(vec);
            }
            else
            {
                return base.HitTest(vec + new Vector2(imageResource.Width / 2, imageResource.Height / 2));
            }
        }

        /// <summary>
        /// 破棄します。
        /// </summary>
        protected override void DisposeResource()
        {
            base.DisposeResource();
            if (vertices != null)
            {
                vertices.Dispose();
                vertices = null;
            }
        }
    }
}
