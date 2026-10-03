using PPDEditor.Forms;
using System.Windows.Controls;
using System.Windows.Input;

namespace PPDEditor
{
    public partial class MemoWindow : DockView
    {
        public MemoWindow()
        {
            InitializeComponent();
        }

        void TextBox1_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            // 以前と同じく、ホイールで文字の大きさを変える
            if (e.Delta > 0)
            {
                MemoFontSize++;
            }
            else if (e.Delta < 0)
            {
                MemoFontSize--;
            }
            e.Handled = true;
        }

        void TextBox1_KeyDown(object sender, KeyEventArgs e)
        {
            if (Keyboard.Modifiers.HasFlag(ModifierKeys.Control))
            {
                switch (e.Key)
                {
                    case Key.OemPlus:
                        MemoFontSize++;
                        e.Handled = true;
                        break;
                    case Key.OemMinus:
                        MemoFontSize--;
                        e.Handled = true;
                        break;
                }
            }
        }

        public void SetLang()
        {
            this.TabText = Utility.Language["Memo"];
        }

        public void Clear()
        {
            MemoText = "";
        }

        public string MemoText
        {
            get
            {
                return textBox1.Text;
            }
            set
            {
                textBox1.Text = value;
            }
        }

        /// <summary>
        /// 文字の大きさ（ポイント）です。設定ファイルには以前と同じくポイントで保存します。
        /// </summary>
        public float MemoFontSize
        {
            get
            {
                return (float)(textBox1.FontSize * 72 / 96);
            }
            set
            {
                if (value >= 5 && value <= 100)
                {
                    textBox1.FontSize = value * 96 / 72;
                    PPDStaticSetting.MemoFontSize = value;
                }
            }
        }

        private void TextBox1_TextChanged(object sender, TextChangedEventArgs e)
        {
            ContentChanged();
        }
    }
}
