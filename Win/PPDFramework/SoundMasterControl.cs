using System;

namespace PPDFramework
{
    /// <summary>
    /// サウンドマスタークラス
    /// </summary>
    public class SoundMasterControl
    {
        /// <summary>
        /// 最大
        /// </summary>
        public const int Max = 0;
        /// <summary>
        /// 最小
        /// </summary>
        public const int Min = -10000;

        private static SoundMasterControl instance = new SoundMasterControl();
        private int masterVolume;
        private int movieVolume;
        private int seVolume;

        /// <summary>
        /// シングルトンのインスタンスを取得します。
        /// </summary>
        public static SoundMasterControl Instance
        {
            get
            {
                return instance;
            }
        }

        /// <summary>
        /// マスターの音量が変更されたときのイベントです。
        /// </summary>
        public event Action MasterVolumeChanged;

        /// <summary>
        /// 動画の音量が変更されたときのイベントです。
        /// </summary>
        public event Action MovieVolumeChanged;

        /// <summary>
        /// SEの音量が変更されたときのイベントです。
        /// </summary>
        public event Action SeVolumeChanged;

        private SoundMasterControl()
        {
            ChangeVolume(PPDFrameworkSetting.Setting.MasterVolume, SoundType.Master);
            ChangeVolume(PPDFrameworkSetting.Setting.MovieVolume, SoundType.Movie);
            ChangeVolume(PPDFrameworkSetting.Setting.SeVolume, SoundType.Se);
        }

        /// <summary>
        /// ボリュームを取得します。
        /// </summary>
        /// <param name="soundType">サウンドのタイプ</param>
        /// <returns>ボリューム</returns>
        public int GetVolume(SoundType soundType)
        {
            switch (soundType)
            {
                case SoundType.Master:
                    return masterVolume;
                case SoundType.Movie:
                    return movieVolume;
                case SoundType.Se:
                    return seVolume;
            }
            return 0;
        }

        /// <summary>
        /// 変換する
        /// </summary>
        /// <param name="volume">ボリューム</param>
        /// <param name="soundType">サウンドのタイプ</param>
        /// <returns></returns>
        public int GetVolume(int volume, SoundType soundType)
        {
            float masterRatio = 1 - (float)masterVolume / Min;
            switch (soundType)
            {
                case SoundType.Movie:
                    masterRatio *= 1 - (float)movieVolume / Min;
                    break;
                case SoundType.Se:
                    masterRatio *= 1 - (float)seVolume / Min;
                    break;
            }
            float targetRatio = 1 - (float)volume / Min;
            return (int)((1 - masterRatio * targetRatio) * Min);
        }

        /// <summary>
        /// 対象のサウンドの種類の音量を変更します。
        /// </summary>
        /// <param name="volume">ボリューム</param>
        /// <param name="soundType">サウンドのタイプ</param>
        public void ChangeVolume(int volume, SoundType soundType)
        {
            if (volume > Max)
            {
                volume = Max;
            }
            if (volume < Min)
            {
                volume = Min;
            }
            switch (soundType)
            {
                case SoundType.Master:
                    if (masterVolume != volume)
                    {
                        masterVolume = volume;
                        OnMasterVolumeChanged();
                        OnMovieVolumeChanged();
                        OnSeVolumeChanged();
                    }
                    break;
                case SoundType.Movie:
                    if (movieVolume != volume)
                    {
                        movieVolume = volume;
                        OnMovieVolumeChanged();
                    }
                    break;
                case SoundType.Se:
                    if (seVolume != volume)
                    {
                        seVolume = volume;
                        OnSeVolumeChanged();
                    }
                    break;
            }
        }

        /// <summary>
        /// サウンドの設定を保存します。
        /// </summary>
        public void Save()
        {
            PPDFrameworkSetting.Setting.MasterVolume = masterVolume;
            PPDFrameworkSetting.Setting.MovieVolume = movieVolume;
            PPDFrameworkSetting.Setting.SeVolume = seVolume;
        }

        private void OnMasterVolumeChanged()
        {
            MasterVolumeChanged?.Invoke();
        }

        private void OnMovieVolumeChanged()
        {
            MovieVolumeChanged?.Invoke();
        }

        private void OnSeVolumeChanged()
        {
            SeVolumeChanged?.Invoke();
        }
    }
}
