using System;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace PPDEditor.Forms
{
    public partial class SoundChangeEditForm : Form
    {
        private string[] sounds;

        public ushort[] Change
        {
            get
            {
                return new ushort[]{
                    (ushort)comboBox1.SelectedIndex,                    (ushort)comboBox2.SelectedIndex,                    (ushort)comboBox3.SelectedIndex,                    (ushort)comboBox4.SelectedIndex,                    (ushort)comboBox5.SelectedIndex,                    (ushort)comboBox6.SelectedIndex,                    (ushort)comboBox7.SelectedIndex,                    (ushort)comboBox8.SelectedIndex,                    (ushort)comboBox9.SelectedIndex,                    (ushort)comboBox10.SelectedIndex                };
            }
        }

        public event Action<string> PlaySound;

        public SoundChangeEditForm()
        {
            InitializeComponent();
        }

        public void SetLang()
        {
            this.Text = Utility.Language["EditSoundChange"];
            this.label1.Text = Utility.Language["SMLabel1"];
            this.label2.Text = Utility.Language["SMLabel2"];
            this.label3.Text = Utility.Language["SMLabel3"];
            this.label4.Text = Utility.Language["SMLabel4"];
            this.label5.Text = Utility.Language["SMLabel5"];
            this.label6.Text = Utility.Language["SMLabel6"];
            this.label7.Text = Utility.Language["SMLabel7"];
            this.label8.Text = Utility.Language["SMLabel8"];
            this.label9.Text = Utility.Language["SMLabel9"];
            this.label10.Text = Utility.Language["SMLabel10"];
            this.button5.Text = Utility.Language["SMButton5"];
        }

        public void SetInfo(ushort[] change, string[] sounds)
        {
            this.sounds = sounds;
            var nameOfSounds = sounds.Select(s => Path.GetFileNameWithoutExtension(s)).ToArray();
            comboBox1.Items.AddRange(nameOfSounds);
            comboBox2.Items.AddRange(nameOfSounds);
            comboBox3.Items.AddRange(nameOfSounds);
            comboBox4.Items.AddRange(nameOfSounds);
            comboBox5.Items.AddRange(nameOfSounds);
            comboBox6.Items.AddRange(nameOfSounds);
            comboBox7.Items.AddRange(nameOfSounds);
            comboBox8.Items.AddRange(nameOfSounds);
            comboBox9.Items.AddRange(nameOfSounds);
            comboBox10.Items.AddRange(nameOfSounds);
            comboBox11.Items.AddRange(nameOfSounds);
            comboBox1.SelectedIndex = change[0];
            comboBox2.SelectedIndex = change[1];
            comboBox3.SelectedIndex = change[2];
            comboBox4.SelectedIndex = change[3];
            comboBox5.SelectedIndex = change[4];
            comboBox6.SelectedIndex = change[5];
            comboBox7.SelectedIndex = change[6];
            comboBox8.SelectedIndex = change[7];
            comboBox9.SelectedIndex = change[8];
            comboBox10.SelectedIndex = change[9];
            comboBox11.SelectedIndex = 0;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.OK;
            Close();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }

        private void button5_Click(object sender, EventArgs e)
        {
            if (comboBox11.SelectedIndex <= 0)
            {
                return;
            }

            OnPlaySound(sounds[comboBox11.SelectedIndex]);
        }

        private void OnPlaySound(string sound)
        {
            PlaySound?.Invoke(sound);
        }
    }
}
