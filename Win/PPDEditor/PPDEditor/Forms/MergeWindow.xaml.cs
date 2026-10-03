using PPDEditor.Controls;
using Microsoft.Win32;
using System;
using System.IO;
using System.Windows;

namespace PPDEditor.Forms
{
    public partial class MergeWindow : Window
    {
        string openprojectfilter = "PPDProjectファイル(*.ppdproj)|*.ppdproj|すべてのファイル(*.*)|*.*";
        public MergeWindow()
        {
            InitializeComponent();
        }

        public void SetLang()
        {
            this.Title = Utility.Language["MergeProject"];
            this.label1.Text = Utility.Language["MFLabel1"];
            this.label2.Text = Utility.Language["MFLabel2"];
            this.label3.Text = Utility.Language["MFLabel3"];
            this.mergeGroupBox1.Label = this.mergeGroupBox2.Label = this.mergeGroupBox3.Label = this.mergeGroupBox4.Label = this.mergeGroupBox5.Label = Utility.Language["MFMergeLabel"];
            openprojectfilter = Utility.Language["ProjectFilter"];
        }

        void MergeBox_ButtonPressed(object sender, EventArgs e)
        {
            var openFileDialog1 = new OpenFileDialog
            {
                Filter = openprojectfilter
            };
            if (openFileDialog1.ShowDialog(this) == true)
            {
                string projectPath = openFileDialog1.FileName;
                var version = EditorForm.GetProjectVersion(projectPath);
                var mgb = sender as MergeGroupBox;
                mgb.ProjectPath = projectPath;
                mgb.ComboBox.Items.Clear();
                mgb.ProjectVersion = version;
                switch (version)
                {
                    case 1:
                        mgb.ComboBox.Items.Add(AvailableDifficulty.Base.ToString());
                        break;
                    default:
                        var ad = EditorForm.GetDifficultyAvailable(Path.Combine(Path.GetDirectoryName(projectPath), Path.GetFileNameWithoutExtension(projectPath)));
                        foreach (AvailableDifficulty difficulty in EditorForm.DifficultyArray)
                        {
                            if (difficulty != AvailableDifficulty.None && (ad & difficulty) == difficulty)
                            {
                                mgb.ComboBox.Items.Add(difficulty);
                            }
                        }
                        break;
                }
                if (mgb.ComboBox.Items.Count > 0)
                {
                    mgb.ComboBox.SelectedIndex = 0;
                }
                else
                {
                    MessageBox.Show(this, Utility.Language["NoAvailableDifficulty"]);
                }
            }
            else
            {
                var mgb = sender as MergeGroupBox;
                mgb.ProjectPath = "";
                mgb.ComboBox.Items.Clear();
            }
            CheckEnable();
        }

        private void CheckEnable()
        {
            AvailableDifficulty mergeDifficulty = MergeDifficulty;
            button1.IsEnabled = mergeDifficulty != AvailableDifficulty.None && this.textBox1.Text != string.Empty;
            comboBox1.Items.Clear();
            foreach (AvailableDifficulty difficulty in EditorForm.DifficultyArray)
            {
                if (difficulty != AvailableDifficulty.None && (mergeDifficulty & difficulty) == difficulty)
                {
                    comboBox1.Items.Add(difficulty.ToString());
                }
            }
            if (comboBox1.Items.Count > 0 && comboBox1.SelectedIndex < 0)
            {
                comboBox1.SelectedIndex = 0;
            }
        }

        public AvailableDifficulty BaseInfoProjectDifficulty
        {
            get
            {
                return EditorForm.GetDifficultyFromString(comboBox1.SelectedItem.ToString());
            }
        }

        private void Button1_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = true;
        }

        private void Button3_Click(object sender, RoutedEventArgs e)
        {
            var saveFileDialog1 = new SaveFileDialog
            {
                Filter = openprojectfilter
            };
            if (saveFileDialog1.ShowDialog(this) == true)
            {
                this.textBox1.Text = saveFileDialog1.FileName;
            }
            else
            {
                this.textBox1.Text = string.Empty;
            }
            CheckEnable();
        }

        public AvailableDifficulty MergeDifficulty
        {
            get
            {
                AvailableDifficulty ret = AvailableDifficulty.None;
                ret |= File.Exists(mergeGroupBox1.ProjectPath) ? AvailableDifficulty.Easy : AvailableDifficulty.None;
                ret |= File.Exists(mergeGroupBox2.ProjectPath) ? AvailableDifficulty.Normal : AvailableDifficulty.None;
                ret |= File.Exists(mergeGroupBox3.ProjectPath) ? AvailableDifficulty.Hard : AvailableDifficulty.None;
                ret |= File.Exists(mergeGroupBox4.ProjectPath) ? AvailableDifficulty.Extreme : AvailableDifficulty.None;
                ret |= File.Exists(mergeGroupBox5.ProjectPath) ? AvailableDifficulty.Base : AvailableDifficulty.None;
                return ret;
            }
        }

        public MergeInfo GetMergeInfo(AvailableDifficulty difficulty)
        {
            string projectPath = "";
            AvailableDifficulty d = AvailableDifficulty.None;
            int version = 0;
            switch (difficulty)
            {
                case AvailableDifficulty.Easy:
                    projectPath = mergeGroupBox1.ProjectPath;
                    d = EditorForm.GetDifficultyFromString(mergeGroupBox1.ComboBox.SelectedItem.ToString());
                    version = mergeGroupBox1.ProjectVersion;
                    break;
                case AvailableDifficulty.Normal:
                    projectPath = mergeGroupBox2.ProjectPath;
                    d = EditorForm.GetDifficultyFromString(mergeGroupBox2.ComboBox.SelectedItem.ToString());
                    version = mergeGroupBox2.ProjectVersion;
                    break;
                case AvailableDifficulty.Hard:
                    projectPath = mergeGroupBox3.ProjectPath;
                    d = EditorForm.GetDifficultyFromString(mergeGroupBox3.ComboBox.SelectedItem.ToString());
                    version = mergeGroupBox3.ProjectVersion;
                    break;
                case AvailableDifficulty.Extreme:
                    projectPath = mergeGroupBox4.ProjectPath;
                    d = EditorForm.GetDifficultyFromString(mergeGroupBox4.ComboBox.SelectedItem.ToString());
                    version = mergeGroupBox4.ProjectVersion;
                    break;
                case AvailableDifficulty.Base:
                    projectPath = mergeGroupBox5.ProjectPath;
                    d = EditorForm.GetDifficultyFromString(mergeGroupBox5.ComboBox.SelectedItem.ToString());
                    version = mergeGroupBox5.ProjectVersion;
                    break;
                case AvailableDifficulty.None:
                    return null;
            }
            return new MergeInfo(projectPath, version, d);
        }

        public string DestProjectFilePath
        {
            get
            {
                return this.textBox1.Text;
            }
        }
    }
}
