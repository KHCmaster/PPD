using PPDFramework.Sprites;
using PPDFramework.Texture;
using PPDFramework.Vertex;
using SharpDX;
using SharpDX.Direct3D;
using SharpDX.Direct3D11;
using SharpDX.DXGI;
using SharpDX.Mathematics.Interop;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace PPDFramework.DX11
{
    /// <summary>
    /// DirectX11のPPDデバイスです。
    /// </summary>
    class PPDDevice : PPDFramework.PPDDevice
    {
        const int MaxFanPrimitiveCount = 4096;

        SharpDX.Direct3D11.Device device;
        SwapChain swapChain;
        Texture2D backBuffer;
        VertexDeclarationBase vertexDeclaration;
        VertexBufferBase currentBuffer;
        IndexBufferBase currentIndex;
        RasterizerState scissorEnabledState;
        RasterizerState scissorDisabledState;
        SharpDX.Direct3D11.Buffer fanIndexBuffer;
        Dictionary<string, BlendState> blendStates;
        Dictionary<PrimitiveType, PrimitiveTopology> primitiveTypeMap = new Dictionary<PrimitiveType, PrimitiveTopology>
        {
            { PrimitiveType.PointList, PrimitiveTopology.PointList },
            { PrimitiveType.LineList, PrimitiveTopology.LineList },
            { PrimitiveType.LineStrip, PrimitiveTopology.LineStrip },
            { PrimitiveType.TriangleList, PrimitiveTopology.TriangleList },
            { PrimitiveType.TriangleStrip, PrimitiveTopology.TriangleStrip },
            { PrimitiveType.TriangleFan, PrimitiveTopology.TriangleList }
        };

        /// <summary>
        /// デバイスを取得します。
        /// </summary>
        public override ComObject Device
        {
            get { return device; }
        }

        /// <summary>
        /// デバイスコンテキストを取得します。
        /// </summary>
        public DeviceContext Context
        {
            get;
            private set;
        }

        /// <summary>
        /// シェーダーで使うサンプラーを取得します（Common.fx の LinearSampler）。
        /// </summary>
        internal SamplerState LinearSampler
        {
            get;
            private set;
        }

        /// <summary>
        /// VertexDeclarationを取得、設定します。
        /// </summary>
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

        /// <summary>
        /// シェーダープリフィックスを取得します。
        /// </summary>
        internal override string ShaderNamePrefix
        {
            get { return "d3d11_"; }
        }

        /// <summary>
        /// 使用しているDirectXのバージョン名を取得します。
        /// </summary>
        public override string DirectXVersion
        {
            get { return "DirectX 11"; }
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
            var swapChainDescription = new SwapChainDescription
            {
                BufferCount = 1,
                ModeDescription = new ModeDescription(width, height, new Rational(60, 1), Texture.DX11.TextureFactory.TextureFormat),
                IsWindowed = true,
                OutputHandle = handle,
                SampleDescription = new SampleDescription(1, 0),
                SwapEffect = SwapEffect.Discard,
                Usage = Usage.RenderTargetOutput
            };
            var featureLevels = new[] { FeatureLevel.Level_11_0, FeatureLevel.Level_10_1, FeatureLevel.Level_10_0 };
            var flag = DeviceCreationFlags.BgraSupport;
            try
            {
                SharpDX.Direct3D11.Device.CreateWithSwapChain(DriverType.Hardware, flag, featureLevels, swapChainDescription, out device, out swapChain);
            }
            catch (SharpDXException)
            {
                SharpDX.Direct3D11.Device.CreateWithSwapChain(DriverType.Warp, flag, featureLevels, swapChainDescription, out device, out swapChain);
            }
            // フルスクリーンの切り替えはアプリケーション側で行うので、Alt+Enterを無効にします
            using (var factory = swapChain.GetParent<SharpDX.DXGI.Factory>())
            {
                factory.MakeWindowAssociation(handle, WindowAssociationFlags.IgnoreAll);
            }
            Context = device.ImmediateContext;
            backBuffer = Texture2D.FromSwapChain<Texture2D>(swapChain, 0);
            Context.Rasterizer.SetViewport(new Viewport(0, 0, width, height, 0.0f, 1.0f));
            scissorDisabledState = new RasterizerState(device, new RasterizerStateDescription
            {
                CullMode = CullMode.None,
                FillMode = FillMode.Solid,
                IsDepthClipEnabled = true,
                IsScissorEnabled = false
            });
            scissorEnabledState = new RasterizerState(device, new RasterizerStateDescription
            {
                CullMode = CullMode.None,
                FillMode = FillMode.Solid,
                IsDepthClipEnabled = true,
                IsScissorEnabled = true
            });
            Context.Rasterizer.State = scissorDisabledState;
            CreateShaderStates();
            CreateFanIndexBuffer();
            SpriteBatch = new SpriteBatch(this);
        }

        /// <summary>
        /// Common.fx で定義しているブレンドステートとサンプラーを作成します。
        /// </summary>
        private void CreateShaderStates()
        {
            LinearSampler = new SamplerState(device, new SamplerStateDescription
            {
                Filter = Filter.MinMagLinearMipPoint,
                AddressU = TextureAddressMode.Clamp,
                AddressV = TextureAddressMode.Clamp,
                AddressW = TextureAddressMode.Clamp,
                ComparisonFunction = Comparison.Never,
                MaximumAnisotropy = 1,
                MinimumLod = float.MinValue,
                MaximumLod = float.MaxValue
            });
            blendStates = new Dictionary<string, BlendState>
            {
                { "NoBlend", CreateBlendState(false, BlendOption.One, BlendOption.Zero) },
                { "AlphaBlending", CreateBlendState(true, BlendOption.SourceAlpha, BlendOption.InverseSourceAlpha) },
                { "PremultipliedAlphaBlending", CreateBlendState(true, BlendOption.One, BlendOption.InverseSourceAlpha) }
            };
        }

        private BlendState CreateBlendState(bool enabled, BlendOption source, BlendOption destination)
        {
            var description = BlendStateDescription.Default();
            description.RenderTarget[0].IsBlendEnabled = enabled;
            description.RenderTarget[0].SourceBlend = source;
            description.RenderTarget[0].DestinationBlend = destination;
            description.RenderTarget[0].BlendOperation = BlendOperation.Add;
            description.RenderTarget[0].SourceAlphaBlend = source;
            description.RenderTarget[0].DestinationAlphaBlend = destination;
            description.RenderTarget[0].AlphaBlendOperation = BlendOperation.Add;
            description.RenderTarget[0].RenderTargetWriteMask = ColorWriteMaskFlags.All;
            return new BlendState(device, description);
        }

        /// <summary>
        /// 名前からブレンドステートを取得します。
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        internal BlendState GetBlendState(string name)
        {
            if (!blendStates.TryGetValue(name, out BlendState blendState))
            {
                throw new ArgumentException("Unknown blend state: " + name);
            }
            return blendState;
        }

        private void CreateFanIndexBuffer()
        {
            var indices = new int[MaxFanPrimitiveCount * 3];
            for (var i = 0; i < MaxFanPrimitiveCount; i++)
            {
                indices[i * 3] = 0;
                indices[i * 3 + 1] = i + 1;
                indices[i * 3 + 2] = i + 2;
            }
            fanIndexBuffer = SharpDX.Direct3D11.Buffer.Create(device, BindFlags.IndexBuffer, indices, 0, ResourceUsage.Immutable);
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
            ((Texture.DX11.Texture)dest).CopyFrom((Texture.DX11.Texture)src, sourceRect, destPoint);
        }

        /// <summary>
        /// クリアします。
        /// </summary>
        /// <param name="color"></param>
        internal override void Clear(Color4 color)
        {
            Context.ClearRenderTargetView(((Texture.DX11.Texture)GetRenderTarget().Texture)._RenderTargetView, color);
        }

        /// <summary>
        /// 描画を開始します。
        /// </summary>
        internal override void DrawStart()
        {
            currentBuffer = null;
            currentIndex = null;
            Reset();
            var newRenderTarget = GetRenderTarget();
            SetRenderTarget(newRenderTarget);
            Context.Rasterizer.SetViewport(new Viewport(0, 0, Width, Height, 0.0f, 1.0f));
            Context.ClearRenderTargetView(((Texture.DX11.Texture)newRenderTarget.Texture)._RenderTargetView, new Color4(0f, 0f, 0f, 1f));
            SetScissorRect(new Rectangle(0, 0, Width, Height), false);
        }

        /// <summary>
        /// 描画を終了します。
        /// </summary>
        internal override void DrawEnd()
        {
            SpriteBatch.Flush();
            SpriteBatch.End();
            Context.OutputMerger.SetTargets((RenderTargetView)null);
            Context.CopyResource(((Texture.DX11.Texture)GetRenderTarget().Texture)._Texture, backBuffer);
        }

        /// <summary>
        /// 表示します。
        /// </summary>
        internal override void Present()
        {
            swapChain.Present(1, PresentFlags.None);
        }

        /// <summary>
        /// プリミティブを描画します。
        /// </summary>
        /// <param name="primitiveType"></param>
        /// <param name="offset"></param>
        /// <param name="primitiveCount"></param>
        internal override void DrawPrimitives(PrimitiveType primitiveType, int offset, int primitiveCount)
        {
            Context.InputAssembler.PrimitiveTopology = primitiveTypeMap[primitiveType];
            if (primitiveType == PrimitiveType.TriangleFan)
            {
                DrawFan(offset, primitiveCount);
                return;
            }
            Context.Draw(GetVertexCount(primitiveType, primitiveCount), offset);
        }

        /// <summary>
        /// プリミティブを描画します。
        /// </summary>
        /// <param name="primitiveType"></param>
        /// <param name="baseVertexIndex"></param>
        /// <param name="minVertexIndex"></param>
        /// <param name="numVertices"></param>
        /// <param name="startIndex"></param>
        /// <param name="primCount"></param>
        internal override void DrawIndexedPrimitives(PrimitiveType primitiveType, int baseVertexIndex, int minVertexIndex, int numVertices, int startIndex, int primCount)
        {
            if (primitiveType == PrimitiveType.TriangleFan)
            {
                throw new NotSupportedException("TriangleFan is not supported for indexed drawing in DirectX11");
            }
            Context.InputAssembler.PrimitiveTopology = primitiveTypeMap[primitiveType];
            Context.DrawIndexed(GetVertexCount(primitiveType, primCount), startIndex, baseVertexIndex);
        }


        private void DrawFan(int offset, int primitiveCount)
        {
            if (primitiveCount > MaxFanPrimitiveCount)
            {
                throw new NotSupportedException("Too many primitives for TriangleFan");
            }
            Context.InputAssembler.SetIndexBuffer(fanIndexBuffer, Format.R32_UInt, 0);
            currentIndex = null;
            Context.DrawIndexed(primitiveCount * 3, 0, offset);
        }

        private static int GetVertexCount(PrimitiveType primitiveType, int primitiveCount)
        {
            switch (primitiveType)
            {
                case PrimitiveType.PointList:
                    return primitiveCount;
                case PrimitiveType.LineList:
                    return primitiveCount * 2;
                case PrimitiveType.LineStrip:
                    return primitiveCount + 1;
                case PrimitiveType.TriangleList:
                    return primitiveCount * 3;
                default:
                    return primitiveCount + 2;
            }
        }

        /// <summary>
        /// ストリームソースを設定します。
        /// </summary>
        /// <param name="buffer"></param>
        internal override void SetStreamSource(VertexBufferBase buffer)
        {
            var dx11Buffer = (Vertex.DX11.VertexBuffer)buffer;
            dx11Buffer.Flush(Context);
            if (currentBuffer == buffer)
            {
                return;
            }
            currentBuffer = buffer;
            Context.InputAssembler.SetVertexBuffers(0, new VertexBufferBinding(dx11Buffer._Buffer, ColoredTexturedVertex.Size, 0));
        }

        /// <summary>
        /// インデックスを設定します。
        /// </summary>
        /// <param name="index"></param>
        internal override void SetIndices(IndexBufferBase index)
        {
            if (currentIndex == index)
            {
                return;
            }
            currentIndex = index;
            Context.InputAssembler.SetIndexBuffer(((Vertex.DX11.IndexBuffer)index)._Buffer, Format.R32_UInt, 0);
        }

        /// <summary>
        /// バックバッファーをファイルに書き込みます。
        /// </summary>
        /// <param name="filepath"></param>
        public override void BackBufferToFile(string filepath)
        {
            var pixels = Texture.DX11.Texture.ReadPixels(Context, backBuffer);
            using (var bitmap = Texture.DX11.Texture.ToBitmap(pixels, backBuffer.Description.Width, backBuffer.Description.Height))
            {
                bitmap.Save(filepath, System.Drawing.Imaging.ImageFormat.Png);
            }
        }

        /// <summary>
        /// シザーレクトを設定します。
        /// </summary>
        /// <param name="rectangle"></param>
        /// <param name="enabled"></param>
        public override void SetScissorRect(Rectangle rectangle, bool enabled)
        {
            SpriteBatch.SetScissorRect(rectangle, enabled);
        }

        /// <summary>
        /// 切り取りをすぐにデバイスに反映します。
        /// </summary>
        /// <param name="rectangle"></param>
        /// <param name="enabled"></param>
        internal override void ApplyScissorRect(Rectangle rectangle, bool enabled)
        {
            Context.Rasterizer.SetScissorRectangle(rectangle.Left, rectangle.Top, rectangle.Right, rectangle.Bottom);
            Context.Rasterizer.State = enabled ? scissorEnabledState : scissorDisabledState;
        }

        /// <summary>
        /// スプライト描画用のアルファブレンドの状態を設定します。
        /// DirectX11ではエフェクトのパスで設定されるので何もしません。
        /// </summary>
        /// <param name="pa"></param>
        internal override void SetSpriteAlphaBlend(bool pa)
        {
        }

        /// <summary>
        /// 破棄します。
        /// </summary>
        protected override void DisposeResource()
        {
            base.DisposeResource();
            if (Context != null)
            {
                Context.ClearState();
                Context.Flush();
            }
            if (fanIndexBuffer != null)
            {
                fanIndexBuffer.Dispose();
                fanIndexBuffer = null;
            }
            if (blendStates != null)
            {
                foreach (var blendState in blendStates.Values)
                {
                    blendState.Dispose();
                }
                blendStates = null;
            }
            if (LinearSampler != null)
            {
                LinearSampler.Dispose();
                LinearSampler = null;
            }
            if (scissorEnabledState != null)
            {
                scissorEnabledState.Dispose();
                scissorEnabledState = null;
            }
            if (scissorDisabledState != null)
            {
                scissorDisabledState.Dispose();
                scissorDisabledState = null;
            }
            if (backBuffer != null)
            {
                backBuffer.Dispose();
                backBuffer = null;
            }
            if (swapChain != null)
            {
                swapChain.Dispose();
                swapChain = null;
            }
            if (Context != null)
            {
                Context.Dispose();
                Context = null;
            }
            if (device != null)
            {
                device.Dispose();
                device = null;
            }
        }
    }
}
