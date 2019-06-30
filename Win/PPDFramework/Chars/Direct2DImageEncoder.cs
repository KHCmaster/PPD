using SharpDX;
using SharpDX.Direct2D1;
using SharpDX.DirectWrite;
using SharpDX.DXGI;
using SharpDX.WIC;
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
            using (var layout = new TextLayout(factoryManager.DwFactory, text, new TextFormat(factoryManager.DwFactory, faceName, fontSize * 1.3f), 4000, 4000))
            {
                width = (int)Math.Ceiling(layout.Metrics.WidthIncludingTrailingWhitespace);
                height = (int)Math.Ceiling(layout.Metrics.Height);
                using (var wicBitmap = new SharpDX.WIC.Bitmap(factoryManager.WicFactory, width, height, SharpDX.WIC.PixelFormat.Format32bppPRGBA, BitmapCreateCacheOption.CacheOnLoad))
                {
                    var renderTargetProperties = new RenderTargetProperties(RenderTargetType.Default,
                        new SharpDX.Direct2D1.PixelFormat(Format.R8G8B8A8_UNorm, SharpDX.Direct2D1.AlphaMode.Unknown), imageDpi, imageDpi, RenderTargetUsage.None, FeatureLevel.Level_DEFAULT);
                    using (var renderTarget = new WicRenderTarget(factoryManager.D2DFactory, wicBitmap, renderTargetProperties))
                    using (var brush = new SolidColorBrush(renderTarget, SharpDX.Color.White))
                    using (var encoder = new BitmapEncoder(factoryManager.WicFactory, Direct2DConverter.ConvertImageFormat(format)))
                    {
                        renderTarget.BeginDraw();
                        renderTarget.Clear(new Color4(1, 1, 1, 0));
                        renderTarget.DrawTextLayout(Vector2.Zero, layout, brush);
                        renderTarget.EndDraw();
                        var stream = new WICStream(factoryManager.WicFactory, systemStream);
                        encoder.Initialize(stream);
                        using (var bitmapFrameEncode = new BitmapFrameEncode(encoder))
                        {
                            bitmapFrameEncode.Initialize();
                            bitmapFrameEncode.SetSize(width, height);
                            bitmapFrameEncode.WriteSource(wicBitmap);
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
    private readonly SharpDX.WIC.ImagingFactory wicFactory;
    private readonly SharpDX.Direct2D1.Factory d2DFactory;
    private readonly SharpDX.DirectWrite.Factory dwFactory;

    public Direct2DFactoryManager()
    {
        wicFactory = new SharpDX.WIC.ImagingFactory();
        d2DFactory = new SharpDX.Direct2D1.Factory();
        dwFactory = new SharpDX.DirectWrite.Factory();
    }

    public SharpDX.WIC.ImagingFactory WicFactory
    {
        get
        {
            return wicFactory;
        }
    }

    public SharpDX.Direct2D1.Factory D2DFactory
    {
        get
        {
            return d2DFactory;
        }
    }

    public SharpDX.DirectWrite.Factory DwFactory
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
