using PPDFramework;
using System;
using System.Drawing;
using System.IO;
using System.Reflection;
using System.Windows.Forms;

namespace PPDEditor.Forms
{
    public partial class CreateProjectForm : Form
    {
        enum Mode
        {
            None = 0,
            SaveDir,
            Movie,
            ScoreSetting,
            InitialSound,
            Finished
        }

        public string ProjectDir
        {
            get
            {
                return this.textBox1.Text;
            }
        }

        public string MoviePath
        {
            get
            {
                return this.textBox2.Text;
            }
        }

        public string ScoreName
        {
            get
            {
                return this.textBox3.Text;
            }
        }

        public NoteType NoteType
        {
            get
            {
                if (radioButton1.Checked)
                {
                    return PPDFramework.NoteType.Normal;
                }
                else if (radioButton2.Checked)
                {
                    return PPDFramework.NoteType.AC;
                }
                else
                {
                    return PPDFramework.NoteType.ACFT;
                }
            }
        }

        public string InitialSoundPath
        {
            get
            {
                return this.textBox4.Text;
            }
        }

        public bool InitialSoundSkipped
        {
            get;
            private set;
        }

        private Mode CurrentMode
        {
            get
            {
                if (panel1.Visible)
                {
                    return Mode.SaveDir;
                }
                if (panel2.Visible)
                {
                    return Mode.Movie;
                }
                if (panel3.Visible)
                {
                    return Mode.ScoreSetting;
                }
                if (panel4.Visible)
                {
                    return Mode.InitialSound;
                }
                if (panel5.Visible)
                {
                    return Mode.Finished;
                }
                return Mode.None;
            }
        }

        private string done = "完了";
        string openmoviefilter = "すべてのファイル(*.*)|*.*";
        string wavefilter = "サウンドファイル(*.wav;*ogg)|*.wav;*ogg|すべてのファイル(*.*)|*.*";

        public CreateProjectForm()
        {
            InitializeComponent();


            var dir = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);
            if (!Directory.Exists(Path.Combine(dir, "Projects")))
            {
                Directory.CreateDirectory(Path.Combine(dir, "Projects"));
            }

            this.textBox1.Text = Path.Combine(dir, "Projects");

            if (File.Exists(Path.Combine(dir, "sound.wav")))
            {
                this.textBox4.Text = Path.Combine(dir, "sound.wav");
            }
        }

        public void SetLang()
        {
            this.Text = Utility.Language["CPF"];
            this.label1.Text = Utility.Language["CPFLabel1"];
            this.label2.Text = Utility.Language["CPFLabel2"];
            this.label3.Text = Utility.Language["CPFLabel3"];
            this.label4.Text = Utility.Language["CPFLabel4"];
            this.label5.Text = Utility.Language["CPFLabel5"];
            this.label6.Text = Utility.Language["CPFLabel6"];
            this.label7.Text = Utility.Language["CPFLabel7"];
            this.label8.Text = Utility.Language["CPFLabel8"];
            this.label9.Text = Utility.Language["CPFLabel9"];
            this.label10.Text = Utility.Language["CPFLabel10"];
            this.label11.Text = Utility.Language["CPFLabel11"];
            this.radioButton1.Text = Utility.Language["CPFRadioButton1"];
            this.radioButton2.Text = Utility.Language["CPFRadioButton2"];
            this.button1.Text = Utility.Language["CPFButton1"];
            this.button2.Text = Utility.Language["CPFButton2"];
            this.button3.Text = Utility.Language["CPFButton3"];
            this.button4.Text = Utility.Language["CPFChange"];
            this.button5.Text = Utility.Language["CPFChange"];
            this.button6.Text = Utility.Language["CPFChange"];
            done = Utility.Language["CPFDone"];
        }

        private void button1_Click(object sender, EventArgs e)
        {
            switch (CurrentMode)
            {
                case Mode.SaveDir:
                    break;
                case Mode.Movie:
                    this.label6.ForeColor = Color.DimGray;
                    pictureBox6.Visible = false;
                    pictureBox1.Visible = false;
                    pictureBox5.Visible = true;
                    this.panel2.Visible = false;
                    this.panel1.Visible = true;

                    this.button1.Enabled = false;
                    break;
                case Mode.ScoreSetting:
                    this.label7.ForeColor = Color.DimGray;
                    pictureBox7.Visible = false;
                    pictureBox2.Visible = false;
                    pictureBox6.Visible = true;
                    this.panel3.Visible = false;
                    this.panel2.Visible = true;
                    break;
                case Mode.InitialSound:
                    this.label8.ForeColor = Color.DimGray;
                    pictureBox8.Visible = false;
                    pictureBox3.Visible = false;
                    pictureBox7.Visible = true;
                    this.panel4.Visible = false;
                    this.panel3.Visible = true;

                    this.button3.Enabled = false;
                    break;
                case Mode.Finished:
                    pictureBox4.Visible = false;
                    pictureBox8.Visible = true;
                    this.panel5.Visible = false;
                    this.panel4.Visible = true;
                    break;
            }
        }

        private bool CheckProjectName()
        {
            if (!Utility.CheckValidFileName(ScoreName))
            {
                MessageBox.Show(String.Format(Utility.Language["ContainsInvalidChars"], Utility.Language["ScoreName"]));
                return false;
            }

            return true;
        }

        private void button2_Click(object sender, EventArgs e)
        {
            switch (CurrentMode)
            {
                case Mode.SaveDir:
                    this.label6.ForeColor = Color.White;
                    pictureBox5.Visible = false;
                    pictureBox1.Visible = true;
                    pictureBox6.Visible = true;
                    panel1.Visible = false;
                    panel2.Visible = true;

                    this.button1.Enabled = true;
                    break;
                case Mode.Movie:
                    this.label7.ForeColor = Color.White;
                    pictureBox6.Visible = false;
                    pictureBox2.Visible = true;
                    pictureBox7.Visible = true;
                    panel2.Visible = false;
                    panel3.Visible = true;
                    break;
                case Mode.ScoreSetting:
                    if (!CheckProjectName())
                    {
                        return;
                    }
                    this.label8.ForeColor = Color.White;
                    pictureBox7.Visible = false;
                    pictureBox3.Visible = true;
                    pictureBox8.Visible = true;
                    panel3.Visible = false;
                    panel4.Visible = true;
                    this.button3.Enabled = true;
                    break;
                case Mode.InitialSound:
                    pictureBox8.Visible = false;
                    pictureBox4.Visible = true;
                    panel4.Visible = false;
                    panel5.Visible = true;

                    this.button2.Enabled = true;
                    this.button1.Enabled = false;
                    this.button3.Enabled = false;
                    this.button2.Text = done;
                    break;
                case Mode.Finished:
                    this.DialogResult = DialogResult.OK;
                    this.Close();
                    break;
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            if (CurrentMode == Mode.InitialSound)
            {
                InitialSoundSkipped = true;
                button2_Click(button2, e);
            }
        }

        private void button4_Click(object sender, EventArgs e)
        {
            folderBrowserDialog1.SelectedPath = this.textBox1.Text;
            if (folderBrowserDialog1.ShowDialog() == DialogResult.OK)
            {
                this.textBox1.Text = folderBrowserDialog1.SelectedPath;
            }
        }

        private void button5_Click(object sender, EventArgs e)
        {
            openFileDialog1.FileName = this.textBox2.Text;
            openFileDialog1.Filter = openmoviefilter;
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                this.textBox2.Text = openFileDialog1.FileName;
            }
        }

        private void button6_Click(object sender, EventArgs e)
        {
            openFileDialog1.FileName = this.textBox4.Text;
            openFileDialog1.Filter = wavefilter;
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                this.textBox4.Text = openFileDialog1.FileName;
            }
        }
    }
}
