using PPDFramework.Resource;
using PPDFramework.Shaders;
using PPDFramework.Vertex;
using SharpDX;
using System;

namespace PPDFramework
{
    /// <summary>
    /// 矩形を描画するコンポーネントです。
    /// </summary>
    public class RectangleComponent : GameComponent
    {
        PPDFramework.Resource.ResourceManager resourceManager;
        ColorTextureResource resource;
        float rectangleWidth;
        float rectangleHeight;
        bool rectangleChanged;
        VertexInfo vertices;

        /// <summary>
        /// 色を取得、設定します。
        /// </summary>
        public Color4 Color
        {
            get
            {
                return resource.Color;
            }
            set
            {
                if (resource == null || resource.Color != value)
                {
                    UpdateResource(value);
                }
            }
        }

        /// <summary>
        /// 幅を取得します。
        /// </summary>
        public override float Width
        {
            get { return rectangleWidth; }
        }

        /// <summary>
        /// 高さを取得します。
        /// </summary>
        public override float Height
        {
            get { return rectangleHeight; }
        }

        /// <summary>
        /// 矩形の幅を取得、設定します。
        /// </summary>
        public float RectangleWidth
        {
            get { return rectangleWidth; }
            set
            {
                if (rectangleWidth != value)
                {
                    rectangleWidth = value;
                    rectangleChanged = true;
                }
            }
        }

        /// <summary>
        /// 矩形の高さを取得、設定します。
        /// </summary>
        public float RectangleHeight
        {
            get { return rectangleHeight; }
            set
            {
                if (rectangleHeight != value)
                {
                    rectangleHeight = value;
                    rectangleChanged = true;
                }
            }
        }

        /// <summary>
        /// コンストラクタです。
        /// </summary>
        /// <param name="device"></param>
        /// <param name="resourceManager"></param>
        /// <param name="color"></param>
        public RectangleComponent(PPDDevice device, PPDFramework.Resource.ResourceManager resourceManager, Color4 color) : base(device)
        {
            this.resourceManager = resourceManager;
            UpdateResource(color);
            vertices = device.GetModule<ShaderCommon>().CreateVertex(4);
        }

        private void UpdateResource(Color4 color)
        {
            var resourceName = String.Format("ColorTexture_{0}", color);
            resource = resourceManager.GetResource<ColorTextureResource>(resourceName);
            if (resource == null)
            {
                resource = new ColorTextureResource(device, color);
                resourceManager.Add(resourceName, resource);
            }
        }

        /// <summary>
        /// 更新します
        /// </summary>
        public override void Update()
        {
            if (rectangleChanged)
            {
                rectangleChanged = false;
                vertices.Write(new[] {
                    new ColoredTexturedVertex(new Vector3(0, 0, 0.5f)),
                    new ColoredTexturedVertex(new Vector3(rectangleWidth, 0, 0.5f)),
                    new ColoredTexturedVertex(new Vector3(0, rectangleHeight, 0.5f)),
                    new ColoredTexturedVertex(new Vector3(rectangleWidth, rectangleHeight, 0.5f))
                });
            }

            base.Update();
        }

        /// <summary>
        /// 描画処理
        /// </summary>
        /// <param name="alphaBlendContext"></param>
        protected override void DrawImpl(Shaders.AlphaBlendContext alphaBlendContext)
        {
            if (resource != null && rectangleWidth > 0 && rectangleHeight > 0)
            {
                alphaBlendContext.Texture = resource.Texture;
                alphaBlendContext.Vertex = vertices;
                device.GetModule<AlphaBlend>().Draw(device, alphaBlendContext);
            }
        }

        /// <summary>
        /// 描画するかどうかを返します
        /// </summary>
        /// <param name="alphaBlendContext"></param>
        /// <param name="depth"></param>
        /// <param name="childIndex"></param>
        /// <returns></returns>
        protected override bool OnCanDraw(AlphaBlendContext alphaBlendContext, int depth, int childIndex)
        {
            return resource != null && rectangleWidth > 0 && rectangleHeight > 0;
        }

        /// <summary>
        /// 破棄します
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
