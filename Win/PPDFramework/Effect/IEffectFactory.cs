namespace PPDFramework.Effect
{
    /// <summary>
    /// Effectのファクトリインターフェースです。
    /// </summary>
    public interface IEffectFactory
    {
        /// <summary>
        /// メモリからエフェクトを作成します。
        /// </summary>
        /// <param name="device"></param>
        /// <param name="bytes"></param>
        /// <returns></returns>
        EffectBase FromMemory(PPDDevice device, byte[] bytes);
    }
}
