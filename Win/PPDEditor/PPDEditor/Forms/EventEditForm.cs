using PPDEditorCommon;
using PPDFramework;
using PPDFramework.PPDStructure.EVDData;
using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace PPDEditor.Forms
{
    public partial class EventEditForm : Form
    {
        ButtonType[] normalInitializeOrder = {
            ButtonType.Square,
            ButtonType.Cross,
            ButtonType.Circle,
            ButtonType.Triangle,
            ButtonType.Left,
            ButtonType.Down,
            ButtonType.Right,
            ButtonType.Up,
            ButtonType.R,
            ButtonType.L
        };
        ButtonType[] ACInitializeOrder = {
            ButtonType.Triangle,
            ButtonType.Square,
            ButtonType.Cross,
            ButtonType.Circle,
            ButtonType.Up,
            ButtonType.Left,
            ButtonType.Down,
            ButtonType.Right,
            ButtonType.R,
            ButtonType.L
        };
        List<ButtonType> initializeOrder;
        string[] orderName;
        private bool isReadOnly;
        public EventEditForm()
        {
            InitializeComponent();
            this.soundVolumeControl1.SetBackColor(this.tabPage1.BackColor);
            this.soundVolumeControl2.SetBackColor(this.tabPage1.BackColor);
            this.soundVolumeControl3.SetBackColor(this.tabPage1.BackColor);
            this.soundVolumeControl4.SetBackColor(this.tabPage1.BackColor);
            this.soundVolumeControl5.SetBackColor(this.tabPage1.BackColor);
            this.soundVolumeControl6.SetBackColor(this.tabPage1.BackColor);
            this.soundVolumeControl7.SetBackColor(this.tabPage1.BackColor);
            this.soundVolumeControl8.SetBackColor(this.tabPage1.BackColor);
            this.soundVolumeControl9.SetBackColor(this.tabPage1.BackColor);
            this.soundVolumeControl10.SetBackColor(this.tabPage1.BackColor);
            this.soundVolumeControl11.SetBackColor(this.tabPage1.BackColor);
            this.radioButton1.CheckedChanged += DstateChanged;
            this.radioButton2.CheckedChanged += DstateChanged;
            this.radioButton3.CheckedChanged += DstateChanged;
            this.radioButton4.CheckedChanged += MstateChanged;
            this.radioButton5.CheckedChanged += MstateChanged;
            this.radioButton6.CheckedChanged += ACChanged;
            this.radioButton7.CheckedChanged += ACChanged;
            this.radioButton8.CheckedChanged += InitializeOrderChanged;
            this.radioButton9.CheckedChanged += InitializeOrderChanged;
            this.radioButton10.CheckedChanged += InitializeOrderChanged;
        }

        private void InitializeOrderChanged(object sender, EventArgs e)
        {
            if (radioButton8.Checked)
            {
                initializeOrder = new List<ButtonType>(normalInitializeOrder);
                UpdateList();
            }
            else if (radioButton9.Checked)
            {
                initializeOrder = new List<ButtonType>(ACInitializeOrder);
                UpdateList();
            }

            this.button3.Enabled = this.button4.Enabled = radioButton10.Checked;
        }

        private void UpdateList()
        {
            listBox1.Items.Clear();
            foreach (ButtonType buttonType in initializeOrder)
            {
                listBox1.Items.Add(orderName[(int)buttonType]);
            }
        }

        private void DstateChanged(object sender, EventArgs e)
        {
            if (radioButton1.Checked)
            {
                radioButton2.Checked = false;
                radioButton3.Checked = false;
            }
            else if (radioButton2.Checked)
            {
                radioButton1.Checked = false;
                radioButton3.Checked = false;
            }
            else if (radioButton3.Checked)
            {
                radioButton1.Checked = false;
                radioButton2.Checked = false;
            }
        }
        private void MstateChanged(object sender, EventArgs e)
        {
            if (radioButton4.Checked) radioButton5.Checked = false;
            else radioButton4.Checked &= !radioButton5.Checked;
        }
        private void ACChanged(object sender, EventArgs e)
        {
            if (radioButton6.Checked) radioButton7.Checked = false;
            else radioButton6.Checked &= !radioButton7.Checked;
        }
        public EventData EventData
        {
            get
            {
                var ret = new EventData
                {
                    MovieVolumePercent = this.soundVolumeControl1.VolumePercent,
                    SquareVolumePercent = this.soundVolumeControl2.VolumePercent,
                    CrossVolumePercent = this.soundVolumeControl3.VolumePercent,
                    CircleVolumePercent = this.soundVolumeControl4.VolumePercent,
                    TriangleVolumePercent = this.soundVolumeControl5.VolumePercent,
                    LeftVolumePercent = this.soundVolumeControl6.VolumePercent,
                    DownVolumePercent = this.soundVolumeControl7.VolumePercent,
                    RightVolumePercent = this.soundVolumeControl8.VolumePercent,
                    UpVolumePercent = this.soundVolumeControl9.VolumePercent,
                    RVolumePercent = this.soundVolumeControl10.VolumePercent,
                    LVolumePercent = this.soundVolumeControl11.VolumePercent,
                    BPMRapidChange = this.checkBox1.Checked
                };
                if (!float.TryParse(this.textBox2.Text, out float bpm))
                {
                    bpm = 100;
                }
                ret.BPM = bpm;
                ret.BPMRapidChange = this.checkBox1.Checked;
                ret.SquareKeepPlaying = this.checkBox2.Checked;
                ret.CrossKeepPlaying = this.checkBox3.Checked;
                ret.CircleKeepPlaying = this.checkBox4.Checked;
                ret.TriangleKeepPlaying = this.checkBox5.Checked;
                ret.LeftKeepPlaying = this.checkBox6.Checked;
                ret.DownKeepPlaying = this.checkBox7.Checked;
                ret.RightKeepPlaying = this.checkBox8.Checked;
                ret.UpKeepPlaying = this.checkBox9.Checked;
                ret.RKeepPlaying = this.checkBox10.Checked;
                ret.LKeepPlaying = this.checkBox11.Checked;
                ret.SquareReleaseSound = this.checkBox12.Checked;
                ret.CrossReleaseSound = this.checkBox13.Checked;
                ret.CircleReleaseSound = this.checkBox14.Checked;
                ret.TriangleReleaseSound = this.checkBox15.Checked;
                ret.LeftReleaseSound = this.checkBox16.Checked;
                ret.DownReleaseSound = this.checkBox17.Checked;
                ret.RightReleaseSound = this.checkBox18.Checked;
                ret.UpReleaseSound = this.checkBox19.Checked;
                ret.RReleaseSound = this.checkBox20.Checked;
                ret.LReleaseSound = this.checkBox21.Checked;
                if (radioButton1.Checked) ret.DisplayState = DisplayState.Normal;
                if (radioButton2.Checked) ret.DisplayState = DisplayState.Sudden;
                if (radioButton3.Checked) ret.DisplayState = DisplayState.Hidden;
                if (radioButton4.Checked) ret.MoveState = MoveState.Normal;
                if (radioButton5.Checked) ret.MoveState = MoveState.Stop;
                if (radioButton6.Checked) ret.NoteType = NoteType.Normal;
                if (radioButton7.Checked) ret.NoteType = NoteType.AC;
                if (radioButton11.Checked) ret.NoteType = NoteType.ACFT;
                ret.InitializeOrder = initializeOrder.ToArray();
                if (!float.TryParse(this.textBox3.Text, out float slideScale))
                {
                    slideScale = 1;
                }
                ret.SlideScale = slideScale;
                UpdateList();
                return ret;
            }
            set
            {
                this.soundVolumeControl1.VolumePercent = value.MovieVolumePercent;
                this.soundVolumeControl2.VolumePercent = value.SquareVolumePercent;
                this.soundVolumeControl3.VolumePercent = value.CrossVolumePercent;
                this.soundVolumeControl4.VolumePercent = value.CircleVolumePercent;
                this.soundVolumeControl5.VolumePercent = value.TriangleVolumePercent;
                this.soundVolumeControl6.VolumePercent = value.LeftVolumePercent;
                this.soundVolumeControl7.VolumePercent = value.DownVolumePercent;
                this.soundVolumeControl8.VolumePercent = value.RightVolumePercent;
                this.soundVolumeControl9.VolumePercent = value.UpVolumePercent;
                this.soundVolumeControl10.VolumePercent = value.RVolumePercent;
                this.soundVolumeControl11.VolumePercent = value.LVolumePercent;
                this.textBox1.Text = value.BPM.ToString();
                this.textBox2.Text = value.BPM.ToString();
                this.checkBox1.Checked = value.BPMRapidChange;
                this.checkBox2.Checked = value.SquareKeepPlaying;
                this.checkBox3.Checked = value.CrossKeepPlaying;
                this.checkBox4.Checked = value.CircleKeepPlaying;
                this.checkBox5.Checked = value.TriangleKeepPlaying;
                this.checkBox6.Checked = value.LeftKeepPlaying;
                this.checkBox7.Checked = value.DownKeepPlaying;
                this.checkBox8.Checked = value.RightKeepPlaying;
                this.checkBox9.Checked = value.UpKeepPlaying;
                this.checkBox10.Checked = value.RKeepPlaying;
                this.checkBox11.Checked = value.LKeepPlaying;
                this.checkBox12.Checked = value.SquareReleaseSound;
                this.checkBox13.Checked = value.CrossReleaseSound;
                this.checkBox14.Checked = value.CircleReleaseSound;
                this.checkBox15.Checked = value.TriangleReleaseSound;
                this.checkBox16.Checked = value.LeftReleaseSound;
                this.checkBox17.Checked = value.DownReleaseSound;
                this.checkBox18.Checked = value.RightReleaseSound;
                this.checkBox19.Checked = value.UpReleaseSound;
                this.checkBox20.Checked = value.RReleaseSound;
                this.checkBox21.Checked = value.LReleaseSound;
                switch (value.DisplayState)
                {
                    case DisplayState.Normal:
                        radioButton1.Checked = true;
                        break;
                    case DisplayState.Sudden:
                        radioButton2.Checked = true;
                        break;
                    case DisplayState.Hidden:
                        radioButton3.Checked = true;
                        break;
                }
                switch (value.MoveState)
                {
                    case MoveState.Normal:
                        radioButton4.Checked = true;
                        break;
                    case MoveState.Stop:
                        radioButton5.Checked = true;
                        break;
                }
                switch (value.NoteType)
                {
                    case NoteType.Normal:
                        radioButton6.Checked = true;
                        break;
                    case NoteType.AC:
                        radioButton7.Checked = true;
                        break;
                    case NoteType.ACFT:
                        radioButton11.Checked = true;
                        break;
                }
                textBox3.Text = value.SlideScale.ToString();

                initializeOrder = new List<ButtonType>(value.InitializeOrder);
                if (Utility.IsSameArray(normalInitializeOrder, value.InitializeOrder))
                {
                    radioButton8.Checked = true;
                }
                else if (Utility.IsSameArray(ACInitializeOrder, value.InitializeOrder))
                {
                    radioButton9.Checked = true;
                }
                else
                {
                    radioButton10.Checked = true;
                }
                UpdateList();

            }
        }

        public bool IsReadOnly
        {
            get
            {
                return isReadOnly;
            }
            set
            {
                if (isReadOnly != value)
                {
                    isReadOnly = value;
                    foreach (Control control in tabPage1.Controls)
                    {
                        control.Enabled = !isReadOnly;
                    }
                    foreach (Control control in tabPage2.Controls)
                    {
                        control.Enabled = !isReadOnly;
                    }
                }
            }
        }

        public void SetLang()
        {
            this.Text = Utility.Language["EEF"];
            this.tabPage1.Text = Utility.Language["EEFTab1"];
            this.tabPage2.Text = Utility.Language["EEFTabOthers"];
            this.label1.Text = Utility.Language["EEFLabel1"];
            this.label2.Text = Utility.Language["EEFLabel2"];
            this.label3.Text = Utility.Language["EEFLabel3"];
            this.label4.Text = Utility.Language["EEFLabel4"];
            this.label5.Text = Utility.Language["EEFLabel5"];
            this.label6.Text = Utility.Language["EEFLabel6"];
            this.label7.Text = Utility.Language["EEFLabel7"];
            this.label8.Text = Utility.Language["EEFLabel8"];
            this.label9.Text = Utility.Language["EEFLabel9"];
            this.label10.Text = Utility.Language["EEFLabel10"];
            this.label11.Text = Utility.Language["EEFLabel11"];
            this.label12.Text = Utility.Language["EEFLabel12"];
            this.label13.Text = Utility.Language["EEFLabel13"];
            this.groupBox1.Text = Utility.Language["EEFGroup1"];
            this.toolTip1.SetToolTip(this.pictureBox1, Utility.Language["EEFError1"]);
            this.groupBox2.Text = Utility.Language["EEFGroup2"];
            this.groupBox3.Text = Utility.Language["EEFGroup3"];
            this.groupBox5.Text = Utility.Language["EEFGroup5"];
            this.radioButton1.Text = Utility.Language["EEFRadioButton1"];
            this.radioButton2.Text = Utility.Language["EEFRadioButton2"];
            this.radioButton3.Text = Utility.Language["EEFRadioButton3"];
            this.radioButton4.Text = Utility.Language["EEFRadioButton4"];
            this.radioButton5.Text = Utility.Language["EEFRadioButton5"];
            this.radioButton6.Text = Utility.Language["EEFRadioButton4"];
            this.radioButton8.Text = Utility.Language["EEFRadioButton4"];
            this.radioButton10.Text = Utility.Language["EEFRadioButton10"];
            this.checkBox1.Text = Utility.Language["EEFCheckBox1"];
            this.checkBox2.Text = Utility.Language["EEFKeepPlaying"];
            this.checkBox3.Text = Utility.Language["EEFKeepPlaying"];
            this.checkBox4.Text = Utility.Language["EEFKeepPlaying"];
            this.checkBox5.Text = Utility.Language["EEFKeepPlaying"];
            this.checkBox6.Text = Utility.Language["EEFKeepPlaying"];
            this.checkBox7.Text = Utility.Language["EEFKeepPlaying"];
            this.checkBox8.Text = Utility.Language["EEFKeepPlaying"];
            this.checkBox9.Text = Utility.Language["EEFKeepPlaying"];
            this.checkBox10.Text = Utility.Language["EEFKeepPlaying"];
            this.checkBox11.Text = Utility.Language["EEFKeepPlaying"];
            this.checkBox12.Text = Utility.Language["EEFReleaseSound"];
            this.checkBox13.Text = Utility.Language["EEFReleaseSound"];
            this.checkBox14.Text = Utility.Language["EEFReleaseSound"];
            this.checkBox15.Text = Utility.Language["EEFReleaseSound"];
            this.checkBox16.Text = Utility.Language["EEFReleaseSound"];
            this.checkBox17.Text = Utility.Language["EEFReleaseSound"];
            this.checkBox18.Text = Utility.Language["EEFReleaseSound"];
            this.checkBox19.Text = Utility.Language["EEFReleaseSound"];
            this.checkBox20.Text = Utility.Language["EEFReleaseSound"];
            this.checkBox21.Text = Utility.Language["EEFReleaseSound"];
            this.groupBox6.Text = Utility.Language["Slide"];
            this.label14.Text = Utility.Language["Scale"];
            orderName = new string[10];
            for (int i = 0; i < orderName.Length; i++)
            {
                orderName[i] = Utility.Language[String.Format("SMLabel{0}", i + 1)];
            }
            this.button3.Text = Utility.Language["EEFButton3"];
            this.button4.Text = Utility.Language["EEFButton4"];
        }
        private void textBox2_TextChanged(object sender, EventArgs e)
        {
            if (!float.TryParse(this.textBox2.Text, out float temp))
            {
                this.pictureBox1.Show();
            }
            else
            {
                this.pictureBox1.Hide();
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }

        private void button3_Click(object sender, EventArgs e)
        {
            if (listBox1.SelectedIndex >= 0)
            {
                int newIndex = listBox1.SelectedIndex - 1;
                if (newIndex >= 0)
                {
                    ButtonType buttonType = initializeOrder[listBox1.SelectedIndex];
                    initializeOrder.RemoveAt(listBox1.SelectedIndex);
                    initializeOrder.Insert(newIndex, buttonType);
                    UpdateList();
                    listBox1.SelectedIndex = newIndex;
                }
            }
        }

        private void button4_Click(object sender, EventArgs e)
        {
            if (listBox1.SelectedIndex >= 0)
            {
                int newIndex = listBox1.SelectedIndex + 1;
                if (newIndex < initializeOrder.Count)
                {
                    ButtonType buttonType = initializeOrder[listBox1.SelectedIndex];
                    initializeOrder.RemoveAt(listBox1.SelectedIndex);
                    initializeOrder.Insert(newIndex, buttonType);
                    UpdateList();
                    listBox1.SelectedIndex = newIndex;
                }
            }
        }
    }
}
