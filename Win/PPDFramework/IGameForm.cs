using System.Windows.Forms;

namespace PPDFramework
{
    /// <summary>
    /// ゲームフォームのインターフェースです。
    /// </summary>
    public interface IGameForm
    {
        /// <summary>
        /// 閉じられようとしているかどうかを取得、設定します。
        /// </summary>
        bool IsCloseRequired
        {
            get;
            set;
        }

        /// <summary>
        /// 最初に閉じされようとしているかどうかを取得、設定します。
        /// </summary>
        bool IsFirstCloseRequierd
        {
            get;
            set;
        }

        /// <summary>
        /// 閉じることが許可されたかどうかを取得、設定します。
        /// </summary>
        bool CloseAdmitted
        {
            get;
            set;
        }

        /// <summary>
        /// フォームを取得します。
        /// </summary>
        Form MainForm
        {
            get;
        }

        /// <summary>
        /// 描画先のフォームを取得します。
        /// </summary>
        Form RenderForm
        {
            get;
        }
    }
}
