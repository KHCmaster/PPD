using PPDEditor.Forms;
using PPDEditorCommon;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using System.Xml.Linq;

namespace PPDEditor
{
    public partial class InfoForm : ScrollableForm
    {
        const string PresetDir = "preset_parameters";

        Dictionary<string, string> copiedParameters;

        public DateTime PresetsLoadTime
        {
            get;
            private set;
        }

        public ParameterPreset[] Presets
        {
            get
            {
                return 読み込みToolStripMenuItem.GetDescendants().OfType<CommandToolStripMenuItem>().Select(c => c.Preset).ToArray();
            }
        }

        public ToolStripItemCollection PresetsMenuList
        {
            get { return 読み込みToolStripMenuItem.DropDownItems; }
        }

        public InfoForm()
        {
            InitializeComponent();
            this.dataGridView1.Rows.Add(new object[] { "幅(px)", "" });
            this.dataGridView1.Rows.Add(new object[] { "高さ(px)", "" });
            this.dataGridView1.Rows.Add(new object[] { "長さ", "" });
            this.dataGridView1.Rows.Add(new object[] { "位置", "" });
            this.dataGridView1.Rows.Add(new object[] { "回転角", "" });
            this.dataGridView1.Rows.Add(new object[] { "ID", "" });
            if (!Directory.Exists(PresetDir))
            {
                Directory.CreateDirectory(PresetDir);
            }
            LoadPresets();
        }
        public void SetLang()
        {
            this.dataGridView1[0, 0].Value = Utility.Language["Width"];
            this.dataGridView1[0, 1].Value = Utility.Language["Height"];
            this.dataGridView1[0, 2].Value = Utility.Language["Length"];
            this.dataGridView1[0, 3].Value = Utility.Language["Position"];
            this.dataGridView1[0, 4].Value = Utility.Language["Angle"];
            dataGridViewTextBoxColumn1.HeaderText = Utility.Language["Key"];
            dataGridViewTextBoxColumn2.HeaderText = Utility.Language["Value"];
            編集ToolStripMenuItem.Text = Utility.Language["Edit"];
            パラメーターToolStripMenuItem.Text = Utility.Language["Parameter"];
            プリセットToolStripMenuItem.Text = Utility.Language["Preset"];
            読み込みToolStripMenuItem.Text = Utility.Language["Load"];
            保存ToolStripMenuItem.Text = Utility.Language["Save"];
            コピーToolStripMenuItem.Text = Utility.Language["Copy"];
            貼り付けToolStripMenuItem.Text = Utility.Language["Paste"];
        }
        public void SetSkin()
        {
        }
        public void SetMovieInfo(string width, string height, string length)
        {
            this.dataGridView1[1, 0].Value = width;
            this.dataGridView1[1, 1].Value = height;
            this.dataGridView1[1, 2].Value = length;
        }
        public void SetMarkInfo(string pos, string angle, string id)
        {
            this.dataGridView1[1, 3].Value = pos;
            this.dataGridView1[1, 4].Value = angle;
            this.dataGridView1[1, 5].Value = id;
        }

        private void UpdateParameters()
        {
            var marks = Utility.GetSelectedMarks();
            UpdateParameters(marks);
        }

        public void UpdateParameters(Mark[] marks)
        {
            dataGridView2.RowCount = 0;
            if (marks.Length == 0)
            {
                return;
            }
            var parameters = Utility.GetCommonParameters(marks);
            foreach (var pair in parameters)
            {
                dataGridView2.Rows.Add(pair.Key, pair.Value);
            }
        }

        private void パラメーターToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var sheet = WindowUtility.LayerManager.SelectedPpdSheet;
            if (sheet == null)
            {
                return;
            }
            var marks = Utility.GetSelectedMarks();
            if (marks.Length == 0)
            {
                MessageBox.Show(Utility.Language["NoSelectedMarks"]);
                return;
            }

            var commanParameters = Utility.GetCommonParameters(marks, out string[] existKeys);
            var form = new EditParameterForm();
            form.SetLang();
            form.Parameters = commanParameters;
            form.ExistKeys = existKeys;
            if (form.ShowDialog() == DialogResult.OK)
            {
                var newParameters = form.Parameters;
                var removeParameters = new HashSet<string>();
                foreach (var key in commanParameters.Keys)
                {
                    if (!newParameters.ContainsKey(key))
                    {
                        removeParameters.Add(key);
                    }
                }
                sheet.StartGroupCommand();
                foreach (var key in removeParameters)
                {
                    sheet.RemoveParameter(key);
                }
                foreach (var pair in newParameters)
                {
                    sheet.ChangeParameter(pair.Key, pair.Value);
                }
                sheet.EndGroupCommand();
                UpdateParameters();
            }
        }

        private void LoadPresets()
        {
            読み込みToolStripMenuItem.DropDownItems.Clear();
            LoadPresets(PresetDir, 読み込みToolStripMenuItem);
            PresetsLoadTime = DateTime.Now;
        }

        private void LoadPresets(string dir, ToolStripMenuItem menuItem)
        {
            foreach (var childDir in Directory.GetDirectories(dir))
            {
                var childMenu = new ToolStripMenuItem();
                menuItem.DropDownItems.Add(childMenu);
                childMenu.Text = childMenu.ToolTipText = Path.GetFileName(childDir);
                LoadPresets(childDir, childMenu);
            }

            foreach (var childFile in Directory.GetFiles(dir))
            {
                try
                {
                    var name = Path.Combine(Path.GetDirectoryName(childFile), Path.GetFileNameWithoutExtension(childFile)).Substring(dir.Length).TrimStart(Path.DirectorySeparatorChar);
                    var preset = new ParameterPreset(childFile, name);
                    var childMenu = new CommandToolStripMenuItem(preset);
                    menuItem.DropDownItems.Add(childMenu);
                    childMenu.Text = childMenu.ToolTipText = Path.GetFileNameWithoutExtension(childFile);
                    childMenu.Click += childMenu_Click;
                }
                catch
                {
                    MessageBox.Show(Utility.Language["ErrorInLoadingParameterPreset"]);
                }
            }
        }

        void childMenu_Click(object sender, EventArgs e)
        {
            var menuItem = (CommandToolStripMenuItem)sender;
            if (menuItem != null)
            {
                if (menuItem.CheckState == CheckState.Checked)
                {
                    WindowUtility.InfoForm.UnapplyParameters(menuItem.Preset.Parameters);
                }
                else
                {
                    WindowUtility.InfoForm.ApplyParameters(menuItem.Preset.Parameters);
                }
            }
        }

        public void ApplyParameters(KeyValuePair<string, string>[] parameters)
        {
            var marks = Utility.GetSelectedMarks();
            if (marks.Length == 0)
            {
                MessageBox.Show(Utility.Language["NoSelectedMarks"]);
                return;
            }

            var sheet = WindowUtility.LayerManager.SelectedPpdSheet;
            sheet.StartGroupCommand();
            foreach (var pair in parameters)
            {
                sheet.ChangeParameter(pair.Key, pair.Value);
            }
            sheet.EndGroupCommand();
            UpdateParameters();
        }

        public void UnapplyParameters(KeyValuePair<string, string>[] parameters)
        {
            var marks = Utility.GetSelectedMarks();
            if (marks.Length == 0)
            {
                MessageBox.Show(Utility.Language["NoSelectedMarks"]);
                return;
            }

            var sheet = WindowUtility.LayerManager.SelectedPpdSheet;
            sheet.StartGroupCommand();
            foreach (var mark in marks)
            {
                foreach (var parameter in parameters)
                {
                    sheet.RemoveParameter(mark, parameter.Key);
                }
            }
            sheet.EndGroupCommand();
            UpdateParameters();
        }

        private void 保存ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (dataGridView2.RowCount == 0)
            {
                MessageBox.Show(Utility.Language["NoParameters"]);
                return;
            }

            saveFileDialog1.InitialDirectory = Path.GetFullPath(PresetDir);
            saveFileDialog1.RestoreDirectory = true;
            saveFileDialog1.FileName = "NewPresetParameter.xml";
            if (saveFileDialog1.ShowDialog() == DialogResult.OK)
            {
                var document = new XDocument(new XElement("Root"));
                for (int i = 0; i < dataGridView2.RowCount; i++)
                {
                    var elem = new XElement("Parameter", new XAttribute("Key", dataGridView2[0, i].Value), new XAttribute("Value", dataGridView2[1, i].Value));
                    document.Root.Add(elem);
                }
                document.Save(saveFileDialog1.FileName);
                LoadPresets();
            }
        }

        private void コピーToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var marks = Utility.GetSelectedMarks();
            if (marks.Length == 0)
            {
                return;
            }

            copiedParameters = Utility.GetCommonParameters(marks);
            if (copiedParameters.Count == 0)
            {
                copiedParameters = null;
            }
        }

        private void 貼り付けToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (copiedParameters == null)
            {
                return;
            }

            var marks = Utility.GetSelectedMarks();
            if (marks.Length == 0)
            {
                return;
            }

            var sheet = WindowUtility.LayerManager.SelectedPpdSheet;
            sheet.StartGroupCommand();
            foreach (var pair in copiedParameters)
            {
                sheet.ChangeParameter(pair.Key, pair.Value);
            }
            sheet.EndGroupCommand();
            UpdateParameters();
        }

        private void 編集ToolStripMenuItem_DropDownOpening(object sender, EventArgs e)
        {
            var marks = Utility.GetSelectedMarks();
            コピーToolStripMenuItem.Enabled = marks.Length > 0;
            貼り付けToolStripMenuItem.Enabled = copiedParameters != null && marks.Length > 0;
        }

        private void プリセットToolStripMenuItem_DropDownOpening(object sender, EventArgs e)
        {
            Utility.ChangeToolStripCheckState(読み込みToolStripMenuItem);
        }

        public class CommandToolStripMenuItem : ToolStripMenuItem
        {
            public ParameterPreset Preset
            {
                get;
                private set;
            }

            public CommandToolStripMenuItem(ParameterPreset preset)
            {
                Preset = preset;
            }

            public CommandToolStripMenuItem Clone()
            {
                var ret = new CommandToolStripMenuItem(Preset);
                ret.Text = ret.ToolTipText = Path.GetFileNameWithoutExtension(Preset.FilePath);
                return ret;
            }
        }
    }
}
