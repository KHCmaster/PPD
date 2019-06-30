namespace PPDFramework.Effect.DX9
{
    class EffectHandle : EffectHandleBase
    {
        public SharpDX.Direct3D9.EffectHandle _EffectHandle
        {
            get;
            private set;
        }

        public EffectHandle(SharpDX.Direct3D9.EffectHandle effectHandle)
        {
            _EffectHandle = effectHandle;
        }

        protected override void DisposeResource()
        {
            if (_EffectHandle != null)
            {
                _EffectHandle.Dispose();
                _EffectHandle = null;
            }
        }
    }
}
