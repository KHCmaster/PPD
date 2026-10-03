using System;
using System.Diagnostics;
using System.IO;
using WinSCP;

namespace FolderDiffChecker.Models
{
    /// <summary>
    /// 配布サーバーに SFTP でつなぎ、インストーラーや更新リストをアップロードします。
    /// </summary>
    sealed class Uploader : IDisposable
    {
        const string credentialsPpkFilePath = "credentials.ppk";
        const string lastUserNameFilePath = "lastUser.txt";

        readonly Session session;

        private Uploader(Session session)
        {
            this.session = session;
        }

        /// <summary>
        /// 前回使ったユーザー名です。
        /// </summary>
        public static string LastUserName
        {
            get { return File.Exists(lastUserNameFilePath) ? File.ReadAllText(lastUserNameFilePath) : ""; }
            set { File.WriteAllText(lastUserNameFilePath, value); }
        }

        /// <summary>
        /// サーバーにつなぎます。
        /// </summary>
        /// <param name="error">つなげなかったときの理由。</param>
        /// <returns>つなげなかったときは null。</returns>
        public static Uploader Connect(string userName, out string error)
        {
            if (!File.Exists(credentialsPpkFilePath))
            {
                error = String.Format("Locate {0}.", credentialsPpkFilePath);
                return null;
            }
            var process = Process.Start("pageant.exe", String.Format("\"{0}\"", Path.GetFullPath(credentialsPpkFilePath)));
            process.WaitForExit(4000);
            Session session = null;
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

                session = new Session
                {
                    DisableVersionCheck = true,
                    SessionLogPath = "log.txt"
                };
                session.Open(sessionOptions);
                error = null;
                return new Uploader(session);
            }
            catch (Exception ex)
            {
                session?.Dispose();
                error = ex.ToString();
                return null;
            }
        }

        /// <summary>
        /// ベータ版のインストーラー、差分、更新リストを送り、ベータ版のインストーラーのリンクを張り替えます。
        /// </summary>
        /// <param name="outPath">release\releases のフォルダー。</param>
        public void UploadBeta(string outPath, string version, string dottedVersion)
        {
            var dir = Path.Combine(outPath, dottedVersion);
            SendFile(Path.Combine(dir, String.Format("PPDInstaller{0}(x64).zip", version)), "installer/");
            SendFile(Path.Combine(outPath, String.Format("PPDDiff{0}_x64", version), String.Format("PPDDiff{0}_x64.zip", version)), "update/");
            SendFile(Path.Combine(dir, "beta_assembly_x64.xml"), "update/");
            SendFile(Path.Combine(dir, "beta_updatelist.xml"), "update/");
            ExecuteCommand(String.Format(@"ln -s -f PPDInstaller{0}\(x64\).zip installer/PPDInstaller_x64_beta.zip", version));
        }

        /// <summary>
        /// 正式版の更新リストを送り、インストーラーのリンクを張り替えます。
        /// </summary>
        /// <param name="outPath">release\releases のフォルダー。</param>
        public void Upload(string outPath, string version, string dottedVersion)
        {
            var dir = Path.Combine(outPath, dottedVersion);
            SendFile(Path.Combine(dir, "updatelist.xml"), "update/");
            SendFile(Path.Combine(dir, "assembly_x64.xml"), "update/");
            ExecuteCommand(String.Format(@"ln -s -f PPDInstaller{0}\(x64\).zip installer/PPDInstaller_x64.zip", version));
        }

        private void SendFile(string filePath, string destDir)
        {
            var transferOptions = new TransferOptions
            {
                TransferMode = TransferMode.Binary
            };

            var transferResult = session.PutFiles(Path.GetFullPath(filePath), destDir + Path.GetFileName(filePath), false, transferOptions);

            // Throw on any error
            transferResult.Check();

            foreach (TransferEventArgs transfer in transferResult.Transfers)
            {
                Console.WriteLine("Upload of {0} succeeded", transfer.FileName);
            }
        }

        private void ExecuteCommand(string command)
        {
            var result = session.ExecuteCommand(command);
            Console.WriteLine(result.Output);
            Console.WriteLine(result.ErrorOutput);
        }

        public void Dispose()
        {
            session.Dispose();
        }
    }
}
