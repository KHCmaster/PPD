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
    public partial class inifilewriter : ScrollableForm
    {
        Seekmain sm;
        string numbererror = "エラー。数値でない可能性があります";
        public inifilewriter()
        {
            InitializeComponent();
            InitializeScroll();
            this.textBox1.TextChanged += new EventHandler(AnyTextChanged);
            this.textBox2.TextChanged += new EventHandler(AnyTextChanged);
            this.textBox3.TextChanged += new EventHandler(AnyTextChanged);
            this.textBox4.TextChanged += new EventHandler(AnyTextChanged);
            this.textBox5.TextChanged += new EventHandler(AnyTextChanged);
            this.textBox6.TextChanged += new EventHandler(AnyTextChanged);
            this.textBox7.TextChanged += new EventHandler(AnyTextChanged);
            this.textBox8.TextChanged += new EventHandler(AnyTextChanged);
            this.textBox9.TextChanged += new EventHandler(AnyTextChanged);
            this.textBox10.TextChanged += new EventHandler(AnyTextChanged);
            this.textBox11.TextChanged += new EventHandler(AnyTextChanged);
            this.textBox12.TextChanged += new EventHandler(AnyTextChanged);
        }
        private void AnyTextChanged(object sender, EventArgs e)
        {
            ContentChanged();
        }
        public void setseekmain(Seekmain sm)
        {
            this.sm = sm;
        }
        public void setlang(settinganalyzer sanal)
        {
            this.Text = sanal.getData("SFW");
            this.button1.Text = sanal.getData("SFWButton");
            this.button2.Text = sanal.getData("SFWButton");
            this.button3.Text = sanal.getData("SFWButton");
            this.button4.Text = sanal.getData("SFWButton");
            this.numbererror = sanal.getData("SFWNumberError");
            this.label1.Text = sanal.getData("SFWLabel1");
            this.label2.Text = sanal.getData("SFWLabel2");
            this.label3.Text = sanal.getData("SFWLabel3");
            this.label4.Text = sanal.getData("SFWLabel4");
            this.label5.Text = sanal.getData("SFWLabel5");
            this.label6.Text = sanal.getData("SFWLabel6");
            this.label7.Text = sanal.getData("SFWLabel7");
            this.label8.Text = sanal.getData("SFWLabel8");
            this.label9.Text = sanal.getData("SFWLabel9");
            this.label10.Text = sanal.getData("SFWLabel10");
            this.label11.Text = sanal.getData("SFWLabel11");
            this.label12.Text = sanal.getData("SFWLabel12");
        }
        public bool saveini(out string ret)
        {
            ret = "";
            float thumbtimestart = 0;
            float thumbtimeend = 0;
            float start = 0;
            float end = 0;
            float bpm = 0;
            int left = 0;
            int right = 0;
            int top = 0;
            int bottom = 0;
            string low = "";
            string medium = "";
            string high = "";
            if (float.TryParse(this.textBox1.Text, out thumbtimestart))
            {

            }
            else
            {
                MessageBox.Show(numbererror);
                return false;
            }
            if (float.TryParse(this.textBox2.Text, out thumbtimeend))
            {

            }
            else
            {
                MessageBox.Show(numbererror);
                return false;
            }
            if (float.TryParse(this.textBox3.Text, out start))
            {

            }
            else
            {
                MessageBox.Show(numbererror);
                return false;
            }
            if (float.TryParse(this.textBox4.Text, out end))
            {

            }
            else
            {
                MessageBox.Show(numbererror);
                return false;
            }
            if (float.TryParse(this.textBox8.Text, out bpm))
            {

            }
            else
            {
                MessageBox.Show(numbererror);
                return false;
            }
            if (int.TryParse(this.textBox9.Text, out left))
            {

            }
            else
            {
                MessageBox.Show(numbererror);
                return false;
            }
            if (int.TryParse(this.textBox10.Text, out right))
            {

            }
            else
            {
                MessageBox.Show(numbererror);
                return false;
            }
            if (int.TryParse(this.textBox11.Text, out top))
            {

            }
            else
            {
                MessageBox.Show(numbererror);
                return false;
            }
            if (int.TryParse(this.textBox12.Text, out bottom))
            {

            }
            else
            {
                MessageBox.Show(numbererror);
                return false;
            }
            low = this.textBox5.Text;
            medium = this.textBox6.Text;
            high = this.textBox7.Text;
            ret += "[thumbtimestart]" + thumbtimestart.ToString() + "\n";
            ret += "[thumbtimeend]" + thumbtimeend.ToString() + "\n";
            ret += "[start]" + start.ToString() + "\n";
            ret += "[end]" + end.ToString() + "\n";
            ret += "[bpm]" + bpm.ToString() + "\n";
            ret += "[difficulty easy]" + low + "\n";
            ret += "[difficulty normal]" + medium + "\n";
            ret += "[difficulty hard]" + high + "\n";
            ret += "[moviecutleft]" + left.ToString() + "\n";
            ret += "[moviecutright]" + right.ToString() + "\n";
            ret += "[moviecuttop]" + top.ToString() + "\n";
            ret += "[moviecutbottom]" + bottom.ToString() + "\n";
            return true;
        }
        public void setini(float[] times,string[] difficults,float bpm,int[] cuts)
        {
            this.textBox1.Text = times[0].ToString();
            this.textBox2.Text = times[1].ToString();
            this.textBox3.Text = times[2].ToString();
            this.textBox4.Text = times[3].ToString();
            this.textBox5.Text = difficults[0];
            this.textBox6.Text = difficults[1];
            this.textBox7.Text = difficults[2];
            this.textBox8.Text = bpm.ToString();
            this.textBox9.Text = cuts[0].ToString();
            this.textBox10.Text = cuts[1].ToString();
            this.textBox11.Text = cuts[2].ToString();
            this.textBox12.Text = cuts[3].ToString();
        }
        private void button1_Click(object sender, EventArgs e)
        {
            this.textBox1.Text = ((float)sm.Currenttime).ToString();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            this.textBox2.Text = ((float)sm.Currenttime).ToString();
        }

        private void button3_Click(object sender, EventArgs e)
        {
            this.textBox3.Text = ((float)sm.Currenttime).ToString();
        }

        private void button4_Click(object sender, EventArgs e)
        {
            this.textBox4.Text = ((float)sm.Currenttime).ToString();
        }

        
    }
}
