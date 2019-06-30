/****************************************************************************
While the underlying libraries are covered by LGPL, this sample is released 
as public domain.  It is distributed in the hope that it will be useful, but 
WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY 
or FITNESS FOR A PARTICULAR PURPOSE.  
*****************************************************************************/

using DirectShow;
using SharpDX;
using SharpDX.Direct3D9;
using SharpDX.Mathematics.Interop;
using System;
using System.Diagnostics;
using System.Drawing;
using System.Runtime.InteropServices;

namespace PPDMovie
{
    [ComVisible(true)]
    [ClassInterface(ClassInterfaceType.None)]
    public class Allocator9 : IVMRSurfaceAllocator9, IVMRImagePresenter9, IDisposable
    {
        // Constants
        private const int E_FAIL = unchecked((int)0x80004005);
        private const int D3DERR_INVALIDCALL = unchecked((int)0x8876086c);
        private const int DxMagicNumber = -759872593;

        private Device device;
        private Direct3D D3D;
        private AdapterInformation adapterInfo;
        private CreationParameters creationParameters;

        private Surface videoSurface;
        private IntPtr unmanagedSurface = IntPtr.Zero;

        private Texture privateTexture;
        private Surface privateSurface;

        private bool needCopy;

        private Size textureSize;
        private Size videoSize;

        private IVMRSurfaceAllocatorNotify9 vmrSurfaceAllocatorNotify;

        private bool disposed;

        public Allocator9(Device dev, Direct3D D3D)
        {
            this.D3D = D3D;
            adapterInfo = D3D.Adapters[0];

            device = dev;

            creationParameters = device.CreationParameters;
        }

        ~Allocator9()
        {
            Dispose(false);
        }

        #region Members of IDisposable

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private void Dispose(bool disposing)
        {
            if (!disposed)
            {
                DeleteSurfaces();
                disposed = true;
            }
        }

        #endregion

        public PPDFramework.Texture.DX9.Texture ManagedTexture
        {
            get;
            private set;
        }

        // Handy Properties to retrieve the texture, its size and the video size
        public Texture Texture
        {
            get
            {
                return privateTexture;
            }
        }

        public Size TextureSize
        {
            get
            {
                return textureSize;
            }
        }

        public Size VideoSize
        {
            get
            {
                return videoSize;
            }
        }

        public bool TextureCreated
        {
            get;
            set;
        }

        // Delete surfaces...
        private void DeleteSurfaces()
        {
            lock (this)
            {
                ManagedTexture = null;

                if (privateTexture != null)
                {
                    privateTexture.Dispose();
                    privateTexture = null;
                }

                if (privateSurface != null)
                {
                    privateSurface.Dispose();
                    privateSurface = null;
                }

                if (videoSurface != null)
                {
                    videoSurface.Dispose();
                    videoSurface = null;
                }
            }
        }

        // Helper method to convert interger FourCC into a readable string
        private string FourCCToStr(int fcc)
        {
            byte[] chars = new byte[4];

            if (fcc < 100)
                return fcc.ToString();

            for (int i = 0; i < 4; i++)
            {
                chars[i] = (byte)(fcc & 0xff);
                fcc = fcc >> 8;
            }

            return System.Text.Encoding.ASCII.GetString(chars);
        }

        #region Members of IVMRSurfaceAllocator9

        public int InitializeDevice(IntPtr dwUserID, ref VMR9AllocationInfo lpAllocInfo, ref int lpNumBuffers)
        {
            int hr = 0;

            Debug.WriteLine(string.Format("{0}x{1} : {2} / {3} / 0x{4:x}", lpAllocInfo.dwWidth, lpAllocInfo.dwHeight, FourCCToStr(lpAllocInfo.Format), lpAllocInfo.Pool, lpAllocInfo.dwFlags));

            // if format is YUV ? (note : 0x30303030 = "    ")
            if (lpAllocInfo.Format > 0x30303030)
            {
                // Check if the hardware support format conversion from this YUV format to the RGB desktop format
                if (!D3D.CheckDeviceFormatConversion(creationParameters.AdapterOrdinal, creationParameters.DeviceType, (Format)lpAllocInfo.Format, adapterInfo.CurrentDisplayMode.Format))
                {
                    // If not, refuse this format!
                    // The VMR9 will propose other formats supported by the downstream filter output pin.
                    return D3DERR_INVALIDCALL;
                }
            }

            try
            {
                IntPtr unmanagedDevice = device.NativePointer;
                var hMonitor = D3D.GetAdapterMonitor(adapterInfo.Adapter);

                // Give our Direct3D device to the VMR9 filter
                hr = vmrSurfaceAllocatorNotify.SetD3DDevice(unmanagedDevice, hMonitor);
                DsError.ThrowExceptionForHR(hr);

                videoSize = new Size(lpAllocInfo.dwWidth, lpAllocInfo.dwHeight);

                int width = 1;
                int height = 1;

                // If hardware require textures to power of two sized
                if ((device.Capabilities.TextureCaps & TextureCaps.Pow2) == TextureCaps.Pow2)
                {
                    // Compute the ideal size
                    while (width < lpAllocInfo.dwWidth)
                        width = width << 1;
                    while (height < lpAllocInfo.dwHeight)
                        height = height << 1;

                    // notify this change to the VMR9 filter
                    lpAllocInfo.dwWidth = width;
                    lpAllocInfo.dwHeight = height;
                }

                textureSize = new Size(lpAllocInfo.dwWidth, lpAllocInfo.dwHeight);

                // Just in case
                DeleteSurfaces();

                // if format is YUV ?
                if (lpAllocInfo.Format > 0x30303030)
                {
                    // An offscreen surface must be created
                    lpAllocInfo.dwFlags |= VMR9SurfaceAllocationFlags.OffscreenSurface;

                    // Create it
                    videoSurface = Surface.CreateOffscreenPlain(device, lpAllocInfo.dwWidth, lpAllocInfo.dwHeight, (Format)lpAllocInfo.Format, (Pool)lpAllocInfo.Pool);
                    // And get it unmanaged pointer
                    unmanagedSurface = videoSurface.NativePointer;

                    // Then create a private texture for the client application
                    privateTexture = new Texture(
                      device,
                      lpAllocInfo.dwWidth,
                      lpAllocInfo.dwHeight,
                      1,
                      Usage.RenderTarget,
                      adapterInfo.CurrentDisplayMode.Format,
                      Pool.Default
                      );
                    // Get the MipMap surface 0 for the copy (see PresentImage)
                    privateSurface = privateTexture.GetSurfaceLevel(0);

                    // This code path need a surface copy
                    needCopy = true;
                }
                else
                {
                    // in RGB pixel format
                    lpAllocInfo.dwFlags |= VMR9SurfaceAllocationFlags.TextureSurface;

                    // Simply create a texture
                    privateTexture = new Texture(
                      device,
                      lpAllocInfo.dwWidth,
                      lpAllocInfo.dwHeight,
                      1,
                      Usage.RenderTarget,
                      adapterInfo.CurrentDisplayMode.Format,
                      Pool.Default
                      );
                    // And get the MipMap surface 0 for the VMR9 filter
                    privateSurface = privateTexture.GetSurfaceLevel(0);
                    unmanagedSurface = privateSurface.NativePointer;

                    // This code path don't need a surface copy.
                    // The client appllication use the same texture the VMR9 filter use.
                    needCopy = false;
                }
                ManagedTexture = new PPDFramework.Texture.DX9.Texture(privateTexture, false);

                // This allocator only support 1 buffer.
                // Notify the VMR9 filter
                lpNumBuffers = 1;
            }

            catch (SharpDXException e)
            {
                // A Direct3D error can occure : Notify it to the VMR9 filter
                return e.ResultCode.Code;
            }
            catch
            {
                // Or else, notify a more general error
                return E_FAIL;
            }

            // This allocation is a success
            return 0;
        }

        public int TerminateDevice(IntPtr dwID)
        {
            DeleteSurfaces();
            return 0;
        }

        public int GetSurface(IntPtr dwUserID, int SurfaceIndex, int SurfaceFlags, out IntPtr lplpSurface)
        {
            lplpSurface = IntPtr.Zero;

            // If the filter ask for an invalid buffer index, return an error.
            if (SurfaceIndex >= 1)
                return E_FAIL;
            if (privateSurface == null) return E_FAIL;
            lock (this)
            {
                // IVMRSurfaceAllocator9.GetSurface documentation state that the caller release the returned 
                // interface so we must increment its reference count.
                lplpSurface = unmanagedSurface;
                Marshal.AddRef(lplpSurface);
                return 0;
            }
        }
        public int AdviseNotify(IVMRSurfaceAllocatorNotify9 lpIVMRSurfAllocNotify)
        {
            lock (this)
            {
                vmrSurfaceAllocatorNotify = lpIVMRSurfAllocNotify;

                // Give our Direct3D device to the VMR9 filter
                IntPtr unmanagedDevice = device.NativePointer;
                var hMonitor = D3D.GetAdapterMonitor(D3D.Adapters[0].Adapter);

                return vmrSurfaceAllocatorNotify.SetD3DDevice(unmanagedDevice, hMonitor);
            }
        }

        #endregion

        #region Membres de IVMRImagePresenter9

        public int StartPresenting(IntPtr dwUserID)
        {
            lock (this)
            {
                if (device == null)
                    return E_FAIL;

                return 0;
            }
        }

        public int StopPresenting(IntPtr dwUserID)
        {
            return 0;
        }

        public int PresentImage(IntPtr dwUserID, ref VMR9PresentationInfo lpPresInfo)
        {
            lock (this)
            {
                try
                {
                    TextureCreated = true;
                    // If YUV mixing is activated, a surface copy is needed
                    if (needCopy)
                    {
                        // Use StretchRectangle to do the Pixel Format conversion
                        device.StretchRectangle(
                          videoSurface,
                          new RawRectangle(0, 0, videoSize.Width, videoSize.Height),
                          privateSurface,
                          new RawRectangle(0, 0, videoSize.Width, videoSize.Height),
                          TextureFilter.None
                          );
                    }
                }
                catch (SharpDXException e)
                {
                    // A Direct3D error can occure : Notify it to the VMR9 filter
                    return e.ResultCode.Code;
                }
                catch
                {
                    // Or else, notify a more general error
                    return E_FAIL;
                }

                // This presentation is a success
                return 0;
            }
        }
        #endregion

    }
}