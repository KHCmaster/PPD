using PPDFramework.Texture;
using PPDFramework.Vertex;
using SharpDX;

namespace PPDFramework.Effect
{
    /// <summary>
    /// エフェクトのクラスです。
    /// </summary>
    public abstract class EffectBase : ResettableComponent
    {
        /// <summary>
        /// コンストラクタです。
        /// </summary>
        /// <param name="device"></param>
        protected EffectBase(PPDDevice device) : base(device)
        {
        }

        /// <summary>
        /// 入力頂点の情報を取得します。
        /// </summary>
        public abstract VertexDeclarationBase VertexDeclaration { get; }

        /// <summary>
        /// テクニックを取得、設定します。
        /// </summary>
        public abstract string Technique { get; set; }

        /// <summary>
        /// パラメーターを取得します。
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public abstract EffectHandleBase GetParameter(string name);

        /// <summary>
        /// 値を設定します。
        /// </summary>
        /// <param name="handle"></param>
        /// <param name="value"></param>
        public abstract void SetValue(EffectHandleBase handle, float value);

        /// <summary>
        /// 値を設定します。
        /// </summary>
        /// <param name="handle"></param>
        /// <param name="values"></param>
        public abstract void SetValue(EffectHandleBase handle, float[] values);

        /// <summary>
        /// 値を設定します。
        /// </summary>
        /// <param name="handle"></param>
        /// <param name="matrix"></param>
        public abstract void SetValue(EffectHandleBase handle, Matrix matrix);

        /// <summary>
        /// 値を設定します。
        /// </summary>
        /// <param name="handle"></param>
        /// <param name="matrices"></param>
        public abstract void SetValue(EffectHandleBase handle, Matrix[] matrices);

        /// <summary>
        /// 値を設定します。
        /// </summary>
        /// <param name="handle"></param>
        /// <param name="vector"></param>
        public abstract void SetValue(EffectHandleBase handle, Vector2 vector);

        /// <summary>
        /// 値を設定します。
        /// </summary>
        /// <param name="handle"></param>
        /// <param name="vector"></param>
        public abstract void SetValue(EffectHandleBase handle, Vector3 vector);

        /// <summary>
        /// 値を設定します。
        /// </summary>
        /// <param name="handle"></param>
        /// <param name="vector"></param>
        public abstract void SetValue(EffectHandleBase handle, Vector4 vector);

        /// <summary>
        /// 値を設定します。
        /// </summary>
        /// <param name="handle"></param>
        /// <param name="color"></param>
        public abstract void SetValue(EffectHandleBase handle, Color4 color);

        /// <summary>
        /// 値を設定します。
        /// </summary>
        /// <param name="handle"></param>
        /// <param name="value"></param>
        public abstract void SetValue<T>(EffectHandleBase handle, T value) where T : struct;

        /// <summary>
        /// 値を設定します。
        /// </summary>
        /// <param name="handle"></param>
        /// <param name="values"></param>
        public abstract void SetValue<T>(EffectHandleBase handle, T[] values) where T : struct;

        /// <summary>
        /// テクスチャを設定します。
        /// </summary>
        /// <param name="handle"></param>
        /// <param name="texture"></param>
        public abstract void SetTexture(EffectHandleBase handle, TextureBase texture);

        /// <summary>
        /// エフェクトを開始します。
        /// </summary>
        /// <returns></returns>
        public abstract int Begin();

        /// <summary>
        /// パスを開始します。
        /// </summary>
        /// <param name="pass"></param>
        public abstract void BeginPass(int pass);

        /// <summary>
        /// エフェクトを終了します。
        /// </summary>
        public abstract void End();

        /// <summary>
        /// パスを終了します。
        /// </summary>
        public abstract void EndPass();

        /// <summary>
        /// 変更をコミットします。
        /// </summary>
        public abstract void CommitChanges();
    }
}
