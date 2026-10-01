using System;

namespace PPDFramework.PPDStructure
{
    /// <summary>
    /// サウンド変更データクラス
    /// </summary>
    /// <remarks>
    /// コンストラクタ
    /// </remarks>
    /// <param name="time">時間</param>
    /// <param name="buttontype">ボタンタイプ</param>
    /// <param name="soundindex">サウンドインデックス</param>
    public class SCDData(float time, ButtonType buttontype, UInt16 soundindex)
    {

        /// <summary>
        /// 時間
        /// </summary>
        public float Time
        {
            get;
            private set;
        } = time;

        /// <summary>
        /// ボタンタイプ
        /// </summary>
        public ButtonType ButtonType
        {
            get;
            private set;
        } = buttontype;

        /// <summary>
        /// サウンドインデックス
        /// </summary>
        public UInt16 SoundIndex
        {
            get;
            private set;
        } = soundindex;
    }
}
