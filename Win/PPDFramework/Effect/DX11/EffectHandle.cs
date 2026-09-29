namespace PPDFramework.Effect.DX11
{
    /// <summary>
    /// エフェクトのパラメーター（定数バッファの変数、またはテクスチャ）を指します。
    /// </summary>
    class EffectHandle : EffectHandleBase
    {
        /// <summary>
        /// 定数バッファの変数です。テクスチャの場合は null です。
        /// </summary>
        public EffectData.Variable Variable
        {
            get;
            private set;
        }

        /// <summary>
        /// テクスチャのスロットです。テクスチャでない場合は -1 です。
        /// </summary>
        public int TextureSlot
        {
            get;
            private set;
        }

        public EffectHandle(EffectData.Variable variable, int textureSlot)
        {
            Variable = variable;
            TextureSlot = textureSlot;
        }
    }
}
