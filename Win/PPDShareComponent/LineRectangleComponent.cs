using PPDFramework;
using PPDFramework.Resource;
using PPDFramework.Shaders;
using PPDFramework.Vertex;
using SharpDX;
using System;

namespace PPDShareComponent
{
    public class LineRectangleComponent : GameComponent
    {
        PPDFramework.Resource.ResourceManager resourceManager;
        ColorTextureResourceBase resource;
        float borderThickness;
        float rectangleHeight;
        float rectangleWidth;
        bool propertyChanged;
        VertexInfo vertices;

        public float BorderThickness
        {
            get { return borderThickness; }
            set
            {
                if (borderThickness != value)
                {
                    borderThickness = value;
                    propertyChanged = true;
                }
            }
        }

        public float RectangleHeight
        {
            get { return rectangleHeight; }
            set
            {
                if (rectangleHeight != value)
                {
                    rectangleHeight = value;
                    propertyChanged = true;
                }
            }
        }

        public float RectangleWidth
        {
            get { return rectangleWidth; }
            set
            {
                if (rectangleWidth != value)
                {
                    rectangleWidth = value;
                    propertyChanged = true;
                }
            }
        }

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
                UpdateResource(value);
            }
        }

        public LineRectangleComponent(PPDDevice device, PPDFramework.Resource.ResourceManager resourceManager, Color4 color) : base(device)
        {
            this.resourceManager = resourceManager;
            UpdateResource(color);
            borderThickness = 1;
            vertices = device.GetModule<ShaderCommon>().CreateVertex(10);
        }

        private void UpdateResource(Color4 color)
        {
            var resourceName = String.Format("ColorTexture_{0}", color);
            resource = resourceManager.GetResource<ColorTextureResourceBase>(resourceName);
            if (resource == null)
            {
                resource = new ColorTextureResource(device, color);
                resourceManager.Add(resourceName, resource);
            }
        }

        protected override void UpdateImpl()
        {
            if (propertyChanged)
            {
                propertyChanged = false;

                var borderThickness2 = borderThickness * 2;
                vertices.Write(new[] {
                    new ColoredTexturedVertex(new Vector3(0, 0, 0.5f)),
                    new ColoredTexturedVertex(new Vector3(borderThickness, borderThickness, 0.5f)),
                    new ColoredTexturedVertex(new Vector3(0, borderThickness2 + rectangleHeight, 0.5f)),
                    new ColoredTexturedVertex(new Vector3(borderThickness, borderThickness + rectangleHeight, 0.5f)),
                    new ColoredTexturedVertex(new Vector3(borderThickness2 + rectangleWidth, borderThickness2 + rectangleHeight, 0.5f)),
                    new ColoredTexturedVertex(new Vector3(borderThickness + rectangleWidth, borderThickness + rectangleHeight, 0.5f)),
                    new ColoredTexturedVertex(new Vector3(borderThickness2 + rectangleWidth, 0, 0.5f)),
                    new ColoredTexturedVertex(new Vector3(borderThickness + rectangleWidth, borderThickness, 0.5f)),
                    new ColoredTexturedVertex(new Vector3(0, 0, 0.5f)),
                    new ColoredTexturedVertex(new Vector3(borderThickness, borderThickness, 0.5f))
                });
            }
        }

        protected override void DrawImpl(PPDFramework.Shaders.AlphaBlendContext alphaBlendContext)
        {
            alphaBlendContext.Texture = resource.Texture;
            alphaBlendContext.Vertex = vertices;
            device.GetModule<AlphaBlend>().Draw(device, alphaBlendContext, PrimitiveType.TriangleStrip, 8, 0, vertices.Count);
        }

        protected override bool OnCanDraw(AlphaBlendContext alphaBlendContext, int depth, int childIndex)
        {
            return resource != null && rectangleWidth > 0 && rectangleHeight > 0 && borderThickness > 0;
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
