using SharpDX.Direct3D9;
using System.IO;

namespace PPDFramework.Texture.DX9
{
    class TextureFactory : ITextureFactory
    {
        public TextureBase Create(PPDDevice device, int width, int height, int level, bool pa)
        {
            return new Texture(new SharpDX.Direct3D9.Texture((SharpDX.Direct3D9.Device)((PPDFramework.DX9.PPDDevice)device).Device, width, height, level, Usage.None, Format.A8R8G8B8, Pool.Managed), pa);
        }

        public TextureBase Create(PPDDevice device, int width, int height, int level, Pool pool, bool pa)
        {
            return new Texture(new SharpDX.Direct3D9.Texture((SharpDX.Direct3D9.Device)((PPDFramework.DX9.PPDDevice)device).Device, width, height, level, Usage.None, Format.A8R8G8B8, pool), pa);
        }

        public TextureBase CreateRenderTarget(PPDDevice device, int width, int height, int level, bool pa)
        {
            return new Texture(new SharpDX.Direct3D9.Texture((SharpDX.Direct3D9.Device)((PPDFramework.DX9.PPDDevice)device).Device, width, height, level, Usage.RenderTarget, Format.A8R8G8B8, Pool.Default), pa);
        }

        public TextureBase FromStream(PPDDevice device, Stream stream, int width, int height, bool pa)
        {
            return new Texture(SharpDX.Direct3D9.Texture.FromStream((SharpDX.Direct3D9.Device)((PPDFramework.DX9.PPDDevice)device).Device, stream, width, height, 0,
                Usage.None, Format.A8R8G8B8, Pool.Managed, Filter.Linear, Filter.Linear, 0), pa);
        }

        public TextureBase FromStream(PPDDevice device, Stream stream, int width, int height, Pool pool, bool pa)
        {
            return new Texture(SharpDX.Direct3D9.Texture.FromStream((SharpDX.Direct3D9.Device)((PPDFramework.DX9.PPDDevice)device).Device, stream, width, height, 0,
                Usage.None, Format.A8R8G8B8, pool, Filter.Linear, Filter.Linear, 0), pa);
        }

        public TextureBase FromFile(PPDDevice device, string filename, int width, int height, bool pa)
        {
            return new Texture(SharpDX.Direct3D9.Texture.FromFile((SharpDX.Direct3D9.Device)((PPDFramework.DX9.PPDDevice)device).Device, filename, width, height,
                0, Usage.None, Format.A8R8G8B8, Pool.Managed, Filter.Linear, Filter.Linear, 0), pa);
        }
    }
}
