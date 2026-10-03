using PPDEditorCommon;
using PPDFramework;
using PPDFramework.PPDStructure.EVDData;
using System;
using System.Collections.Generic;
using System.Windows;

namespace PPDEditor.Forms
{
    public partial class EventEditWindow : Window
    {
        readonly ButtonType[] normalInitializeOrder = [
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
        ];
        readonly ButtonType[] ACInitializeOrder = [
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
        ];
        List<ButtonType> initializeOrder;
        string[] orderName;
        private bool isReadOnly;
        public EventEditWindow()
        {
            InitializeComponent();
        }

        private void InitializeOrderChanged(object sender, RoutedEventArgs e)
        {
            if (radioButton8.IsChecked == true)
            {
                initializeOrder = [.. normalInitializeOrder];
                UpdateList();
            }
            else if (radioButton9.IsChecked == true)
            {
                initializeOrder = [.. ACInitializeOrder];
                UpdateList();
            }

            this.button3.IsEnabled = this.button4.IsEnabled = radioButton10.IsChecked == true;
        }

        private void UpdateList()
        {
            listBox1.Items.Clear();
            foreach (ButtonType buttonType in initializeOrder)
            {
                listBox1.Items.Add(orderName[(int)buttonType]);
            }
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
                    BPMRapidChange = (this.checkBox1.IsChecked == true)
                };
                if (!float.TryParse(this.textBox2.Text, out float bpm))
                {
                    bpm = 100;
                }
                ret.BPM = bpm;
                ret.BPMRapidChange = (this.checkBox1.IsChecked == true);
                ret.SquareKeepPlaying = (this.checkBox2.IsChecked == true);
                ret.CrossKeepPlaying = (this.checkBox3.IsChecked == true);
                ret.CircleKeepPlaying = (this.checkBox4.IsChecked == true);
                ret.TriangleKeepPlaying = (this.checkBox5.IsChecked == true);
                ret.LeftKeepPlaying = (this.checkBox6.IsChecked == true);
                ret.DownKeepPlaying = (this.checkBox7.IsChecked == true);
                ret.RightKeepPlaying = (this.checkBox8.IsChecked == true);
                ret.UpKeepPlaying = (this.checkBox9.IsChecked == true);
                ret.RKeepPlaying = (this.checkBox10.IsChecked == true);
                ret.LKeepPlaying = (this.checkBox11.IsChecked == true);
                ret.SquareReleaseSound = (this.checkBox12.IsChecked == true);
                ret.CrossReleaseSound = (this.checkBox13.IsChecked == true);
                ret.CircleReleaseSound = (this.checkBox14.IsChecked == true);
                ret.TriangleReleaseSound = (this.checkBox15.IsChecked == true);
                ret.LeftReleaseSound = (this.checkBox16.IsChecked == true);
                ret.DownReleaseSound = (this.checkBox17.IsChecked == true);
                ret.RightReleaseSound = (this.checkBox18.IsChecked == true);
                ret.UpReleaseSound = (this.checkBox19.IsChecked == true);
                ret.RReleaseSound = (this.checkBox20.IsChecked == true);
                ret.LReleaseSound = (this.checkBox21.IsChecked == true);
                if (radioButton1.IsChecked == true) ret.DisplayState = DisplayState.Normal;
                if (radioButton2.IsChecked == true) ret.DisplayState = DisplayState.Sudden;
                if (radioButton3.IsChecked == true) ret.DisplayState = DisplayState.Hidden;
                if (radioButton4.IsChecked == true) ret.MoveState = MoveState.Normal;
                if (radioButton5.IsChecked == true) ret.MoveState = MoveState.Stop;
                if (radioButton6.IsChecked == true) ret.NoteType = NoteType.Normal;
                if (radioButton7.IsChecked == true) ret.NoteType = NoteType.AC;
                if (radioButton11.IsChecked == true) ret.NoteType = NoteType.ACFT;
                ret.InitializeOrder = [.. initializeOrder];
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
                this.checkBox1.IsChecked = value.BPMRapidChange;
                this.checkBox2.IsChecked = value.SquareKeepPlaying;
                this.checkBox3.IsChecked = value.CrossKeepPlaying;
                this.checkBox4.IsChecked = value.CircleKeepPlaying;
                this.checkBox5.IsChecked = value.TriangleKeepPlaying;
                this.checkBox6.IsChecked = value.LeftKeepPlaying;
                this.checkBox7.IsChecked = value.DownKeepPlaying;
                this.checkBox8.IsChecked = value.RightKeepPlaying;
                this.checkBox9.IsChecked = value.UpKeepPlaying;
                this.checkBox10.IsChecked = value.RKeepPlaying;
                this.checkBox11.IsChecked = value.LKeepPlaying;
                this.checkBox12.IsChecked = value.SquareReleaseSound;
                this.checkBox13.IsChecked = value.CrossReleaseSound;
                this.checkBox14.IsChecked = value.CircleReleaseSound;
                this.checkBox15.IsChecked = value.TriangleReleaseSound;
                this.checkBox16.IsChecked = value.LeftReleaseSound;
                this.checkBox17.IsChecked = value.DownReleaseSound;
                this.checkBox18.IsChecked = value.RightReleaseSound;
                this.checkBox19.IsChecked = value.UpReleaseSound;
                this.checkBox20.IsChecked = value.RReleaseSound;
                this.checkBox21.IsChecked = value.LReleaseSound;
                switch (value.DisplayState)
                {
                    case DisplayState.Normal:
                        radioButton1.IsChecked = true;
                        break;
                    case DisplayState.Sudden:
                        radioButton2.IsChecked = true;
                        break;
                    case DisplayState.Hidden:
                        radioButton3.IsChecked = true;
                        break;
                }
                switch (value.MoveState)
                {
                    case MoveState.Normal:
                        radioButton4.IsChecked = true;
                        break;
                    case MoveState.Stop:
                        radioButton5.IsChecked = true;
                        break;
                }
                switch (value.NoteType)
                {
                    case NoteType.Normal:
                        radioButton6.IsChecked = true;
                        break;
                    case NoteType.AC:
                        radioButton7.IsChecked = true;
                        break;
                    case NoteType.ACFT:
                        radioButton11.IsChecked = true;
                        break;
                }
                textBox3.Text = value.SlideScale.ToString();

                initializeOrder = [.. value.InitializeOrder];
                if (Utility.IsSameArray(normalInitializeOrder, value.InitializeOrder))
                {
                    radioButton8.IsChecked = true;
                }
                else if (Utility.IsSameArray(ACInitializeOrder, value.InitializeOrder))
                {
                    radioButton9.IsChecked = true;
                }
                else
                {
                    radioButton10.IsChecked = true;
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
                    soundPanel.IsEnabled = !isReadOnly;
                    otherPanel.IsEnabled = !isReadOnly;
                }
            }
        }

        public void SetLang()
        {
            this.Title = Utility.Language["EEF"];
            this.tabPage1.Header = Utility.Language["EEFTab1"];
            this.tabPage2.Header = Utility.Language["EEFTabOthers"];
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
            this.groupBox1.Header = Utility.Language["EEFGroup1"];
            this.pictureBox1.ToolTip = Utility.Language["EEFError1"];
            this.groupBox2.Header = Utility.Language["EEFGroup2"];
            this.groupBox3.Header = Utility.Language["EEFGroup3"];
            this.groupBox5.Header = Utility.Language["EEFGroup5"];
            this.radioButton1.Content = Utility.Language["EEFRadioButton1"];
            this.radioButton2.Content = Utility.Language["EEFRadioButton2"];
            this.radioButton3.Content = Utility.Language["EEFRadioButton3"];
            this.radioButton4.Content = Utility.Language["EEFRadioButton4"];
            this.radioButton5.Content = Utility.Language["EEFRadioButton5"];
            this.radioButton6.Content = Utility.Language["EEFRadioButton4"];
            this.radioButton8.Content = Utility.Language["EEFRadioButton4"];
            this.radioButton10.Content = Utility.Language["EEFRadioButton10"];
            this.checkBox1.Content = Utility.Language["EEFCheckBox1"];
            this.checkBox2.Content = Utility.Language["EEFKeepPlaying"];
            this.checkBox3.Content = Utility.Language["EEFKeepPlaying"];
            this.checkBox4.Content = Utility.Language["EEFKeepPlaying"];
            this.checkBox5.Content = Utility.Language["EEFKeepPlaying"];
            this.checkBox6.Content = Utility.Language["EEFKeepPlaying"];
            this.checkBox7.Content = Utility.Language["EEFKeepPlaying"];
            this.checkBox8.Content = Utility.Language["EEFKeepPlaying"];
            this.checkBox9.Content = Utility.Language["EEFKeepPlaying"];
            this.checkBox10.Content = Utility.Language["EEFKeepPlaying"];
            this.checkBox11.Content = Utility.Language["EEFKeepPlaying"];
            this.checkBox12.Content = Utility.Language["EEFReleaseSound"];
            this.checkBox13.Content = Utility.Language["EEFReleaseSound"];
            this.checkBox14.Content = Utility.Language["EEFReleaseSound"];
            this.checkBox15.Content = Utility.Language["EEFReleaseSound"];
            this.checkBox16.Content = Utility.Language["EEFReleaseSound"];
            this.checkBox17.Content = Utility.Language["EEFReleaseSound"];
            this.checkBox18.Content = Utility.Language["EEFReleaseSound"];
            this.checkBox19.Content = Utility.Language["EEFReleaseSound"];
            this.checkBox20.Content = Utility.Language["EEFReleaseSound"];
            this.checkBox21.Content = Utility.Language["EEFReleaseSound"];
            this.groupBox6.Header = Utility.Language["Slide"];
            this.label14.Text = Utility.Language["Scale"];
            orderName = new string[10];
            for (int i = 0; i < orderName.Length; i++)
            {
                orderName[i] = Utility.Language[String.Format("SMLabel{0}", i + 1)];
            }
            this.button3.Content = Utility.Language["EEFButton3"];
            this.button4.Content = Utility.Language["EEFButton4"];
        }
        private void TextBox2_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            this.pictureBox1.Visibility = float.TryParse(this.textBox2.Text, out _) ? Visibility.Hidden : Visibility.Visible;
        }

        private void Button1_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = true;
        }

        private void Button3_Click(object sender, RoutedEventArgs e)
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

        private void Button4_Click(object sender, RoutedEventArgs e)
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
