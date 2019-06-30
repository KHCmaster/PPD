using System.Collections.Generic;
using System.Linq;

namespace PPDFramework.PPDStructure.PPDData
{
    /// <summary>
    /// PPDDataクラス
    /// </summary>
    public abstract class MarkDataBase
    {
        private Dictionary<string, string> parameters;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="x">X</param>
        /// <param name="y">Y</param>
        /// <param name="angle">回転角</param>
        /// <param name="time">時間</param>
        /// <param name="buttontype">ボタンタイプ</param>
        /// <param name="id">ID</param>
        protected MarkDataBase(float x, float y, float angle, float time, ButtonType buttontype, uint id)
        {
            X = x;
            Y = y;
            Angle = angle;
            Time = time;
            ButtonType = buttontype;
            ID = id;
            parameters = new Dictionary<string, string>();
        }

        /// <summary>
        /// X
        /// </summary>
        public float X
        {
            get;
            private set;
        }

        /// <summary>
        /// Y
        /// </summary>
        public float Y
        {
            get;
            private set;
        }

        /// <summary>
        /// 回転角
        /// </summary>
        public float Angle
        {
            get;
            private set;
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
        /// ID
        /// </summary>
        public uint ID
        {
            get;
            private set;
        }

        /// <summary>
        /// パラメーター。
        /// </summary>
        public KeyValuePair<string, string>[] Parameters
        {
            get { return parameters.ToArray(); }
        }

        /// <summary>
        /// パラメーターの数。
        /// </summary>
        public int ParameterCount
        {
            get { return parameters.Count; }
        }

        /// <summary>
        /// パラメーターを追加します。
        /// </summary>
        /// <param name="key">キー。</param>
        /// <param name="value">値。</param>
        public void AddParameter(string key, string value)
        {
            parameters[key] = value;
        }
    }
}
