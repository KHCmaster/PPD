using System.Windows.Forms;
using System.Windows.Forms.Integration;

namespace PPDEditor.Forms
{
    /// <summary>
    /// WPF の画面（<see cref="DockView"/>）を WinForms のドックに表示します。メインのウィンドウを WPF にするまでの間に使います。
    /// </summary>
    /// <remarks>
    /// 配置のファイルとの互換のため、以前のドックのクラスの名前を保存する名前にします。
    /// 背景は透明にして、以前と同じくメインのウィンドウの背景（スキン）を透かして表示します。
    /// </remarks>
    public class WpfDockShell : ScrollableForm
    {
        readonly string persistString;

        public DockView View
        {
            get;
            private set;
        }

        public WpfDockShell(DockView view, string persistString)
        {
            View = view;
            this.persistString = persistString;
            AutoScroll = false;
            HideOnClose = true;
            // Controls に加えるので、シェルと一緒に破棄される
            var host = new ElementHost
            {
                Dock = DockStyle.Fill,
                BackColorTransparent = true,
                Child = view
            };
            Controls.Add(host);
            TabText = Text = view.TabText;
            view.TabTextChanged += (sender, e) => TabText = Text = view.TabText;
        }

        protected override string GetPersistString()
        {
            return persistString;
        }
    }
}
