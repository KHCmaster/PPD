namespace PPDFramework
{
    /// <summary>
    /// サウンドインターフェース
    /// </summary>
    public interface ISound
    {
        /// <summary>
        /// サウンド追加
        /// </summary>
        /// <param name="filename">ファイルパス</param>
        /// <returns></returns>
        bool AddSound(string filename);
        /// <summary>
        /// サウンド追加
        /// </summary>
        /// <param name="filename">ファイル名</param>
        /// <param name="dicname">辞書名</param>
        /// <returns></returns>
        bool AddSound(string filename, string dicname);
        /// <summary>
        /// サウンド追加
        /// </summary>
        /// <param name="bytes">バイトデータ。</param>
        /// <param name="dicname">辞書名</param>
        /// <returns></returns>
        bool AddSound(byte[] bytes, string dicname);
        /// <summary>
        /// サウンド削除
        /// </summary>
        /// <param name="filename">ファイルパス</param>
        /// <returns></returns>
        bool DeleteSound(string filename);
        /// <summary>
        /// サウンド削除
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        void DeleteSoundStartsWith(string name);
        /// <summary>
        /// 全てのサウンド削除
        /// </summary>
        /// <returns></returns>
        bool DeleteAllSound();
        /// <summary>
        /// サウンド再生
        /// </summary>
        /// <param name="filename">ファイルパス</param>
        /// <param name="vol"></param>
        void Play(string filename, int vol);
        /// <summary>
        /// サウンド再生
        /// </summary>
        /// <param name="filename">ファイルパス</param>
        /// <param name="vol"></param>
        /// <param name="playRatio"></param>
        void Play(string filename, int vol, double playRatio);
        /// <summary>
        /// サウンド停止
        /// </summary>
        /// <param name="filename">ファイルパス</param>
        void Stop(string filename);
        /// <summary>
        /// 無効化
        /// </summary>
        Disposable Disable();
    }
}
