using System;
using DigitalRune.Windows.Docking;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Collections;
using System.Windows.Forms;
using System.Runtime.InteropServices;

namespace testgame
{
    public partial class BPMMeasure : ScrollableForm
    {
        [DllImport("winmm.dll")]
        static extern long timeGetTime();

        ArrayList data;
        public BPMMeasure()
        {
            InitializeComponent();
            data = new ArrayList(10);
            InitializeScroll();
        }
        public void setlang(settinganalyzer sanal)
        {
            this.label1.Text = sanal.getData("BMLabel1");
            this.label2.Text = sanal.getData("BMLabel2");
            this.button1.Text = sanal.getData("BMButton1");
            this.button2.Text = sanal.getData("BMButton2");
        }
        private void button1_Click(object sender, EventArgs e)
        {
            data.Add(timeGetTime());
            updateText();
        }
        private void updateText()
        {
            if (data.Count <= 1) return;
            long tempdata = (long)data[data.Count - 1] - (long)data[0];
            this.label3.Text = ((long)data[data.Count - 1] - (long)data[data.Count - 2]).ToString();
            this.label4.Text = (60f / (tempdata / 1000f) * data.Count).ToString();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            data.Clear();
            this.label3.Text = "0";
            this.label4.Text = "0";
        }
        private void BPMMeasure_PreviewKeyDown(object sender, PreviewKeyDownEventArgs e)
        {
            if (e.KeyData == Keys.Return)
            {
                e.IsInputKey = true;
                data.Add(timeGetTime());
                updateText();
            }
        }
    }
}
