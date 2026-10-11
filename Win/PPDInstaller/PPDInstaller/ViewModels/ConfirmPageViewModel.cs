using CommunityToolkit.Mvvm.ComponentModel;
using PPDInstaller.Executor;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Threading;

namespace PPDInstaller.ViewModels
{
    /// <summary>
    /// インストールの進み具合です（一覧のアイコンになる）。
    /// </summary>
    public enum InstallState
    {
        NotYet,
        Progress,
        OK,
        NotOK
    }

    /// <summary>
    /// インストールする項目の一覧の 1 項目です（必須・任意の見出しと、その下の項目）。
    /// </summary>
    public partial class InstallNode(string text) : ObservableObject
    {
        public string Text { get; } = text;

        public ObservableCollection<InstallNode> Children { get; } = [];

        [ObservableProperty]
        private InstallState state;
    }

    /// <summary>
    /// インストールするものを確かめるページです。次へを押すと、ここでダウンロードとインストールを行います。
    /// </summary>
    public partial class ConfirmPageViewModel(LanguageTexts texts, InstallFolderPageViewModel folderPage, InstallComponentPageViewModel componentPage, string dataDirectory) : PageViewModel(texts)
    {
        const string cacheFolder = "Cache";
        const string ipaFontName = "ipag00302.zip";
        const string lavFiltersName = "lavFilters.exe";
        const string lavFilters = "LAVFilters";

        readonly InstallFolderPageViewModel folderPage = folderPage;
        readonly InstallComponentPageViewModel componentPage = componentPage;
        readonly string dataDirectory = dataDirectory;
        readonly Queue<(CExecutor Executor, InstallNode Node)> executeQueue = [];

        /// <summary>
        /// インストールが終わりました（失敗した項目があっても終わります）。
        /// </summary>
        public event EventHandler InstallFinished;

        [ObservableProperty]
        private string statusText = texts["Label7"];

        public ObservableCollection<InstallNode> Nodes { get; } = [];

        /// <summary>
        /// インストールしたもの（このページを表示したときの選択）です。
        /// </summary>
        public InstallInfo InstallInfo
        {
            get;
            private set;
        }

        public bool IsInstallFinished
        {
            get;
            private set;
        }

        /// <summary>
        /// インストールでエラーになった項目です。
        /// </summary>
        public string ErrorText
        {
            get;
            private set;
        }

        public override void OnShown()
        {
            InstallInfo = componentPage.InstallInfo;
            Nodes.Clear();
            var necessary = new InstallNode(Texts["Necessary"]);
            var optional = new InstallNode(Texts["Optional"]);
            Nodes.Add(necessary);
            Nodes.Add(optional);
            if (InstallInfo.PPD) necessary.Children.Add(new InstallNode(Texts["CheckBox1"]));
            if (InstallInfo.IPAFont) necessary.Children.Add(new InstallNode(Texts["CheckBox4"]));
            if (InstallInfo.BMSTOPPD) optional.Children.Add(new InstallNode(Texts["CheckBox6"]));
            if (InstallInfo.LAVFilters) optional.Children.Add(new InstallNode(lavFilters));
        }

        /// <summary>
        /// インストールを始めます。まずダウンロードするもののリストを取ってきます。
        /// </summary>
        public void Install()
        {
            var installListExecutor = new InstallListExecutor(SynchronizationContext.Current);
            installListExecutor.Finished += InstallListExecutor_Finished;
            installListExecutor.Execute();
        }

        private void InstallListExecutor_Finished(object sender, EventArgs e)
        {
            var context = SynchronizationContext.Current;
            var installDirectory = folderPage.InstallDirectory;
            var children = new List<InstallNode>(Nodes[0].Children);
            children.AddRange(Nodes[1].Children);
            int index = 0;
            void Enqueue(CExecutor executor, string progressText)
            {
                executor.Name = children[index].Text;
                executor.ProgressText = progressText;
                executeQueue.Enqueue((executor, children[index]));
                index++;
            }

            // 一覧（OnShown）と同じ順に入れる
            if (InstallInfo.PPD)
            {
                Enqueue(new PPDInstallExecutor(dataDirectory, installDirectory, context), Texts["Installing"]);
            }
            if (InstallInfo.IPAFont)
            {
                Enqueue(new IPAFontInstallExecutor(Path.Combine(cacheFolder, ipaFontName), Path.Combine(cacheFolder, Path.GetFileNameWithoutExtension(ipaFontName)), context), Texts["Downloading"]);
            }
            if (InstallInfo.BMSTOPPD)
            {
                Enqueue(new BMSTOPPDInstallExecutor(dataDirectory, installDirectory, context), Texts["Installing"]);
            }
            if (InstallInfo.LAVFilters)
            {
                Enqueue(new LAVFiltersInstallExecutor(Path.Combine(cacheFolder, lavFiltersName), context), Texts["Downloading"]);
            }
            ExecuteNext();
        }

        private void ExecuteNext()
        {
            if (executeQueue.Count == 0)
            {
                IsInstallFinished = true;
                InstallFinished?.Invoke(this, EventArgs.Empty);
                return;
            }

            var (executor, node) = executeQueue.Dequeue();
            ShowProgress(executor);
            node.State = InstallState.Progress;
            executor.Progressed += (sender, e) => ShowProgress(executor);
            executor.Finished += (sender, e) =>
            {
                if (!string.IsNullOrEmpty(executor.ErrorLog))
                {
                    ErrorText = string.IsNullOrEmpty(ErrorText) ? executor.ErrorLog : ErrorText + "\n" + executor.ErrorLog;
                }
                node.State = executor.Success ? InstallState.OK : InstallState.NotOK;
                ExecuteNext();
            };
            new Thread(executor.Execute).Start();
        }

        private void ShowProgress(CExecutor executor)
        {
            StatusText = String.Format("{0} {1} {2}%", executor.ProgressText, executor.Name, executor.Progress);
        }
    }
}
