using System;
using DigitalRune.Windows.Docking;
using System.Collections.Generic;
using System.IO;
using SlimDX;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace testgame
{
    public partial class pandaloadersaver : ScrollableForm
    {
        string[] filenames = null;
        Seekmain sm;
        Form1 fm1;
        settinganalyzer sanal;
        string noselectedpoint = "選択点がないため設定できませんでした";
        string noselectarea = "選択エリアがないため設定できませんでした";
        string headererror = "ヘッダエラー";
        string invalidstring = "無効な文字列が含まれています";
        string invalidnumber = "整数以外が入力されています。0にしました。";
        bool varidangle;
        float[] xs = new float[0];
        float[] ys = new float[0];
        float[] angles = new float[0];
        float[] displayxs;
        float[] displayys;
        float[] displayangles;
        public pandaloadersaver()
        {
            InitializeComponent();
            radioButton1.CheckedChanged += new EventHandler(changeradiocheck);
            radioButton2.CheckedChanged += new EventHandler(changeradiocheck);
            radioButton3.CheckedChanged += new EventHandler(changeradiocheck);
            radioButton1.Checked = true;
            this.textBox1.TextChanged += new EventHandler(textBoxTextChanged);
            this.textBox2.TextChanged += new EventHandler(textBoxTextChanged);
            this.textBox3.TextChanged += new EventHandler(textBoxTextChanged);
            this.textBox4.TextChanged += new EventHandler(textBoxTextChanged);
            this.textBox5.TextChanged += new EventHandler(textBoxTextChanged);
            this.textBox6.TextChanged += new EventHandler(textBoxTextChanged);
            readdir();
            InitializeScroll();
        }
        public void setlang(settinganalyzer sanal)
        {
            this.sanal = sanal;
            this.Text = sanal.getData("PAALS");
            this.checkBox1.Text = sanal.getData("PAALSCheckBox");
            this.checkBox2.Text = sanal.getData("PAALSCheckBox2");
            this.button1.Text = sanal.getData("PAALSButton1");
            this.button2.Text = sanal.getData("PAALSButton2");
            this.button3.Text = sanal.getData("PAALSButton3");
            this.button4.Text = sanal.getData("PAALSButton4");
            this.button5.Text = sanal.getData("PAALSButton5");
            this.button6.Text = sanal.getData("PAALSButton6");
            this.button7.Text = sanal.getData("PAALSButton7");
            this.button8.Text = sanal.getData("PAALSButton8");
            this.button9.Text = sanal.getData("PAALSButton9");
            this.button10.Text = sanal.getData("PAALSButton10");
            this.groupBox1.Text = sanal.getData("PAALSGroupText");
            this.radioButton1.Text = sanal.getData("PAALSRadioButton1");
            this.radioButton2.Text = sanal.getData("PAALSRadioButton2");
            this.radioButton3.Text = sanal.getData("PAALSRadioButton3");
            noselectedpoint = sanal.getData("PAALSNoSelectedPoint");
            noselectarea = sanal.getData("PAALSNoSelectedArea");
            headererror = sanal.getData("PAALSHeaderError");
            invalidstring = sanal.getData("PAALSInvalidString");
            invalidnumber = sanal.getData("PAALSInvalidNumber");
            /*this.button3.Location = new Point(Math.Max(this.button1.Width, this.button2.Width) + this.button1.Location.X + 5, this.button3.Location.Y);
            this.button4.Location = new Point(Math.Max(this.button1.Width, this.button2.Width) + this.button1.Location.X + 5, this.button4.Location.Y);
            this.button5.Location = new Point(Math.Max(this.button3.Width, this.button4.Width) + this.button3.Location.X + 5, this.button5.Location.Y);
            this.button6.Location = new Point(Math.Max(this.button3.Width, this.button4.Width) + this.button3.Location.X + 5, this.button6.Location.Y);
            this.button7.Location = new Point(Math.Max(this.button5.Width, this.button6.Width) + this.button5.Location.X + 5, this.button7.Location.Y);
            this.button8.Location = new Point(Math.Max(this.button5.Width, this.button6.Width) + this.button5.Location.X + 5, this.button8.Location.Y);
            this.button9.Location = new Point(listBox1.Location.X - 10 - this.button9.Width, this.button9.Location.Y);
            this.button10.Location = new Point(this.button9.Location.X - 10 - this.button10.Width, this.button10.Location.Y);*/

        }
        public bool displayangle
        {
            get
            {
                return this.checkBox1.Checked;
            }
            set
            {
                this.checkBox1.Checked = value;
            }
        }
        public void setseekmain(Seekmain sm)
        {
            this.sm = sm;
        }
        public void setForm1(Form1 fm1)
        {
            this.fm1 = fm1;
        }
        public bool ifdrawangle()
        {
            return this.checkBox1.Checked && varidangle;
        }
        private void textBoxTextChanged(object sender, EventArgs e)
        {
            TextBox tb = sender as TextBox;
            if (tb != null)
            {
                int result = 0;
                if (!int.TryParse(tb.Text, out result))
                {
                    tb.Text = "0";
                    MessageBox.Show(invalidnumber);
                }
                else
                {
                    changedisplaydata();
                }
            }
        }
        private void changeradiocheck(object sender, EventArgs e)
        {
            if (radioButton1.Checked)
            {
                checkBox2.Enabled = false;
                radioButton2.Checked = false;
                radioButton3.Checked = false;
                this.textBox1.Enabled = false;
                this.textBox2.Enabled = false;
                this.textBox3.Enabled = false;
                this.textBox4.Enabled = false;
                this.textBox5.Enabled = false;
                this.textBox6.Enabled = false;
            }
            else if (radioButton2.Checked)
            {
                checkBox2.Enabled = true;
                radioButton1.Checked = false;
                radioButton3.Checked = false;
                this.textBox1.Enabled = true;
                this.textBox2.Enabled = true;
                this.textBox3.Enabled = false;
                this.textBox4.Enabled = false;
                this.textBox5.Enabled = false;
                this.textBox6.Enabled = false;
            }
            else if (radioButton3.Checked)
            {
                checkBox2.Enabled = true;
                radioButton1.Checked = false;
                radioButton2.Checked = false;
                this.textBox1.Enabled = false;
                this.textBox2.Enabled = false;
                this.textBox3.Enabled = true;
                this.textBox4.Enabled = true;
                this.textBox5.Enabled = true;
                this.textBox6.Enabled = true;
            }
            changedisplaydata();
        }
        private void readdir()
        {
            if (!Directory.Exists("posdat"))
            {
                Directory.CreateDirectory("posdat");
            }
            this.listBox1.BeginUpdate();
            listBox1.Items.Clear();
            filenames = Directory.GetFiles("posdat");
            for (int i = 0; i < filenames.Length; i++)
            {
                this.listBox1.Items.Add(Path.GetFileNameWithoutExtension(filenames[i]));
            }
            this.listBox1.EndUpdate();
        }
        private string removeextra(string st)
        {
            int a = st.IndexOf(" ");
            while (a != -1)
            {
                st = st.Remove(a, 1);
                a = st.IndexOf(" ");
            }
            a = st.IndexOf("\t");
            while (a != -1)
            {
                st = st.Remove(a, 1);
                a = st.IndexOf("\t");
            }
            a = st.IndexOf("\r");
            while (a != -1)
            {
                st = st.Remove(a, 1);
                a = st.IndexOf("\r");
            }
            return st;
        }
        private int countline(string st)
        {
            int ret = 0;
            if (st[st.Length - 1] == '\n')
            {
                st = st.Substring(0, st.Length - 1);
            }
            int a = st.IndexOf("\n");
            while (a != -1)
            {
                ret++;
                a = st.IndexOf("\n", a + 1);
            }
            ret++;
            return ret;
        }
        private void listBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            using (StreamReader sr = new StreamReader(filenames[listBox1.SelectedIndex]))
            {
                string st = sr.ReadToEnd();
                sr.Close();
                try
                {
                    int a = st.IndexOf("\n");
                    if (a < 0)
                    {
                        Exception ex = new Exception(headererror);
                        throw ex;
                    }
                    int[] parm = new int[] { -1, -1, -1 };
                    string subst = st.Substring(0, a);
                    int start = 0;
                    int iter = 0;
                    int b = subst.IndexOf(",");
                    while (b != -1)
                    {
                        string meta = subst.Substring(start, b - start);
                        if (meta == "x")
                        {
                            parm[iter] = 0;
                            iter++;
                        }
                        else if (meta == "y")
                        {
                            parm[iter] = 1;
                            iter++;
                        }
                        else if (meta == "angle")
                        {
                            parm[iter] = 2;
                            iter++;
                        }
                        else
                        {
                            Exception ex = new Exception(headererror);
                            throw ex;
                        }
                        start = b + 1;
                        if (iter == 2)
                        {
                            break;
                        }
                        b = subst.IndexOf(",", start);
                    }
                    string met = removeextra(subst.Substring(start));
                    if (met == "x")
                    {
                        parm[iter] = 0;
                    }
                    else if (met == "y")
                    {
                        parm[iter] = 1;
                    }
                    else if (met == "angle")
                    {
                        parm[iter] = 2;
                    }
                    string data = removeextra(st.Substring(a + 1));
                    int num = countline(data);
                    float[][] alldata = new float[3][];
                    alldata[0] = new float[num];
                    alldata[1] = new float[num];
                    alldata[2] = new float[num];
                    int[] counts = new int[2];
                    int anchor = 0;
                    for (int i = 0; i < data.Length; i++)
                    {
                        if (data[i] == ',')
                        {
                            string numdata = data.Substring(anchor, i - anchor);
                            float result = 0;
                            if (float.TryParse(numdata, out result))
                            {
                                alldata[counts[0]][counts[1]] = result;
                                counts[0]++;
                                anchor = i + 1;
                            }
                            else
                            {
                                Exception ex = new Exception(invalidstring);
                                throw ex;
                            }
                        }
                        else if (data[i] == '\n')
                        {
                            string numdata = data.Substring(anchor, i - anchor);
                            float result = 0;
                            if (float.TryParse(numdata, out result))
                            {
                                alldata[counts[0]][counts[1]] = result;
                                counts[0]++;
                                anchor = i + 1;
                            }
                            else
                            {
                                Exception ex = new Exception(invalidstring);
                                throw ex;
                            }
                            counts[0] = 0;
                            counts[1]++;
                        }
                        else if (i == data.Length - 1)
                        {
                            string numdata = data.Substring(anchor);
                            float result = 0;
                            if (float.TryParse(numdata, out result))
                            {
                                alldata[counts[0]][counts[1]] = result;
                                counts[0]++;
                                anchor = i + 1;
                            }
                            else
                            {
                                Exception ex = new Exception(invalidstring);
                                throw ex;
                            }
                        }
                    }
                    varidangle = false;
                    for (int i = 0; i < parm.Length; i++)
                    {
                        if (parm[i] == 0)
                        {
                            xs = alldata[i];
                        }
                        else if (parm[i] == 1)
                        {
                            ys = alldata[i];
                        }
                        else if (parm[i] == 2)
                        {
                            angles = alldata[i];
                            varidangle = true;
                        }
                        else
                        {
                            break;
                        }
                    }
                    changedisplaydata();
                }
                catch (Exception exception)
                {
                    MessageBox.Show(exception.Message);
                }
            }
        }
        private void changedisplaydata()
        {
            displayxs = new float[xs.Length];
            displayys = new float[xs.Length];
            displayangles = new float[xs.Length];
            if (radioButton1.Checked)
            {
                Array.Copy(xs, displayxs, xs.Length);
                Array.Copy(ys, displayys, xs.Length);
                if (varidangle)
                {
                    Array.Copy(angles, displayangles, xs.Length);
                }
                else
                {

                }
            }
            else if (radioButton2.Checked)
            {
                Vector2 point = new Vector2(int.Parse(this.textBox1.Text), int.Parse(this.textBox2.Text));
                for (int i = 0; i < xs.Length; i++)
                {
                    displayxs[i] = xs[i] + 2 * (point.X - xs[i]);
                    displayys[i] = ys[i] + 2 * (point.Y - ys[i]);
                    if (varidangle)
                    {
                        if (checkBox2.Checked)
                        {
                            double na = getnormalangle(angles[i]);
                            displayangles[i] = (float)(Math.PI * 2 - na);
                        }
                        else
                        {
                            Array.Copy(angles, displayangles, angles.Length);
                        }
                    }
                }
            }
            else if (radioButton3.Checked)
            {
                int x1 = int.Parse(this.textBox3.Text), y1 = int.Parse(this.textBox4.Text), x2 = int.Parse(this.textBox5.Text), y2 = int.Parse(this.textBox6.Text);
                Vector2 basicvec = Vector2.Normalize(new Vector2(x2 - x1, y2 - y1));
                for (int i = 0; i < xs.Length; i++)
                {
                    Vector2 targetvec = new Vector2(xs[i] - x1, ys[i] - y1);
                    Vector2 targetvecnormal = Vector2.Normalize(targetvec);
                    float bitweenangle = Vector2.Dot(basicvec, targetvecnormal);
                    Vector2 anservec = Vector2.Add(targetvecnormal, 2 * (bitweenangle * basicvec - targetvecnormal));
                    displayxs[i] = anservec.X * Vector2.Distance(Vector2.Zero, targetvec) + x1;
                    displayys[i] = anservec.Y * Vector2.Distance(Vector2.Zero, targetvec) + y1;
                    if (varidangle)
                    {
                        if (checkBox2.Checked)
                        {
                            double na = getnormalangle(angles[i]);
                            float d = (float)Math.Acos(-basicvec.X);
                            float trans = (float)(getnormalangle(na - d));
                            trans = (float)(Math.PI * 2 - trans);
                            displayangles[i] = trans + d;
                        }
                        else
                        {
                            Array.Copy(angles, displayangles, angles.Length);
                        }
                    }
                }
            }
            for (int j = 0; j < xs.Length; j++)
            {
                if (displayxs[j] < 0) displayxs[j] = 0;
                if (displayxs[j] > 800) displayxs[j] = 800;
                if (displayys[j] < 0) displayys[j] = 0;
                if (displayys[j] > 450) displayys[j] = 450;
            }
            if (fm1 != null)
            {
                fm1.changedata(displayxs, displayys, displayangles);
            }
        }
        private double getnormalangle(double angle)
        {
            int shou = (int)Math.Floor(angle / (Math.PI * 2));
            return angle - Math.PI * 2 * shou;
        }
        private void button1_Click(object sender, EventArgs e)
        {
            if (this.listBox1.SelectedIndex >= 0)
            {
                if (!sm.setdata(displayxs, displayys, displayangles, false, 0, varidangle))
                {
                    MessageBox.Show(noselectedpoint);
                }
            }
        }
        private void button2_Click(object sender, EventArgs e)
        {
            if (this.listBox1.SelectedIndex >= 0)
            {
                if (!sm.setdata(displayxs, displayys, displayangles, true, 0, varidangle))
                {
                    MessageBox.Show(noselectedpoint);
                }
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            if (this.listBox1.SelectedIndex >= 0)
            {
                if (!sm.setdata(displayxs, displayys, displayangles, false, 1, varidangle))
                {
                    MessageBox.Show(noselectarea);
                }
            }
        }

        private void button4_Click(object sender, EventArgs e)
        {
            if (this.listBox1.SelectedIndex >= 0)
            {
                if (!sm.setdata(displayxs, displayys, displayangles, true, 1, varidangle))
                {
                    MessageBox.Show(noselectarea);
                }
            }
        }
        private void button5_Click(object sender, EventArgs e)
        {
            Form3 fm3 = new Form3(button5.Text);
            fm3.setlang(sanal);
            if (fm3.ShowDialog() == DialogResult.OK)
            {
                Mark[] mks = null;
                if (sm.getsorteddata(false, 0, out mks))
                {
                    writedata(mks, fm3.filename, fm3.select);
                }
                else
                {
                    MessageBox.Show(noselectedpoint);
                }
                readdir();
            }
        }

        private void button6_Click(object sender, EventArgs e)
        {
            Form3 fm3 = new Form3(button6.Text);
            fm3.setlang(sanal);
            if (fm3.ShowDialog() == DialogResult.OK)
            {
                Mark[] mks = null;
                if (sm.getsorteddata(true, 0, out mks))
                {
                    writedata(mks, fm3.filename, fm3.select);
                }
                else
                {
                    MessageBox.Show(noselectedpoint);
                }
                readdir();
            }
        }

        private void button7_Click(object sender, EventArgs e)
        {
            Form3 fm3 = new Form3(button7.Text);
            fm3.setlang(sanal);
            if (fm3.ShowDialog() == DialogResult.OK)
            {
                Mark[] mks = null;
                if (sm.getsorteddata(false, 1, out mks))
                {
                    writedata(mks, fm3.filename, fm3.select);
                }
                else
                {
                    MessageBox.Show(noselectedpoint);
                }
                readdir();
            }
        }

        private void button8_Click(object sender, EventArgs e)
        {
            Form3 fm3 = new Form3(button8.Text);
            fm3.setlang(sanal);
            if (fm3.ShowDialog() == DialogResult.OK)
            {
                Mark[] mks = null;
                if (sm.getsorteddata(true, 1, out mks))
                {
                    writedata(mks, fm3.filename, fm3.select);
                }
                else
                {
                    MessageBox.Show(noselectedpoint);
                }
                readdir();
            }
        }
        private void writedata(Mark[] mks, string filename, int select)
        {
            StreamWriter sw = new StreamWriter("posdat\\" + filename + ".txt");
            if (select == 0)
            {
                sw.Write("x,y" + System.Environment.NewLine);
                for (int i = 0; i < mks.Length; i++)
                {
                    sw.Write(mks[i].position.X + ",");
                    sw.Write(mks[i].position.Y);
                    if (i != mks.Length - 1)
                    {
                        sw.Write(System.Environment.NewLine);
                    }
                }
            }
            else
            {
                sw.Write("x,y,angle" + System.Environment.NewLine);
                for (int i = 0; i < mks.Length; i++)
                {
                    sw.Write(mks[i].position.X + ",");
                    sw.Write(mks[i].position.Y + ",");
                    sw.Write(mks[i].angle);
                    if (i != mks.Length - 1)
                    {
                        sw.Write(System.Environment.NewLine);
                    }
                }
            }
            sw.Close();
        }

        private void button9_Click(object sender, EventArgs e)
        {
            readdir();
        }

        private void button10_Click(object sender, EventArgs e)
        {
            if (this.listBox1.SelectedIndex >= 0)
            {
                if (File.Exists(filenames[this.listBox1.SelectedIndex]))
                {
                    File.Delete(filenames[this.listBox1.SelectedIndex]);
                    readdir();
                }
            }
        }

        private void label1_ValueChange(object sender, ValueChangeEventArgs e)
        {
            if (textBox1.Enabled)
            {
                textBox1.Text = (int.Parse(textBox1.Text) + e.ChangedValue).ToString();
            }
        }

        private void label2_ValueChange(object sender, ValueChangeEventArgs e)
        {
            if (textBox2.Enabled)
            {
                textBox2.Text = (int.Parse(textBox2.Text) + e.ChangedValue).ToString();
            }
        }

        private void label3_ValueChange(object sender, ValueChangeEventArgs e)
        {
            if (textBox3.Enabled)
            {
                textBox3.Text = (int.Parse(textBox3.Text) + e.ChangedValue).ToString();
            }
        }

        private void label4_ValueChange(object sender, ValueChangeEventArgs e)
        {
            if (textBox4.Enabled)
            {
                textBox4.Text = (int.Parse(textBox4.Text) + e.ChangedValue).ToString();
            }
        }
        private void label5_ValueChange(object sender, ValueChangeEventArgs e)
        {
            if (textBox5.Enabled)
            {
                textBox5.Text = (int.Parse(textBox5.Text) + e.ChangedValue).ToString();
            }
        }
        private void label6_ValueChange(object sender, ValueChangeEventArgs e)
        {
            if (textBox6.Enabled)
            {
                textBox6.Text = (int.Parse(textBox6.Text) + e.ChangedValue).ToString();
            }
        }

        private void checkBox2_CheckedChanged(object sender, EventArgs e)
        {
            changedisplaydata();
        }


    }
}
