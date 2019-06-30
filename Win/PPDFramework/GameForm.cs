using System.Windows.Forms;

namespace PPDFramework
{
    /// <summary>
    /// ゲームのフォームです。
    /// </summary>
    public abstract class GameForm : Form, IGameForm
    {
        private const int WM_GRAPHNOTIFY = 0x0400 + 13;
        private const int WM_CLOSE = 0x0010;

        /// <summary>
        /// 閉じられようとしているかどうかを取得、設定します。
        /// </summary>
        public bool IsCloseRequired
        {
            get;
            set;
        }

        /// <summary>
        /// 最初に閉じされようとしているかどうかを取得、設定します。
        /// </summary>
        public bool IsFirstCloseRequierd
        {
            get;
            set;
        }

        /// <summary>
        /// 閉じることが許可されたかどうかを取得、設定します。
        /// </summary>
        public bool CloseAdmitted
        {
            get;
            set;
        }

        /// <summary>
        /// フォームを取得します。
        /// </summary>
        public Form MainForm
        {
            get { return this; }
        }

        /// <summary>
        /// 描画先のフォームを取得します。
        /// </summary>
        public Form RenderForm
        {
            get { return this; }
        }

        /// <summary>
        /// ウィンドウのイベントをハンドリングします。
        /// </summary>
        /// <param name="m"></param>
        protected override void WndProc(ref Message m)
        {
            switch (m.Msg)
            {
                case WM_CLOSE:
                    if (!CloseAdmitted)
                    {
                        IsFirstCloseRequierd = !IsCloseRequired;
                        IsCloseRequired = true;
                        return;
                    }
                    break;
            }
            base.WndProc(ref m);
        }
    }
}
