using SharpDX.Direct3D11;
using SharpDX.DXGI;
using System.IO;

namespace PPDFramework.Texture.DX11
{
    class TextureFactory : ITextureFactory
    {
        public TextureBase Create(PPDDevice device, int width, int height, int level, bool pa)
        {
            return new Texture(device, new Texture2D((SharpDX.Direct3D11.Device)((PPDFramework.DX11.PPDDevice)device).Device, new Texture2DDescription
            {
                Width = width,
                Height = height,
                MipLevels = level,
                Usage = ResourceUsage.Dynamic,
                Format = Format.R8G8B8A8_UNorm,
                SampleDescription = new SampleDescription(1, 0),
                CpuAccessFlags = CpuAccessFlags.Write,
                ArraySize = 1,
                BindFlags = BindFlags.ShaderResource
            }), false, pa);
        }

        public TextureBase CreateRenderTarget(PPDDevice device, int width, int height, int level, bool pa)
        {
            /* TODO
            return new Texture(device, new Texture2D(((PPDFramework.DX11.PPDDevice)device).Device, new Texture2DDescription
            {
                Width = width,
                Height = height,
                MipLevels = level,
                Usage = ResourceUsage.Default,
                Format = Format.R8G8B8A8_UNorm,
                SampleDescription = new SampleDescription(1, 0),
                ArraySize = 1,
                BindFlags = BindFlags.ShaderResource | BindFlags.RenderTarget
            }), true);
            */
            return null;
        }

        public TextureBase FromStream(PPDDevice device, Stream stream, int width, int height, bool pa)
        {
            /* TODO
            return new Texture(device, Texture2D.FromStream<Texture2D>(((PPDFramework.DX11.PPDDevice)device).Device, stream, (int)(stream.Length - stream.Position), new ImageLoadInformation
            {
                Width = width,
                Height = height,
                MipLevels = 0,
                Filter = FilterFlags.Linear,
                Format = Format.R8G8B8A8_UNorm,
                MipFilter = FilterFlags.None,
                Usage = ResourceUsage.Default,
                BindFlags = BindFlags.ShaderResource
            }));
            */
            return null;
        }

        public TextureBase FromFile(PPDDevice device, string filename, int width, int height, bool pa)
        {
            /* TODO
            return new Texture(device, Texture2D.FromFile<Texture2D>(((PPDFramework.DX11.PPDDevice)device).Device, filename, new ImageLoadInformation
            {
                Width = width,
                Height = height,
                MipLevels = 0,
                Filter = FilterFlags.Linear,
                Format = Format.R8G8B8A8_UNorm,
                MipFilter = FilterFlags.None,
                Usage = ResourceUsage.Default,
                BindFlags = BindFlags.ShaderResource
            }));
            */
            return null;
        }
    }
}
