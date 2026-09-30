using PPDFramework.Texture;
using PPDFramework.Vertex;
using System.Numerics;
using PPDMath;
using Matrix = System.Numerics.Matrix4x4;
using Color4 = Vortice.Mathematics.Color4;
using Vortice.Direct3D11;
using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace PPDFramework.Effect.DX11
{
    /// <summary>
    /// DirectX11 のエフェクトです。
    /// ShaderCompiler が .fx から作ったバイナリ（EffectData）を読み、シェーダー・定数バッファ・テクスチャ・ブレンドステートを自前で設定します。
    /// </summary>
    class Effect : EffectBase
    {
        const int MaxTextureSlots = 8;

        EffectData data;
        ID3D11VertexShader[] vertexShaders;
        ID3D11PixelShader[] pixelShaders;
        ID3D11Buffer constantBuffer;
        byte[] constants;
        bool constantsDirty = true;
        ID3D11ShaderResourceView[] textures = new ID3D11ShaderResourceView[MaxTextureSlots];
        string technique;
        EffectData.Technique currentTechnique;
        EffectData.Pass currentPass;
        VertexDeclarationBase vertexDeclaration;

        static readonly ID3D11ShaderResourceView[] nullShaderResources = new ID3D11ShaderResourceView[MaxTextureSlots];

        public override string Technique
        {
            get { return technique; }
            set { technique = value; }
        }

        /// <summary>
        /// 入力レイアウトの作成に使う頂点シェーダーのバイトコードを取得します。
        /// </summary>
        public byte[] InputSignatureBytecode
        {
            get { return data.VertexShaders[0]; }
        }

        public override VertexDeclarationBase VertexDeclaration
        {
            get { return vertexDeclaration; }
        }

        private PPDFramework.DX11.PPDDevice DX11Device
        {
            get { return (PPDFramework.DX11.PPDDevice)device; }
        }

        private ID3D11DeviceContext Context
        {
            get { return DX11Device.Context; }
        }

        public Effect(PPDDevice device, byte[] bytes) : base(device)
        {
            data = new EffectData(bytes);
            vertexShaders = new ID3D11VertexShader[data.VertexShaders.Length];
            pixelShaders = new ID3D11PixelShader[data.PixelShaders.Length];
            constants = new byte[Math.Max(16, data.ConstantBufferSize)];
            constantBuffer = ((ID3D11Device)device.Device).CreateBuffer(new BufferDescription
            {
                ByteWidth = (uint)constants.Length,
                Usage = ResourceUsage.Default,
                BindFlags = BindFlags.ConstantBuffer,
                CPUAccessFlags = CpuAccessFlags.None
            }, (SubresourceData?)null);
            vertexDeclaration = VertexDeclarationFactoryManager.Factory.Create(device, this);
        }

        public override EffectHandleBase GetParameter(string name)
        {
            data.Variables.TryGetValue(name, out EffectData.Variable variable);
            if (!data.TextureSlots.TryGetValue(name, out int slot))
            {
                slot = -1;
            }
            // 存在しない（最適化で消えた）パラメーターへの設定は無視する（Effects11 と同じ）
            return new EffectHandle(variable, slot);
        }

        protected override void DisposeResource()
        {
            base.DisposeResource();
            if (vertexDeclaration != null)
            {
                vertexDeclaration.Dispose();
                vertexDeclaration = null;
            }
            DisposeAll(vertexShaders);
            DisposeAll(pixelShaders);
            if (constantBuffer != null)
            {
                constantBuffer.Dispose();
                constantBuffer = null;
            }
        }

        private static void DisposeAll<T>(T[] objects) where T : class, IDisposable
        {
            for (var i = 0; i < objects.Length; i++)
            {
                if (objects[i] != null)
                {
                    objects[i].Dispose();
                    objects[i] = null;
                }
            }
        }

        public override int Begin()
        {
            if (!data.Techniques.TryGetValue(technique, out currentTechnique))
            {
                throw new ArgumentException("Technique not found: " + technique);
            }
            return currentTechnique.Passes.Length;
        }

        public override void BeginPass(int pass)
        {
            currentPass = currentTechnique.Passes[pass];
            var context = Context;
            context.VSSetShader(GetVertexShader(currentPass.VertexShaderIndex));
            context.PSSetShader(GetPixelShader(currentPass.PixelShaderIndex));
            context.OMSetBlendState(DX11Device.GetBlendState(currentPass.BlendState));
            context.VSSetConstantBuffer(0, constantBuffer);
            context.PSSetConstantBuffer(0, constantBuffer);
            context.PSSetSampler(0, DX11Device.LinearSampler);
            ApplyChanges();
        }

        public override void End()
        {
            currentTechnique = null;
        }

        public override void EndPass()
        {
            // 次に描画先として使うテクスチャがシェーダーリソースとして残らないように、パイプラインからだけ外す。
            // 設定した値は残す（複数パスのエフェクトで次のパスでも同じテクスチャを使うため）。
            Context.PSSetShaderResources(0, (uint)nullShaderResources.Length, nullShaderResources);
            currentPass = null;
        }

        public override void CommitChanges()
        {
            if (currentPass != null)
            {
                ApplyChanges();
            }
        }

        private void ApplyChanges()
        {
            var context = Context;
            if (constantsDirty)
            {
                var handle = GCHandle.Alloc(constants, GCHandleType.Pinned);
                try
                {
                    context.UpdateSubresource(constantBuffer, 0, null, handle.AddrOfPinnedObject(), 0, 0);
                }
                finally
                {
                    handle.Free();
                }
                constantsDirty = false;
            }
            context.PSSetShaderResources(0, (uint)textures.Length, textures);
        }

        private ID3D11VertexShader GetVertexShader(int index)
        {
            return vertexShaders[index] ?? (vertexShaders[index] = ((ID3D11Device)device.Device).CreateVertexShader(data.VertexShaders[index], null));
        }

        private ID3D11PixelShader GetPixelShader(int index)
        {
            // AlphaBlend は 400 を超えるピクセルシェーダーを持つので、使うときに作る
            return pixelShaders[index] ?? (pixelShaders[index] = ((ID3D11Device)device.Device).CreatePixelShader(data.PixelShaders[index], null));
        }

        public override void SetTexture(EffectHandleBase handle, TextureBase texture)
        {
            var slot = ((EffectHandle)handle).TextureSlot;
            if (slot < 0)
            {
                return;
            }
            if (texture is Texture.DX11.Texture dx11Texture)
            {
                dx11Texture.Flush(Context);
                textures[slot] = dx11Texture._ShaderResourceView;
            }
            else
            {
                textures[slot] = null;
            }
        }

        public override void SetValue(EffectHandleBase handle, float value)
        {
            var variable = ((EffectHandle)handle).Variable;
            if (variable == null)
            {
                return;
            }
            if (variable.BaseType == EffectData.VariableBaseType.Int)
            {
                WriteInt(variable.Offset, (int)value);
            }
            else
            {
                WriteFloat(variable.Offset, value);
            }
        }

        public override void SetValue(EffectHandleBase handle, float[] values)
        {
            var variable = ((EffectHandle)handle).Variable;
            if (variable == null)
            {
                return;
            }
            // cbuffer の配列は要素ごとに 16 バイト境界に揃っている
            var count = Math.Min(values.Length, Math.Max(1, variable.Elements));
            for (var i = 0; i < count; i++)
            {
                WriteFloat(variable.Offset + i * variable.Stride, values[i]);
            }
        }

        public override void SetValue(EffectHandleBase handle, Matrix matrix)
        {
            // シェーダーは row_major なので、Matrix4x4 をそのまま書き込む
            WriteStruct(((EffectHandle)handle).Variable, matrix, 0);
        }

        public override void SetValue(EffectHandleBase handle, Matrix[] matrices)
        {
            var variable = ((EffectHandle)handle).Variable;
            if (variable == null)
            {
                return;
            }
            var count = Math.Min(matrices.Length, Math.Max(1, variable.Elements));
            for (var i = 0; i < count; i++)
            {
                WriteStruct(variable, matrices[i], i * variable.Stride);
            }
        }

        public override void SetValue(EffectHandleBase handle, Vector2 vector)
        {
            WriteStruct(((EffectHandle)handle).Variable, vector, 0);
        }

        public override void SetValue(EffectHandleBase handle, Vector3 vector)
        {
            WriteStruct(((EffectHandle)handle).Variable, vector, 0);
        }

        public override void SetValue(EffectHandleBase handle, Vector4 vector)
        {
            WriteStruct(((EffectHandle)handle).Variable, vector, 0);
        }

        public override void SetValue(EffectHandleBase handle, Color4 color)
        {
            WriteStruct(((EffectHandle)handle).Variable, color, 0);
        }

        public override void SetValue<T>(EffectHandleBase handle, T value)
        {
            WriteStruct(((EffectHandle)handle).Variable, value, 0);
        }

        public override void SetValue<T>(EffectHandleBase handle, T[] values)
        {
            var variable = ((EffectHandle)handle).Variable;
            if (variable == null)
            {
                return;
            }
            var count = Math.Min(values.Length, Math.Max(1, variable.Elements));
            for (var i = 0; i < count; i++)
            {
                WriteStruct(variable, values[i], i * variable.Stride);
            }
        }

        private void WriteFloat(int offset, float value)
        {
            var bytes = BitConverter.GetBytes(value);
            System.Buffer.BlockCopy(bytes, 0, constants, offset, 4);
            constantsDirty = true;
        }

        private void WriteInt(int offset, int value)
        {
            var bytes = BitConverter.GetBytes(value);
            System.Buffer.BlockCopy(bytes, 0, constants, offset, 4);
            constantsDirty = true;
        }

        private void WriteStruct<T>(EffectData.Variable variable, T value, int elementOffset) where T : struct
        {
            if (variable == null)
            {
                return;
            }
            var offset = variable.Offset + elementOffset;
            // 変数の範囲を超えて書き込まないようにする
            var size = Math.Min(Unsafe.SizeOf<T>(), Math.Min(variable.Stride, constants.Length - offset));
            if (size <= 0)
            {
                return;
            }
            var buffer = new byte[Unsafe.SizeOf<T>()];
            MemoryMarshal.Write(buffer, in value);
            System.Buffer.BlockCopy(buffer, 0, constants, offset, size);
            constantsDirty = true;
        }
    }
}
