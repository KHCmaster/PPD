using PPDFramework.Texture;
using System;

namespace PPDFramework
{
    /// <summary>
    /// ムービーインターフェース
    /// </summary>
    public interface IMovie : IGameComponent
    {
        /// <summary>
        /// 終了時イベント
        /// </summary>
        event EventHandler Finished;

        /// <summary>
        /// 動画のフェード状態
        /// </summary>
        MovieFadeState FadeState { get; }

        /// <summary>
        /// 動画のファイルパス
        /// </summary>
        string FileName { get; set; }

        /// <summary>
        /// 動画の幅
        /// </summary>
        int MovieWidth { get; }

        /// <summary>
        /// 動画の高さ
        /// </summary>
        int MovieHeight { get; }

        /// <summary>
        /// 動画のテクスチャ
        /// </summary>
        TextureBase Texture { get; }

        /// <summary>
        /// 動画の長さ
        /// </summary>
        double Length { get; }

        /// <summary>
        /// 動画の位置
        /// </summary>
        double MoviePosition { get; }

        /// <summary>
        /// 初期化されたか
        /// </summary>
        bool Initialized { get; }

        /// <summary>
        /// 再生中か
        /// </summary>
        bool Playing { get; }

        /// <summary>
        /// 最大のU
        /// </summary>
        float MaxU { get; }

        /// <summary>
        /// 最大のV
        /// </summary>
        float MaxV { get; }

        /// <summary>
        /// 反転しているか(SampleGrabberを使用しているか)
        /// </summary>
        bool Rotated { get; }

        /// <summary>
        /// 動画のトリミングデータ
        /// </summary>
        MovieTrimmingData TrimmingData { get; set; }

        /// <summary>
        /// 最大のボリューム
        /// </summary>
        int MaximumVolume { get; set; }

        /// <summary>
        /// 表示する幅
        /// </summary>
        float MovieDisplayWidth
        {
            get;
            set;
        }

        /// <summary>
        /// 表示する高さ
        /// </summary>
        float MovieDisplayHeight
        {
            get;
            set;
        }

        /// <summary>
        /// ボリューム
        /// </summary>
        int Volume
        {
            get;
            set;
        }

        /// <summary>
        /// 再生レート
        /// </summary>
        double PlayRate
        {
            get;
            set;
        }

        /// <summary>
        /// 音声のみかどうか
        /// </summary>
        bool IsAudioOnly
        {
            get;
        }

        /// <summary>
        /// 初期化する
        /// </summary>
        /// <returns></returns>
        int Initialize();

        /// <summary>
        /// シークする
        /// </summary>
        /// <param name="time">時間</param>
        void Seek(double time);

        /// <summary>
        /// 再生する
        /// </summary>
        void Play();

        /// <summary>
        /// 停止する
        /// </summary>
        void Stop();

        /// <summary>
        /// 一時停止する
        /// </summary>
        void Pause();

        /// <summary>
        /// フェードインする
        /// </summary>
        void FadeIn(float fadeStep = 2);

        /// <summary>
        /// フェードアウトする
        /// </summary>
        void FadeOut(float fadeStep = 2);

        /// <summary>
        /// 規定可視状態にする
        /// </summary>
        void SetDefaultVisible();
    }
}
