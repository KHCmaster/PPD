using System;
using System.Windows.Controls;

namespace PPDEditor.Forms
{
    /// <summary>
    /// ドックに表示する画面（WPF）の基底クラスです。
    /// </summary>
    /// <remarks>
    /// メインのウィンドウでは AvalonDock の LayoutAnchorable に入れて表示します。
    /// タブの表示名と、保存していない変更があるか（タブの名前の先頭の「*」）を持ちます（以前の ChangableDockContent と同じ）。
    /// </remarks>
    public class DockView : UserControl
    {
        string tabText = "";
        bool changed;

        /// <summary>
        /// タブの表示名が変わったときに発生します。
        /// </summary>
        public event EventHandler TabTextChanged;

        /// <summary>
        /// このドックを表示して前に出すように頼まれたときに発生します（ドックを入れている側が処理する）。
        /// </summary>
        public event EventHandler ShowRequested;

        /// <summary>
        /// このドックを表示して前に出します。
        /// </summary>
        protected void ShowDock()
        {
            ShowRequested?.Invoke(this, EventArgs.Empty);
        }

        /// <summary>
        /// タブの表示名です。
        /// </summary>
        public string TabText
        {
            get { return tabText; }
            set
            {
                value ??= "";
                if (tabText != value)
                {
                    tabText = value;
                    TabTextChanged?.Invoke(this, EventArgs.Empty);
                }
            }
        }

        public void ContentSaved()
        {
            changed = false;
            if (this.TabText.StartsWith('*') && this.TabText.Length > 1)
            {
                this.TabText = this.TabText[1..];
            }
        }

        public void ContentChanged()
        {
            changed = true;
            if (!this.TabText.StartsWith('*'))
            {
                this.TabText = "*" + this.TabText;
            }
        }

        public bool IsContentChanged
        {
            get
            {
                return changed;
            }
        }
    }
}
