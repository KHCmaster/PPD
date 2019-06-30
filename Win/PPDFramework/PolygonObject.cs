using PPDFramework.Resource;
using PPDFramework.Shaders;
using SharpDX;

namespace PPDFramework
{
    public class PolygonObject : GameComponent
    {
        ImageResourceBase imageResource;
        Vertex.VertexInfo vertex;

        public PolygonObject(PPDDevice device, ImageResourceBase imageResource, Vertex.VertexInfo vertex) : base(device)
        {
            this.imageResource = imageResource;
            this.vertex = vertex;
        }

        public Vector2 GetActualUV(Vector2 textureCoorinates)
        {
            if (imageResource == null)
            {
                return textureCoorinates;
            }
            return imageResource.GetActualUVWithHalfPixel(textureCoorinates);
        }

        protected override void DrawImpl(AlphaBlendContext alphaBlendContext)
        {
            alphaBlendContext.Texture = this.imageResource.Texture;
            alphaBlendContext.Vertex = vertex;
            device.GetModule<AlphaBlend>().Draw(device, alphaBlendContext, PrimitiveType, PrimitiveCount, StartIndex, VertexCount);
        }

        public ImageResourceBase ImageResource
        {
            get { return imageResource; }
        }

        public PrimitiveType PrimitiveType
        {
            get;
            set;
        }

        public int PrimitiveCount
        {
            get;
            set;
        }

        public int StartIndex
        {
            get;
            set;
        }

        public int VertexCount
        {
            get;
            set;
        }
    }
}
