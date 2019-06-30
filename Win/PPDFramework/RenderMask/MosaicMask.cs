using PPDFramework.Shaders;

namespace PPDFramework.RenderMask
{
    /// <summary>
    /// モザイクマスクのクラスです。
    /// </summary>
    public class MosaicMask : RenderMaskBase
    {
        /// <summary>
        /// 有効化かどうかを取得します。
        /// </summary>
        public override bool Enabled
        {
            get { return Size > 0; }
        }

        /// <summary>
        /// 優先度を取得します。
        /// </summary>
        public override int Priority
        {
            get { return 0; }
        }

        /// <summary>
        /// サイズを取得します。
        /// </summary>
        public int Size
        {
            get;
            set;
        }

        /// <summary>
        /// 描画します。
        /// </summary>
        /// <param name="device"></param>
        /// <param name="maskTexture"></param>
        public override void Draw(PPDDevice device, WorkspaceTexture maskTexture)
        {
            device.GetModule<MosaicFilter>().Draw(device, maskTexture, Size);
        }
    }
}
