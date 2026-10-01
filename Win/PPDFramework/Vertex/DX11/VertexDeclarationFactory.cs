using Vortice.Direct3D11;
using Vortice.DXGI;

namespace PPDFramework.Vertex.DX11
{
    class VertexDeclarationFactory : IVertexDeclarationFactory
    {
        public VertexDeclarationBase Create(PPDDevice device, Effect.EffectBase effect)
        {
            var layout = ((ID3D11Device)device.Device).CreateInputLayout(
            [
                new InputElementDescription("POSITION", 0, Format.R32G32B32_Float, 0, 0),
                new InputElementDescription("COLOR", 0, Format.B8G8R8A8_UNorm, 12, 0),
                new InputElementDescription("TEXCOORD", 0, Format.R32G32_Float, 16, 0)
            ], ((PPDFramework.Effect.DX11.Effect)effect).InputSignatureBytecode);
            return new VertexDeclaration(layout);
        }
    }
}
