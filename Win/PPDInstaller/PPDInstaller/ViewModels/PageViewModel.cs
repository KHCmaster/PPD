using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;

namespace PPDInstaller.ViewModels
{
    /// <summary>
    /// インストーラーの 1 ページです。下のボタン（前へ・次へ・キャンセル）を使えるか、表示するかを決めます。
    /// </summary>
    public abstract class PageViewModel(LanguageTexts texts) : ObservableObject
    {
        public LanguageTexts Texts { get; } = texts;

        public virtual bool CanPrevious
        {
            get { return true; }
        }

        public virtual bool CanNext
        {
            get { return true; }
        }

        public virtual bool CanCancel
        {
            get { return true; }
        }

        public virtual bool IsPreviousVisible
        {
            get { return true; }
        }

        public virtual bool IsNextVisible
        {
            get { return true; }
        }

        public virtual bool IsCancelVisible
        {
            get { return true; }
        }

        /// <summary>
        /// 次へボタンの文字です。
        /// </summary>
        public virtual string NextText
        {
            get { return Texts["Button2"]; }
        }

        /// <summary>
        /// ページを表示したときに呼ばれます。
        /// </summary>
        public virtual void OnShown()
        {
        }
    }

    /// <summary>
    /// 最初のページです。かんたんインストール（確認のページまで進める）か、カスタムインストールかを選びます。
    /// </summary>
    public partial class StartPageViewModel(LanguageTexts texts, Action easyInstall, Action customInstall) : PageViewModel(texts)
    {
        public override bool CanPrevious
        {
            get { return false; }
        }

        public override bool IsPreviousVisible
        {
            get { return false; }
        }

        public override bool CanNext
        {
            get { return false; }
        }

        public override bool IsNextVisible
        {
            get { return false; }
        }

        [RelayCommand]
        private void EasyInstall()
        {
            easyInstall();
        }

        [RelayCommand]
        private void CustomInstall()
        {
            customInstall();
        }
    }

    /// <summary>
    /// スタートメニューに登録するかを選ぶページです。
    /// </summary>
    public partial class LinkCreatePageViewModel(LanguageTexts texts) : PageViewModel(texts)
    {
        [ObservableProperty]
        public partial bool RegisterStartMenu { get; set; } = true;
    }

    /// <summary>
    /// インストールが終わったページです。エラーがあれば表示します。
    /// </summary>
    public partial class FinishPageViewModel(LanguageTexts texts) : PageViewModel(texts)
    {
        public override bool CanPrevious
        {
            get { return false; }
        }

        public override bool IsPreviousVisible
        {
            get { return false; }
        }

        public override bool CanCancel
        {
            get { return false; }
        }

        public override bool IsCancelVisible
        {
            get { return false; }
        }

        public override string NextText
        {
            get { return Texts["Finish"]; }
        }

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(HasError))]
        public partial string ErrorText { get; set; }

        public bool HasError
        {
            get { return !string.IsNullOrEmpty(ErrorText); }
        }
    }

    /// <summary>
    /// インストールを中断したページです。
    /// </summary>
    public class InstallAbortedPageViewModel(LanguageTexts texts) : PageViewModel(texts)
    {
        public override bool CanPrevious
        {
            get { return false; }
        }

        public override bool IsPreviousVisible
        {
            get { return false; }
        }

        public override bool CanCancel
        {
            get { return false; }
        }

        public override bool IsCancelVisible
        {
            get { return false; }
        }

        public override string NextText
        {
            get { return Texts["Finish"]; }
        }
    }
}
