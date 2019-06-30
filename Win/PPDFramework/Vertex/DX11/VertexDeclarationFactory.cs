using SharpDX.Direct3D11;
using SharpDX.DXGI;

namespace PPDFramework.Vertex.DX11
{
    class VertexDeclarationFactory : IVertexDeclarationFactory
    {
        public VertexDeclarationBase Create(PPDDevice device, Effect.EffectBase effect)
        {
            var layout = new InputLayout((SharpDX.Direct3D11.Device)((PPDFramework.DX11.PPDDevice)device).Device, ((PPDFramework.Effect.DX11.Effect)effect).ShaderBytecode, new[]
            {
                new InputElement("Position", 0, Format.R32G32B32A32_Float, 0, 0),
                new InputElement("Color", 0, Format.R8G8B8A8_UNorm, 12, 0),
                new InputElement("TexCoord", 0, Format.R32G32_Float, 16, 0)
            });
            return new VertexDeclaration(layout);
        }
    }
}
