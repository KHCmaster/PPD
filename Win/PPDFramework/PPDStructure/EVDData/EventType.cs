namespace PPDFramework.PPDStructure.EVDData
{
    /// <summary>
    /// イベントタイプ
    /// </summary>
    public enum EventType : byte
    {
        /// <summary>
        /// ボリューム変更
        /// </summary>
        ChangeVolume = 0,
        /// <summary>
        /// BPM変更
        /// </summary>
        ChangeBPM = 1,
        /// <summary>
        /// 急速BPM変更
        /// </summary>
        RapidChangeBPM = 2,
        /// <summary>
        /// サウンド再生方法変更
        /// </summary>
        ChangeSoundPlayMode = 3,
        /// <summary>
        /// 表示状態変更
        /// </summary>
        ChangeDisplayState = 4,
        /// <summary>
        /// 移動状態変更
        /// </summary>
        ChangeMoveState = 5,
        /// <summary>
        /// リリース時のサウンド変更
        /// </summary>
        ChangeReleaseSound = 6,
        /// <summary>
        /// ノーツタイプの変更
        /// </summary>           
        ChangeNoteType = 7,
        /// <summary>
        /// 初期化順の変更
        /// </summary>
        ChangeInitializeOrder = 8,
        /// <summary>
        /// スライドスケールの変更
        /// </summary>
        ChangeSlideScale = 9,
    }
}
