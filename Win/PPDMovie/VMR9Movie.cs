using DirectShow;
using PPDFramework;
using PPDFramework.Shaders;
using PPDFramework.Texture;
using SharpDX.Direct3D9;
using System;
using System.IO;
using System.Runtime.InteropServices;

namespace PPDMovie
{
    public class VMR9Movie : MovieBase
    {
        private IBaseFilter vmr9;
        protected Allocator9 allocator;
        private IntPtr userID = new IntPtr(unchecked((int)0xACDCACDC));
        private Direct3D d3d;

        public VMR9Movie(PPDDevice device, Direct3D d3d, string filename) : base(device, filename)
        {
            this.d3d = d3d;
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
                this.graphBuilder2 = (IFilterGraph2)new FilterGraph();
                vmr9 = (IBaseFilter)new VideoMixingRenderer9();
                var config = vmr9 as IVMRFilterConfig9;
                hr = config.SetRenderingMode(VMR9Mode.Renderless);
                DsError.ThrowExceptionForHR(hr);
                hr = config.SetNumberOfStreams(1);
                DsError.ThrowExceptionForHR(hr);

                allocator = new Allocator9((SharpDX.Direct3D9.Device)device.Device, d3d);
                var vmrSurfAllocNotify = (IVMRSurfaceAllocatorNotify9)vmr9;
                hr = vmrSurfAllocNotify.AdviseSurfaceAllocator(userID, allocator);
                DsError.ThrowExceptionForHR(hr);
                hr = allocator.AdviseNotify(vmrSurfAllocNotify);
                DsError.ThrowExceptionForHR(hr);
                var mixerControl = (IVMRMixerControl9)vmr9;
                hr = mixerControl.SetMixingPrefs(VMR9MixerPrefs.RenderTargetYUV | VMR9MixerPrefs.NoDecimation | VMR9MixerPrefs.ARAdjustXorY | VMR9MixerPrefs.BiLinearFiltering);
                DsError.ThrowExceptionForHR(hr);

                var dc9 = (IVMRDeinterlaceControl9)vmr9;
                var pref = VMR9DeinterlacePrefs.Weave;
                dc9.GetDeinterlacePrefs(out pref);
                hr = dc9.SetDeinterlaceMode(unchecked((int)0xFFFFFFFF), Guid.NewGuid());
                DsError.ThrowExceptionForHR(hr);
                try
                {
                    hr = graphBuilder2.AddFilter(vmr9, "Video Mixing Renderer 9");
                }
                catch (Exception e)
                {
                    throw new Exception("Fatal Error in Movie Loading", e);
                }
                DsError.ThrowExceptionForHR(hr);

                base.Initialize();

                if (allocator != null)
                {
                    this.width = allocator.VideoSize.Width;
                    this.height = allocator.VideoSize.Height;
                    this.maxu = (float)allocator.VideoSize.Width / allocator.TextureSize.Width;
                    this.maxv = (float)allocator.VideoSize.Height / allocator.TextureSize.Height;
                }
            }
            catch (Exception e)
            {
                throw new Exception("Fatal Error in Movie Loading", e);
            }
            return 0;
        }

        public override void Play()
        {
            base.Play();
            allocator.TextureCreated = false;
        }

        protected override bool OnCanDraw(AlphaBlendContext alphaBlendContext, int depth, int childIndex)
        {
            return allocator != null && allocator.TextureCreated;
        }

        public override void releaseCOM()
        {
            if (Initialized)
            {
                // Release DirectShow interfaces
                if (vmr9 != null)
                {
                    while (Marshal.ReleaseComObject(this.vmr9) > 0)
                    {
                    }
                    this.vmr9 = null;
                }
                if (allocator != null)
                {
                    allocator.Dispose();
                    allocator = null;
                }
                base.releaseCOM();
            }
        }

        public override TextureBase Texture
        {
            get
            {
                if (allocator == null || !allocator.TextureCreated) return null;
                return allocator.ManagedTexture;
            }
        }

        public override bool Rotated
        {
            get
            {
                return false;
            }
        }
    }
}