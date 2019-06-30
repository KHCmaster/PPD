namespace PPDFramework.Mod
{
    /// <summary>
    /// 数値の設定を扱うクラスです。
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public abstract class NumberModSetting<T> : TemplateModSetting<T> where T : struct
    {
        /// <summary>
        /// 最小値を取得します。
        /// </summary>
        public T MinValue
        {
            get;
            private set;
        }

        /// <summary>
        /// クラスの最小値を取得します。
        /// </summary>
        protected abstract T ClassMinValue
        {
            get;
        }

        /// <summary>
        /// 最小値がクラスの最小値かどうかを取得します。
        /// </summary>
        public bool IsMinClassMinValue
        {
            get
            {
                return MinValue.Equals(ClassMinValue);
            }
        }

        /// <summary>
        /// 最大値を取得します。
        /// </summary>
        public T MaxValue
        {
            get;
            private set;
        }

        /// <summary>
        /// クラスの最大値を取得します。
        /// </summary>
        protected abstract T ClassMaxValue
        {
            get;
        }

        /// <summary>
        /// 最大値がクラスの最大値かどうかを取得します。
        /// </summary>
        public bool IsMaxClassMaxValue
        {
            get
            {
                return MaxValue.Equals(ClassMaxValue);
            }
        }

        /// <summary>
        /// コンストラクタです。
        /// </summary>
        /// <param name="key"></param>
        /// <param name="name"></param>
        /// <param name="description"></param>
        /// <param name="defaultValue"></param>
        /// <param name="minValue"></param>
        /// <param name="maxValue"></param>
        protected NumberModSetting(string key, string name, string description, T defaultValue, T minValue, T maxValue)
            : base(key, name, description, defaultValue)
        {
            MinValue = minValue;
            MaxValue = maxValue;
        }
    }
}
