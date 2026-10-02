using PPDConfiguration;
using PPDInstaller.Executor;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Threading;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace PPDInstaller.Controls
{
    public partial class ConfirmPanel : PanelBase
    {
        public event EventHandler InstallFinished;

        static readonly ImageSource NotYet = LoadIcon("nothing.png");
        static readonly ImageSource OK = LoadIcon("ok.png");
        static readonly ImageSource NotOK = LoadIcon("notok.png");
        static readonly ImageSource Progress = LoadIcon("progress.png");

        readonly string cachefolder = "Cache\\";

        readonly string ipafontname = "ipag00302.zip";
        readonly string lavFiltersName = "lavFilters.exe";

        string necessary = "必須";
        string optional = "任意";

        string downloading = "ダウンロードしています...";
        string unziping = "解凍しています";
        string installing = "インストールしています...";

        string ppd = "PPD";
        string ipaFont = "IPA Font";
        string bmsToPpd = "BMSTOPPD";
        string effect2DEditor = "Effect2DEditor";
        readonly string lavFilters = "LAVFilters";

        readonly ObservableCollection<InstallNode> nodes = [];
        InstallInfo installInfo;

        public ConfirmPanel()
        {
            InitializeComponent();
            treeView1.ItemsSource = nodes;
        }

        private static BitmapImage LoadIcon(string fileName)
        {
            var image = new BitmapImage(new Uri("pack://application:,,,/img/" + fileName));
            image.Freeze();
            return image;
        }

        public override void SetLang(SettingReader setting)
        {
            base.SetLang(setting);
            this.label7.Text = setting["Label7"];
            necessary = setting["Necessary"];
            optional = setting["Optional"];
            downloading = setting["Downloading"];
            unziping = setting["Extracting"];
            installing = setting["Installing"];

            ppd = setting["CheckBox1"];
            ipaFont = setting["CheckBox4"];
            effect2DEditor = setting["CheckBox10"];
            bmsToPpd = setting["CheckBox6"];
        }

        public InstallInfo InstallInfo
        {
            get
            {
                return installInfo;
            }
        }

        public override void OnShown(bool skip)
        {
            base.OnShown(skip);

            nodes.Clear();
            nodes.Add(new InstallNode(necessary));
            nodes.Add(new InstallNode(optional));
            var icp = PanelManager.GetPanel<InstallComponentPanel>();
            installInfo = icp.InstallInfo;
            if (installInfo.PPD) nodes[0].Children.Add(new InstallNode(ppd));
            if (installInfo.IPAFont) nodes[0].Children.Add(new InstallNode(ipaFont));
            if (installInfo.BMSTOPPD) nodes[1].Children.Add(new InstallNode(bmsToPpd));
            if (installInfo.LAVFilters) nodes[1].Children.Add(new InstallNode(lavFilters));
        }

        public bool IsInstallFinishsd
        {
            get;
            private set;
        }

        Thread thread;
        Queue<Executor.CExecutor> executeQueues;

        public void Install()
        {
            executeQueues = new Queue<CExecutor>();

            var installListExecutor = new InstallListExecutor(Dispatcher);
            installListExecutor.Finished += InstallListExecutor_Finished;
            installListExecutor.Execute();
        }

        void InstallListExecutor_Finished(object sender, EventArgs e)
        {
            var ip = PanelManager.GetPanel<InstallPanel>();
            if (installInfo.PPD)
            {
                executeQueues.Enqueue(new PPDInstallExecutor(PanelManager.DataDirectory, ip.InstallDirectory, Dispatcher) { Name = ppd });
            }
            if (installInfo.IPAFont)
            {
                executeQueues.Enqueue(new IPAFontInstallExecutor(Path.Combine(cachefolder, ipafontname), Path.Combine(cachefolder, Path.GetFileNameWithoutExtension(ipafontname)), Dispatcher) { Name = ipaFont });
            }
            if (installInfo.BMSTOPPD)
            {
                executeQueues.Enqueue(new BMSTOPPDInstallExecutor(PanelManager.DataDirectory, ip.InstallDirectory, Dispatcher) { Name = bmsToPpd });
            }
            if (installInfo.LAVFilters)
            {
                executeQueues.Enqueue(new LAVFiltersInstallExecutor(Path.Combine(cachefolder, lavFiltersName), Dispatcher) { Name = lavFilters });
            }
            InnerInstall();
        }

        private void InnerInstall()
        {
            if (executeQueues.Count > 0)
            {
                var executor = executeQueues.Dequeue();
                string targetText = executor.Name;
                InstallNode targetNode = null;
                if (executor is PPDInstallExecutor)
                {
                    executor.ProgressText = installing;
                    targetNode = nodes[0];
                }
                else if (executor is IPAFontInstallExecutor)
                {
                    executor.ProgressText = downloading;
                    targetNode = nodes[0];
                }
                else if (executor is BMSTOPPDInstallExecutor)
                {
                    executor.ProgressText = installing;
                    targetNode = nodes[1];
                }
                else if (executor is LAVFiltersInstallExecutor)
                {
                    executor.ProgressText = downloading;
                    targetNode = nodes[1];
                }
                label7.Text = String.Format("{0} {1} {2}%", executor.ProgressText, executor.Name, executor.Progress);
                var node = GetNodeFromText(targetNode, targetText);
                node?.Icon = Progress;
                executor.Progressed += Executor_Progressed;
                executor.Finished += Executor_Finished;

                thread = new Thread(new ThreadStart(executor.Execute));
                thread.Start();
                return;
            }
            IsInstallFinishsd = true;
            InstallFinished?.Invoke(this, EventArgs.Empty);
        }

        void Executor_Finished(object sender, EventArgs e)
        {
            var executor = sender as CExecutor;
            if (executor.ErrorLog != "")
            {
                ErrorText += string.Format(string.IsNullOrEmpty(ErrorText) ? "{0}" : "\n{0}", executor.ErrorLog);
            }
            if (executor.Success)
            {
                SetIcon(nodes[0], executor.Name, OK);
                SetIcon(nodes[1], executor.Name, OK);
            }
            else
            {
                SetIcon(nodes[0], executor.Name, NotOK);
                SetIcon(nodes[1], executor.Name, NotOK);
            }
            InnerInstall();
        }

        void Executor_Progressed(object sender, EventArgs e)
        {
            var executor = sender as CExecutor;
            label7.Text = String.Format("{0} {1} {2}%", executor.ProgressText, executor.Name, executor.Progress);
        }

        static void SetIcon(InstallNode parent, string targetText, ImageSource icon)
        {
            var node = GetNodeFromText(parent, targetText);
            node?.Icon = icon;
        }

        static InstallNode GetNodeFromText(InstallNode node, string text)
        {
            if (node == null || text == null) return null;
            foreach (var child in node.Children)
            {
                if (child.Text == text)
                {
                    return child;
                }
            }
            return null;
        }

        public string ErrorText
        {
            get;
            private set;
        }

        /// <summary>
        /// インストールする項目の一覧の 1 項目です。
        /// </summary>
        public class InstallNode(string text) : INotifyPropertyChanged
        {
            private ImageSource icon = NotYet;

            public event PropertyChangedEventHandler PropertyChanged;

            public string Text { get; } = text;

            public ObservableCollection<InstallNode> Children { get; } = [];

            public ImageSource Icon
            {
                get { return icon; }
                set
                {
                    icon = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Icon)));
                }
            }
        }
    }
}
