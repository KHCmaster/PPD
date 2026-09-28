using SharpDX.Direct3D11;
using SharpDX.DXGI;

namespace PPDFramework.Vertex.DX11
{
    class VertexDeclarationFactory : IVertexDeclarationFactory
    {
        public VertexDeclarationBase Create(PPDDevice device, Effect.EffectBase effect)
        {
            var layout = new InputLayout((SharpDX.Direct3D11.Device)device.Device, ((PPDFramework.Effect.DX11.Effect)effect).ShaderBytecode, new[]
            {
                new InputElement("POSITION", 0, Format.R32G32B32_Float, 0, 0),
                new InputElement("COLOR", 0, Format.B8G8R8A8_UNorm, 12, 0),
                new InputElement("TEXCOORD", 0, Format.R32G32_Float, 16, 0)
            });
            return new VertexDeclaration(layout);
        }
    }
}
