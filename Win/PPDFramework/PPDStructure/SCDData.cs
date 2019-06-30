using System;

namespace PPDFramework.PPDStructure
{
    /// <summary>
    /// サウンド変更データクラス
    /// </summary>
    public class SCDData
    {
        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="time">時間</param>
        /// <param name="buttontype">ボタンタイプ</param>
        /// <param name="soundindex">サウンドインデックス</param>
        public SCDData(float time, ButtonType buttontype, UInt16 soundindex)
        {
            Time = time;
            ButtonType = buttontype;
            SoundIndex = soundindex;
        }

        /// <summary>
        /// 時間
        /// </summary>
        public float Time
        {
            get;
            private set;
        }

        /// <summary>
        /// ボタンタイプ
        /// </summary>
        public ButtonType ButtonType
        {
            get;
            private set;
        }

        /// <summary>
        /// サウンドインデックス
        /// </summary>
        public UInt16 SoundIndex
        {
            get;
            private set;
        }
    }
}
