using PPDFramework.Texture;
using PPDFramework.Vertex;
using SharpDX;

namespace PPDFramework.Effect.DX9
{
    class Effect : EffectBase
    {
        SharpDX.Direct3D9.Effect effect;
        string technique;
        Vertex.VertexDeclarationBase vertexDeclaration;

        public override string Technique
        {
            get { return technique; }
            set
            {
                if (technique != value)
                {
                    technique = value;
                    effect.Technique = technique;
                }
            }
        }

        public override VertexDeclarationBase VertexDeclaration
        {
            get { return vertexDeclaration; }
        }

        public Effect(PPDDevice device, byte[] bytes) : base(device)
        {
            effect = SharpDX.Direct3D9.Effect.FromMemory((SharpDX.Direct3D9.Device)((PPDFramework.DX9.PPDDevice)device).Device, bytes, SharpDX.Direct3D9.ShaderFlags.SkipValidation);
            vertexDeclaration = VertexDeclarationFactoryManager.Factory.Create(device, this);
        }

        public override void OnLostDevice()
        {
            effect.OnLostDevice();
        }

        public override void OnResetDevice()
        {
            effect.OnResetDevice();
        }

        public override EffectHandleBase GetParameter(string name)
        {
            return new EffectHandle(effect.GetParameter(null, name));
        }

        protected override void DisposeResource()
        {
            base.DisposeResource();
            if (effect != null)
            {
                effect.Dispose();
                effect = null;
            }
        }

        public override int Begin()
        {
            return effect.Begin();
        }

        public override void BeginPass(int pass)
        {
            effect.BeginPass(pass);
        }

        public override void End()
        {
            effect.End();
        }

        public override void EndPass()
        {
            effect.EndPass();
        }

        public override void SetTexture(EffectHandleBase handle, TextureBase texture)
        {
            effect.SetTexture(((DX9.EffectHandle)handle)._EffectHandle, texture == null ? null : ((Texture.DX9.Texture)texture)._Texture);
        }

        public override void SetValue(EffectHandleBase handle, float value)
        {
            effect.SetValue(((DX9.EffectHandle)handle)._EffectHandle, value);
        }

        public override void SetValue(EffectHandleBase handle, float[] values)
        {
            effect.SetValue(((DX9.EffectHandle)handle)._EffectHandle, values);
        }

        public override void SetValue(EffectHandleBase handle, Matrix matrix)
        {
            effect.SetValue(((DX9.EffectHandle)handle)._EffectHandle, matrix);
        }

        public override void SetValue(EffectHandleBase handle, Matrix[] matrices)
        {
            effect.SetValue(((DX9.EffectHandle)handle)._EffectHandle, matrices);
        }

        public override void SetValue(EffectHandleBase handle, Vector2 vector)
        {
            effect.SetValue(((DX9.EffectHandle)handle)._EffectHandle, vector);
        }

        public override void SetValue(EffectHandleBase handle, Vector3 vector)
        {
            effect.SetValue(((DX9.EffectHandle)handle)._EffectHandle, vector);
        }

        public override void SetValue(EffectHandleBase handle, Vector4 vector)
        {
            effect.SetValue(((DX9.EffectHandle)handle)._EffectHandle, vector);
        }

        public override void SetValue(EffectHandleBase handle, Color4 color)
        {
            effect.SetValue(((DX9.EffectHandle)handle)._EffectHandle, color);
        }

        public override void SetValue<T>(EffectHandleBase handle, T value)
        {
            effect.SetValue(((DX9.EffectHandle)handle)._EffectHandle, value);
        }

        public override void SetValue<T>(EffectHandleBase handle, T[] values)
        {
            effect.SetValue(((DX9.EffectHandle)handle)._EffectHandle, values);
        }

        public override void CommitChanges()
        {
            effect.CommitChanges();
        }
    }
}
