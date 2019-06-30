using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows.Forms;
using System.Xml.Linq;
using WinSCP;

namespace FolderDiffChecker
{
    public partial class MainForm : Form
    {
        const string credentialsPpkFilePath = "credentials.ppk";

        private long startTime;
        private string outPath;

        public MainForm()
        {
            InitializeComponent();
            Load += Form1_Load;
        }

        void Form1_Load(object sender, EventArgs e)
        {
            string path = Assembly.GetExecutingAssembly().Location;
            for (int i = 0; i < 5; i++)
            {
                path = Path.GetDirectoryName(path);
            }

            path = Path.Combine(path, "release\\releases");
            var dirs = new List<string>();
            var regex = new Regex("\\d+\\.\\d+\\.\\d+\\.\\d+");
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
                textBox1.Text = Path.Combine(path, Path.Combine(dirs[dirs.Count - 2], "PPDInstaller(x64)"));
                textBox2.Text = Path.Combine(path, Path.Combine(dirs[dirs.Count - 1], "PPDInstaller(x64)"));
            }
            outPath = folderBrowserDialog1.SelectedPath = path;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (folderBrowserDialog1.ShowDialog() == DialogResult.OK)
            {
                this.textBox1.Text = folderBrowserDialog1.SelectedPath;
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            if (folderBrowserDialog1.ShowDialog() == DialogResult.OK)
            {
                this.textBox2.Text = folderBrowserDialog1.SelectedPath;
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            if (Directory.Exists(this.textBox1.Text) && Directory.Exists(this.textBox2.Text))
            {
                this.button3.Enabled = this.button4.Enabled = this.button5.Enabled = false;
                dataGridView1.RowCount = 0;

                var df = new DiffFinder(this.textBox1.Text, this.textBox2.Text,
                    new Regex[]{
                        new Regex("thumbs\\.db", RegexOptions.IgnoreCase),
                        new Regex("PPDUpdater\\.exe", RegexOptions.IgnoreCase),
                        new Regex("PPDInstaller\\.exe", RegexOptions.IgnoreCase)
                    },
                    textBox3.Text.Split(new string[] { ":" }, StringSplitOptions.RemoveEmptyEntries).Select(s => new Regex(Regex.Escape(s), RegexOptions.IgnoreCase)).ToArray(),
                    this);
                df.Finished += df_Finished;
                df.DiffFound += df_DiffFound;
                df.BeforeCompare += df_BeforeCompare;
                var t = new Thread(new ThreadStart(df.Execute));
                t.Start();
                startTime = Environment.TickCount;
            }
            else
            {
                MessageBox.Show("存在しないディレクトリを比較しています。");
            }
        }

        void df_BeforeCompare(string obj)
        {
            toolStripStatusLabel1.Text = obj;
        }

        void df_Finished(object sender, EventArgs e)
        {
            Console.WriteLine("Finished:{0}", Environment.TickCount - startTime);
            this.button3.Enabled = this.button4.Enabled = this.button5.Enabled = true;
            toolStripStatusLabel1.Text = "";
        }

        void df_DiffFound(DiffModel diffModel)
        {
            var fileName = Path.GetFileName(diffModel.FilePath);
            if (diffModel.IsFile)
            {
                var ignoreFiles = this.textBox4.Text.Split(':');
                if (Array.IndexOf(ignoreFiles, fileName) >= 0)
                {
                    return;
                }
            }
            dataGridView1.Rows.Add(diffModel, fileName, diffModel.IsFile, diffModel.Mode, diffModel.FilePath);
        }

        private void button4_Click(object sender, EventArgs e)
        {
            if (folderBrowserDialog1.ShowDialog() == DialogResult.OK)
            {
                CopyToDir(folderBrowserDialog1.SelectedPath);
            }
        }

        private void CopyToDir(string outDir)
        {
            string srcDir = this.textBox2.Text + (this.textBox2.Text.EndsWith(Path.DirectorySeparatorChar.ToString()) ? "" : Path.DirectorySeparatorChar.ToString());
            for (int i = 0; i < dataGridView1.RowCount; i++)
            {
                var model = dataGridView1[0, i].Value as DiffModel;
                var path = Path.Combine(outDir, model.FilePath.Substring(srcDir.Length));
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

        private void CopyFile(string src, string dest)
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

        private void button5_Click(object sender, EventArgs e)
        {
            var regex = new Regex("(\\d+)\\.(\\d+)\\.(\\d+)\\.(\\d+)\\\\PPDInstaller\\(x(\\d{2})\\)");
            var m = regex.Match(textBox2.Text);
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

        private void button6_Click(object sender, EventArgs e)
        {
            var regex = new Regex("(\\d+)\\.(\\d+)\\.(\\d+)\\.(\\d+)\\\\PPDInstaller");
            var m = regex.Match(textBox2.Text);
            if (m.Success)
            {
                // installer zip
                var dir = Path.GetDirectoryName(textBox2.Text);
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
                    Utility.Zip(Path.Combine(dir, installerName), installerDir, new Regex[] { new Regex("^Data$") });
                }

                // diff zip
                dir = Path.GetDirectoryName(Path.GetDirectoryName(textBox2.Text));
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

        private void button7_Click(object sender, EventArgs e)
        {
            var regex = new Regex("(\\d+)\\.(\\d+)\\.(\\d+)\\.(\\d+)\\\\PPDInstaller");
            var m = regex.Match(textBox2.Text);
            if (m.Success)
            {
                var dir = Path.GetDirectoryName(textBox1.Text);
                var targetDir = Path.GetDirectoryName(textBox2.Text);
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

        private void button8_Click(object sender, EventArgs e)
        {
            var regex = new Regex("(\\d+)\\.(\\d+)\\.(\\d+)\\.(\\d+)\\\\PPDInstaller\\(x(\\d{2})\\)");
            var m = regex.Match(textBox2.Text);
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
                SendFiles(new UploadFileInfo[]{
                    new UploadFileInfo
                    {
                        FilePath = Path.Combine(dir, String.Format("PPDInstaller{0}(x64).zip",version)),
                        DestDir = "installer/"
                    },
                    new UploadFileInfo
                    {
                        FilePath = Path.Combine(outPath, String.Format("PPDDiff{0}_x64", version), String.Format("PPDDiff{0}_x64.zip", version)),
                        DestDir = "update/"
                    },
                    new UploadFileInfo
                    {
                        FilePath = Path.Combine(dir, "beta_assembly_x64.xml"),
                        DestDir = "update/"
                    },
                    new UploadFileInfo
                    {
                        FilePath = Path.Combine(dir, "beta_updatelist.xml"),
                        DestDir = "update/"
                    }
                },
                session);
                ExecuteCommand(String.Format(@"ln -s -f PPDInstaller{0}\(x64\).zip installer/PPDInstaller_x64_beta.zip", version), session);
            }
        }

        private void button9_Click(object sender, EventArgs e)
        {
            var regex = new Regex("(\\d+)\\.(\\d+)\\.(\\d+)\\.(\\d+)\\\\PPDInstaller\\(x(\\d{2})\\)");
            var m = regex.Match(textBox2.Text);
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
                SendFiles(new UploadFileInfo[]{
                    new UploadFileInfo
                    {
                        FilePath = Path.Combine(dir, "updatelist.xml"),
                        DestDir = "update/"
                    },
                    new UploadFileInfo
                    {
                        FilePath = Path.Combine(dir, "assembly_x64.xml"),
                        DestDir = "update/"
                    }                }, session);
                ExecuteCommand(String.Format(@"ln -s -f PPDInstaller{0}\(x64\).zip installer/PPDInstaller_x64.zip", version), session);
            }
        }

        private Session CreateSession()
        {
            var userForm = new UserForm();
            if (userForm.ShowDialog() != DialogResult.OK)
            {
                return null;
            }

            return CreateSession(userForm.UserName);
        }

        private Session CreateSession(string userName)
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

        private void SendFiles(UploadFileInfo[] uploadFiles, Session session)
        {
            foreach (var uploadFile in uploadFiles)
            {
                SendFile(uploadFile.FilePath, uploadFile.DestDir, session);
            }
        }

        private void SendFile(string filePath, string destDir, Session session)
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

        private void ExecuteCommand(string command, Session session)
        {
            var result = session.ExecuteCommand(command);
            Console.WriteLine(result.Output);
            Console.WriteLine(result.ErrorOutput);
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

        private void button10_Click(object sender, EventArgs e)
        {
            var session = CreateSession();
            if (session == null)
            {
                return;
            }
            session.Dispose();
        }
    }
}
