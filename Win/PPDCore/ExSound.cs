using PPDFramework;
using System;
using System.IO;
using System.Windows.Forms;

namespace PPDCore
{
    class ExSound
    {
        int count = 1;
        ISound sound;
        public ExSound(ISound sound)
        {
            this.sound = sound;
        }
        public bool ExAddSound(string filename)
        {
            if (File.Exists(filename))
            {
                try
                {
                    sound.AddSound(filename, count.ToString());
                    count++;
                    return true;
                }
                catch (Exception e)
                {
                    MessageBox.Show(e.Message);
                    return false;
                }
            }
            else
            {
                MessageBox.Show("ファイルが存在しませんでした\n" + filename);
                return false;
            }
        }
        public bool ExPlay(int num, int vol, double playRatio)
        {
            if (num < count)
            {
                sound.Play(num.ToString(), vol);
                return true;
            }
            return false;
        }
        public bool ExStop(int num)
        {
            if (num < count)
            {
                sound.Stop(num.ToString());
                return true;
            }
            return false;
        }
        public bool ExDeleteSoundAllNum()
        {
            try
            {
                for (int i = 0; i < count; i++)
                {
                    sound.DeleteSound(i.ToString());
                }
                count = 1;
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message);
                return false;
            }
            return true;
        }
    }
}
