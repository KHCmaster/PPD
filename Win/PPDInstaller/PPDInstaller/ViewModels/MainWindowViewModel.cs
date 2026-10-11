using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PPDInstaller.Models;
using System;
using System.Collections.Generic;

namespace PPDInstaller.ViewModels
{
    /// <summary>
    /// ViewModel から使う、ウィンドウの機能です。
    /// </summary>
    public interface IWindowService
    {
        /// <summary>
        /// フォルダーを選ばせます。
        /// </summary>
        /// <returns>キャンセルしたときは null。</returns>
        string SelectFolder(string initialDirectory);

        void ShowMessage(string text);

        /// <summary>
        /// はい・いいえで聞きます。
        /// </summary>
        bool Confirm(string text, string caption);

        void Close();
    }

    /// <summary>
    /// インストーラーのページを順に進める画面です。
    /// </summary>
    public partial class MainWindowViewModel : ObservableObject
    {
        readonly IWindowService window;
        readonly List<PageViewModel> pages;
        readonly InstallFolderPageViewModel folderPage;
        readonly InstallComponentPageViewModel componentPage;
        readonly LinkCreatePageViewModel linkCreatePage;
        readonly ConfirmPageViewModel confirmPage;
        readonly FinishPageViewModel finishPage;
        int currentIndex;

        /// <summary>
        /// 中断したか、インストールが終わったかです（次へで閉じる）。
        /// </summary>
        bool interrupted;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(NextText))]
        [NotifyCanExecuteChangedFor(nameof(NextOrCloseCommand), nameof(PreviousCommand), nameof(CancelCommand))]
        public partial PageViewModel CurrentPage { get; set; }

        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(NextOrCloseCommand), nameof(PreviousCommand), nameof(CancelCommand))]
        public partial bool IsInstalling { get; set; }

        public MainWindowViewModel(IWindowService window, LanguageTexts texts, string dataDirectory)
        {
            this.window = window;
            Texts = texts;
            folderPage = new InstallFolderPageViewModel(texts, window);
            componentPage = new InstallComponentPageViewModel(texts, folderPage);
            linkCreatePage = new LinkCreatePageViewModel(texts);
            confirmPage = new ConfirmPageViewModel(texts, folderPage, componentPage, dataDirectory);
            confirmPage.InstallFinished += ConfirmPage_InstallFinished;
            finishPage = new FinishPageViewModel(texts);
            pages =
            [
                new StartPageViewModel(texts, EasyInstall, Next),
                folderPage,
                componentPage,
                linkCreatePage,
                confirmPage,
                finishPage,
                new InstallAbortedPageViewModel(texts)
            ];
            ShowPage(0);
        }

        public LanguageTexts Texts { get; }

        public string NextText
        {
            get { return CurrentPage.NextText; }
        }

        private void ShowPage(int index)
        {
            currentIndex = index;
            CurrentPage = pages[index];
            CurrentPage.OnShown();
        }

        private bool CanNext()
        {
            return CurrentPage.CanNext && !IsInstalling;
        }

        [RelayCommand(CanExecute = nameof(CanNext))]
        private void NextOrClose()
        {
            if (interrupted)
            {
                window.Close();
                return;
            }
            Next();
        }

        private void Next()
        {
            if (CurrentPage == confirmPage && !confirmPage.IsInstallFinished)
            {
                IsInstalling = true;
                confirmPage.Install();
                return;
            }
            ShowPage(currentIndex + 1);
        }

        // 確認のページまで進めて、インストールを始める
        private void EasyInstall()
        {
            while (CurrentPage != confirmPage)
            {
                Next();
            }
            Next();
        }

        private bool CanPrevious()
        {
            return CurrentPage.CanPrevious && !IsInstalling;
        }

        [RelayCommand(CanExecute = nameof(CanPrevious))]
        private void Previous()
        {
            ShowPage(currentIndex - 1);
        }

        private bool CanCancel()
        {
            return CurrentPage.CanCancel && !IsInstalling;
        }

        [RelayCommand(CanExecute = nameof(CanCancel))]
        private void Cancel()
        {
            window.Close();
        }

        /// <summary>
        /// ウィンドウを閉じてよいかを返します。インストール中は閉じず、中断するかを聞いたときは中断のページを表示します。
        /// </summary>
        public bool CanClose()
        {
            if (IsInstalling)
            {
                return false;
            }
            if (interrupted)
            {
                return true;
            }
            if (window.Confirm(Texts["AskFinishText"], Texts["AskFinishCaption"]))
            {
                interrupted = true;
                ShowPage(pages.Count - 1);
            }
            return false;
        }

        private void ConfirmPage_InstallFinished(object sender, EventArgs e)
        {
            var installDirectory = folderPage.InstallDirectory;
            var installInfo = confirmPage.InstallInfo;
            installInfo.RegisterToMenu = linkCreatePage.RegisterStartMenu;
            InstallFinisher.WriteLanguage(installDirectory, installInfo, Texts["Language"], Texts["FontName"]);
            InstallFinisher.WriteInstallData(installDirectory, installInfo, componentPage.Versions, window.ShowMessage);
            InstallFinisher.RegisterStartMenu(installDirectory, installInfo, (Texts["Config"], Texts["KeyConfig"], Texts["ScoreFolder"]));
            finishPage.ErrorText = confirmPage.ErrorText;
            IsInstalling = false;
            interrupted = true;
            Next();
        }
    }
}
