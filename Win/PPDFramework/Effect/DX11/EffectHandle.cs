namespace PPDFramework.Effect.DX11
{
    class EffectHandle : EffectHandleBase
    {
        public SharpDX.Direct3D11.EffectVariable _EffectVariable
        {
            get;
            private set;
        }

        public EffectHandle(SharpDX.Direct3D11.EffectVariable effectVariable)
        {
            _EffectVariable = effectVariable;
        }

        protected override void DisposeResource()
        {
            if (_EffectVariable != null)
            {
                _EffectVariable.Dispose();
                _EffectVariable = null;
            }
        }
    }
}
