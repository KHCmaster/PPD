using System;
using DigitalRune.Windows.Docking;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace testgame
{
    public partial class InfoForm : ScrollableForm
    {
        TimeLineForm tlf;
        public InfoForm()
        {
            InitializeComponent();
            InitializeScroll();
        }

        
        public void setlang(settinganalyzer sanal)
        {
            this.label2.Text = sanal.getData("Width");
            this.label3.Text = sanal.getData("Height");
            this.label4.Text = sanal.getData("Length");
            this.label6.Text = sanal.getData("Position");
            this.label7.Text = sanal.getData("Angle");
            this.checkBox1.Text = sanal.getData("FixOp");
            this.comboBox1.Items.Clear();
            this.comboBox1.Items.Add(sanal.getData("Displaylevel1"));
            this.comboBox1.Items.Add(sanal.getData("Displaylevel2"));
            this.comboBox1.Items.Add(sanal.getData("Displaylevel3"));
            this.comboBox1.Items.Add(sanal.getData("Displaylevel4"));
            this.comboBox1.Items.Add(sanal.getData("Displaylevel5"));
            this.label8.Text = sanal.getData("DisplayWidth");
            this.comboBox1.SelectedIndex = 0;
            this.comboBox2.SelectedIndex = 0;
        }
        public void settlf(TimeLineForm tlf)
        {
            this.tlf = tlf;
            this.comboBox1.SelectedIndex = 0;
            this.comboBox2.SelectedIndex = 2;
        }
        public void setinfo(string width, string height, string length)
        {
            this.textBox2.Text = width;
            this.textBox3.Text = height;
            this.textBox4.Text = length;
        }
        public void setinfo(string pos, string angle)
        {
            textBox6.Text = pos;
            textBox7.Text = angle;
        }
        public void setinfo(string bpm, string bpmstart, int displaymode, bool bpmfixed,int speedscale,string displaywidth)
        {
            this.textBox1.Text = bpm;
            this.textBox5.Text = bpmstart;
            this.comboBox1.SelectedIndex = displaymode;
            this.checkBox1.Checked = bpmfixed;
            this.comboBox2.SelectedIndex = speedscale;
            this.textBox8.Text = displaywidth;
        }
        public bool Bpmfixed
        {
            get
            {
                return this.checkBox1.Checked;
            }
        }
        public int Displaymode
        {
            get
            {
                return this.comboBox1.SelectedIndex;
            }
        }
        public int DisplayWidth
        {
            get
            {
                int ret = 0;
                if (int.TryParse(this.textBox8.Text, out ret))
                {
                    return ret;
                }
                return 240;
            }
        }
        public int SpeedScale
        {
            get
            {
                return comboBox2.SelectedIndex;
            }
        }
        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            float result = 100;
            if (float.TryParse(this.textBox1.Text, out result))
            {
                if (result <= 500)
                {
                    tlf.seekmain.BPM = result;
                    //this.tlf.seekmain.DrawandReflash();
                }
            }
        }

        private void textBox5_TextChanged(object sender, EventArgs e)
        {
            float result = 0;
            if (float.TryParse(this.textBox5.Text, out result))
            {
                tlf.seekmain.BPMSTART = result;
                //this.tlf.seekmain.DrawandReflash();
            }
        }

        private void textBox8_TextChanged(object sender, EventArgs e)
        {
            int result = 0;
            if (int.TryParse(this.textBox8.Text, out result))
            {
                tlf.seekmain.DisplayWidth = result;
            }
        }
        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            if (this.checkBox1.Checked)
            {
                tlf.seekmain.bpmfixed = true;
            }
            else
            {
                tlf.seekmain.bpmfixed = false;
            }
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            tlf.seekmain.hyoujimode = this.comboBox1.SelectedIndex;
            tlf.seekmain.DrawandReflash();
        }

        private void comboBox2_SelectedIndexChanged(object sender, EventArgs e)
        {
            float result = 0;
            switch (comboBox2.SelectedIndex)
            {
                case 0:
                    result = 0.5f;
                    break;
                case 1:
                    result = 0.75f;
                    break;
                case 2:
                    result = 1.0f;
                    break;
                case 3:
                    result = 1.25f;
                    break;
                case 4:
                    result = 1.5f;
                    break;
                case 5:
                    result = 1.75f;
                    break;
                case 6:
                    result = 2.0f;
                    break;
            }
            tlf.seekmain.SpeedScale = result;
        }


        

    }
}
