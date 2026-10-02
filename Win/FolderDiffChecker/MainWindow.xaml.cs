using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows;
using System.Xml.Linq;
using WinSCP;

namespace FolderDiffChecker
{
    public partial class MainWindow : Window
    {
        const string credentialsPpkFilePath = "credentials.ppk";

        private long startTime;
        private string outPath;
        private string lastFolder;
        private readonly ObservableCollection<DiffRow> rows = [];

        public MainWindow()
        {
            InitializeComponent();
            diffGrid.ItemsSource = rows;
        }

        void Window_Loaded(object sender, RoutedEventArgs e)
        {
            string path = Assembly.GetExecutingAssembly().Location;
            for (int i = 0; i < 5; i++)
            {
                path = Path.GetDirectoryName(path);
            }

            path = Path.Combine(path, "release\\releases");
            var dirs = new List<string>();
            var regex = VersionRegex();
            foreach (string dir in Directory.GetDirectories(path))
            {
                if (regex.IsMatch(Path.GetFileName(dir)))
                {
                    dirs.Add(dir);
                }
            }
            dirs.Sort((s1, s2) =>
            {
                var version1 = new Version(Path.GetFileName(s1));
                var version2 = new Version(Path.GetFileName(s2));
                return version1.CompareTo(version2);
            });
            if (dirs.Count >= 2)
            {
                oldFolderTextBox.Text = Path.Combine(path, Path.Combine(dirs[^2], "PPDInstaller(x64)"));
                newFolderTextBox.Text = Path.Combine(path, Path.Combine(dirs[^1], "PPDInstaller(x64)"));
            }
            outPath = lastFolder = path;
        }

        // フォルダーを選ぶ。前回選んだフォルダーから始める
        private string SelectFolder()
        {
            var dialog = new Microsoft.Win32.OpenFolderDialog { InitialDirectory = lastFolder };
            if (dialog.ShowDialog(this) == true)
            {
                lastFolder = dialog.FolderName;
                return dialog.FolderName;
            }
            return null;
        }

        private void OldFolderButton_Click(object sender, RoutedEventArgs e)
        {
            var folder = SelectFolder();
            if (folder != null)
            {
                this.oldFolderTextBox.Text = folder;
            }
        }

        private void NewFolderButton_Click(object sender, RoutedEventArgs e)
        {
            var folder = SelectFolder();
            if (folder != null)
            {
                this.newFolderTextBox.Text = folder;
            }
        }

        private void DiffCheckButton_Click(object sender, RoutedEventArgs e)
        {
            if (Directory.Exists(this.oldFolderTextBox.Text) && Directory.Exists(this.newFolderTextBox.Text))
            {
                this.diffCheckButton.IsEnabled = this.copyDiffButton.IsEnabled = this.copyDiffCleverButton.IsEnabled = false;
                rows.Clear();

                var df = new DiffFinder(this.oldFolderTextBox.Text, this.newFolderTextBox.Text,
                    [
                        ThumbsDbRegex(),
                        PPDUpdaterExeRegex(),
                        PPDInstallerExeRegex()
                    ],
                    [.. forceIncludeTextBox.Text.Split([":"], StringSplitOptions.RemoveEmptyEntries).Select(s => new Regex(Regex.Escape(s), RegexOptions.IgnoreCase))],
                    Dispatcher);
                df.Finished += Df_Finished;
                df.DiffFound += Df_DiffFound;
                df.BeforeCompare += Df_BeforeCompare;
                var t = new Thread(new ThreadStart(df.Execute));
                t.Start();
                startTime = Environment.TickCount;
            }
            else
            {
                MessageBox.Show("存在しないディレクトリを比較しています。");
            }
        }

        void Df_BeforeCompare(string obj)
        {
            statusText.Text = obj;
        }

        void Df_Finished(object sender, EventArgs e)
        {
            Console.WriteLine("Finished:{0}", Environment.TickCount - startTime);
            this.diffCheckButton.IsEnabled = this.copyDiffButton.IsEnabled = this.copyDiffCleverButton.IsEnabled = true;
            statusText.Text = "";
        }

        void Df_DiffFound(DiffModel diffModel)
        {
            var fileName = Path.GetFileName(diffModel.FilePath);
            if (diffModel.IsFile)
            {
                var ignoreFiles = this.ignoreFilesTextBox.Text.Split(':');
                if (Array.IndexOf(ignoreFiles, fileName) >= 0)
                {
                    return;
                }
            }
            rows.Add(new DiffRow(diffModel, fileName));
        }

        private void CopyDiffButton_Click(object sender, RoutedEventArgs e)
        {
            var folder = SelectFolder();
            if (folder != null)
            {
                CopyToDir(folder);
            }
        }

        private void CopyToDir(string outDir)
        {
            string srcDir = this.newFolderTextBox.Text + (this.newFolderTextBox.Text.EndsWith(Path.DirectorySeparatorChar.ToString()) ? "" : Path.DirectorySeparatorChar.ToString());
            foreach (var row in rows)
            {
                var model = row.Model;
                var path = Path.Combine(outDir, model.FilePath[srcDir.Length..]);
                if (model.IsFile)
                {
                    CopyFile(model.FilePath, path);
                }
                else
                {
                    Directory.CreateDirectory(path);
                }
            }
        }

        private static void CopyFile(string src, string dest)
        {
            var sep = dest.Split(Path.DirectorySeparatorChar);
            var dir = String.Format("{0}{1}{2}", sep[0], Path.DirectorySeparatorChar, sep[1]);
            for (int i = 2; i < sep.Length; i++)
            {
                if (!Directory.Exists(dir))
                {
                    Directory.CreateDirectory(dir);
                }
                dir += Path.DirectorySeparatorChar + sep[i];
            }
            File.Copy(src, dest, true);
        }

        private void CopyDiffCleverButton_Click(object sender, RoutedEventArgs e)
        {
            var regex = InstallerWithArchitectureRegex();
            var m = regex.Match(newFolderTextBox.Text);
            if (m.Success)
            {
                var outDir = Path.Combine(outPath, String.Format("PPDDiff{0}{1}{2}{3}_x{4}",
                    m.Groups[1].Value,
                    m.Groups[2].Value,
                    m.Groups[3].Value,
                    m.Groups[4].Value,
                    m.Groups[5].Value));
                if (!Directory.Exists(outDir))
                {
                    Directory.CreateDirectory(outDir);
                }
                CopyToDir(outDir);
            }
        }

        private void ZipAllButton_Click(object sender, RoutedEventArgs e)
        {
            var regex = InstallerRegex();
            var m = regex.Match(newFolderTextBox.Text);
            if (m.Success)
            {
                // installer zip
                var dir = Path.GetDirectoryName(newFolderTextBox.Text);
                var assemblies = new string[] { "x64" };
                foreach (var assemble in assemblies)
                {
                    var installerDir = Path.Combine(dir, String.Format("PPDInstaller({0})", assemble));
                    Utility.Zip(Path.Combine(installerDir, "Data.pak"), Path.Combine(installerDir, "Data"));
                    var installerName = String.Format("PPDInstaller{0}{1}{2}{3}({4}).zip",
                    m.Groups[1].Value,
                    m.Groups[2].Value,
                    m.Groups[3].Value,
                    m.Groups[4].Value,
                    assemble);
                    Utility.Zip(Path.Combine(dir, installerName), installerDir, [DataFolderRegex()]);
                }

                // diff zip
                dir = Path.GetDirectoryName(Path.GetDirectoryName(newFolderTextBox.Text));
                foreach (var assemble in assemblies)
                {
                    var childDirName = String.Format("PPDDiff{0}{1}{2}{3}_{4}", m.Groups[1].Value,
                    m.Groups[2].Value,
                    m.Groups[3].Value,
                    m.Groups[4].Value,
                    assemble);
                    var zipName = String.Format("PPDDiff{0}{1}{2}{3}_{4}.zip", m.Groups[1].Value,
                    m.Groups[2].Value,
                    m.Groups[3].Value,
                    m.Groups[4].Value,
                    assemble);
                    Utility.Zip(Path.Combine(dir, childDirName, zipName), Path.Combine(dir, childDirName, "Data"));
                }
            }
        }

        private void UpdateListButton_Click(object sender, RoutedEventArgs e)
        {
            var regex = InstallerRegex();
            var m = regex.Match(newFolderTextBox.Text);
            if (m.Success)
            {
                var dir = Path.GetDirectoryName(oldFolderTextBox.Text);
                var targetDir = Path.GetDirectoryName(newFolderTextBox.Text);
                var assemblies = new string[] { "x64" };
                var fileNames = new string[] { "updatelist.xml", "beta_updatelist.xml" };
                foreach (var fileName in fileNames)
                {
                    var document = XDocument.Load(Path.Combine(dir, fileName));
                    foreach (var assemble in assemblies)
                    {
                        document.Root.Add(new XElement("Update", new XAttribute("Version", String.Format("{0}.{1}.{2}.{3}", m.Groups[1].Value,
                            m.Groups[2].Value, m.Groups[3].Value, m.Groups[4].Value)), new XAttribute("Path", String.Format("PPDDiff{0}{1}{2}{3}_{4}.zip", m.Groups[1].Value,
                            m.Groups[2].Value, m.Groups[3].Value, m.Groups[4].Value, assemble)), new XAttribute("AssemblyType", assemble)));
                    }
                    document.Save(Path.Combine(targetDir, fileName));
                }
            }
        }

        private void UploadBetaButton_Click(object sender, RoutedEventArgs e)
        {
            var regex = InstallerWithArchitectureRegex();
            var m = regex.Match(newFolderTextBox.Text);
            if (!m.Success)
            {
                return;
            }

            var version = String.Format("{0}{1}{2}{3}", m.Groups[1].Value,
                    m.Groups[2].Value,
                    m.Groups[3].Value,
                    m.Groups[4].Value);
            var dir = Path.Combine(outPath, String.Format("{0}.{1}.{2}.{3}", m.Groups[1].Value,
                    m.Groups[2].Value,
                    m.Groups[3].Value,
                    m.Groups[4].Value));
            var session = CreateSession();
            if (session == null)
            {
                return;
            }
            using (session)
            {
                SendFiles([
                    new() {
                        FilePath = Path.Combine(dir, String.Format("PPDInstaller{0}(x64).zip",version)),
                        DestDir = "installer/"
                    },
                    new() {
                        FilePath = Path.Combine(outPath, String.Format("PPDDiff{0}_x64", version), String.Format("PPDDiff{0}_x64.zip", version)),
                        DestDir = "update/"
                    },
                    new() {
                        FilePath = Path.Combine(dir, "beta_assembly_x64.xml"),
                        DestDir = "update/"
                    },
                    new() {
                        FilePath = Path.Combine(dir, "beta_updatelist.xml"),
                        DestDir = "update/"
                    }
                ],
                session);
                ExecuteCommand(String.Format(@"ln -s -f PPDInstaller{0}\(x64\).zip installer/PPDInstaller_x64_beta.zip", version), session);
            }
        }

        private void UploadButton_Click(object sender, RoutedEventArgs e)
        {
            var regex = InstallerWithArchitectureRegex();
            var m = regex.Match(newFolderTextBox.Text);
            if (!m.Success)
            {
                return;
            }
            var session = CreateSession();
            if (session == null)
            {
                return;
            }
            using (session)
            {
                var version = String.Format("{0}{1}{2}{3}", m.Groups[1].Value,
                        m.Groups[2].Value,
                        m.Groups[3].Value,
                        m.Groups[4].Value);
                var dir = Path.Combine(outPath, String.Format("{0}.{1}.{2}.{3}", m.Groups[1].Value,
                        m.Groups[2].Value,
                        m.Groups[3].Value,
                        m.Groups[4].Value));
                SendFiles([
                    new() {
                        FilePath = Path.Combine(dir, "updatelist.xml"),
                        DestDir = "update/"
                    },
                    new() {
                        FilePath = Path.Combine(dir, "assembly_x64.xml"),
                        DestDir = "update/"
                    }                ], session);
                ExecuteCommand(String.Format(@"ln -s -f PPDInstaller{0}\(x64\).zip installer/PPDInstaller_x64.zip", version), session);
            }
        }

        private static Session CreateSession()
        {
            var userWindow = new UserWindow { Owner = Application.Current.MainWindow };
            if (userWindow.ShowDialog() != true)
            {
                return null;
            }

            return CreateSession(userWindow.UserName);
        }

        private static Session CreateSession(string userName)
        {
            if (!File.Exists(credentialsPpkFilePath))
            {
                MessageBox.Show(String.Format("Locate {0}.", credentialsPpkFilePath));
                return null;
            }
            var process = Process.Start("pageant.exe", String.Format("\"{0}\"", Path.GetFullPath(credentialsPpkFilePath)));
            process.WaitForExit(4000);
            try
            {
                var sessionOptions = new SessionOptions
                {
                    Protocol = Protocol.Sftp,
                    HostName = "219.94.253.225",
                    PortNumber = 52614,
                    UserName = userName,
                    SshPrivateKeyPath = Path.GetFullPath(credentialsPpkFilePath),
                    SshHostKeyFingerprint = "ssh-rsa 2048 09:d6:57:5f:6a:b8:6a:03:8a:b2:37:3d:be:ad:2c:08"
                };

                var session = new Session
                {
                    DisableVersionCheck = true,
                    SessionLogPath = "log.txt"
                };
                session.Open(sessionOptions);
                return session;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }

            return null;
        }

        private static void SendFiles(UploadFileInfo[] uploadFiles, Session session)
        {
            foreach (var uploadFile in uploadFiles)
            {
                SendFile(uploadFile.FilePath, uploadFile.DestDir, session);
            }
        }

        private static void SendFile(string filePath, string destDir, Session session)
        {
            // Upload files
            var transferOptions = new TransferOptions
            {
                TransferMode = TransferMode.Binary
            };

            TransferOperationResult transferResult;
            transferResult = session.PutFiles(Path.GetFullPath(filePath), destDir + Path.GetFileName(filePath), false, transferOptions);

            // Throw on any error
            transferResult.Check();

            // Print results
            foreach (TransferEventArgs transfer in transferResult.Transfers)
            {
                Console.WriteLine("Upload of {0} succeeded", transfer.FileName);
            }
        }

        private static void ExecuteCommand(string command, Session session)
        {
            var result = session.ExecuteCommand(command);
            Console.WriteLine(result.Output);
            Console.WriteLine(result.ErrorOutput);
        }

        /// <summary>
        /// 差分の一覧の 1 行です。
        /// </summary>
        class DiffRow(DiffModel model, string name)
        {
            public DiffModel Model { get; } = model;

            public string Name { get; } = name;
        }

        class UploadFileInfo
        {
            public string FilePath
            {
                get;
                set;
            }

            public string DestDir
            {
                get;
                set;
            }
        }

        private void SessionTestButton_Click(object sender, RoutedEventArgs e)
        {
            var session = CreateSession();
            if (session == null)
            {
                return;
            }
            session.Dispose();
        }

        [GeneratedRegex("\\d+\\.\\d+\\.\\d+\\.\\d+")]
        private static partial Regex VersionRegex();
        [GeneratedRegex("thumbs\\.db", RegexOptions.IgnoreCase)]
        private static partial Regex ThumbsDbRegex();
        [GeneratedRegex("PPDUpdater\\.exe", RegexOptions.IgnoreCase)]
        private static partial Regex PPDUpdaterExeRegex();
        [GeneratedRegex("PPDInstaller\\.exe", RegexOptions.IgnoreCase)]
        private static partial Regex PPDInstallerExeRegex();
        [GeneratedRegex("(\\d+)\\.(\\d+)\\.(\\d+)\\.(\\d+)\\\\PPDInstaller\\(x(\\d{2})\\)")]
        private static partial Regex InstallerWithArchitectureRegex();
        [GeneratedRegex("(\\d+)\\.(\\d+)\\.(\\d+)\\.(\\d+)\\\\PPDInstaller")]
        private static partial Regex InstallerRegex();
        [GeneratedRegex("^Data$")]
        private static partial Regex DataFolderRegex();
    }
}
