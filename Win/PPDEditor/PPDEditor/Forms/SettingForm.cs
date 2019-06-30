using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace PPDEditor.Forms
{
    public partial class SettingForm : Form
    {
        bool ignore;
        ShortcutManager shortcutManager;
        int regularShortcutCount;
        public SettingForm()
        {
            InitializeComponent();
        }

        public void SetLangAngShortcut(ShortcutManager shortcutManager)
        {
            label1.Text = Utility.Language["SFAutherName"];
            label2.Text = Utility.Language["SFMovieLatency"];
            groupBox1.Text = Utility.Language["SFGroupBox1"];
            groupBox2.Text = Utility.Language["SFGroupBox2"];
            checkBox1.Text = Utility.Language["SFCheckBox1"];
            checkBox2.Text = Utility.Language["SFCheckBox2"];
            listBox1.Items.Clear();
            listBox1.Items.Add(Utility.Language["General"]);
            listBox1.Items.Add(Utility.Language["ShortcutKey"]);
            listBox1.Items.Add(Utility.Language["EnvironmentVariable"]);
            dataGridView1.Columns[0].HeaderText = Utility.Language["Command"];
            dataGridView1.Columns[1].HeaderText = Utility.Language["Key"];
            groupBox3.Text = Utility.Language["ToggleOption"];
            moveLabel1.Text = Utility.Language["Move1"];
            moveLabel2.Text = Utility.Language["Move2"];
            moveLabel3.Text = Utility.Language["Move3"];
            moveLabel4.Text = Utility.Language["Move4"];
            moveLabel5.Text = Utility.Language["Move5"];
            moveLabel6.Text = Utility.Language["Move6"];
            moveLabel7.Text = Utility.Language["Move7"];
            moveLabel8.Text = Utility.Language["Move8"];
            label13.Text = Utility.Language["None"];
            button3.Text = Utility.Language["DeleteShortcut"];
            label6.Text = Utility.Language["DragAngleRestriction"];
            checkBox3.Text = Utility.Language["SFCheckBox3"];
            button4.Text = Utility.Language["AddCommand"];
            button5.Text = Utility.Language["RemoveCommand"];


            this.shortcutManager = shortcutManager;
            ShortcutInfo[] shortcuts = shortcutManager.Shortcuts;
            dataGridView1.Rows.Clear();

            regularShortcutCount = 0;
            foreach (ShortcutType type in Enum.GetValues(typeof(ShortcutType)))
            {
                if (type == ShortcutType.None)
                {
                    continue;
                }
                else if (type == ShortcutType.Custom)
                {
                    foreach (var shortcut in shortcuts.Where(s => s.ShortcutType == ShortcutType.Custom))
                    {
                        dataGridView1.Rows.Add(GetCustomShortcutText(shortcut), GetReadableShortcut(shortcut));
                    }
                }
                else
                {
                    var info = shortcuts.FirstOrDefault(i => i.ShortcutType == type);
                    dataGridView1.Rows.Add(Utility.Language[type.ToString()], info == null ? "" : GetReadableShortcut(info));
                    regularShortcutCount++;
                }
            }

            dataGridView1.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
        }

        private string GetCustomShortcutText(ShortcutInfo shortcut)
        {
            return GetCustomShortcutText(shortcut.ScriptPath);
        }

        private string GetCustomShortcutText(string scriptPath)
        {
            return String.Join(":", scriptPath.Split(Path.DirectorySeparatorChar).Skip(1).ToArray());
        }

        private string GetCustomShortcutScriptPath(string str)
        {
            return Path.Combine("Commands", String.Join(Path.DirectorySeparatorChar.ToString(), str.Split(':')));
        }

        public string AuthorName
        {
            get
            {
                return textBox1.Text;
            }
            set
            {
                ignore = true;
                textBox1.Text = value;
                ignore = false;
            }
        }
        public float MovieLatency
        {
            get
            {
                float.TryParse(this.textBox2.Text, out float ret);
                return ret;
            }
            set
            {
                ignore = true;
                this.textBox2.Text = value.ToString();
                ignore = false;
            }
        }

        public int[] Moves
        {
            get
            {
                return new int[]{
                    (int)move1.Value,                    (int)move2.Value,                    (int)move3.Value,                    (int)move4.Value,                    (int)move5.Value,                    (int)move6.Value,                    (int)move7.Value,                    (int)move8.Value                };
            }
            set
            {
                move1.Value = value[0];
                move2.Value = value[1];
                move3.Value = value[2];
                move4.Value = value[3];
                move5.Value = value[4];
                move6.Value = value[5];
                move7.Value = value[6];
                move8.Value = value[7];
            }
        }

        public int[] Angles
        {
            get
            {
                return new int[]{
                    (int)angle1.Value,                    (int)angle2.Value,                    (int)angle3.Value,                    (int)angle4.Value,                    (int)angle5.Value,                    (int)angle6.Value,                    (int)angle7.Value,                    (int)angle8.Value                };
            }
            set
            {
                angle1.Value = value[0];
                angle2.Value = value[1];
                angle3.Value = value[2];
                angle4.Value = value[3];
                angle5.Value = value[4];
                angle6.Value = value[5];
                angle7.Value = value[6];
                angle8.Value = value[7];
            }
        }

        public bool HideToggleArrow
        {
            get
            {
                return !checkBox1.Checked;
            }
            set
            {
                checkBox1.Checked = !value;
            }
        }

        public bool HideToggleRectangle
        {
            get
            {
                return !checkBox2.Checked;
            }
            set
            {
                checkBox2.Checked = !value;
            }
        }

        public bool EnableToChangeMarkTypeAndTime
        {
            get
            {
                return checkBox3.Checked;
            }
            set
            {
                checkBox3.Checked = value;
            }
        }

        public bool ShouldRestart
        {
            get;
            private set;
        }

        private bool CheckSameShortcut()
        {
            for (int i = 0; i < dataGridView1.RowCount; i++)
            {
                for (int j = i + 1; j < dataGridView1.RowCount; j++)
                {
                    string val1 = dataGridView1[1, i].Value.ToString(), val2 = dataGridView1[1, j].Value.ToString();
                    if (!String.IsNullOrEmpty(val1) && !String.IsNullOrEmpty(val2) && val1 == val2)
                    {
                        MessageBox.Show(String.Format("{0}\n{1}\n{2}", Utility.Language["SameShortcutKeyAssigned"],
                            GetRowContent(i), GetRowContent(j)));
                        return true;
                    }
                }
            }

            return false;
        }

        private string GetRowContent(int index)
        {
            return String.Format("{0} {1}", dataGridView1[0, index].Value, dataGridView1[1, index].Value);
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (CheckSameShortcut())
            {
                return;
            }

            shortcutManager.ClearShortcut();
            int iter = 0;
            foreach (ShortcutType type in Enum.GetValues(typeof(ShortcutType)))
            {
                if (type == ShortcutType.None)
                {
                    continue;
                }
                else if (type == ShortcutType.Custom)
                {
                    for (int i = iter; i < dataGridView1.Rows.Count; i++)
                    {
                        var shortcut = GetCustomShortcutInfo(dataGridView1[1, iter].Value.ToString(),
                            GetCustomShortcutScriptPath(dataGridView1[0, iter].Value.ToString()));
                        if (shortcut != null)
                        {
                            shortcutManager.RegisterShortcut(shortcut);
                        }
                        iter++;
                    }
                }
                else
                {
                    var shortcut = GetShortcutInfo(dataGridView1[1, iter].Value.ToString(), type);
                    if (shortcut != null)
                    {
                        shortcutManager.RegisterShortcut(shortcut);
                    }
                    iter++;
                }
            }

            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        private void GetModifiers(string str, out Keys key, out bool ctrl, out bool shift, out bool alt)
        {
            var split = str.Split('+');
            key = Keys.None;
            ctrl = false;
            shift = false;
            alt = false;
            foreach (string sp in split)
            {
                switch (sp)
                {
                    case "Ctrl":
                        ctrl = true;
                        break;
                    case "Shift":
                        shift = true;
                        break;
                    case "Alt":
                        alt = true;
                        break;
                    default:
                        if (!Enum.TryParse(sp, out key))
                        {
                            key = Keys.None;
                        }

                        break;
                }
            }
        }

        private ShortcutInfo GetShortcutInfo(string str, ShortcutType shortcutType)
        {
            Keys key = Keys.None;
            GetModifiers(str, out key, out bool ctrl, out bool shift, out bool alt);

            if (key != Keys.None)
            {
                return new ShortcutInfo(key, shift, ctrl, alt, shortcutType);
            }

            return null;
        }

        private ShortcutInfo GetCustomShortcutInfo(string str, string scriptPath)
        {
            Keys key = Keys.None;
            GetModifiers(str, out key, out bool ctrl, out bool shift, out bool alt);

            if (key != Keys.NoName)
            {
                return new ShortcutInfo(key, shift, ctrl, alt, scriptPath);
            }
            return null;
        }

        private string GetReadableShortcut(ShortcutInfo info)
        {
            var list = new List<string>();
            if (info.Control)
            {
                list.Add("Ctrl");
            }
            if (info.Shift)
            {
                list.Add("Shift");
            }
            if (info.Alt)
            {
                list.Add("Alt");
            }
            list.Add(info.Key.ToString());
            return String.Join("+", list.ToArray());
        }

        private void button2_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }

        private void listBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            switch (listBox1.SelectedIndex)
            {
                case 0:
                    HideExclude(generalPanel);
                    break;
                case 1:
                    HideExclude(shortcutPanel);
                    break;
                case 2:
                    HideExclude(variablePanel);
                    break;
            }
        }

        private void HideExclude(Control exclude)
        {
            foreach (Control c in splitContainer2.Panel1.Controls)
            {
                c.Visible = c == exclude;
            }
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            if (ignore) return;
            ShouldRestart = true;
        }

        private void textBox2_TextChanged(object sender, EventArgs e)
        {
            if (ignore) return;
            ShouldRestart = true;
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (ignore) return;
            ShouldRestart = true;
        }

        private void button3_Click(object sender, EventArgs e)
        {
            if (dataGridView1.CurrentRow == null)
            {
                return;
            }

            dataGridView1.CurrentRow.Cells[1].Value = "";
        }

        private void dataGridView1_SelectionChanged(object sender, EventArgs e)
        {
            if (dataGridView1.CurrentRow == null)
            {
                return;
            }

            button5.Enabled = dataGridView1.CurrentRow.Index >= regularShortcutCount;
            EnableEditMode();
        }

        private void EnableEditMode()
        {
            dataGridView1.CurrentCell = dataGridView1.CurrentRow.Cells[1];
            textBox3.Text = dataGridView1.CurrentCell.Value.ToString();
            textBox3.Enabled = true;
            textBox3.Focus();
            textBox3.SelectAll();
        }

        private void textBox3_PreviewKeyDown(object sender, PreviewKeyDownEventArgs e)
        {
            e.IsInputKey = true;
        }

        private void textBox3_KeyDown(object sender, KeyEventArgs e)
        {
            e.SuppressKeyPress = true;
            e.Handled = true;

            if (e.KeyCode == Keys.ControlKey || e.KeyCode == Keys.ShiftKey || e.KeyCode == Keys.Menu)
            {
                return;
            }

            if (e.KeyCode == Keys.Return || e.KeyCode == Keys.Space)
            {
                dataGridView1.Focus();
                textBox3.Text = "";
                return;
            }

            textBox3.Text = GetReadableShortcut(new ShortcutInfo(e.KeyCode, e.Shift, e.Control, e.Alt, ShortcutType.None));
            if (dataGridView1.CurrentRow != null)
            {
                dataGridView1.CurrentRow.Cells[1].Value = textBox3.Text;
            }
        }

        private void textBox3_KeyPress(object sender, KeyPressEventArgs e)
        {
            e.Handled = true;
        }

        private void textBox3_Leave(object sender, EventArgs e)
        {
            textBox3.Enabled = false;
        }

        private void button4_Click(object sender, EventArgs e)
        {
            var form = new CommandSelectForm();
            form.SetLang();
            if (form.ShowDialog() == DialogResult.OK)
            {
                dataGridView1.Rows.Add(GetCustomShortcutText(form.SelectedScriptPath), "");
                dataGridView1.FirstDisplayedScrollingRowIndex = dataGridView1.RowCount - 1;
                dataGridView1.CurrentCell = dataGridView1.Rows[dataGridView1.RowCount - 1].Cells[1];
                EnableEditMode();
            }
        }

        private void button5_Click(object sender, EventArgs e)
        {
            dataGridView1.Rows.RemoveAt(dataGridView1.CurrentRow.Index);
        }
    }
}
