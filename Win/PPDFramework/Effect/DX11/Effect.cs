using PPDFramework.Texture;
using PPDFramework.Vertex;
using SharpDX;
using SharpDX.D3DCompiler;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace PPDFramework.Effect.DX11
{
    class Effect : EffectBase
    {
        SharpDX.Direct3D11.Effect effect;
        string technique;
        SharpDX.Direct3D11.EffectTechnique currentTechnique;
        SharpDX.Direct3D11.EffectPass currentPass;
        Vertex.VertexDeclarationBase vertexDeclaration;
        HashSet<EffectHandleBase> textureSetHandles;

        public override string Technique
        {
            get { return technique; }
            set { technique = value; }
        }

        public ShaderBytecode ShaderBytecode
        {
            get { return effect.GetTechniqueByIndex(0).GetPassByIndex(0).Description.Signature; }
        }

        public override VertexDeclarationBase VertexDeclaration
        {
            get { return vertexDeclaration; }
        }

        public Effect(PPDDevice device, byte[] bytes) : base(device)
        {
            textureSetHandles = new HashSet<EffectHandleBase>();
            effect = new SharpDX.Direct3D11.Effect((SharpDX.Direct3D11.Device)((PPDFramework.DX11.PPDDevice)device).Device, bytes);
            var tech = effect.GetTechniqueByIndex(0);
            vertexDeclaration = Vertex.VertexDeclarationFactoryManager.Factory.Create(device, this);
        }

        public override EffectHandleBase GetParameter(string name)
        {
            return new EffectHandle(effect.GetVariableByName(name));
        }

        protected override void DisposeResource()
        {
            if (effect != null)
            {
                effect.Dispose();
                effect = null;
            }
        }

        public override int Begin()
        {
            currentTechnique = effect.GetTechniqueByName(technique);
            return currentTechnique.Description.PassCount;
        }

        public override void BeginPass(int pass)
        {
            currentPass = currentTechnique.GetPassByIndex(pass);
            currentPass.Apply(((PPDFramework.DX11.PPDDevice)device).Context);
        }

        public override void End()
        {
        }

        public override void EndPass()
        {
            foreach (var handle in textureSetHandles)
            {
                ((DX11.EffectHandle)handle)._EffectVariable.AsShaderResource().SetResource(null);
                textureSetHandles.Add(handle);
            }
            textureSetHandles.Clear();
            currentPass.Apply(((PPDFramework.DX11.PPDDevice)device).Context);
        }

        public override void SetTexture(EffectHandleBase handle, TextureBase texture)
        {
            ((DX11.EffectHandle)handle)._EffectVariable.AsShaderResource().SetResource(((Texture.DX11.Texture)texture)._ShaderResourceView);
            textureSetHandles.Add(handle);
        }

        public override void SetValue(EffectHandleBase handle, float value)
        {
            ((DX11.EffectHandle)handle)._EffectVariable.AsScalar().Set(value);
        }

        public override void SetValue(EffectHandleBase handle, float[] values)
        {
            ((DX11.EffectHandle)handle)._EffectVariable.AsScalar().Set(values);
        }

        public override void SetValue(EffectHandleBase handle, Matrix matrix)
        {
            ((DX11.EffectHandle)handle)._EffectVariable.AsMatrix().SetMatrix(matrix);
        }

        public override void SetValue(EffectHandleBase handle, Matrix[] matrices)
        {
            ((DX11.EffectHandle)handle)._EffectVariable.AsMatrix().SetMatrix(matrices);
        }

        public override void SetValue(EffectHandleBase handle, Vector2 vector)
        {
            ((DX11.EffectHandle)handle)._EffectVariable.AsVector().Set(vector);
        }

        public override void SetValue(EffectHandleBase handle, Vector3 vector)
        {
            ((DX11.EffectHandle)handle)._EffectVariable.AsVector().Set(vector);
        }

        public override void SetValue(EffectHandleBase handle, Vector4 vector)
        {
            ((DX11.EffectHandle)handle)._EffectVariable.AsVector().Set(vector);
        }

        public override void SetValue(EffectHandleBase handle, Color4 color)
        {
            ((DX11.EffectHandle)handle)._EffectVariable.AsVector().Set(color);
        }

        public override void SetValue<T>(EffectHandleBase handle, T value)
        {
            using (var dataStream = new DataStream(Marshal.SizeOf(typeof(T)), true, true))
            {
                dataStream.Write(value);
                dataStream.Seek(0, System.IO.SeekOrigin.Begin);
                ((DX11.EffectHandle)handle)._EffectVariable.SetRawValue(dataStream, 1);
            }
        }

        public override void SetValue<T>(EffectHandleBase handle, T[] values)
        {
            using (var dataStream = new DataStream(Marshal.SizeOf(typeof(T)) * values.Length, true, true))
            {
                dataStream.WriteRange(values, 0, values.Length);
                dataStream.Seek(0, System.IO.SeekOrigin.Begin);
                ((DX11.EffectHandle)handle)._EffectVariable.SetRawValue(dataStream, values.Length);
            }
        }

        public override void CommitChanges()
        {
        }
    }
}
