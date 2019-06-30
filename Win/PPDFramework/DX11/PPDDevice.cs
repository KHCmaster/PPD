using PPDFramework.Texture;
using PPDFramework.Vertex;
using SharpDX;
using SharpDX.Direct3D11;
using SharpDX.DXGI;
using SharpDX.Mathematics.Interop;
using System;
using System.Collections.Generic;

namespace PPDFramework.DX11
{
    class PPDDevice : PPDFramework.PPDDevice
    {
        SharpDX.Direct3D11.Device device;
        SwapChain swapChain;
        SwapChainDescription swapChainDescription;
        RenderTargetView originalRenderTargetView;
        Texture2D backBuffer;
        VertexDeclarationBase vertexDeclaration;
        Dictionary<PrimitiveType, SharpDX.Direct3D.PrimitiveTopology> primitiveTypeMap = new Dictionary<PrimitiveType, SharpDX.Direct3D.PrimitiveTopology>
        {
            { PrimitiveType.LineList, SharpDX.Direct3D.PrimitiveTopology.LineList },            { PrimitiveType.LineStrip, SharpDX.Direct3D.PrimitiveTopology.LineStrip },            { PrimitiveType.PointList, SharpDX.Direct3D.PrimitiveTopology.PointList },            { PrimitiveType.TriangleFan, SharpDX.Direct3D.PrimitiveTopology.TriangleStrip },            { PrimitiveType.TriangleList, SharpDX.Direct3D.PrimitiveTopology.TriangleList },            { PrimitiveType.TriangleStrip, SharpDX.Direct3D.PrimitiveTopology.TriangleStrip }        };
        VertexBufferBase currentBuffer;

        public override ComObject Device
        {
            get { return device; }
        }

        public DeviceContext Context
        {
            get;
            private set;
        }

        internal override VertexDeclarationBase VertexDeclaration
        {
            get { return vertexDeclaration; }
            set
            {
                if (vertexDeclaration != value)
                {
                    vertexDeclaration = value;
                    Context.InputAssembler.InputLayout = ((Vertex.DX11.VertexDeclaration)vertexDeclaration)._InputLayout;
                }
            }
        }

        internal override string ShaderNamePrefix
        {
            get { return "d3d11_"; }
        }

        /// <summary>
        /// コンストラクタです
        /// </summary>
        /// <param name="handle"></param>
        /// <param name="width"></param>
        /// <param name="height"></param>
        /// <param name="expectedWidth"></param>
        /// <param name="expectedHeight"></param>
        /// <param name="expectedRatio"></param>
        public PPDDevice(IntPtr handle, int width, int height, float expectedWidth, float expectedHeight, float expectedRatio) :
            base(width, height, expectedWidth, expectedHeight, expectedRatio)
        {
            swapChainDescription = new SwapChainDescription
            {
                BufferCount = 1,
                ModeDescription = new ModeDescription(width, height, new Rational(60, 1), Format.R8G8B8A8_UNorm),
                IsWindowed = true,
                OutputHandle = handle,
                SampleDescription = new SampleDescription(1, 0),
                Usage = Usage.RenderTargetOutput
            };

            var flag = DeviceCreationFlags.Debug;
            try
            {
                SharpDX.Direct3D11.Device.CreateWithSwapChain(SharpDX.Direct3D.DriverType.Hardware, flag, swapChainDescription, out device, out swapChain);
            }
            catch (SharpDXException)
            {
                try
                {
                    SharpDX.Direct3D11.Device.CreateWithSwapChain(SharpDX.Direct3D.DriverType.Software, flag, swapChainDescription, out device, out swapChain);
                }
                catch (SharpDXException)
                {
                    try
                    {
                        SharpDX.Direct3D11.Device.CreateWithSwapChain(SharpDX.Direct3D.DriverType.Reference, flag, swapChainDescription, out device, out swapChain);
                    }
                    catch (SharpDXException)
                    {
                        throw;
                    }
                }
            }
            Context = device.ImmediateContext;
            backBuffer = Texture2D.FromSwapChain<Texture2D>(swapChain, 0);
            originalRenderTargetView = new RenderTargetView(device, backBuffer);
            Context.Rasterizer.SetViewport(new Viewport(0, 0, width, height, 0.0f, 1.0f));
            Context.Rasterizer.State = new RasterizerState(device, new RasterizerStateDescription
            {
                CullMode = CullMode.None,
                FillMode = FillMode.Solid
            });
        }

        /// <summary>
        /// 現在のレンダーターゲットを設定します。
        /// </summary>
        /// <param name="workSpaceTexture"></param>
        internal override void SetRenderTarget(WorkspaceTexture workSpaceTexture)
        {
            renderTarget = workSpaceTexture;
            Context.OutputMerger.SetTargets(((Texture.DX11.Texture)workSpaceTexture.Texture)._RenderTargetView);
        }

        /// <summary>
        /// サーフェースをコピーします。
        /// </summary>
        /// <param name="src"></param>
        /// <param name="dest"></param>
        internal override void StretchRectangle(WorkspaceTexture src, WorkspaceTexture dest)
        {
            Context.CopyResource(((Texture.DX11.Texture)src.Texture)._Texture, ((Texture.DX11.Texture)dest.Texture)._Texture);
        }

        /// <summary>
        /// サーフェースをコピーします。
        /// </summary>
        /// <param name="src"></param>
        /// <param name="sourceRect"></param>
        /// <param name="dest"></param>
        /// <param name="destPoint"></param>
        internal override void StretchRectangle(TextureBase src, RawRectangle? sourceRect, TextureBase dest, RawPoint? destPoint)
        {
            throw new NotImplementedException();
        }

        internal override void Clear(Color4 color)
        {
            var currentRenderTargets = Context.OutputMerger.GetRenderTargets(1, out DepthStencilView currentView);
            Context.ClearRenderTargetView(currentRenderTargets[0], color);
        }

        internal override void ResetDevice()
        {
        }

        internal override void DrawStart()
        {
            Reset();
            SetRenderTarget(Workspace.Primal);
            Context.ClearRenderTargetView(((Texture.DX11.Texture)Workspace.Primal.Texture)._RenderTargetView, PPDColors.Black);
        }

        internal override void DrawEnd()
        {
            Context.CopyResource(((Texture.DX11.Texture)Workspace.Primal.Texture)._Texture, backBuffer);
        }

        internal override void Present()
        {
            swapChain.Present(1, PresentFlags.None);
        }

        internal override void DrawPrimitives(PrimitiveType primitiveType, int offset, int primitiveCount)
        {
            Context.InputAssembler.PrimitiveTopology = primitiveTypeMap[primitiveType];
            Context.Draw(primitiveCount + 2, offset);
        }

        internal override void DrawIndexedPrimitives(PrimitiveType primitiveType, int baseVertexIndex, int minVertexIndex, int numVertices, int startIndex, int primCount)
        {
            throw new NotImplementedException();
        }

        internal override void DrawUserPrimitives<T>(PrimitiveType primitiveType, int offset, int primitiveCount, T[] data)
        {
            throw new NotImplementedException();
        }

        internal override void SetStreamSource(VertexBufferBase buffer)
        {
            if (currentBuffer == buffer)
            {
                return;
            }
            currentBuffer = buffer;
            Context.InputAssembler.SetVertexBuffers(0, new VertexBufferBinding(((Vertex.DX11.VertexBuffer)buffer)._Buffer, ColoredTexturedVertex.Size, 0));
        }

        internal override void SetIndices(IndexBufferBase index)
        {
            throw new NotImplementedException();
        }

        public override void BackBufferToFile(string filepath)
        {
            /* TODO
            Texture2D.ToFile(Context, backBuffer, ImageFileFormat.Png, filepath);
            */
        }

        public override void SetScissorRect(Rectangle rectangle, bool enabled)
        {
            Context.Rasterizer.SetScissorRectangle(rectangle.Left, rectangle.Top, rectangle.Right, rectangle.Bottom);
            var desc = Context.Rasterizer.State.Description;
            desc.IsScissorEnabled = enabled;
            Context.Rasterizer.State = new RasterizerState(device, desc);
        }

        /// <summary>
        /// 破棄します。
        /// </summary>
        protected override void DisposeResource()
        {
            base.DisposeResource();
        }
    }
}
