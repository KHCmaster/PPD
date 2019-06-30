namespace PPDFramework.Effect.DX11
{
    class EffectFactory : IEffectFactory
    {
        public EffectBase FromMemory(PPDDevice device, byte[] bytes)
        {
            return new DX11.Effect(device, bytes);
        }
    }
}
