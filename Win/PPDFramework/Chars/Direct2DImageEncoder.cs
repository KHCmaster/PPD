using System.Numerics;
using Color4 = Vortice.Mathematics.Color4;
using Vortice.DCommon;
using Vortice.Direct2D1;
using Vortice.DirectWrite;
using Vortice.DXGI;
using Vortice.WIC;
using System;
using System.IO;

namespace PPDFramework.Chars
{
    class Direct2DImageEncoder
    {
        private readonly Direct2DFactoryManager factoryManager;

        private readonly int imageWidth, imageHeight, imageDpi;

        public Direct2DImageEncoder(int imageWidth, int imageHeight, int imageDpi)
        {
            this.imageWidth = imageWidth;
            this.imageHeight = imageHeight;
            this.imageDpi = imageDpi;

            factoryManager = new Direct2DFactoryManager();
        }

        public void Save(Stream systemStream, Direct2DImageFormat format, string text, string faceName, float fontSize, out int width, out int height)
        {
#if BENCHMARK
            using (var handler = Benchmark.Instance.Start("DirectWrite", "Save"))
#endif
            using (var textFormat = factoryManager.DwFactory.CreateTextFormat(faceName, fontSize * 1.3f))
            using (var layout = factoryManager.DwFactory.CreateTextLayout(text, textFormat, 4000, 4000))
            {
                var metrics = layout.Metrics;
                width = (int)Math.Ceiling(metrics.WidthIncludingTrailingWhitespace);
                height = (int)Math.Ceiling(metrics.Height);
                using (var wicBitmap = factoryManager.WicFactory.CreateBitmap((uint)width, (uint)height, Vortice.WIC.PixelFormat.Format32bppPRGBA, BitmapCreateCacheOption.CacheOnLoad))
                {
                    var renderTargetProperties = new RenderTargetProperties(RenderTargetType.Default,
                        new Vortice.DCommon.PixelFormat(Format.R8G8B8A8_UNorm, Vortice.DCommon.AlphaMode.Unknown), imageDpi, imageDpi, RenderTargetUsage.None, Vortice.Direct2D1.FeatureLevel.Default);
                    using (var renderTarget = factoryManager.D2DFactory.CreateWicBitmapRenderTarget(wicBitmap, renderTargetProperties))
                    using (var brush = renderTarget.CreateSolidColorBrush(new Color4(1f, 1f, 1f, 1f), null))
                    using (var encoder = factoryManager.WicFactory.CreateEncoder(Direct2DConverter.ConvertImageFormat(format)))
                    using (var stream = factoryManager.WicFactory.CreateStream(systemStream))
                    {
                        renderTarget.BeginDraw();
                        renderTarget.Clear(new Color4(1, 1, 1, 0));
                        renderTarget.DrawTextLayout(new Vector2(0, 0), layout, brush);
                        renderTarget.EndDraw().CheckError();
                        encoder.Initialize(stream, BitmapEncoderCacheOption.NoCache);
                        using (var bitmapFrameEncode = encoder.CreateNewFrame(out var encoderOptions))
                        {
                            encoderOptions?.Dispose();
                            bitmapFrameEncode.Initialize().CheckError();
                            bitmapFrameEncode.SetSize((uint)width, (uint)height).CheckError();
                            bitmapFrameEncode.WriteSource(wicBitmap).CheckError();
                            bitmapFrameEncode.Commit();
                        }
                        encoder.Commit();
                    }
                }
            }
        }
    }
}

class Direct2DFactoryManager
{
    private readonly IWICImagingFactory wicFactory;
    private readonly ID2D1Factory d2DFactory;
    private readonly IDWriteFactory dwFactory;

    public Direct2DFactoryManager()
    {
        wicFactory = new IWICImagingFactory();
        d2DFactory = D2D1.D2D1CreateFactory<ID2D1Factory>(Vortice.Direct2D1.FactoryType.SingleThreaded, DebugLevel.None);
        dwFactory = DWrite.DWriteCreateFactory<IDWriteFactory>(Vortice.DirectWrite.FactoryType.Shared);
    }

    public IWICImagingFactory WicFactory
    {
        get
        {
            return wicFactory;
        }
    }

    public ID2D1Factory D2DFactory
    {
        get
        {
            return d2DFactory;
        }
    }

    public IDWriteFactory DwFactory
    {
        get
        {
            return dwFactory;
        }
    }
}

enum Direct2DImageFormat
{
    Png, Gif, Ico, Jpeg, Wmp, Tiff, Bmp
}

class Direct2DConverter
{
    public static Guid ConvertImageFormat(Direct2DImageFormat format)
    {
        switch (format)
        {
            case Direct2DImageFormat.Bmp:
                return ContainerFormatGuids.Bmp;
            case Direct2DImageFormat.Ico:
                return ContainerFormatGuids.Ico;
            case Direct2DImageFormat.Gif:
                return ContainerFormatGuids.Gif;
            case Direct2DImageFormat.Jpeg:
                return ContainerFormatGuids.Jpeg;
            case Direct2DImageFormat.Png:
                return ContainerFormatGuids.Png;
            case Direct2DImageFormat.Tiff:
                return ContainerFormatGuids.Tiff;
            case Direct2DImageFormat.Wmp:
                return ContainerFormatGuids.Wmp;
        }
        throw new NotSupportedException();
    }
}
