using Effect2D;
using Microsoft.Win32;
using PPDConfiguration;
using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;

namespace Effect2DEditor.DockForm
{
    public partial class TimeLineDock : UserControl
    {
        public event Action<float> FpsChanged;
        public event Action<string> EffectLoaded;
        public event Action EffectDeleted;
        public event Action EffectReversed;
        public event Action EffectCopyed;
        public event Action EffectCleared;

        public MainWindow MainForm
        {
            get;
            set;
        }

        public float CurrentFrame
        {
            get { return timeLine1.CurrentFrame; }
        }

        public TimeLineDock()
        {
            InitializeComponent();
        }

        public void DrawAndRefresh()
        {
            timeLine1.DrawAndRefresh();
            timeLineItem1.DrawAndRefresh();
        }

        public void AdjustHScroll()
        {
            timeLine1.AdjustHScrollBar();
        }

        public void MoveVScroll(int val)
        {
            timeLine1.MoveVScroll(val);
        }

        public void Initialize()
        {
            toolStripTextBox1.Text = MainForm.CanvasDock.EffectManager.FPS.ToString();
            timeLine1.EffectManager = MainForm.CanvasDock.EffectManager;
            timeLine1.MainForm = MainForm;
            timeLineItem1.EffectManager = MainForm.CanvasDock.EffectManager;
            timeLineItem1.MainForm = MainForm;
            timeLine1.SetUtility(MainForm.CanvasDock.SelectedManager);
            timeLineItem1.SetUtility(MainForm.CanvasDock.SelectedManager);
            timeLineItem1.TimeLine = timeLine1;
        }

        public void SetLang(SettingReader lang)
        {
            toolStripButton1.ToolTip = lang["Add"];
            toolStripButton2.ToolTip = lang["Delete"];
            toolStripButton3.ToolTip = lang["SwapPermutation"];
            toolStripButton4.ToolTip = lang["Copy"];
            toolStripButton5.ToolTip = lang["Stop"];
            toolStripButton6.ToolTip = lang["Clear"];
            exToolStripSplitButton1.ToolTip = lang["PlayOnce"];
            回再生ToolStripMenuItem.Header = lang["PlayOnce"];
            リバース1回再生ToolStripMenuItem.Header = lang["PlayOnceReverse"];
            ループ再生ToolStripMenuItem.Header = lang["PlayLoop"];
            リバースループ再生ToolStripMenuItem.Header = lang["PlayReverseLoop"];
            timeLine1.SetLang(lang);
        }

        private void ToolStripButton1_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new OpenFileDialog
            {
                Filter = "JPEG, GIF, PNG, ETD|*.jpg;*.jpeg;*.gif;*.png;*.etd|JPEG|*.jpg;*.jpeg|GIF|*.gif|PNG|*.png|ETD|*.etd",
                Multiselect = true
            };
            if (dialog.ShowDialog(Window.GetWindow(this)) == true)
            {
                foreach (string fn in dialog.FileNames)
                {
                    OnEffectLoaded(fn);
                }
            }
        }

        private void ToolStripButton2_Click(object sender, RoutedEventArgs e)
        {
            EffectDeleted?.Invoke();
        }

        private void ToolStripButton3_Click(object sender, RoutedEventArgs e)
        {
            EffectReversed?.Invoke();
        }

        private void ToolStripButton4_Click(object sender, RoutedEventArgs e)
        {
            EffectCopyed?.Invoke();
        }

        private void ToolStripButton5_Click(object sender, RoutedEventArgs e)
        {
            MainForm.CanvasDock.Stop();
            exToolStripSplitButton1.IsChecked = false;
            ChangeImage();
        }

        private void ToolStripButton6_Click(object sender, RoutedEventArgs e)
        {
            EffectCleared?.Invoke();
        }

        private void PlayTypeButton_Click(object sender, RoutedEventArgs e)
        {
            playTypeMenu.PlacementTarget = playTypeButton;
            playTypeMenu.Placement = PlacementMode.Bottom;
            playTypeMenu.IsOpen = true;
        }

        private void 回再生ToolStripMenuItem_Click(object sender, RoutedEventArgs e)
        {
            SelectPlayType(sender as MenuItem);
        }

        private void ループ再生ToolStripMenuItem_Click(object sender, RoutedEventArgs e)
        {
            SelectPlayType(sender as MenuItem);
        }

        private void リバースループ再生ToolStripMenuItem_Click(object sender, RoutedEventArgs e)
        {
            SelectPlayType(sender as MenuItem);
        }

        private void リバース1回再生ToolStripMenuItem_Click(object sender, RoutedEventArgs e)
        {
            SelectPlayType(sender as MenuItem);
        }

        private void SelectPlayType(MenuItem menuItem)
        {
            exToolStripSplitButton1.IsChecked = true;
            TurnOffExcept(menuItem);
            Play();
        }

        public void EffectManagerPlayFinished()
        {
            this.exToolStripSplitButton1.IsChecked = false;
            ChangeImage();
        }

        private void TurnOffExcept(MenuItem tsmi)
        {
            foreach (MenuItem ttsmi in playTypeMenu.Items)
            {
                ttsmi.IsChecked = ttsmi == tsmi;
            }
        }

        private void ExToolStripSplitButton1_ButtonClick(object sender, RoutedEventArgs e)
        {
            // ToggleButton は押したときに IsChecked が切り替わる
            if (exToolStripSplitButton1.IsChecked == true)
            {
                Play();
            }
            else
            {
                MainForm.CanvasDock.Pause();
                ChangeImage();
            }
        }

        private void ChangeImage()
        {
            string image = null;
            if (回再生ToolStripMenuItem.IsChecked) image = "playonce.png";
            else if (リバース1回再生ToolStripMenuItem.IsChecked) image = "playreverse.png";
            else if (ループ再生ToolStripMenuItem.IsChecked) image = "playloop.png";
            else if (リバースループ再生ToolStripMenuItem.IsChecked) image = "playreverseloop.png";
            playImage.Source = image != null ? Utility.LoadImage(image) : null;
        }

        private void Play()
        {
            string tooltiptext = "";
            EffectManager.PlayType playType = Effect2D.EffectManager.PlayType.Once;
            if (回再生ToolStripMenuItem.IsChecked)
            {
                playType = EffectManager.PlayType.Once;
                tooltiptext = 回再生ToolStripMenuItem.Header as string;
            }
            else if (リバース1回再生ToolStripMenuItem.IsChecked)
            {
                playType = EffectManager.PlayType.ReverseOnce;
                tooltiptext = リバース1回再生ToolStripMenuItem.Header as string;
            }
            else if (ループ再生ToolStripMenuItem.IsChecked)
            {
                playType = EffectManager.PlayType.Loop;
                tooltiptext = ループ再生ToolStripMenuItem.Header as string;
            }
            else if (リバースループ再生ToolStripMenuItem.IsChecked)
            {
                playType = EffectManager.PlayType.ReverseLoop;
                tooltiptext = リバース1回再生ToolStripMenuItem.Header as string;
            }
            playImage.Source = Utility.LoadImage("pause.png");
            exToolStripSplitButton1.ToolTip = tooltiptext;
            MainForm.CanvasDock.PlayType = playType;
            MainForm.CanvasDock.Play();
        }

        private void ToolStripTextBox1_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (float.TryParse(toolStripTextBox1.Text, out float val))
            {
                FpsChanged?.Invoke(val);
            }
        }

        private void TimeLineDock_DragEnter(object sender, DragEventArgs e)
        {
            e.Effects = e.Data.GetDataPresent(DataFormats.FileDrop) ? DragDropEffects.Copy : DragDropEffects.None;
            e.Handled = true;
        }

        private void TimeLineDock_DragDrop(object sender, DragEventArgs e)
        {
            if (e.Data.GetData(DataFormats.FileDrop, false) is not string[] fileName)
            {
                return;
            }
            e.Handled = true;
            foreach (string fn in fileName)
            {
                var extension = Path.GetExtension(fn);
                if (CheckValidExtension(extension))
                {
                    OnEffectLoaded(fn);
                }
            }
        }

        private static bool CheckValidExtension(string extension)
        {
            extension = extension.ToLower();
            return extension == ".jpg" || extension == ".jpeg" || extension == ".gif" || extension == ".png" || extension == ".etd";
        }

        private void OnEffectLoaded(string fileName)
        {
            EffectLoaded?.Invoke(fileName);
        }
    }
}
