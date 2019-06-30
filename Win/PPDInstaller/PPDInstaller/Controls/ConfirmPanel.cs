using PPDConfiguration;
using PPDInstaller.Executor;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Threading;
using System.Windows.Forms;

namespace PPDInstaller.Controls
{
    public partial class ConfirmPanel : PanelBase
    {
        public event EventHandler InstallFinished;
        const string NotYet = "notyet";
        const string OK = "ok";
        const string NotOK = "notok";
        const string Progress = "progress";

        string cachefolder = "Cache\\";

        string ipafontname = "ipag00302.zip";
        string lavFiltersName = "lavFilters.exe";

        string necessary = "必須";
        string optional = "任意";

        string downloading = "ダウンロードしています...";
        string unziping = "解凍しています";
        string installing = "インストールしています...";

        string ppd = "PPD";
        string ipaFont = "IPA Font";
        string bmsToPpd = "BMSTOPPD";
        string effect2DEditor = "Effect2DEditor";
        string lavFilters = "LAVFilters";

        InstallInfo installInfo;

        public ConfirmPanel()
        {
            InitializeComponent();
            treeView1.ImageList = new ImageList
            {
                ColorDepth = ColorDepth.Depth32Bit,
                ImageSize = new Size(16, 16)
            };
            treeView1.ImageList.Images.Add(NotYet, PPDInstaller.Properties.Resources.nothing);
            treeView1.ImageList.Images.Add(OK, PPDInstaller.Properties.Resources.ok);
            treeView1.ImageList.Images.Add(NotOK, PPDInstaller.Properties.Resources.notok);
            treeView1.ImageList.Images.Add(Progress, PPDInstaller.Properties.Resources.progress);
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

            treeView1.Nodes.Clear();
            treeView1.Nodes.Add(necessary);
            treeView1.Nodes.Add(optional);
            var icp = PanelManager.GetPanel<InstallComponentPanel>();
            installInfo = icp.InstallInfo;
            if (installInfo.PPD) treeView1.Nodes[0].Nodes.Add(NotYet, ppd);
            if (installInfo.IPAFont) treeView1.Nodes[0].Nodes.Add(NotYet, ipaFont);
            if (installInfo.BMSTOPPD) treeView1.Nodes[1].Nodes.Add(NotYet, bmsToPpd);
            if (installInfo.LAVFilters) treeView1.Nodes[1].Nodes.Add(NotYet, lavFilters);
            treeView1.ExpandAll();
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

            var installListExecutor = new InstallListExecutor(this);
            installListExecutor.Finished += installListExecutor_Finished;
            installListExecutor.Execute();
        }

        void installListExecutor_Finished(object sender, EventArgs e)
        {
            var ip = PanelManager.GetPanel<InstallPanel>();
            if (installInfo.PPD)
            {
                executeQueues.Enqueue(new PPDInstallExecutor(PanelManager.DataDirectory, ip.InstallDirectory, this) { Name = ppd });
            }
            if (installInfo.IPAFont)
            {
                executeQueues.Enqueue(new IPAFontInstallExecutor(Path.Combine(cachefolder, ipafontname), Path.Combine(cachefolder, Path.GetFileNameWithoutExtension(ipafontname)), this) { Name = ipaFont });
            }
            if (installInfo.BMSTOPPD)
            {
                executeQueues.Enqueue(new BMSTOPPDInstallExecutor(PanelManager.DataDirectory, ip.InstallDirectory, this) { Name = bmsToPpd });
            }
            if (installInfo.LAVFilters)
            {
                executeQueues.Enqueue(new LAVFiltersInstallExecutor(Path.Combine(cachefolder, lavFiltersName), this) { Name = lavFilters });
            }
            InnerInstall();
        }

        private void InnerInstall()
        {
            if (executeQueues.Count > 0)
            {
                var executor = executeQueues.Dequeue();
                string targetText = executor.Name;
                TreeNode targetNode = null;
                if (executor is PPDInstallExecutor)
                {
                    executor.ProgressText = installing;
                    targetNode = treeView1.Nodes[0];
                }
                else if (executor is IPAFontInstallExecutor)
                {
                    executor.ProgressText = downloading;
                    targetNode = treeView1.Nodes[0];
                }
                else if (executor is BMSTOPPDInstallExecutor)
                {
                    executor.ProgressText = installing;
                    targetNode = treeView1.Nodes[1];
                }
                else if (executor is LAVFiltersInstallExecutor)
                {
                    executor.ProgressText = downloading;
                    targetNode = treeView1.Nodes[1];
                }
                label7.Text = String.Format("{0} {1} {2}%", executor.ProgressText, executor.Name, executor.Progress);
                var node = GetNodeFromText(targetNode, targetText);
                if (node != null) node.ImageKey = Progress;
                executor.Progressed += executor_Progressed;
                executor.Finished += executor_Finished;

                thread = new Thread(new ThreadStart(executor.Execute));
                thread.Start();
                return;
            }
            IsInstallFinishsd = true;
            if (InstallFinished != null)
            {
                InstallFinished.Invoke(this, EventArgs.Empty);
            }
        }

        void executor_Finished(object sender, EventArgs e)
        {
            var executor = sender as CExecutor;
            if (executor.ErrorLog != "")
            {
                ErrorText += string.Format(string.IsNullOrEmpty(ErrorText) ? "{0}" : "\n{0}", executor.ErrorLog);
            }
            if (executor.Success)
            {
                SetOk(treeView1.Nodes[0], executor.Name);
                SetOk(treeView1.Nodes[1], executor.Name);
            }
            else
            {
                SetNotOk(treeView1.Nodes[0], executor.Name);
                SetNotOk(treeView1.Nodes[1], executor.Name);
            }
            InnerInstall();
        }

        void executor_Progressed(object sender, EventArgs e)
        {
            var executor = sender as CExecutor;
            label7.Text = String.Format("{0} {1} {2}%", executor.ProgressText, executor.Name, executor.Progress);
        }

        void SetOk(TreeNode Node, string targetText)
        {
            var node = GetNodeFromText(Node, targetText);
            if (node != null) node.ImageKey = OK;
        }

        void SetNotOk(TreeNode Node, string targetText)
        {
            var node = GetNodeFromText(Node, targetText);
            if (node != null) node.ImageKey = NotOK;
        }

        TreeNode GetNodeFromText(TreeNode node, string text)
        {
            TreeNode ret = null;
            if (node == null | text == null) return ret;
            foreach (TreeNode tn in node.Nodes)
            {
                if (tn.Text == text)
                {
                    ret = tn;
                    break;
                }
            }
            return ret;
        }

        public string ErrorText
        {
            get;
            private set;
        }
    }
}
