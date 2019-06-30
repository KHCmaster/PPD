using System;
using System.Collections.Generic;
using System.Text;
using SlimDX;
using SlimDX.Direct3D9;
using System.IO;
using PPDFramework;
using PPDFramework.PPDStructure;

namespace PPD
{
    class SoundManager : UpdatableGameComponent
    {
        const string soundsetname = "soundset.txt";
        ExSound sou;
        int[] buttonsound = new int[10];
        int[] pressingbuttonsound = new int[10];
        SCDData[] data;
        int iter = 0;
        bool initialized = true;
        public SoundManager(ExSound sou, PPDGameUtility ppdgameutility)
        {
            this.sou = sou;
            loadsounddata(ppdgameutility.SongInformation.DirectoryPath);
            loadchangedata(ppdgameutility.SongInformation.DirectoryPath, ppdgameutility.Difficulty);
        }
        private void loadsounddata(string dir)
        {
            string path = Path.Combine(dir, soundsetname);
            if (File.Exists(path))
            {
                foreach (string filename in SoundSetReader.Read(path))
                {
                    sou.ExAddSound(dir + "\\sound\\" + filename);
                }
                initialized &= true;
            }
            else
            {
                initialized &= false;
            }
        }
        private void loadchangedata(string dir, Difficulty difficulty)
        {
            string filename = DifficultyUtility.ConvertDifficulty(difficulty) + ".scd";
            string path = Path.Combine(dir, filename);
            if (File.Exists(path))
            {
                data = SCDReader.Read(path);
                initialized &= true;
            }
            else
            {
                initialized &= false;
            }
        }
        private void changesound(float time)
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
            retry();
            if (!initialized) return;
            changesound(time);
        }
        private void playsound(bool[] b, int[] VolPercents, bool[] SoundKeepPlaying, bool[] released)
        {
            for (int i = 0; i < buttonsound.Length; i++)
            {
                if (b[i])
                {
                    sou.ExPlay(buttonsound[i], -100 * (100 - VolPercents[i]));
                    if (SoundKeepPlaying[i])
                    {
                        pressingbuttonsound[i] = buttonsound[i];
                    }
                }
                if (released[i])
                {
                    sou.ExStop(pressingbuttonsound[i]);
                    pressingbuttonsound[i] = 0;
                }
            }
        }
        public void spPlaysound(int num, int VolPercent)
        {
            if (num < buttonsound.Length)
            {
                sou.ExPlay(buttonsound[num], -100 * (100 - VolPercent));
            }
        }
        public void spKeepPlaysound(int num, int VolPercent, bool[] SoundKeepPlaying)
        {
            if (num < buttonsound.Length)
            {
                sou.ExPlay(buttonsound[num], -100 * (100 - VolPercent));
                if (SoundKeepPlaying[num])
                {
                    pressingbuttonsound[num] = buttonsound[num];
                }
            }
        }
        public void spStopsound(int num)
        {
            sou.ExStop(pressingbuttonsound[num]);
            pressingbuttonsound[num] = -1;
        }
        public void retry()
        {
            iter = 0;
            for (int i = 0; i < buttonsound.Length; i++)
            {
                buttonsound[i] = 0;
                pressingbuttonsound[i] = 0;
            }
        }
        public void Update(float time, bool[] b, int[] VolumePercent, bool[] SoundKeepPlaying, bool[] released)
        {
            if (initialized)
            {
                changesound(time);
                playsound(b, VolumePercent, SoundKeepPlaying, released);
            }
        }
        public override void Update()
        {

        }
        protected override void DisposeResource()
        {
            sou.exdeletesoundallnum();
        }
    }
}
