using PPDFramework;
using PPDFramework.PPDStructure;
using PPDFrameworkCore;
using PPDPack;
using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Windows.Forms;

namespace PPDEditor.Forms
{
    public partial class PublishForm : Form
    {
        public PublishForm()
        {
            InitializeComponent();
            if (Directory.Exists("publish"))
            {
                Directory.CreateDirectory("publish");
            }
            this.textBox1.Text = Path.GetFullPath("publish");
            checkBox1.CheckedChanged += CheckedChanged;
            checkBox2.CheckedChanged += CheckedChanged;
            checkBox3.CheckedChanged += CheckedChanged;
            checkBox4.CheckedChanged += CheckedChanged;
        }

        public void SetLang()
        {
            this.Text = Utility.Language["Publish"];
            this.label1.Text = Utility.Language["PFLabel1"];
            this.label2.Text = Utility.Language["PFLabel2"];
            this.label3.Text = Utility.Language["PFLabel3"];
            this.groupBox1.Text = Utility.Language["PFGroupBox1"];
        }

        void CheckedChanged(object sender, EventArgs e)
        {
            CheckOKButtonEnable();
        }

        private void CheckOKButtonEnable()
        {
            button1.Enabled = (checkBox1.Checked || checkBox2.Checked || checkBox3.Checked || checkBox4.Checked) && textBox2.Text != "";
        }

        private void textBox2_TextChanged(object sender, EventArgs e)
        {
            CheckOKButtonEnable();
        }

        public AvailableDifficulty AvailableDifficulty
        {
            set
            {
                checkBox1.Enabled = checkBox1.Checked = (value & AvailableDifficulty.Easy) == AvailableDifficulty.Easy;
                checkBox2.Enabled = checkBox2.Checked = (value & AvailableDifficulty.Normal) == AvailableDifficulty.Normal;
                checkBox3.Enabled = checkBox3.Checked = (value & AvailableDifficulty.Hard) == AvailableDifficulty.Hard;
                checkBox4.Enabled = checkBox4.Checked = (value & AvailableDifficulty.Extreme) == AvailableDifficulty.Extreme;
                CheckOKButtonEnable();
            }
        }

        public AvailableDifficulty PublishDifficulty
        {
            get
            {
                AvailableDifficulty ret = AvailableDifficulty.None;
                ret |= checkBox1.Checked ? AvailableDifficulty.Easy : AvailableDifficulty.None;
                ret |= checkBox2.Checked ? AvailableDifficulty.Normal : AvailableDifficulty.None;
                ret |= checkBox3.Checked ? AvailableDifficulty.Hard : AvailableDifficulty.None;
                ret |= checkBox4.Checked ? AvailableDifficulty.Extreme : AvailableDifficulty.None;
                return ret;
            }
        }

        public string PublishFolderPath
        {
            get
            {
                return this.textBox1.Text;
            }
        }

        public string PublishFolderName
        {
            get
            {
                return this.textBox2.Text;
            }
            set
            {
                this.textBox2.Text = value;
            }
        }

        private bool CheckFolderName()
        {
            if (!Utility.CheckValidFileName(PublishFolderName))
            {
                MessageBox.Show(String.Format(Utility.Language["ContainsInvalidChars"], Utility.Language["PFLabel3"]));
                return false;
            }
            return true;
        }

        private bool CheckSameScore()
        {
            var hashs = new Dictionary<string, PPDEditor.AvailableDifficulty>();
            var sha256 = new SHA256Managed();
            bool found = false;
            foreach (AvailableDifficulty difficulty in EditorForm.DifficultyArray)
            {
                if (difficulty == AvailableDifficulty.None) continue;
                if ((PublishDifficulty & difficulty) == difficulty)
                {
                    var filePath = Path.Combine(WindowUtility.MainForm.CurrentProjectDir, difficulty + ".ppd");
                    var hash = CryptographyUtility.Getx2Encoding(sha256.ComputeHash(File.ReadAllBytes(filePath)));
                    if (hashs.ContainsKey(hash))
                    {
                        MessageBox.Show(String.Format(Utility.Language["ScoreHashCollision"], hashs[hash], difficulty));
                        found = true;
                    }
                    else
                    {
                        hashs.Add(hash, difficulty);
                    }
                }
            }
            return !found;
        }

        private bool CheckNearTime()
        {
            bool found = false;
            var checkTimeInterval = 1 / 60f;
            foreach (AvailableDifficulty difficulty in EditorForm.DifficultyArray)
            {
                if (difficulty == AvailableDifficulty.None) continue;
                if ((PublishDifficulty & difficulty) == difficulty)
                {

                    var filePath = Path.Combine(WindowUtility.MainForm.CurrentProjectDir, difficulty + ".ppd");
                    using (PackReader reader = new PackReader(filePath))
                    {
                        if (reader.FileList.Contains("ppd"))
                        {
                            var r = reader.Read("ppd");
                            var data = PPDReader.Read(r);
                            var timeDatas = new Dictionary<ButtonType, SortedSet<float>>();
                            foreach (var d in data)
                            {
                                if (!timeDatas.TryGetValue(d.ButtonType, out SortedSet<float> set))
                                {
                                    set = new SortedSet<float>();
                                    timeDatas.Add(d.ButtonType, set);
                                }
                                foreach (var setVal in set)
                                {
                                    if (Math.Abs(setVal - d.Time) <= checkTimeInterval)
                                    {
                                        MessageBox.Show(Utility.Language["NearTimeWarning"] + "\n" +
                                            Utility.Language["Difficulty"] + ":" + difficulty + "\n" +
                                            Utility.Language["Time"] + ":" + d.Time + "\n" +
                                            Utility.Language["ButtonType"] + ":" + Utility.Language[d.ButtonType.ToString()] + "\n");
                                        found = true;
                                        break;
                                    }
                                }
                                set.Add(d.Time);
                            }
                        }
                    }
                }
            }
            return !found;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (!CheckSameScore() || !CheckNearTime())
            {
                return;
            }
            DialogResult = DialogResult.OK;
            this.Close();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            this.Close();
        }

        private void button3_Click(object sender, EventArgs e)
        {
            folderBrowserDialog1.SelectedPath = this.textBox1.Text;
            if (folderBrowserDialog1.ShowDialog() == DialogResult.OK)
            {
                this.textBox1.Text = folderBrowserDialog1.SelectedPath;
            }
        }
    }
}
