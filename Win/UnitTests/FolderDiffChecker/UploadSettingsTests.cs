using FolderDiffChecker.Models;
using System.IO;
using Xunit;

namespace UnitTests.FolderDiffChecker
{
    /// <summary>
    /// アップロード先の設定ファイルを読むテストです（サーバーにはつながない）。
    /// </summary>
    public class UploadSettingsTests
    {
        const string Valid = """
            # comment
            HostName = upload.example.com

            Port=2222
            HostKeyFingerprint=ssh-rsa 2048 00:11:22
            CredentialsPath=keys/test.ppk
            """;

        [Fact]
        public void Load_ReadsValuesAndFindsCredentialsNextToTheFile()
        {
            using var folder = new TempFolder();
            var path = folder.Write("upload_settings.txt", Valid);
            var key = folder.Write(Path.Combine("keys", "test.ppk"), "not a real key");

            var settings = UploadSettings.Load(path, out var error);

            Assert.Null(error);
            Assert.Equal("upload.example.com", settings.HostName);
            Assert.Equal(2222, settings.Port);
            Assert.Equal("ssh-rsa 2048 00:11:22", settings.HostKeyFingerprint);
            Assert.Equal(Path.GetFullPath(key), settings.CredentialsPath);
        }

        [Fact]
        public void Load_AbsoluteCredentialsPath()
        {
            using var folder = new TempFolder();
            var key = folder.Write("elsewhere.ppk", "not a real key");
            var path = folder.Write("upload_settings.txt", Valid.Replace("keys/test.ppk", key));

            Assert.Equal(key, UploadSettings.Load(path, out _).CredentialsPath);
        }

        [Fact]
        public void Load_MissingFile_ReportsIt()
        {
            using var folder = new TempFolder();

            Assert.Null(UploadSettings.Load(folder.Combine("upload_settings.txt"), out var error));
            Assert.Contains("Locate", error);
        }

        [Theory]
        [InlineData("HostName = upload.example.com", "", "HostName is missing")]
        [InlineData("Port=2222", "", "Port is missing")]
        [InlineData("Port=2222", "Port=abc", "Port must be")]
        [InlineData("Port=2222", "Port=70000", "Port must be")]
        [InlineData("HostKeyFingerprint=ssh-rsa 2048 00:11:22", "HostKeyFingerprint=", "HostKeyFingerprint is missing")]
        [InlineData("CredentialsPath=keys/test.ppk", "CredentialsPath=keys/none.ppk", "Locate")]
        [InlineData("# comment", "no equal sign", "invalid line")]
        public void Load_InvalidSettings_ReportsTheReason(string original, string replaced, string expected)
        {
            using var folder = new TempFolder();
            folder.Write(Path.Combine("keys", "test.ppk"), "not a real key");
            var path = folder.Write("upload_settings.txt", Valid.Replace(original, replaced));

            Assert.Null(UploadSettings.Load(path, out var error));
            Assert.Contains(expected, error);
        }
    }
}
