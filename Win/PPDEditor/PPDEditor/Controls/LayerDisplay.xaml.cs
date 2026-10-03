using PPDEditor.Command.PPDSheet;
using PPDEditor.Forms;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace PPDEditor.Controls
{
    public partial class LayerDisplay : UserControl
    {
        public event EventHandler VisibleStateChanged;
        public event EventHandler SelectStateChanged;
        public event EventHandler Deleted;
        public event EventHandler Duplicated;
        bool selected;
        bool visible = true;
        readonly PPDSheet data;
        System.Drawing.Color markColor;
        string deleteconfirm = "削除してもよろしいですか?";
        string confirm = "確認";
        public LayerDisplay()
        {
            data = new PPDSheet();
            InitializeComponent();
            MarkColor = PPDEditorSkin.Skin.TimeLineMarkColor;
        }
        public void SetLang()
        {
            this.このレイヤーを複製MenuItem.Header = Utility.Language["LMMenu2"];
            this.このレイヤーを削除MenuItem.Header = Utility.Language["LMMenu3"];
            deleteconfirm = Utility.Language["LMDeleteConfirm"];
            confirm = Utility.Language["LMConfirm"];
        }

        private void Panel1_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            // 選択やドラッグにならないように、ここで止める
            e.Handled = true;
            DisplayVisible = !visible;
        }

        private void VisibleChange()
        {
            VisibleStateChanged?.Invoke(this, EventArgs.Empty);
        }

        private void LayerDisplay_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            Select();
        }

        private void LayerDisplay_ContextMenuOpening(object sender, ContextMenuEventArgs e)
        {
            Select();
        }

        private void Select()
        {
            selected = true;
            this.Focus();
            SelectStateChanged?.Invoke(this, EventArgs.Empty);
            ChangeDisplayMode();
        }

        private void ChangeDisplayMode()
        {
            this.Background = selected ? Brushes.LightBlue : Brushes.LightGray;
        }

        private void Label1_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ClickCount == 2)
            {
                e.Handled = true;
                this.textBox1.Text = DisplayName;
                this.textBox1.Visibility = Visibility.Visible;
                this.textBox1.Focus();
                this.textBox1.SelectAll();
            }
        }

        private void TextBox1_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Return)
            {
                e.Handled = true;
                Hidetextbox();
            }
        }

        private void TextBox1_LostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            Hidetextbox();
        }

        private void Hidetextbox()
        {
            if (this.textBox1.Visibility == Visibility.Visible)
            {
                DisplayName = this.textBox1.Text == "" ? "no text" : this.textBox1.Text;
                data.DisplayName = this.DisplayName;
                this.textBox1.Visibility = Visibility.Collapsed;
            }
        }

        private void このレイヤーを削除MenuItem_Click(object sender, RoutedEventArgs e)
        {
            if (this.Deleted != null)
            {
                if (MessageBox.Show(Window.GetWindow(this), deleteconfirm, confirm, MessageBoxButton.OKCancel) == MessageBoxResult.OK)
                {
                    this.Deleted.Invoke(this, EventArgs.Empty);
                }
            }
        }

        private void このレイヤーを複製MenuItem_Click(object sender, RoutedEventArgs e)
        {
            this.Duplicated?.Invoke(this, EventArgs.Empty);
        }
        public PPDSheet PPDData
        {
            get
            {
                return data;
            }
        }
        public bool Selected
        {
            get
            {
                return selected;
            }
            set
            {
                selected = value;
                ChangeDisplayMode();
            }
        }
        public bool SelectedWithEvent
        {
            get
            {
                return selected;
            }
            set
            {
                selected = value;
                if (selected)
                {
                    this.Focus();
                    SelectStateChanged?.Invoke(this, EventArgs.Empty);
                }
                ChangeDisplayMode();
            }
        }
        public bool DisplayVisible
        {
            get
            {
                return visible;
            }
            set
            {
                this.pictureBox1.Visibility = value ? Visibility.Visible : Visibility.Hidden;
                visible = value;
                VisibleChange();
            }
        }
        public string DisplayName
        {
            get
            {
                return this.label1.Text;
            }
            set
            {
                this.label1.Text = value;
            }
        }
        public string BPM
        {
            get
            {
                return this.label2.Text;
            }
            set
            {
                this.label2.Text = value;
            }
        }

        public System.Drawing.Color MarkColor
        {
            get
            {
                return markColor;
            }
            set
            {
                markColor = value;
                panel2.Background = WpfSkin.ToBrush(value);
            }
        }

        private void Panel2_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            // 選択やドラッグにならないように、ここで止める
            e.Handled = true;
            if (e.ClickCount != 2)
            {
                return;
            }
            var dialog = new FlowScriptDrawControl.Dialog.ColorDialog
            {
                CurrentColor = Color.FromRgb(markColor.R, markColor.G, markColor.B)
            };
            if (WpfDialog.ShowDialog(dialog))
            {
                MarkColor = System.Drawing.Color.FromArgb(dialog.CurrentColor.R, dialog.CurrentColor.G, dialog.CurrentColor.B);
                WindowUtility.TimeLineForm.Seekmain.DrawAndRefresh();
            }
        }
    }
}
