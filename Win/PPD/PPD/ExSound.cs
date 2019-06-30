using System;
using System.Collections;
using System.Windows.Forms;
using System.Collections.Generic;
using System.Text;
using System.IO;
using SlimDX;
using SlimDX.DirectSound;
using SlimDX.Multimedia;
using SlimDX.Direct3D9;

namespace PPD
{
    class ExSound : Sound
    {
        int count = 1;
        public ExSound(Form Window)
            : base(Window)
        {

        }
        public bool ExAddSound(string filename)
        {
            if (File.Exists(filename))
            {
                try
                {
                    base.AddSound(filename, count.ToString());
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
        public void ExPlay(int num, int vol)
        {
            if (num < count)
            {
                base.Play(num.ToString(), vol);
            }
        }
        public void ExStop(int num)
        {
            if (num < count)
            {
                base.Stop(num.ToString());
            }
        }
        public bool exdeletesoundallnum()
        {
            try
            {
                for (int i = 0; i < count; i++)
                {
                    base.DeleteSound(i.ToString());
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
