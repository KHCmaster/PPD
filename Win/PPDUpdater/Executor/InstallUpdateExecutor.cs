using PPDUpdater.Model;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Windows.Forms;

namespace PPDUpdater.Executor
{
    class InstallUpdateExecutor(string filePath, VersionInfo installversion, InstallInfo installInfo, int index, Control control) : CExecutor(control)
    {
        const int bufferSize = 1024;

        public string FilePath
        {
            get;
            private set;
        } = filePath;

        public InstallInfo InstallInfo
        {
            get;
            private set;
        } = installInfo;

        public int Index
        {
            get;
            private set;
        } = index;

        public VersionInfo InstallVersion
        {
            get;
            private set;
        } = installversion;

        public override void Execute()
        {
            base.Execute();
            try
            {
                var unzipDir = Path.Combine(Path.GetDirectoryName(FilePath), Path.GetFileNameWithoutExtension(FilePath));
                Unzip(FilePath, unzipDir);
                InstallData(unzipDir);
                Directory.Delete(Path.GetDirectoryName(FilePath), true);
                Success = true;
            }
            catch (Exception e)
            {
                ErrorLog = e.Message + "\r\n" + e.StackTrace;
                Success = false;
            }
            OnFinish();
        }

        private void InstallData(string unzipDir)
        {
            var CopyList = new List<string[]>();
            var dirs = new Queue<string>();
            dirs.Enqueue(Path.Combine(unzipDir, Utility.DataDir));
            {
                var dir = dirs.Dequeue();
                foreach (string indir in Directory.GetDirectories(dir))
                {
                    var dirname = Path.GetFileName(indir).ToUpper();
                    bool add = false;
                    switch (dirname)
                    {
                        case "PPD":
                            add = InstallInfo.PPD;
                            break;
                        case "BMSTOPPD":
                            add = InstallInfo.BMSTOPPD;
                            break;
                        case "EFFECT2DEDITOR":
                            add = InstallInfo.Effect2DEditor;
                            break;
                    }
                    if (add) dirs.Enqueue(indir);
                }
            }
            while (dirs.Count > 0)
            {
                var dir = dirs.Dequeue();
                foreach (string indir in Directory.GetDirectories(dir))
                {
                    dirs.Enqueue(indir);
                }
                foreach (string filename in Directory.GetFiles(dir))
                {
                    var localfn = Utility.GetRelativePath(filename, 4);
                    var copydir = Path.GetDirectoryName(Path.Combine(Utility.KHCDir, localfn));
                    if (!Directory.Exists(copydir))
                    {
                        Directory.CreateDirectory(copydir);
                    }
                    CopyList.Add([Path.Combine(Utility.KHCDir, localfn), filename]);
                }
            }
            int count = 0;
            foreach (string[] copys in CopyList)
            {
                string dest = copys[0];
                string src = copys[1];
                var fn = Path.GetFileName(src).ToUpper();
                switch (fn)
                {
                    case "PPD.EXE":
                        InstallInfo.PPDVersion = GetVersionInfo(src);
                        break;
                    case "PPDEDITOR.EXE":
                        InstallInfo.PPDeditorVersion = GetVersionInfo(src);
                        break;
                    case "BMSTOPPD.EXE":
                        InstallInfo.BMSTOPPDVersion = GetVersionInfo(src);
                        break;
                    case "EFFECT2DEDITOR.EXE":
                        InstallInfo.Effect2DEditorVersion = GetVersionInfo(src);
                        break;
                }
                CopyFile(src, dest);
                count++;
                Progress = (int)(count * 100 / CopyList.Count);
                OnProgress();
            }
        }

        private static void CopyFile(string src, string dest)
        {
            var parentDir = Path.GetDirectoryName(dest);
            if (!Directory.Exists(parentDir))
            {
                Directory.CreateDirectory(parentDir);
            }

            File.Copy(src, dest, true);
            if (!CheckSame(src, dest))
            {
                throw new Exception("Error in checking copied file.");
            }
        }

        private static bool CheckSame(string src, string dest)
        {
            if (!File.Exists(src) || !File.Exists(dest))
            {
                return false;
            }

            var fi1 = new FileInfo(src);
            var fi2 = new FileInfo(dest);
            if (fi1.Length != fi2.Length) return false;
            using FileStream fs1 = fi1.Open(FileMode.Open);
            using FileStream fs2 = fi2.Open(FileMode.Open);
            byte[] buffer1 = new byte[bufferSize], buffer2 = new byte[bufferSize];
            while (fs1.Position < fs1.Length)
            {
                var readSize = fs1.Read(buffer1, 0, buffer1.Length);
                fs2.ReadAtLeast(buffer2, buffer2.Length, throwOnEndOfStream: false);
                for (int i = 0; i < readSize; i++)
                {
                    if (buffer1[i] != buffer2[i])
                    {
                        return false;
                    }
                }
            }
            return true;
        }

        private static void Unzip(string path, string unzipDir)
        {
            Utility.FastZip.ExtractZip(path, unzipDir, Utility.ZipFilter);
        }

        private static VersionInfo GetVersionInfo(string path)
        {
            try
            {
                var fvi = FileVersionInfo.GetVersionInfo(path);
                return new VersionInfo(fvi.FileVersion);
            }
            catch
            {
                return VersionInfo.Zero;
            }
        }
    }
}
