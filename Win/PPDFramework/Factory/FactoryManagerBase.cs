namespace PPDFramework.Factory
{
#pragma warning disable RECS0014 // If all fields, properties and methods members are static, the class can be made static.
    /// <summary>
    /// FactoryManagerです。
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class FactoryManagerBase<T>
#pragma warning restore RECS0014 // If all fields, properties and methods members are static, the class can be made static.
    {
        static T factory;

        /// <summary>
        /// ファクトリを取得します。
        /// </summary>
        public static T Factory
        {
            get { return factory; }
        }

        /// <summary>
        /// 初期化します。
        /// </summary>
        /// <param name="factory"></param>
        public static void Initialize(T factory)
        {
            FactoryManagerBase<T>.factory = factory;
        }
    }
}
