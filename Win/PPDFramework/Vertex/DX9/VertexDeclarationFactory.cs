using SharpDX.Direct3D9;

namespace PPDFramework.Vertex.DX9
{
    class VertexDeclarationFactory : IVertexDeclarationFactory
    {
        public VertexDeclarationBase Create(PPDDevice device, Effect.EffectBase effect)
        {
            var elements = new VertexElement[]{
                new VertexElement(0, 0, DeclarationType.Float3, DeclarationMethod.Default, DeclarationUsage.Position, 0),
                new VertexElement(0, 12, DeclarationType.Color, DeclarationMethod.Default, DeclarationUsage.Color, 0),
                new VertexElement(0, 16, DeclarationType.Float2, DeclarationMethod.Default, DeclarationUsage.TextureCoordinate, 0),
                VertexElement.VertexDeclarationEnd
            };
            return new VertexDeclaration(new SharpDX.Direct3D9.VertexDeclaration((SharpDX.Direct3D9.Device)((PPDFramework.DX9.PPDDevice)device).Device, elements));
        }
    }
}
