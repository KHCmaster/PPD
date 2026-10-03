using System;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace PPDEditor.Forms
{
    public partial class SoundChangeEditWindow : Window
    {
        private string[] sounds;

        /// <summary>
        /// ボタンごとの音を選ぶコンボボックスです（スクエアからエルの順）。
        /// </summary>
        private ComboBox[] ButtonComboBoxes
        {
            get
            {
                return [comboBox1, comboBox2, comboBox3, comboBox4, comboBox5, comboBox6, comboBox7, comboBox8, comboBox9, comboBox10];
            }
        }

        public ushort[] Change
        {
            get
            {
                return [.. ButtonComboBoxes.Select(c => (ushort)c.SelectedIndex)];
            }
        }

        public event Action<string> PlaySound;

        public SoundChangeEditWindow()
        {
            InitializeComponent();
        }

        public void SetLang()
        {
            this.Title = Utility.Language["EditSoundChange"];
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
            this.button5.Content = Utility.Language["SMButton5"];
        }

        public void SetInfo(ushort[] change, string[] sounds)
        {
            this.sounds = sounds;
            var nameOfSounds = sounds.Select(s => Path.GetFileNameWithoutExtension(s)).ToArray();
            var comboBoxes = ButtonComboBoxes;
            for (var i = 0; i < comboBoxes.Length; i++)
            {
                comboBoxes[i].ItemsSource = nameOfSounds;
                comboBoxes[i].SelectedIndex = change[i];
            }
            comboBox11.ItemsSource = nameOfSounds;
            comboBox11.SelectedIndex = 0;
        }

        private void Button1_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
        }

        private void Button5_Click(object sender, RoutedEventArgs e)
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
