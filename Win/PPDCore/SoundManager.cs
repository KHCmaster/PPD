using PPDCoreModel;
using PPDCoreModel.Data;
using PPDFramework;
using PPDFramework.PPDStructure;
using PPDFrameworkCore;
using System.IO;

namespace PPDCore
{
    class SoundManager : UpdatableGameComponent, ISoundManager
    {
        const string soundsetname = "soundset.txt";
        ExSound sou;
        int[] buttonsound = new int[10];
        int[] pressingbuttonsound = new int[10];
        SCDData[] data;
        int iter;
        bool initialized = true;

        public EventManager EventManager
        {
            get;
            set;
        }

        public SoundManager(ExSound sou, PPDGameUtility ppdgameutility)
        {
            this.sou = sou;
            LoadSoundSet(ppdgameutility.SongInformation.DirectoryPath);
            LoadSoundChangeData(ppdgameutility.SongInformation.DirectoryPath, ppdgameutility.Difficulty);
        }
        private void LoadSoundSet(string dir)
        {
            var path = Path.Combine(dir, soundsetname);
            if (File.Exists(path))
            {
                foreach (string filename in SoundSetReader.Read(path))
                {
                    sou.ExAddSound(dir + "\\sound\\" + filename);
                }
            }
            else
            {
                initialized = false;
            }
        }
        private void LoadSoundChangeData(string dir, Difficulty difficulty)
        {
            string filename = DifficultyUtility.ConvertDifficulty(difficulty) + ".scd";
            var path = Path.Combine(dir, filename);
            if (File.Exists(path))
            {
                data = SCDReader.Read(path);
            }
            else
            {
                initialized = false;
            }
        }
        private void ChangeSound(float time)
        {
            if (iter >= data.Length) return;
            SCDData scddata = data[iter];
            while (time >= scddata.Time)
            {
                buttonsound[(int)scddata.ButtonType] = scddata.SoundIndex;
                iter++;
                if (iter >= data.Length) return;
                scddata = data[iter];
            }
        }
        public void Seek(float time)
        {
            Retry();
            if (!initialized) return;
            ChangeSound(time);
        }
        public bool Play(MarkType button, double playRatio)
        {
            return Play(button, -100 * (100 - EventManager.VolumePercents[(int)button]), playRatio);
        }
        public bool Play(MarkType button, int volume, double playRatio)
        {
            if (0 <= button && button <= MarkType.L)
            {
                return Play(buttonsound[(int)button], volume, playRatio);
            }
            return false;
        }
        public bool Play(int index, int volume, double playRatio)
        {
            return sou.ExPlay(index, volume, playRatio);
        }
        public bool Stop(MarkType button)
        {
            if (0 <= button && button <= MarkType.L)
            {
                return Stop(buttonsound[(int)button]);
            }
            return false;
        }
        public bool Stop(int index)
        {
            return sou.ExStop(index);
        }
        private void Play(bool[] pressed, bool[] released, double playRatio)
        {
            int[] VolPercents = EventManager.VolumePercents;
            bool[] SoundKeepPlaying = EventManager.KeepPlayings;
            for (int i = 0; i < buttonsound.Length; i++)
            {
                if (pressed[i])
                {
                    Play(buttonsound[i], -100 * (100 - VolPercents[i]), playRatio);
                    if (SoundKeepPlaying[i])
                    {
                        pressingbuttonsound[i] = buttonsound[i];
                    }
                }
                if (released[i])
                {
                    Stop(pressingbuttonsound[i]);
                    pressingbuttonsound[i] = 0;
                }
            }
        }
        public void SpPlay(int num, int VolPercent, double playRatio)
        {
            if (num < buttonsound.Length)
            {
                Play(buttonsound[num], -100 * (100 - VolPercent), playRatio);
            }
        }
        public void SpKeepPlay(int num, int VolPercent, bool[] SoundKeepPlaying, double playRatio)
        {
            if (num < buttonsound.Length)
            {
                Play(buttonsound[num], -100 * (100 - VolPercent), playRatio);
                if (SoundKeepPlaying[num])
                {
                    pressingbuttonsound[num] = buttonsound[num];
                }
            }
        }
        public void SpStopSound(int num)
        {
            Stop(pressingbuttonsound[num]);
            pressingbuttonsound[num] = -1;
        }
        public void Retry()
        {
            iter = 0;
            for (int i = 0; i < buttonsound.Length; i++)
            {
                buttonsound[i] = 0;
                pressingbuttonsound[i] = 0;
            }
        }
        public void Update(float time, bool[] presed, bool[] released, double playRatio)
        {
            if (initialized)
            {
                ChangeSound(time);
                Play(presed, released, playRatio);
            }
        }
        protected override void DisposeResource()
        {
            sou.ExDeleteSoundAllNum();
        }
    }
}
