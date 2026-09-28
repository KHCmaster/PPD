namespace PPDFramework.Effect.DX11
{
    class EffectHandle : EffectHandleBase
    {
        SharpDX.Direct3D11.EffectShaderResourceVariable shaderResource;
        SharpDX.Direct3D11.EffectScalarVariable scalar;
        SharpDX.Direct3D11.EffectMatrixVariable matrix;
        SharpDX.Direct3D11.EffectVectorVariable vector;

        public SharpDX.Direct3D11.EffectVariable _EffectVariable
        {
            get;
            private set;
        }

        public SharpDX.Direct3D11.EffectShaderResourceVariable ShaderResource
        {
            get { return shaderResource ?? (shaderResource = Effect.SuppressRelease(_EffectVariable.AsShaderResource())); }
        }

        public SharpDX.Direct3D11.EffectScalarVariable Scalar
        {
            get { return scalar ?? (scalar = Effect.SuppressRelease(_EffectVariable.AsScalar())); }
        }

        public SharpDX.Direct3D11.EffectMatrixVariable Matrix
        {
            get { return matrix ?? (matrix = Effect.SuppressRelease(_EffectVariable.AsMatrix())); }
        }

        public SharpDX.Direct3D11.EffectVectorVariable Vector
        {
            get { return vector ?? (vector = Effect.SuppressRelease(_EffectVariable.AsVector())); }
        }

        public EffectHandle(SharpDX.Direct3D11.EffectVariable effectVariable)
        {
            _EffectVariable = Effect.SuppressRelease(effectVariable);
        }

        protected override void DisposeResource()
        {
            // エフェクトの変数はエフェクトが所有しているので、ここでは解放しません
            _EffectVariable = null;
            shaderResource = null;
            scalar = null;
            matrix = null;
            vector = null;
        }
    }
}
