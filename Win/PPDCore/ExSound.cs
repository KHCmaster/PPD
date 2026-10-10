using PPDFramework;
using System;
using System.IO;
using System.Windows.Forms;

namespace PPDCore
{
    class ExSound(ISound sound)
    {
        int count = 1;
        readonly ISound sound = sound;

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
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0060:Remove unused parameter", Justification = "呼び出し側との互換のため残している。効果音の再生速度は変えない（従来の動作）")]
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
