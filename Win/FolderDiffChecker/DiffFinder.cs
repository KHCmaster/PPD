using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;

namespace FolderDiffChecker
{
    /// <summary>
    /// 2 つのフォルダーを比べて、追加・変更されたファイルを探します。イベントは作ったスレッドの <see cref="SynchronizationContext"/> で起こします。
    /// </summary>
    class DiffFinder(string oldFolderPath, string newFolderPath, Regex[] exceptFilePatterns, Regex[] forceIncludeFilePatterns, SynchronizationContext context)
    {
        public delegate void DiffEventHandler(DiffModel diffModel);
        public event DiffEventHandler DiffFound;
        public event EventHandler Finished;
        public event Action<string> BeforeCompare;

        protected void OnDiffFind(DiffModel diffModel)
        {
            context.Send(_ => DiffFound?.Invoke(diffModel), null);
        }

        protected void OnFinished()
        {
            context.Send(_ => Finished?.Invoke(this, EventArgs.Empty), null);
        }

        protected void OnBeforeCompare(string text)
        {
            context.Send(_ => BeforeCompare?.Invoke(text), null);
        }

        public string OldFolderPath
        {
            get;
            private set;
        } = oldFolderPath;

        public string NewFolderPath
        {
            get;
            private set;
        } = newFolderPath;

        public Regex[] ExceptFilePatterns
        {
            get;
            private set;
        } = exceptFilePatterns;

        public Regex[] ForceIncludeFilePatterns
        {
            get;
            private set;
        } = forceIncludeFilePatterns;

        public void Execute()
        {
            Recursive(OldFolderPath, NewFolderPath);
            OnFinished();
        }

        private bool IsShouldBeIgnored(string filePath)
        {
            var fileName = Path.GetFileName(filePath);
            return ExceptFilePatterns.Any(e => e.IsMatch(fileName));
        }

        private bool IsShouldBeAdded(string filePath)
        {
            var fileName = Path.GetFileName(filePath);
            return ForceIncludeFilePatterns.Any(e => e.IsMatch(fileName));
        }

        private void Recursive(string oldPath, string newPath)
        {
            foreach (string newFolder in Directory.GetDirectories(newPath))
            {
                var checkPath = Path.Combine(oldPath, Path.GetFileName(newFolder));
                if (!Directory.Exists(checkPath))
                {
                    RecursiveAddFolderAndFile(newFolder);
                }
                else
                {
                    Recursive(checkPath, newFolder);
                }
            }
            foreach (string newFile in Directory.GetFiles(newPath))
            {
                if (IsShouldBeIgnored(newFile))
                {
                    continue;
                }
                var checkPath = Path.Combine(oldPath, Path.GetFileName(newFile));
                if (!File.Exists(checkPath))
                {
                    FileFound(newFile);
                }
                else
                {
                    OnBeforeCompare(String.Format("{0} {1}", newPath, Path.GetFileName(newFile)));
                    /*PatchInfo[] patchInfos = Patcher.GetPatch(checkPath, newFile);
                    if (patchInfos.Length != 0)
                    {
                        FileEdited(Path.Combine(newPath, newFile));
                    }*/
                    if (IsShouldBeAdded(newFile) || !CheckSameFileContent(checkPath, newFile))
                    {
                        FileEdited(Path.Combine(newPath, newFile));
                    }
                    OnBeforeCompare("");
                }
            }
        }

        private void RecursiveAddFolderAndFile(string path)
        {
            DirectoryFound(path);
            foreach (string folderName in Directory.GetDirectories(path))
            {
                RecursiveAddFolderAndFile(folderName);
            }
            foreach (string fileName in Directory.GetFiles(path))
            {
                FileFound(fileName);
            }
        }

        private void DirectoryFound(string directoryPath)
        {
            var model = new DiffModel
            {
                FilePath = directoryPath,
                IsFile = false,
                Mode = DiffModel.DiffMode.Add
            };
            OnDiffFind(model);
        }

        private void FileFound(string filePath)
        {
            var model = new DiffModel
            {
                FilePath = filePath,
                IsFile = true,
                Mode = DiffModel.DiffMode.Add
            };
            OnDiffFind(model);
        }

        private void FileEdited(string filePath)
        {
            var model = new DiffModel
            {
                FilePath = filePath,
                IsFile = true,
                Mode = DiffModel.DiffMode.Edit
            };
            OnDiffFind(model);
        }

        const int bufferSize = 1024;

        private static bool CheckSameFileContent(string filePath1, string filePath2)
        {
            var extension = Path.GetExtension(filePath1).ToLower();
            if (extension == ".exe" || extension == ".dll")
            {
                FileVersionInfo fvi1 = FileVersionInfo.GetVersionInfo(filePath1),
                    fvi2 = FileVersionInfo.GetVersionInfo(filePath2);
                return fvi1.ProductVersion == fvi2.ProductVersion && fvi1.FileVersion == fvi2.FileVersion;
            }
            else
            {
                var fi1 = new FileInfo(filePath1);
                var fi2 = new FileInfo(filePath2);
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
        }
    }
}
