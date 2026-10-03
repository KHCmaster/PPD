using PPDFramework;
using PPDFramework.PPDStructure;
using PPDFrameworkCore;
using PPDPack;
using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Win32;
using System.Security.Cryptography;
using System.Windows;
using System.Windows.Controls;

namespace PPDEditor.Forms
{
    public partial class PublishWindow : Window
    {
        public PublishWindow()
        {
            InitializeComponent();
            if (Directory.Exists("publish"))
            {
                Directory.CreateDirectory("publish");
            }
            this.textBox1.Text = Path.GetFullPath("publish");
        }

        public void SetLang()
        {
            this.Title = Utility.Language["Publish"];
            this.label1.Text = Utility.Language["PFLabel1"];
            this.label2.Text = Utility.Language["PFLabel2"];
            this.label3.Text = Utility.Language["PFLabel3"];
            this.groupBox1.Header = Utility.Language["PFGroupBox1"];
        }

        void CheckBox_Click(object sender, RoutedEventArgs e)
        {
            CheckOKButtonEnable();
        }

        private void CheckOKButtonEnable()
        {
            // XAML の初期化中に TextChanged が来るときは、まだボタンがない
            if (button1 == null)
            {
                return;
            }
            button1.IsEnabled = (IsChecked(checkBox1) || IsChecked(checkBox2) || IsChecked(checkBox3) || IsChecked(checkBox4)) && textBox2.Text != "";
        }

        private static bool IsChecked(CheckBox checkBox)
        {
            return checkBox.IsChecked == true;
        }

        private void TextBox2_TextChanged(object sender, TextChangedEventArgs e)
        {
            CheckOKButtonEnable();
        }

        public AvailableDifficulty AvailableDifficulty
        {
            set
            {
                checkBox1.IsChecked = checkBox1.IsEnabled = (value & AvailableDifficulty.Easy) == AvailableDifficulty.Easy;
                checkBox2.IsChecked = checkBox2.IsEnabled = (value & AvailableDifficulty.Normal) == AvailableDifficulty.Normal;
                checkBox3.IsChecked = checkBox3.IsEnabled = (value & AvailableDifficulty.Hard) == AvailableDifficulty.Hard;
                checkBox4.IsChecked = checkBox4.IsEnabled = (value & AvailableDifficulty.Extreme) == AvailableDifficulty.Extreme;
                CheckOKButtonEnable();
            }
        }

        public AvailableDifficulty PublishDifficulty
        {
            get
            {
                AvailableDifficulty ret = AvailableDifficulty.None;
                ret |= IsChecked(checkBox1) ? AvailableDifficulty.Easy : AvailableDifficulty.None;
                ret |= IsChecked(checkBox2) ? AvailableDifficulty.Normal : AvailableDifficulty.None;
                ret |= IsChecked(checkBox3) ? AvailableDifficulty.Hard : AvailableDifficulty.None;
                ret |= IsChecked(checkBox4) ? AvailableDifficulty.Extreme : AvailableDifficulty.None;
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
                MessageBox.Show(this, String.Format(Utility.Language["ContainsInvalidChars"], Utility.Language["PFLabel3"]));
                return false;
            }
            return true;
        }

        private bool CheckSameScore()
        {
            var hashs = new Dictionary<string, PPDEditor.AvailableDifficulty>();
            bool found = false;
            foreach (AvailableDifficulty difficulty in EditorForm.DifficultyArray)
            {
                if (difficulty == AvailableDifficulty.None) continue;
                if ((PublishDifficulty & difficulty) == difficulty)
                {
                    var filePath = Path.Combine(WindowUtility.MainForm.CurrentProjectDir, difficulty + ".ppd");
                    var hash = CryptographyUtility.Getx2Encoding(SHA256.HashData(File.ReadAllBytes(filePath)));
                    if (hashs.TryGetValue(hash, out AvailableDifficulty value))
                    {
                        MessageBox.Show(this, String.Format(Utility.Language["ScoreHashCollision"], value, difficulty));
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
                    using PackReader reader = new(filePath);
                    if (reader.FileList.Contains("ppd"))
                    {
                        var r = reader.Read("ppd");
                        var data = PPDReader.Read(r);
                        var timeDatas = new Dictionary<ButtonType, SortedSet<float>>();
                        foreach (var d in data)
                        {
                            if (!timeDatas.TryGetValue(d.ButtonType, out SortedSet<float> set))
                            {
                                set = [];
                                timeDatas.Add(d.ButtonType, set);
                            }
                            foreach (var setVal in set)
                            {
                                if (Math.Abs(setVal - d.Time) <= checkTimeInterval)
                                {
                                    MessageBox.Show(this, Utility.Language["NearTimeWarning"] + "\n" +
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
            return !found;
        }

        private void Button1_Click(object sender, RoutedEventArgs e)
        {
            if (!CheckSameScore() || !CheckNearTime())
            {
                return;
            }
            DialogResult = true;
        }

        private void Button3_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new OpenFolderDialog
            {
                InitialDirectory = this.textBox1.Text
            };
            if (dialog.ShowDialog(this) == true)
            {
                this.textBox1.Text = dialog.FolderName;
            }
        }
    }
}
