using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;

namespace PPDEditor.ViewModels
{
    /// <summary>
    /// ダイアログの ViewModel から使う、ウィンドウの機能です。
    /// </summary>
    public interface IDialogView
    {
        void ShowMessage(string text);

        /// <summary>
        /// フォルダーを選ばせます。
        /// </summary>
        /// <returns>キャンセルしたときは null。</returns>
        string SelectFolder(string initialDirectory);

        /// <summary>
        /// 開くファイルを選ばせます。
        /// </summary>
        /// <returns>キャンセルしたときは null。</returns>
        string OpenFile(string filter);

        /// <summary>
        /// 保存するファイルを選ばせます。
        /// </summary>
        /// <returns>キャンセルしたときは null。</returns>
        string SaveFile(string filter);
    }

    /// <summary>
    /// OK とキャンセルで閉じるダイアログの ViewModel の基底クラスです。
    /// </summary>
    /// <remarks>
    /// OK（<see cref="OkCommand"/>）は、<see cref="CanOk"/> で押せるかを決め、<see cref="Validate"/> で確かめてから閉じる。
    /// キャンセルはボタンの IsCancel で閉じる。
    /// </remarks>
    public abstract partial class DialogViewModel : ObservableObject
    {
        /// <summary>
        /// 閉じてほしいときに起こります（OK のときは true）。
        /// </summary>
        public event Action<bool> CloseRequested;

        public LanguageTexts Texts
        {
            get { return LanguageTexts.Instance; }
        }

        /// <summary>
        /// 表示しているウィンドウです（メッセージを出すときに使う）。
        /// </summary>
        public IDialogView View
        {
            get;
            set;
        }

        protected virtual bool CanOk()
        {
            return true;
        }

        /// <summary>
        /// OK で閉じてよいかを確かめます。だめなときは理由を表示して false を返します。
        /// </summary>
        protected virtual bool Validate()
        {
            return true;
        }

        [RelayCommand(CanExecute = nameof(CanOk))]
        private void Ok()
        {
            if (Validate())
            {
                CloseRequested?.Invoke(true);
            }
        }

        /// <summary>
        /// メッセージを表示します。
        /// </summary>
        protected void ShowMessage(string text)
        {
            View?.ShowMessage(text);
        }
    }
}
