using PPDConfiguration;
using PPDFramework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using System.Xml;

namespace KeyConfiger
{
    public partial class Form1 : GameForm
    {
        const string iniFileName = "KeyConfiger.ini";
        const string keyConfigFileName = "keyconfig.ini";
        bool waitinginput;
        List<KeyConfig> keyConfigs = new List<KeyConfig>();
        int currentConfigIndex;

        MyGame mygame;

        private string langFileISO = "";

        string deletionError = "default の設定を削除することはできません";
        string confirmDeletion = "現在の設定を削除してもよろしいですか？(元には戻せません)";
        string confirmDeletionCaption = "削除確認";

        public KeyConfig CurrentConfig
        {
            get
            {
                return keyConfigs[currentConfigIndex];
            }
        }

        public Form1()
        {
            InitializeComponent();
            for (int i = 0; i < ButtonUtility.Array.Length; i++)
            {
                this.dataGridView1.Rows.Add();
            }
            for (int i = 0; i < dataGridView1.RowCount; i++)
            {
                this.dataGridView1.Rows[i].Cells[0].Value = "0";
                this.dataGridView1.Rows[i].Cells[1].Value = "0";
            }
            ReadKeySetting();
            if (File.Exists(keyConfigFileName))
            {
                CheckSetting();
            }
            CheckLangFiles();
            SetLanguage(langFileISO);
        }

        private void ReadKeySetting()
        {
            if (File.Exists(keyConfigFileName))
            {
                try
                {
                    ReadKeySettingFromXml();
                }
                catch
                {
                    ReadKeySettingFromIni();
                }
            }
            else
            {
                AddKeyConfig(new KeyConfig());
            }
            ApplyConfigToDataGrid(keyConfigs[0]);
            (this.設定ToolStripMenuItem.DropDownItems[3] as ToolStripMenuItem).Checked = true;
        }

        private void ReadKeySettingFromXml()
        {
            var reader = XmlReader.Create(keyConfigFileName, new XmlReaderSettings());
            while (reader.Read())
            {
                if (reader.IsStartElement())
                {
                    switch (reader.LocalName)
                    {
                        case "KeyConfigs":
                            keyConfigs.Clear();
                            break;
                        case "KeyConfig":
                            var keyConfig = new KeyConfig
                            {
                                Name = reader.GetAttribute("Name")
                            };
                            AddKeyConfig(keyConfig);
                            ReadKeySetting(reader.ReadSubtree(), keyConfig);
                            break;
                    }
                }
            }
            reader.Close();

            if (keyConfigs.Count == 0)
            {
                AddKeyConfig(new KeyConfig());
            }
        }

        private void AddKeyConfig(KeyConfig keyConfig)
        {
            keyConfigs.Add(keyConfig);
            var tsmi = new ToolStripMenuItem(keyConfig.Name);
            tsmi.Text = tsmi.ToolTipText = keyConfig.Name;
            tsmi.Tag = keyConfig;
            tsmi.Click += tsmi_Click1;
            this.設定ToolStripMenuItem.DropDownItems.Add(tsmi);
        }

        private void ReadKeySetting(XmlReader reader, KeyConfig keyConfig)
        {
            while (reader.Read())
            {
                if (reader.IsStartElement())
                {
                    switch (reader.LocalName)
                    {
                        case "Config":
                            int key = int.Parse(reader.GetAttribute("Key")), button = int.Parse(reader.GetAttribute("Button")),
                                index = FindIndex(reader.GetAttribute("Type"));
                            if (index >= 0)
                            {
                                keyConfig.SetKeyMap(ButtonUtility.Array[index], key);
                                keyConfig.SetButtonMap(ButtonUtility.Array[index], button);
                            }
                            break;
                    }
                }
            }
            reader.Close();
        }

        private int FindIndex(string value)
        {
            for (int i = 0; i < ButtonUtility.Array.Length; i++)
            {
                if (ButtonUtility.Array[i].ToString() == value)
                {
                    return i;
                }
            }
            return -1;
        }

        private void ReadKeySettingFromIni()
        {
            var keyConfig = new KeyConfig();
            var sr = new StreamReader(keyConfigFileName);
            var s = sr.ReadToEnd().Replace("\r\n", "\n").Replace("\r", "\n");
            sr.Close();
            var sp = s.Split('\n');
            for (int i = 0; i < Math.Min(sp.Length, ButtonUtility.Array.Length); i++)
            {
                var secondsp = sp[i].Split(':');
                if (secondsp.Length >= 2)
                {
                    keyConfig.SetKeyMap(ButtonUtility.Array[i], int.Parse(secondsp[0]));
                    keyConfig.SetButtonMap(ButtonUtility.Array[i], int.Parse(secondsp[1]));
                }
            }
            AddKeyConfig(keyConfig);
        }

        private void ApplyConfigToDataGrid(KeyConfig config)
        {
            for (int i = 0; i < ButtonUtility.Array.Length; i++)
            {
                this.dataGridView1.Rows[i].Cells[0].Value = config.GetKeyMap(ButtonUtility.Array[i]).ToString();
                this.dataGridView1.Rows[i].Cells[1].Value = config.GetButtonMap(ButtonUtility.Array[i]).ToString();
            }
        }

        private void CheckSetting()
        {
            if (File.Exists(iniFileName))
            {
                var sr = new StreamReader(iniFileName);
                var setting = new SettingReader(sr.ReadToEnd());
                sr.Close();
                langFileISO = setting["Language"];
            }
        }

        private void CheckLangFiles()
        {
            if (Directory.Exists("Lang"))
            {
                var componentName = this.GetType().Assembly.GetName().Name;
                foreach (string fileName in Directory.GetFiles("Lang", String.Format("lang_{0}_*.ini", componentName)))
                {
                    if (!Regex.IsMatch(Path.GetFileName(fileName), String.Format("lang_{0}_[0-9a-zA-Z]+\\.ini", componentName)))
                    {
                        continue;
                    }
                    var sr = new StreamReader(fileName);
                    var setting = new SettingReader(sr.ReadToEnd());
                    sr.ReadToEnd();
                    string name = setting["DisplayName"];
                    var tsmi = new ToolStripMenuItem
                    {
                        Text = name,
                        Name = fileName.ToLower(),
                        Checked = Path.GetFileName(fileName).ToLower() == String.Format("lang_{0}_{1}.ini", this.GetType().Assembly.GetName().Name, langFileISO).ToLower()
                    };
                    tsmi.Click += tsmi_Click;
                    言語ToolStripMenuItem.DropDownItems.Add(tsmi);
                }
            }
        }

        void tsmi_Click(object sender, EventArgs e)
        {
            if (sender is ToolStripMenuItem tsmi)
            {
                var langFileName = tsmi.Name;
                var m = Regex.Match(Path.GetFileName(langFileName), "^lang_\\w+_(?<ISO>\\w+).ini$");
                if (m.Success)
                {
                    langFileISO = m.Groups["ISO"].Value;
                }
                SetLanguage(langFileISO);
                foreach (ToolStripMenuItem child in (tsmi.OwnerItem as ToolStripMenuItem).DropDownItems)
                {
                    child.Checked = false;
                }
                tsmi.Checked = true;
            }
        }


        private void SetLanguage(string langIso)
        {
            Utility.ChangeLanguage(langIso);
            言語ToolStripMenuItem.Text = Utility.Language["Language"];
            dataGridView1.Rows[0].HeaderCell.Value = Utility.Language["Square"];
            dataGridView1.Rows[1].HeaderCell.Value = Utility.Language["Cross"];
            dataGridView1.Rows[2].HeaderCell.Value = Utility.Language["Circle"];
            dataGridView1.Rows[3].HeaderCell.Value = Utility.Language["Triangle"];
            dataGridView1.Rows[4].HeaderCell.Value = Utility.Language["Left"];
            dataGridView1.Rows[5].HeaderCell.Value = Utility.Language["Down"];
            dataGridView1.Rows[6].HeaderCell.Value = Utility.Language["Right"];
            dataGridView1.Rows[7].HeaderCell.Value = Utility.Language["Up"];
            dataGridView1.Rows[8].HeaderCell.Value = Utility.Language["R"];
            dataGridView1.Rows[9].HeaderCell.Value = Utility.Language["L"];
            dataGridView1.Rows[10].HeaderCell.Value = Utility.Language["Start"];
            dataGridView1.Rows[11].HeaderCell.Value = Utility.Language["Home"];
            dataGridView1.Columns[0].HeaderText = Utility.Language["Keyboard"];
            dataGridView1.Columns[1].HeaderText = Utility.Language["Gamepad"];
            button1.Text = Utility.Language["Button1"];
            label1.Text = Utility.Language["Label1"];
            設定ToolStripMenuItem.Text = Utility.Language["Setting"];
            新しい設定を作成ToolStripMenuItem.Text = Utility.Language["CreateNewSetting"];
            現在の設定を削除ToolStripMenuItem.Text = Utility.Language["DeleteCurrentSetting"];
            コントローラーToolStripMenuItem.Text = Utility.Language["Gamepad"];
            コントローラーを再取得ToolStripMenuItem.Text = Utility.Language["ResetControllerList"];
            deletionError = Utility.Language["DeletionError"];
            confirmDeletion = Utility.Language["ConfirmDeletion"];
            confirmDeletionCaption = Utility.Language["ConfirmDeletionCaption"];
        }
        public MyGame MyGame
        {
            set
            {
                this.mygame = value;
                ReloadController();
            }
        }
        public void SetNumber(int num, bool keybord)
        {
            if (waitinginput && this.dataGridView1.CurrentRow != null)
            {
                int index = this.dataGridView1.CurrentRow.Index;
                if (keybord)
                {
                    this.dataGridView1.CurrentRow.Cells[0].Value = num.ToString();
                    CurrentConfig.SetKeyMap(ButtonUtility.Array[index], num);
                }
                else
                {
                    this.dataGridView1.CurrentRow.Cells[1].Value = num.ToString();
                    CurrentConfig.SetButtonMap(ButtonUtility.Array[index], num);
                }
                waitinginput = false;
                this.label1.Visible = false;
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            WaitForInput();
        }

        private void WaitForInput()
        {
            waitinginput = true;
            this.label1.Visible = true;
        }

        private void dataGridView1_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            WaitForInput();
        }

        private void dataGridView1_KeyDown(object sender, KeyEventArgs e)
        {
            e.Handled |= waitinginput;
        }

        private void button2_Click(object sender, EventArgs e)
        {
            var writer = XmlWriter.Create(keyConfigFileName, new XmlWriterSettings
            {
                Indent = true,
                IndentChars = "   ",
                NewLineChars = System.Environment.NewLine
            });

            writer.WriteStartElement("root");
            writer.WriteStartElement("KeyConfigs");
            foreach (KeyConfig keyConfig in keyConfigs)
            {
                writer.WriteStartElement("KeyConfig");
                writer.WriteAttributeString("Name", keyConfig.Name);
                WriteSetting(writer, keyConfig);
                writer.WriteEndElement();
            }
            writer.WriteEndElement();
            writer.WriteEndElement();

            writer.Close();
            SaveSetting();
            this.Close();
        }

        private void WriteSetting(XmlWriter writer, KeyConfig keyConfig)
        {
            foreach (ButtonType buttonType in ButtonUtility.Array)
            {
                writer.WriteStartElement("Config");
                writer.WriteAttributeString("Type", buttonType.ToString());
                writer.WriteAttributeString("Key", keyConfig.GetKeyMap(buttonType).ToString());
                writer.WriteAttributeString("Button", keyConfig.GetButtonMap(buttonType).ToString());
                writer.WriteEndElement();
            }
        }

        private void SaveSetting()
        {
            var sr = new StreamReader(iniFileName);
            var setting = new SettingReader(sr.ReadToEnd());
            sr.Close();
            setting.ReplaceOrAdd("Language", langFileISO);
            using (SettingWriter sw = new SettingWriter(iniFileName, false))
            {
                foreach (KeyValuePair<string, string> kvp in setting.Dictionary)
                {
                    sw.Write(kvp.Key, kvp.Value);
                }
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void 新しい設定を作成ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var ncw = new NewConfigWindow();
            ncw.SetLang();
            if (ncw.ShowDialog() == DialogResult.OK)
            {
                var keyConfig = CurrentConfig.Clone();
                keyConfig.Name = ncw.SettingName;
                AddKeyConfig(keyConfig);
                currentConfigIndex = keyConfigs.Count - 1;
                UncheckAllMenuItem(設定ToolStripMenuItem, 3);
                (this.設定ToolStripMenuItem.DropDownItems[3 + currentConfigIndex] as ToolStripMenuItem).Checked = true;
                ApplyConfigToDataGrid(CurrentConfig);
            }
        }

        void tsmi_Click1(object sender, EventArgs e)
        {
            if (sender is ToolStripMenuItem tsmi && tsmi.Tag is KeyConfig)
            {
                var keyConfig = tsmi.Tag as KeyConfig;
                var index = keyConfigs.IndexOf(keyConfig);
                if (index >= 0)
                {
                    currentConfigIndex = index;
                    ApplyConfigToDataGrid(CurrentConfig);
                }
                UncheckAllMenuItem(設定ToolStripMenuItem, 3);
                tsmi.Checked = true;
            }
        }

        private void UncheckAllMenuItem(ToolStripMenuItem tsmi, int startIndex)
        {
            for (int i = startIndex; i < tsmi.DropDownItems.Count; i++)
            {
                if (tsmi.DropDownItems[i] is ToolStripMenuItem temp)
                {
                    temp.Checked = false;
                }
            }
        }

        private void 現在の設定を削除ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (currentConfigIndex == 0)
            {
                MessageBox.Show(deletionError);
                return;
            }

            if (MessageBox.Show(confirmDeletion, confirmDeletionCaption, MessageBoxButtons.OKCancel) == DialogResult.OK)
            {
                keyConfigs.RemoveAt(currentConfigIndex);
                this.設定ToolStripMenuItem.DropDownItems.RemoveAt(3 + currentConfigIndex);
                if (currentConfigIndex >= keyConfigs.Count)
                {
                    currentConfigIndex--;
                }
                ApplyConfigToDataGrid(CurrentConfig);
                UncheckAllMenuItem(設定ToolStripMenuItem, 3);
                (this.設定ToolStripMenuItem.DropDownItems[3 + currentConfigIndex] as ToolStripMenuItem).Checked = true;
            }
        }

        private void コントローラーを再取得ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            mygame.Input.Load();
            while (コントローラーToolStripMenuItem.DropDownItems.Count >= 3)
            {
                コントローラーToolStripMenuItem.DropDownItems.RemoveAt(コントローラーToolStripMenuItem.DropDownItems.Count - 1);
            }
            ReloadController();
        }

        private void ReloadController()
        {
            int iter = 0;
            foreach (string controllerName in mygame.Input.JoyStickNames)
            {
                var tsmi = new ToolStripMenuItem(controllerName);
                tsmi.Text = tsmi.ToolTipText = controllerName;
                tsmi.Tag = iter;
                コントローラーToolStripMenuItem.DropDownItems.Add(tsmi);
                tsmi.Click += tsmi_Click2;
                iter++;
            }
#pragma warning disable RECS0033 // Convert 'if' to '||' expression
            if (コントローラーToolStripMenuItem.DropDownItems.Count >= 3)
#pragma warning restore RECS0033 // Convert 'if' to '||' expression
            {
                (コントローラーToolStripMenuItem.DropDownItems[2] as ToolStripMenuItem).Checked = true;
            }
        }

        void tsmi_Click2(object sender, EventArgs e)
        {
            var tsmi = sender as ToolStripMenuItem;
            UncheckAllMenuItem(tsmi, 2);
            var index = (int)tsmi.Tag;
            mygame.Input.CurrentJoyStickIndex = index;
            tsmi.Checked = true;
        }
    }
}
