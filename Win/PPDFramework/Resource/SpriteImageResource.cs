using PPDFramework.Sprites;
using PPDFramework.Texture;
using PPDFramework.Vertex;
using SharpDX;

namespace PPDFramework.Resource
{
    class SpriteImageResource : ImageResourceBase
    {
        VertexInfo vertices;
        ImageResourceBase imageResource;
        SpriteInfo spriteInfo;
        ImageScale imageScale;
        Vector2 uv;
        Vector2 uvSize;
        Vector2 actualUVSize;
        Vector2 halfPixel;

        public SpriteImageResource(PPDDevice device, ImageResourceBase imageResource, SpriteInfo spriteInfo, ImageScale imageScale)
        {
            this.imageResource = imageResource;
            this.spriteInfo = spriteInfo;
            this.imageScale = imageScale;
            uv = new Vector2(spriteInfo.X / (float)imageResource.Width, spriteInfo.Y / (float)imageResource.Height);
            uvSize = new Vector2(spriteInfo.SpaceWidth / (float)imageResource.Width, spriteInfo.SpaceHeight / (float)imageResource.Height);
            actualUVSize = new Vector2(spriteInfo.Width / (float)imageResource.Width, spriteInfo.Height / (float)imageResource.Height);
            halfPixel = new Vector2(0.5f / imageResource.Width, 0.5f / imageResource.Height);
            CreateVertexBuffer(device);
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

        public ImageScale ImageScale
        {
            get
            {
                return imageScale;
            }
        }

        public override string FileName
        {
            get
            {
                return spriteInfo.Path;
            }
        }

        public override float Height
        {
            get
            {
                return spriteInfo.Height / ImageScale.Ratio;
            }
        }

        public override TextureBase Texture
        {
            get
            {
                return imageResource.Texture;
            }
        }

        public override Vector2 UV
        {
            get
            {
                return uv;
            }
        }

        public override Vector2 UVSize
        {
            get
            {
                return uvSize;
            }
        }

        public override Vector2 ActualUVSize
        {
            get
            {
                return actualUVSize;
            }
        }

        public override VertexInfo Vertex
        {
            get
            {
                return vertices;
            }
        }

        public override float Width
        {
            get
            {
                return spriteInfo.Width / ImageScale.Ratio;
            }
        }

        /// <summary>
        /// 半ピクセルを取得します。
        /// </summary>
        public override Vector2 HalfPixel
        {
            get { return halfPixel; }
        }

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
