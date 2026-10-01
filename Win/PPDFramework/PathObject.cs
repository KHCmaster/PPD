namespace PPDFramework
{
    /// <summary>
    /// パスの情報を表すクラスです。
    /// </summary>
    /// <remarks>
    /// コンストラクタです。
    /// </remarks>
    /// <param name="path"></param>
    public class PathObject(string path)
    {
        readonly string path = path;

        /// <summary>
        /// 文字列化します。
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return path;
        }

        /// <summary>
        /// 文字列に暗黙的にキャストします。
        /// </summary>
        /// <param name="path"></param>
        public static implicit operator string(PathObject path)
        {
            return path.ToString();
        }

        /// <summary>
        /// 絶対パスとして取得します。
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static PathObject Absolute(string path)
        {
            return new PathObject(path);
        }
    }
}
