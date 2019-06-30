using PPDFramework;
using PPDFramework.Resource;

namespace PPDSound
{
    /// <summary>
    /// サウンドリソース
    /// </summary>
    public class SoundResource : ResourceBase
    {
        private ISound sound;
        private string name;

        /// <summary>
        /// 名前を取得します。
        /// </summary>
        public string Name
        {
            get
            {
                return name;
            }
        }

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="sound"></param>
        /// <param name="name"></param>
        /// <param name="data"></param>
        public SoundResource(ISound sound, string name, byte[] data)
        {
            this.sound = sound;
            this.name = name;
            sound.AddSound(data, name);
        }

        /// <summary>
        /// 再生します。Volumeは-1000です。
        /// </summary>
        public void Play()
        {
            Play(-1000, 1);
        }

        /// <summary>
        /// 再生します。Volumeは-1000です。
        /// </summary>
        public void Play(double playRatio)
        {
            Play(-1000, playRatio);
        }

        /// <summary>
        /// Volumeを指定して再生します。
        /// </summary>
        /// <param name="vol"></param>
        public void Play(int vol, double playRatio)
        {
            sound.Play(name, vol, playRatio);
        }

        /// <summary>
        /// 再生を停止します。
        /// </summary>
        public void Stop()
        {
            sound.Stop(name);
        }

        /// <summary>
        /// リソース開放
        /// </summary>
        protected override void DisposeResource()
        {
            sound.DeleteSound(name);
            base.DisposeResource();
        }
    }
}
