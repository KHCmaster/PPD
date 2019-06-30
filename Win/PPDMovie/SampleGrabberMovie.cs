using DirectShow;
using PPDFramework;
using PPDFramework.Texture;
using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;

namespace PPDMovie
{
    [ComImport, Guid("B98D13E7-55DB-4385-A33D-09FD1BA26338")]
    class LAVSplitter
    {

    }

    [ComImport, Guid("EE30215D-164F-4A92-A4EB-9D4C13390F9F")]
    class LAVVideoDecoder
    {

    }

    [ComImport, Guid("E8E73B6B-4CB3-44A4-BE99-4F7BCB96E491")]
    class LAVAudioDecoder
    {

    }

    [ComImport, Guid("79376820-07D0-11CF-A24D-0020AFD79767")]
    class DirectSoundDevice
    {

    }

    [ComImport, Guid("70E102B0-5556-11CE-97C0-00AA0055595A")]
    class VideoRenderer
    {

    }

    public class SampleGrabberMovie : MovieBase, ISampleGrabberCB
    {
        private ISampleGrabber sampleGrabber;
        private TextureBase[] textures;
        private int currentTextureIndex;
        private IVideoWindow videoWindow;
        IBaseFilter lavSplitter;
        IBaseFilter lavVideoDecoder;
        IBaseFilter lavAudioDecoder;
        IBaseFilter soundDevice;
        IBaseFilter videoRenderer;

        public SampleGrabberMovie(PPDDevice device, string filename)
            : base(device, filename)
        {
            autoRender = false;
        }

        public override int Initialize()
        {
            if (!File.Exists(filename))
            {
                return -1;
            }

            try
            {
                int hr = 0;
                graphBuilder2 = (IFilterGraph2)new FilterGraph();
                lavSplitter = new LAVSplitter() as IBaseFilter;
                lavVideoDecoder = new LAVVideoDecoder() as IBaseFilter;
                lavAudioDecoder = new LAVAudioDecoder() as IBaseFilter;
                var lavSplitterSource = lavSplitter as IFileSourceFilter;
                soundDevice = new DirectSoundDevice() as IBaseFilter;
                videoRenderer = new VideoRenderer() as IBaseFilter;
                lavSplitterSource.Load(filename, null);
                hr = graphBuilder2.AddFilter(lavSplitter, "LAV Splitter");
                DsError.ThrowExceptionForHR(hr);
                hr = graphBuilder2.AddFilter(lavVideoDecoder, "LAV Video Decoder");
                DsError.ThrowExceptionForHR(hr);
                hr = graphBuilder2.AddFilter(lavAudioDecoder, "LAV Audio Decoder");
                DsError.ThrowExceptionForHR(hr);
                hr = graphBuilder2.AddFilter(soundDevice, "Default Direct Sound Device");
                DsError.ThrowExceptionForHR(hr);
                hr = graphBuilder2.AddFilter(videoRenderer, "Video Renderer");
                DsError.ThrowExceptionForHR(hr);
                var videoPin = GetPin(lavSplitter, "Video");
                var audioPin = GetPin(lavSplitter, "Audio");
                var videoDecoderInputPin = GetPin(lavVideoDecoder, "Input");
                var videoDecoderOutputPin = GetPin(lavVideoDecoder, "Output");
                var audioDecoderInputPin = GetPin(lavAudioDecoder, "Input");
                var audioDecoderOutputPin = GetPin(lavAudioDecoder, "Output");
                var soundInputPin = GetPin(soundDevice, "Audio Input pin (rendered)");
                var videoRendererInputPin = GetPin(videoRenderer, "Input");
                hr = graphBuilder2.Connect(videoPin, videoDecoderInputPin);
                DsError.ThrowExceptionForHR(hr);
                hr = graphBuilder2.Connect(audioPin, audioDecoderInputPin);
                DsError.ThrowExceptionForHR(hr);
                hr = graphBuilder2.Connect(audioDecoderOutputPin, soundInputPin);
                DsError.ThrowExceptionForHR(hr);
                sampleGrabber = new SampleGrabber() as ISampleGrabber;
                var amMediaType = new AMMediaType
                {
                    majorType = MediaType.Video,
                    subType = MediaSubType.RGB32,
                    formatType = FormatType.VideoInfo
                };
                hr = sampleGrabber.SetMediaType(amMediaType);
                DsError.ThrowExceptionForHR(hr);
                DsUtils.FreeAMMediaType(amMediaType);
                hr = graphBuilder2.AddFilter((IBaseFilter)sampleGrabber, "SampleGrabber");
                DsError.ThrowExceptionForHR(hr);
                var sampleGrabberInputPin = GetPin((IBaseFilter)sampleGrabber, "Input");
                var sampleGrabberOutputPin = GetPin((IBaseFilter)sampleGrabber, "Output");
                hr = graphBuilder2.Connect(videoDecoderOutputPin, sampleGrabberInputPin);
                DsError.ThrowExceptionForHR(hr);
                hr = graphBuilder2.Connect(sampleGrabberOutputPin, videoRendererInputPin);
                DsError.ThrowExceptionForHR(hr);
                base.Initialize();
                sampleGrabber.SetCallback(this, 1);
                sampleGrabber.SetBufferSamples(true);
                sampleGrabber.SetOneShot(false);
                var mediaType = new AMMediaType();
                videoPin.ConnectionMediaType(mediaType);
                var bitmapInfoHeader = (BitmapInfoHeader)mediaType;
                this.width = bitmapInfoHeader.Width;
                this.height = bitmapInfoHeader.Height;
                this.maxu = 1;
                this.maxv = 1;
                textures = new TextureBase[5];
                for (var i = 0; i < textures.Length; i++)
                {
                    textures[i] = TextureFactoryManager.Factory.Create(device, width, height, 1, false);
                }

                videoWindow = (IVideoWindow)graphBuilder2;

                hr = videoWindow.put_Visible((int)OABool.False);
                DsError.ThrowExceptionForHR(hr);
                hr = videoWindow.put_WindowState((int)WindowState.Hide);
                DsError.ThrowExceptionForHR(hr);
                hr = videoWindow.SetWindowPosition(-1000, -1000, 10, 10);
                DsError.ThrowExceptionForHR(hr);
                videoWindow.put_AutoShow((int)OABool.False);
                DsError.ThrowExceptionForHR(hr);
                hr = hr = videoWindow.put_Owner(MovieUtility.Window);
                DsError.ThrowExceptionForHR(hr);
            }
            catch (Exception e)
            {
                throw new Exception("Fatal Error in Movie Loading", e);
            }
            return 0;
        }

        private IPin GetPin(IBaseFilter filter, string name)
        {
            filter.EnumPins(out IEnumPins enumPins);
            IntPtr ptr = IntPtr.Zero;
            IPin[] pin = new IPin[1];
            while (true)
            {
                enumPins.Next(1, pin, ptr);
                var pinInfo = new PinInfo();
                pin[0].QueryPinInfo(out pinInfo);
                if (pinInfo.name == name)
                {
                    return pin[0];
                }
            }
        }

        public override void releaseCOM()
        {
            if (Initialized)
            {
                TextureBase[] _textures = null;
                _textures = this.textures;
                this.textures = null;
                var thread = new Thread(() =>
                {
                    Thread.Sleep(1000);
                    if (_textures != null)
                    {
                        foreach (var texture in _textures)
                        {
                            if (texture != null)
                            {
                                texture.Dispose();
                            }
                        }
                    }
                })
                {
                    IsBackground = true
                };
                thread.Start();

                if (sampleGrabber != null)
                {
                    Marshal.ReleaseComObject(sampleGrabber);
                    sampleGrabber = null;
                }

                if (videoWindow != null)
                {
                    Marshal.ReleaseComObject(videoWindow); videoWindow = null;
                }
                if (lavSplitter != null)
                {
                    Marshal.ReleaseComObject(lavSplitter); lavSplitter = null;
                }
                if (lavVideoDecoder != null)
                {
                    Marshal.ReleaseComObject(lavVideoDecoder); lavVideoDecoder = null;
                }
                if (lavAudioDecoder != null)
                {
                    Marshal.ReleaseComObject(lavAudioDecoder); lavAudioDecoder = null;
                }
                if (soundDevice != null)
                {
                    Marshal.ReleaseComObject(soundDevice); soundDevice = null;
                }
                if (videoRenderer != null)
                {
                    Marshal.ReleaseComObject(videoRenderer); videoRenderer = null;
                }

                base.releaseCOM();
            }
        }

        public override TextureBase Texture
        {
            get
            {
                if (textures == null)
                {
                    return null;
                }
                return textures[currentTextureIndex];
            }
        }

        #region ISampleGrabberCB メンバ

        public int BufferCB(double SampleTime, IntPtr pBuffer, int BufferLen)
        {
            if (Initialized)
            {
                var nextTextureIndex = currentTextureIndex + 1;
                if (nextTextureIndex >= textures.Length)
                {
                    nextTextureIndex = 0;
                }
                textures[nextTextureIndex].Write(pBuffer, BufferLen);
                currentTextureIndex = nextTextureIndex;
            }
            return 0;
        }

        public int SampleCB(double SampleTime, IntPtr pSample)
        {
            return 0;
        }

        #endregion

        public override bool Rotated
        {
            get
            {
                return true;
            }
        }
    }
}
