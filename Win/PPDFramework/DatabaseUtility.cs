using System.Data.SQLite;

namespace PPDFramework
{
    /// <summary>
    /// データベースのユーティリティです。
    /// </summary>
    public static class DatabaseUtility
    {
        /// <summary>
        /// Int32を取得します。
        /// </summary>
        /// <param name="reader">リーダーです。</param>
        /// <param name="index">インデックスです。</param>
        /// <returns></returns>
        public static int GetInt32(SQLiteDataReader reader, int index)
        {
            try
            {
                return reader.GetInt32(index);
            }
            catch
            {
                return 0;
            }
        }

        /// <summary>
        /// Floatを取得します。
        /// </summary>
        /// <param name="reader">リーダーです。</param>
        /// <param name="index">インデックスです。</param>
        /// <returns></returns>
        public static float GetFloat(SQLiteDataReader reader, int index)
        {
            try
            {
                return reader.GetFloat(index);
            }
            catch
            {
                return 0;
            }
        }
    }
}
