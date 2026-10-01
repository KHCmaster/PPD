using PPDEditor.Controls;
using PPDEditor.Forms;
using System.Numerics;
using PPDMath;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace PPDEditor
{
    public partial class PosAndAngleLoaderSaver : ScrollableForm
    {
        enum SyncMode
        {
            None = 0,
            X = 1,
            Y = 2
        }
        SyncMode syncmode = SyncMode.None;
        string[] filenames;
        string noselectedpoint = "選択点がないため設定できませんでした";
        string noselectarea = "選択エリアがないため設定できませんでした";
        string headererror = "ヘッダエラー";
        string invalidstring = "無効な文字列が含まれています";
        string invalidnumber = "整数以外が入力されています。0にしました。";
        bool hasAngle;
        PosAndAngleInfo[] infos;
        Vector2[] displayPositions;
        float[] displayAngles;

        public PosAndAngleLoaderSaver()
        {
            InitializeComponent();
            radioButton1.Checked = true;
            this.textBox1.TextChanged += TextBoxTextChanged;
            this.textBox2.TextChanged += TextBoxTextChanged;
            this.textBox3.TextChanged += TextBoxTextChanged;
            this.textBox4.TextChanged += TextBoxTextChanged;
            this.textBox5.TextChanged += TextBoxTextChanged;
            this.textBox6.TextChanged += TextBoxTextChanged;
            this.radioButton1.CheckedChanged += ChangeRadioCheck;
            this.radioButton2.CheckedChanged += ChangeRadioCheck;
            this.radioButton3.CheckedChanged += ChangeRadioCheck;
            Reload();
            comboBox1.SelectedIndex = 0;
            radioButton1.Checked = true;
        }

        public void SetLang()
        {
            this.Text = Utility.Language["PAALS"];
            this.checkBox1.Text = Utility.Language["PAALSCheckBox"];
            this.checkBox2.Text = Utility.Language["PAALSCheckBox2"];
            this.button1.Text = Utility.Language["PAALSButton1"];
            this.button2.Text = Utility.Language["PAALSButton2"];
            this.button3.Text = Utility.Language["PAALSButton3"];
            this.button4.Text = Utility.Language["PAALSButton4"];
            this.button5.Text = Utility.Language["PAALSButton5"];
            this.button6.Text = Utility.Language["PAALSButton6"];
            this.button7.Text = Utility.Language["PAALSButton7"];
            this.button8.Text = Utility.Language["PAALSButton8"];
            this.button9.Text = Utility.Language["PAALSButton9"];
            this.button10.Text = Utility.Language["PAALSButton10"];
            this.groupBox1.Text = Utility.Language["PAALSGroupText"];
            this.radioButton1.Text = Utility.Language["PAALSRadioButton1"];
            this.radioButton2.Text = Utility.Language["PAALSRadioButton2"];
            this.radioButton3.Text = Utility.Language["PAALSRadioButton3"];
            noselectedpoint = Utility.Language["PAALSNoSelectedPoint"];
            noselectarea = Utility.Language["PAALSNoSelectedArea"];
            headererror = Utility.Language["PAALSHeaderError"];
            invalidstring = Utility.Language["PAALSInvalidString"];
            invalidnumber = Utility.Language["PAALSInvalidNumber"];
            this.comboBox1.Items.Clear();
            this.comboBox1.Items.Add(Utility.Language["PAALSComboBox1"]);
            this.comboBox1.Items.Add(Utility.Language["PAALSComboBox2"]);
            this.comboBox1.Items.Add(Utility.Language["PAALSComboBox3"]);
            comboBox1.SelectedIndex = 0;
        }

        public void SetSkin()
        {
            this.label1.ForeColor = PPDEditorSkin.Skin.TransparentTextColor;
            this.label2.ForeColor = PPDEditorSkin.Skin.TransparentTextColor;
            this.label3.ForeColor = PPDEditorSkin.Skin.TransparentTextColor;
            this.label4.ForeColor = PPDEditorSkin.Skin.TransparentTextColor;
            this.label5.ForeColor = PPDEditorSkin.Skin.TransparentTextColor;
            this.label6.ForeColor = PPDEditorSkin.Skin.TransparentTextColor;
            this.checkBox1.ForeColor = PPDEditorSkin.Skin.TransparentTextColor;
            this.checkBox2.ForeColor = PPDEditorSkin.Skin.TransparentTextColor;
            this.radioButton1.ForeColor = PPDEditorSkin.Skin.TransparentTextColor;
            this.radioButton2.ForeColor = PPDEditorSkin.Skin.TransparentTextColor;
            this.radioButton3.ForeColor = PPDEditorSkin.Skin.TransparentTextColor;
            this.groupBox1.ForeColor = PPDEditorSkin.Skin.TransparentTextColor;
            this.button1.ForeColor = System.Drawing.Color.Black;
            this.button2.ForeColor = System.Drawing.Color.Black;
            this.button3.ForeColor = System.Drawing.Color.Black;
            this.button4.ForeColor = System.Drawing.Color.Black;
        }

        public bool DisplayAngle
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

        public bool DrawAngle
        {
            get
            {
                return this.checkBox1.Checked && hasAngle;
            }
        }

        private void TextBoxTextChanged(object sender, EventArgs e)
        {
            if (sender is TextBox tb)
            {
                if (!int.TryParse(tb.Text, out _))
                {
                    tb.Text = "0";
                    MessageBox.Show(invalidnumber);
                }
                else
                {
                    ChangeDisplayData();
                }
            }
        }
        private void ChangeRadioCheck(object sender, EventArgs e)
        {
            if (radioButton2.Checked || radioButton3.Checked)
            {
                checkBox2.Enabled = true;
                if (radioButton2.Checked)
                {
                    textBox3.Enabled = false;
                    textBox4.Enabled = false;
                    textBox5.Enabled = false;
                    textBox6.Enabled = false;
                    comboBox1.Enabled = false;
                    textBox1.Enabled = true;
                    textBox2.Enabled = true;
                }
                else
                {
                    textBox3.Enabled = comboBox1.SelectedIndex == 0 || comboBox1.SelectedIndex == 2;
                    textBox4.Enabled = comboBox1.SelectedIndex == 0 || comboBox1.SelectedIndex == 1;
                    textBox5.Enabled = textBox3.Enabled;
                    textBox6.Enabled = textBox4.Enabled;
                    comboBox1.Enabled = true;
                    textBox1.Enabled = false;
                    textBox2.Enabled = false;
                }
            }
            else
            {
                checkBox2.Enabled = false;
                textBox3.Enabled = false;
                textBox4.Enabled = false;
                textBox5.Enabled = false;
                textBox6.Enabled = false;
                comboBox1.Enabled = false;
                textBox1.Enabled = false;
                textBox2.Enabled = false;
            }
            ChangeDisplayData();
        }
        private void Reload()
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
        private void ListBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (listBox1.SelectedIndex < 0 || listBox1.SelectedIndex >= listBox1.Items.Count)
            {
                return;
            }

            try
            {
                infos = PosAndAngleInfo.Load(filenames[listBox1.SelectedIndex]);
                hasAngle = infos.Any(i => i.Rotation.HasValue);
                ChangeDisplayData();
            }
            catch (Exception exception)
            {
                MessageBox.Show(exception.Message);
            }
        }

        private void ChangeDisplayData()
        {
            if (infos == null)
            {
                return;
            }

            displayPositions = new Vector2[infos.Length];
            displayAngles = new float[infos.Length];
            if (radioButton1.Checked)
            {
                for (int i = 0; i < infos.Length; i++)
                {
                    Vector2 position = infos[i].Position ?? Vector2.Zero;
                    float angle = infos[i].Rotation ?? 0;
                    displayPositions[i] = position;
                    if (hasAngle)
                    {
                        displayAngles[i] = angle;
                    }
                }
            }
            else if (radioButton2.Checked)
            {
                var point = new Vector2(float.Parse(this.textBox1.Text), float.Parse(this.textBox2.Text));
                for (int i = 0; i < infos.Length; i++)
                {
                    Vector2 position = infos[i].Position ?? Vector2.Zero;
                    float angle = infos[i].Rotation ?? 0;
                    displayPositions[i] = new Vector2(position.X + 2 * (point.X - position.X), position.Y + 2 * (point.Y - position.Y));
                    if (hasAngle)
                    {
                        if (checkBox2.Checked)
                        {
                            var na = GetNormalAngle(angle);
                            displayAngles[i] = (float)(na - Math.PI);
                        }
                        else
                        {
                            displayAngles[i] = angle;
                        }
                    }
                }
            }
            else if (radioButton3.Checked)
            {
                float x1 = float.Parse(this.textBox3.Text), y1 = float.Parse(this.textBox4.Text), x2 = float.Parse(this.textBox5.Text), y2 = float.Parse(this.textBox6.Text);
                var basicvec = MathCompat.NormalizeSharpDX(new Vector2(x2 - x1, y2 - y1));
                for (int i = 0; i < infos.Length; i++)
                {
                    Vector2 position = infos[i].Position ?? Vector2.Zero;
                    float angle = infos[i].Rotation ?? 0;
                    var targetvec = new Vector2(position.X - x1, position.Y - y1);
                    var targetvecnormal = MathCompat.NormalizeSharpDX(targetvec);
                    var bitweenangle = Vector2.Dot(basicvec, targetvecnormal);
                    var anservec = Vector2.Add(targetvecnormal, 2 * (bitweenangle * basicvec - targetvecnormal));
                    displayPositions[i] = new Vector2(anservec.X * Vector2.Distance(Vector2.Zero, targetvec) + x1, anservec.Y * Vector2.Distance(Vector2.Zero, targetvec) + y1);
                    if (hasAngle)
                    {
                        if (checkBox2.Checked)
                        {
                            var na = GetNormalAngle(angle);
                            var d = (float)Math.Acos(-basicvec.X);
                            var trans = (float)(GetNormalAngle(na - d));
                            trans = (float)(Math.PI * 2 - trans);
                            displayAngles[i] = trans + d;
                        }
                        else
                        {
                            displayAngles[i] = angle;
                        }
                    }
                }
            }
            for (int i = 0; i < infos.Length; i++)
            {
                displayPositions[i] = MathCompat.ClampSharpDX(displayPositions[i], Vector2.Zero, new Vector2(800, 450));
                displayAngles[i] = NormalizeAngle(displayAngles[i]);
            }
            WindowUtility.MainForm?.ChangeData(displayPositions, displayAngles);
        }

        private static double GetNormalAngle(double angle)
        {
            var shou = (int)Math.Floor(angle / (Math.PI * 2));
            return angle - Math.PI * 2 * shou;
        }

        private static float NormalizeAngle(float angle)
        {
            var val = Math.IEEERemainder(angle, Math.PI * 2);
            if (val < 0) val += Math.PI * 2;
            return (float)val;
        }

        private void Button1_Click(object sender, EventArgs e)
        {
            if (this.listBox1.SelectedIndex >= 0)
            {
                if (!WindowUtility.Seekmain.SetData(displayPositions, displayAngles, false, 0, hasAngle))
                {
                    MessageBox.Show(noselectedpoint);
                }
            }
        }
        private void Button2_Click(object sender, EventArgs e)
        {
            if (this.listBox1.SelectedIndex >= 0)
            {
                if (!WindowUtility.Seekmain.SetData(displayPositions, displayAngles, true, 0, hasAngle))
                {
                    MessageBox.Show(noselectedpoint);
                }
            }
        }

        private void Button3_Click(object sender, EventArgs e)
        {
            if (this.listBox1.SelectedIndex >= 0)
            {
                if (!WindowUtility.Seekmain.SetData(displayPositions, displayAngles, false, 1, hasAngle))
                {
                    MessageBox.Show(noselectarea);
                }
            }
        }

        private void Button4_Click(object sender, EventArgs e)
        {
            if (this.listBox1.SelectedIndex >= 0)
            {
                if (!WindowUtility.Seekmain.SetData(displayPositions, displayAngles, true, 1, hasAngle))
                {
                    MessageBox.Show(noselectarea);
                }
            }
        }
        private void Button5_Click(object sender, EventArgs e)
        {
            var fm3 = new SavePosAndAngleForm(button5.Text);
            fm3.SetLang();
            if (fm3.ShowDialog() == DialogResult.OK)
            {
                if (WindowUtility.Seekmain.GetSortedData(false, false, out Mark[] mks))
                {
                    WriteData(mks, fm3.FileName, fm3.WithAngle);
                }
                else
                {
                    MessageBox.Show(noselectedpoint);
                }
                Reload();
            }
        }

        private void Button6_Click(object sender, EventArgs e)
        {
            var fm3 = new SavePosAndAngleForm(button6.Text);
            fm3.SetLang();
            if (fm3.ShowDialog() == DialogResult.OK)
            {
                if (WindowUtility.Seekmain.GetSortedData(true, false, out Mark[] mks))
                {
                    WriteData(mks, fm3.FileName, fm3.WithAngle);
                }
                else
                {
                    MessageBox.Show(noselectedpoint);
                }
                Reload();
            }
        }

        private void Button7_Click(object sender, EventArgs e)
        {
            var fm3 = new SavePosAndAngleForm(button7.Text);
            fm3.SetLang();
            if (fm3.ShowDialog() == DialogResult.OK)
            {
                if (WindowUtility.Seekmain.GetSortedData(false, true, out Mark[] mks))
                {
                    WriteData(mks, fm3.FileName, fm3.WithAngle);
                }
                else
                {
                    MessageBox.Show(noselectedpoint);
                }
                Reload();
            }
        }

        private void Button8_Click(object sender, EventArgs e)
        {
            var fm3 = new SavePosAndAngleForm(button8.Text);
            fm3.SetLang();
            if (fm3.ShowDialog() == DialogResult.OK)
            {
                if (WindowUtility.Seekmain.GetSortedData(true, true, out Mark[] mks))
                {
                    WriteData(mks, fm3.FileName, fm3.WithAngle);
                }
                else
                {
                    MessageBox.Show(noselectedpoint);
                }
                Reload();
            }
        }
        private static void WriteData(Mark[] mks, string fileName, bool withAngle)
        {
            var filePath = Path.Combine("posdat", Path.ChangeExtension(fileName, ".txt"));
            var infos = new List<PosAndAngleInfo>();
            foreach (var mk in mks)
            {
                if (withAngle)
                {
                    infos.Add(new PosAndAngleInfo(mk.Position, mk.Rotation));
                }
                else
                {
                    infos.Add(new PosAndAngleInfo(mk.Position));
                }
            }
            PosAndAngleInfo.Save([.. infos], filePath);
        }

        private void Button9_Click(object sender, EventArgs e)
        {
            Reload();
        }

        private void Button10_Click(object sender, EventArgs e)
        {
            if (this.listBox1.SelectedIndex >= 0)
            {
                if (File.Exists(filenames[this.listBox1.SelectedIndex]))
                {
                    File.Delete(filenames[this.listBox1.SelectedIndex]);
                    Reload();
                }
            }
        }

        private void Label1_ValueChange(object sender, ValueChangeEventArgs e)
        {
            if (textBox1.Enabled)
            {
                textBox1.Text = (int.Parse(textBox1.Text) + e.ChangedValue).ToString();
            }
        }

        private void Label2_ValueChange(object sender, ValueChangeEventArgs e)
        {
            if (textBox2.Enabled)
            {
                textBox2.Text = (int.Parse(textBox2.Text) + e.ChangedValue).ToString();
            }
        }

        private void Label3_ValueChange(object sender, ValueChangeEventArgs e)
        {
            if (textBox3.Enabled)
            {
                textBox3.Text = (int.Parse(textBox3.Text) + e.ChangedValue).ToString();
                if (syncmode == SyncMode.X) textBox5.Text = textBox3.Text;
            }
        }

        private void Label4_ValueChange(object sender, ValueChangeEventArgs e)
        {
            if (textBox4.Enabled)
            {
                textBox4.Text = (int.Parse(textBox4.Text) + e.ChangedValue).ToString();
                if (syncmode == SyncMode.Y) textBox6.Text = textBox4.Text;
            }
        }
        private void Label5_ValueChange(object sender, ValueChangeEventArgs e)
        {
            if (textBox5.Enabled)
            {
                textBox5.Text = (int.Parse(textBox5.Text) + e.ChangedValue).ToString();
                if (syncmode == SyncMode.X) textBox3.Text = textBox5.Text;
            }
        }
        private void Label6_ValueChange(object sender, ValueChangeEventArgs e)
        {
            if (textBox6.Enabled)
            {
                textBox6.Text = (int.Parse(textBox6.Text) + e.ChangedValue).ToString();
                if (syncmode == SyncMode.Y) textBox4.Text = textBox6.Text;
            }
        }

        private void CheckBox2_CheckedChanged(object sender, EventArgs e)
        {
            ChangeDisplayData();
        }

        private void ComboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (!comboBox1.Enabled) return;
            switch (comboBox1.SelectedIndex)
            {
                case 0:
                    this.textBox3.Enabled = true;
                    this.textBox4.Enabled = true;
                    this.textBox5.Enabled = true;
                    this.textBox6.Enabled = true;
                    syncmode = SyncMode.None;
                    break;
                case 1:
                    this.textBox3.Enabled = false;
                    this.textBox4.Enabled = true;
                    this.textBox5.Enabled = false;
                    this.textBox6.Enabled = true;
                    this.textBox3.Text = "0";
                    this.textBox4.Text = "225";
                    this.textBox5.Text = "800";
                    this.textBox6.Text = "225";
                    syncmode = SyncMode.Y;
                    break;
                case 2:
                    this.textBox3.Enabled = true;
                    this.textBox4.Enabled = false;
                    this.textBox5.Enabled = true;
                    this.textBox6.Enabled = false;
                    this.textBox3.Text = "400";
                    this.textBox4.Text = "0";
                    this.textBox5.Text = "400";
                    this.textBox6.Text = "450";
                    syncmode = SyncMode.X;
                    break;
            }
        }


    }
}
