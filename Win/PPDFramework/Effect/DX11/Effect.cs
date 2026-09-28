using PPDFramework.Texture;
using PPDFramework.Vertex;
using SharpDX;
using SharpDX.D3DCompiler;
using System.Collections.Generic;

namespace PPDFramework.Effect.DX11
{
    class Effect : EffectBase
    {
        SharpDX.Direct3D11.Effect effect;
        string technique;
        SharpDX.Direct3D11.EffectPass[] currentPasses;
        SharpDX.Direct3D11.EffectPass currentPass;
        Dictionary<string, SharpDX.Direct3D11.EffectPass[]> techniquePasses;
        VertexDeclarationBase vertexDeclaration;
        HashSet<EffectHandle> textureSetHandles;

        public override string Technique
        {
            get { return technique; }
            set { technique = value; }
        }

        public ShaderBytecode ShaderBytecode
        {
            get
            {
                var tech = SuppressRelease(effect.GetTechniqueByIndex(0));
                return SuppressRelease(tech.GetPassByIndex(0)).Description.Signature;
            }
        }

        public override VertexDeclarationBase VertexDeclaration
        {
            get { return vertexDeclaration; }
        }

        private SharpDX.Direct3D11.DeviceContext Context
        {
            get { return ((PPDFramework.DX11.PPDDevice)device).Context; }
        }

        public Effect(PPDDevice device, byte[] bytes) : base(device)
        {
            textureSetHandles = new HashSet<EffectHandle>();
            techniquePasses = new Dictionary<string, SharpDX.Direct3D11.EffectPass[]>();
            effect = new SharpDX.Direct3D11.Effect((SharpDX.Direct3D11.Device)device.Device, bytes);
            vertexDeclaration = VertexDeclarationFactoryManager.Factory.Create(device, this);
        }

        /// <summary>
        /// Effects11のテクニック、パス、変数はCOMの参照カウントを持たないため、
        /// SharpDXのファイナライザーからReleaseが呼ばれるとアクセス違反になります。
        /// これらはエフェクトが所有しているので、ファイナライザーを抑制します。
        /// </summary>
        internal static T SuppressRelease<T>(T obj) where T : class
        {
            if (obj != null)
            {
                System.GC.SuppressFinalize(obj);
            }
            return obj;
        }

        public override EffectHandleBase GetParameter(string name)
        {
            return new EffectHandle(effect.GetVariableByName(name));
        }

        protected override void DisposeResource()
        {
            base.DisposeResource();
            if (vertexDeclaration != null)
            {
                vertexDeclaration.Dispose();
                vertexDeclaration = null;
            }
            if (effect != null)
            {
                effect.Dispose();
                effect = null;
            }
        }

        public override int Begin()
        {
            if (!techniquePasses.TryGetValue(technique, out currentPasses))
            {
                var tech = SuppressRelease(effect.GetTechniqueByName(technique));
                currentPasses = new SharpDX.Direct3D11.EffectPass[tech.Description.PassCount];
                for (var i = 0; i < currentPasses.Length; i++)
                {
                    currentPasses[i] = SuppressRelease(tech.GetPassByIndex(i));
                }
                techniquePasses.Add(technique, currentPasses);
            }
            return currentPasses.Length;
        }

        public override void BeginPass(int pass)
        {
            currentPass = currentPasses[pass];
            currentPass.Apply(Context);
        }

        public override void End()
        {
            currentPasses = null;
        }

        public override void EndPass()
        {
            if (textureSetHandles.Count > 0)
            {
                foreach (var handle in textureSetHandles)
                {
                    handle.ShaderResource.SetResource(null);
                }
                textureSetHandles.Clear();
                currentPass.Apply(Context);
            }
            currentPass = null;
        }

        public override void SetTexture(EffectHandleBase handle, TextureBase texture)
        {
            var effectHandle = (EffectHandle)handle;
            if (texture is Texture.DX11.Texture dx11Texture)
            {
                dx11Texture.Flush(Context);
                effectHandle.ShaderResource.SetResource(dx11Texture._ShaderResourceView);
                textureSetHandles.Add(effectHandle);
            }
            else
            {
                effectHandle.ShaderResource.SetResource(null);
            }
        }

        public override void SetValue(EffectHandleBase handle, float value)
        {
            ((EffectHandle)handle).Scalar.Set(value);
        }

        public override void SetValue(EffectHandleBase handle, float[] values)
        {
            ((EffectHandle)handle).Scalar.Set(values);
        }

        public override void SetValue(EffectHandleBase handle, Matrix matrix)
        {
            ((EffectHandle)handle).Matrix.SetMatrix(matrix);
        }

        public override void SetValue(EffectHandleBase handle, Matrix[] matrices)
        {
            ((EffectHandle)handle).Matrix.SetMatrix(matrices);
        }

        public override void SetValue(EffectHandleBase handle, Vector2 vector)
        {
            ((EffectHandle)handle).Vector.Set(vector);
        }

        public override void SetValue(EffectHandleBase handle, Vector3 vector)
        {
            ((EffectHandle)handle).Vector.Set(vector);
        }

        public override void SetValue(EffectHandleBase handle, Vector4 vector)
        {
            ((EffectHandle)handle).Vector.Set(vector);
        }

        public override void SetValue(EffectHandleBase handle, Color4 color)
        {
            ((EffectHandle)handle).Vector.Set(color);
        }

        public override void SetValue<T>(EffectHandleBase handle, T value)
        {
            var size = Utilities.SizeOf<T>();
            using (var dataStream = new DataStream(size, true, true))
            {
                dataStream.Write(value);
                ((EffectHandle)handle)._EffectVariable.SetRawValue(dataStream.DataPointer, 0, size);
            }
        }

        public override void SetValue<T>(EffectHandleBase handle, T[] values)
        {
            var size = Utilities.SizeOf<T>() * values.Length;
            using (var dataStream = new DataStream(size, true, true))
            {
                dataStream.WriteRange(values, 0, values.Length);
                ((EffectHandle)handle)._EffectVariable.SetRawValue(dataStream.DataPointer, 0, size);
            }
        }

        public override void CommitChanges()
        {
            if (currentPass != null)
            {
                currentPass.Apply(Context);
            }
        }
    }
}
