namespace PPDFramework.Effect.DX9
{
    class EffectFactory : IEffectFactory
    {
        public EffectBase FromMemory(PPDDevice device, byte[] bytes)
        {
            return new DX9.Effect(device, bytes);
        }
    }
}
