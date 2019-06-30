using System;
using System.IO;

namespace PPDFramework
{
    /// <summary>
    /// パスマネージャーのクラスです。
    /// </summary>
    public class PathManager
    {
        string baseDir;

        /// <summary>
        /// 基底フォルダを取得します。
        /// </summary>
        public string BaseDir
        {
            get { return baseDir; }
        }

        /// <summary>
        /// コンストラクタです。
        /// </summary>
        /// <param name="baseDir"></param>
        public PathManager(string baseDir)
        {
            this.baseDir = baseDir;
        }

        /// <summary>
        /// 結合します。
        /// </summary>
        /// <param name="args"></param>
        /// <returns></returns>
        public PathObject Combine(params string[] args)
        {
            var newArgs = new string[args.Length + 1];
            newArgs[0] = baseDir;
            Array.Copy(args, 0, newArgs, 1, args.Length);
            return new PathObject(Path.Combine(newArgs));
        }
    }
}
