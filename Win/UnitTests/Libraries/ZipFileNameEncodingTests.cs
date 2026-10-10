using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using Xunit;

namespace UnitTests.Libraries
{
    /// <summary>
    /// 配布する ZIP（SharpZipLib）のファイル名が、従来（SharpZipLib 0.86）と同じく OEM コードページで書かれ、読めることを確かめます。
    /// </summary>
    /// <remarks>
    /// 古い PPDUpdater は UTF-8 のファイル名を読めないので、作る側は OEM コードページで書き、UTF-8 の印を付けない。
    /// </remarks>
    public class ZipFileNameEncodingTests
    {
        static ZipFileNameEncodingTests()
        {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
        }

        private static Encoding OemEncoding
        {
            get { return Encoding.GetEncoding(CultureInfo.CurrentCulture.TextInfo.OEMCodePage); }
        }

        /// <summary>
        /// OEM コードページで書ける、ASCII でないファイル名です（日本語の環境では日本語）。
        /// </summary>
        private static string FileName
        {
            get
            {
                foreach (var name in new[] { "譜面.txt", "café.txt" })
                {
                    if (OemEncoding.GetString(OemEncoding.GetBytes(name)) == name)
                    {
                        return name;
                    }
                }
                return "plain.txt";
            }
        }

        [Fact]
        public void FolderDiffChecker_WritesNamesInOemCodePage()
        {
            using var folder = new TempFolder();
            var source = folder.Combine("src");
            Directory.CreateDirectory(source);
            File.WriteAllText(Path.Combine(source, FileName), "a");
            var zip = folder.Combine("test.zip");

            global::FolderDiffChecker.Utility.Zip(zip, source);

            // OEM コードページで読むと名前が合う
            using (var archive = new ZipArchive(File.OpenRead(zip), ZipArchiveMode.Read, false, OemEncoding))
            {
                Assert.Contains(archive.Entries, e => e.FullName == "src/" + FileName);
            }
            // UTF-8 の印は付けない（付けると、ほかの文字コードで読んでも名前が合ってしまう）
            if (FileName != "plain.txt")
            {
                using var archive = new ZipArchive(File.OpenRead(zip), ZipArchiveMode.Read, false, Encoding.Latin1);
                Assert.DoesNotContain(archive.Entries, e => e.FullName.EndsWith(FileName));
            }
        }

        /// <summary>
        /// UTF-8 の印を付けずに、OEM コードページのファイル名で ZIP を作ります（古い版で作った ZIP と同じ）。
        /// </summary>
        private static string CreateOemZip(TempFolder folder)
        {
            var zip = folder.Combine("oem.zip");
            using var archive = new ZipArchive(File.Create(zip), ZipArchiveMode.Create, false, OemEncoding);
            using (var writer = new StreamWriter(archive.CreateEntry("dir/" + FileName).Open()))
            {
                writer.Write("a");
            }
            return zip;
        }

        [Fact]
        public void PPDInstaller_ReadsOemNames()
        {
            using var folder = new TempFolder();
            var zip = CreateOemZip(folder);
            var target = folder.Combine("out");

            global::PPDInstaller.Utility.Unzip(zip, target);

            Assert.Equal("a", File.ReadAllText(Path.Combine(target, "dir", FileName)));
        }

        [Fact]
        public void PPDUpdater_ReadsOemNames()
        {
            using var folder = new TempFolder();
            var zip = CreateOemZip(folder);
            var target = folder.Combine("out");

            global::PPDUpdater.Utility.FastZip.ExtractZip(zip, target, "");

            Assert.Equal(["dir"], Directory.GetDirectories(target).Select(Path.GetFileName));
            Assert.Equal("a", File.ReadAllText(Path.Combine(target, "dir", FileName)));
        }
    }
}
