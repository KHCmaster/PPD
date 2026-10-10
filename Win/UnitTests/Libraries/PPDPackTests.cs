using PPDPack;
using System;
using System.IO;
using System.Linq;
using System.Text;
using Xunit;

namespace UnitTests.Libraries
{
    public class PPDPackTests
    {
        private static void WritePack(string path, params (string Name, string Content)[] files)
        {
            using var writer = new PackWriter(path);
            foreach (var stream in writer.Write([.. files.Select(f => f.Name)]))
            {
                var bytes = Encoding.UTF8.GetBytes(files.First(f => f.Name == stream.Filename).Content);
                stream.Write(bytes, 0, bytes.Length);
                Assert.Equal(bytes.Length, stream.Position);
            }
        }

        private static string ReadAll(Stream stream)
        {
            using var reader = new StreamReader(stream, Encoding.UTF8, false, 1024, leaveOpen: true);
            return reader.ReadToEnd();
        }

        [Fact]
        public void WriteThenRead_KeepsFiles()
        {
            using var folder = new TempFolder();
            var path = folder.Combine("a.pak");
            WritePack(path, ("ppd", "marks"), ("evd", ""), ("日本語", "テキスト"));

            using var reader = new PackReader(path);

            Assert.Equal(["ppd", "evd", "日本語"], reader.FileList);
            Assert.Equal("marks", ReadAll(reader.Read("ppd")));
            Assert.Equal("", ReadAll(reader.Read("evd")));
            Assert.Equal("テキスト", ReadAll(reader.Read("日本語")));
            Assert.Null(reader.Read("missing"));
            Assert.Equal(new FileInfo(path).Length, reader.LastIndex);
        }

        [Fact]
        public void CopyTo_WritesIntoThePack()
        {
            // PPDEditor は、保存するときにスクリプトのファイルを CopyTo で書き込む（.NET は書き込めない先には書かない）
            using var folder = new TempFolder();
            var path = folder.Combine("a.pak");
            using (var writer = new PackWriter(path))
            {
                foreach (var stream in writer.Write(["script"]))
                {
                    Assert.True(stream.CanWrite);
                    using var source = new MemoryStream(Encoding.UTF8.GetBytes("<Root />"));
                    source.CopyTo(stream);
                }
            }

            using var reader = new PackReader(path);
            Assert.Equal("<Root />", ReadAll(reader.Read("script")));
        }

        [Fact]
        public void LongFileName_IsCutTo255Bytes()
        {
            var stream = new MemoryStream();
            var name = new string('a', 300);
            using (var writer = new PackWriter(stream))
            {
                foreach (var s in writer.Write([name]))
                {
                    s.WriteByte(1);
                }
                stream = new MemoryStream(stream.ToArray());
            }

            using var reader = new PackReader(stream);
            Assert.Equal(255, reader.FileList.Single().Length);
        }

        [Fact]
        public void NotAPack_ThrowsAndReleasesFile()
        {
            using var folder = new TempFolder();
            folder.Write("short.pak", "PPD");
            folder.Write("other.pak", "NOTAPACKFILE");

            Assert.Throws<ArgumentException>(() => new PackReader(folder.Combine("missing.pak")));
            Assert.Throws<Exception>(() => new PackReader(folder.Combine("short.pak")));
            Assert.Throws<Exception>(() => new PackReader(folder.Combine("other.pak")));
            // ファイルを開いたままにしない
            File.Delete(folder.Combine("other.pak"));
            File.Delete(folder.Combine("short.pak"));
        }

        [Fact]
        public void IncompleteHeader_Throws()
        {
            var bytes = PackUtility.FileSignature.Concat(new byte[] { 3, 4 }).ToArray();

            Assert.Throws<Exception>(() => new PackReader(new MemoryStream(bytes)));
        }

        private static PackReader CreateReader()
        {
            var stream = new MemoryStream();
            using (var writer = new PackWriter(stream))
            {
                foreach (var s in writer.Write(["first", "second"]))
                {
                    var bytes = Encoding.ASCII.GetBytes(s.Filename == "first" ? "0123456789" : "abcdef");
                    s.Write(bytes, 0, bytes.Length);
                    Assert.False(s.CanRead || s.CanSeek);
                    Assert.True(s.CanWrite);
                    Assert.Equal(0, s.Length);
                    Assert.Equal(0, s.Read(new byte[1], 0, 1));
                    Assert.Equal(0, s.Seek(0, SeekOrigin.Begin));
                    s.SetLength(0);
                    s.Flush();
                    Assert.Throws<NotImplementedException>(() => s.Position = 0);
                }
                stream = new MemoryStream(stream.ToArray());
            }
            return new PackReader(stream);
        }

        [Fact]
        public void StreamReader_ReadsOnlyItsFile()
        {
            using var reader = CreateReader();
            var first = reader.Read("first");
            var buffer = new byte[100];

            Assert.Equal(10, first.Length);
            Assert.True(first.CanRead);
            Assert.False(first.CanSeek || first.CanWrite);
            Assert.Equal(10, first.Read(buffer, 0, 100));
            Assert.Equal(0, first.Read(buffer, 0, 100));
            Assert.Equal("abcdef", ReadAll(reader.Read("second")));
            Assert.Throws<ArgumentException>(() => first.Read(buffer, -1, 1));
            Assert.Throws<ArgumentException>(() => first.Read(buffer, 0, -1));
            Assert.Throws<NullReferenceException>(() => first.Read(null, 0, 1));
            Assert.Throws<NotImplementedException>(() => first.Write(buffer, 0, 1));
            Assert.Throws<NotImplementedException>(() => first.SetLength(1));
            Assert.Throws<NotImplementedException>(() => first.Flush());
        }

        [Fact]
        public void StreamReader_SeekReturnsPositionInFile()
        {
            using var reader = CreateReader();
            var second = reader.Read("second");

            // 以前はパック全体の中の位置を返していた
            Assert.Equal(2, second.Seek(2, SeekOrigin.Begin));
            Assert.Equal(3, second.Seek(1, SeekOrigin.Current));
            Assert.Equal(4, second.Seek(-2, SeekOrigin.End));
            Assert.Equal(4, second.Position);
            Assert.Equal("ef", ReadAll(second));

            Assert.Throws<ArgumentException>(() => second.Seek(-1, SeekOrigin.Begin));
            Assert.Throws<ArgumentException>(() => second.Seek(7, SeekOrigin.Begin));
            Assert.Throws<ArgumentException>(() => second.Seek(1, SeekOrigin.Current));
            Assert.Throws<ArgumentException>(() => second.Seek(-7, SeekOrigin.Current));
            Assert.Throws<ArgumentException>(() => second.Seek(1, SeekOrigin.End));
            Assert.Throws<ArgumentException>(() => second.Seek(-7, SeekOrigin.End));
        }

        [Fact]
        public void StreamReader_PositionStaysInFile()
        {
            using var reader = CreateReader();
            var first = reader.Read("first");

            first.Position = 8;
            Assert.Equal("89", ReadAll(first));
            Assert.Throws<ArgumentException>(() => first.Position = -1);
            // 以前はパック全体の長さで確かめていたので、次のファイルの中まで動けた（その後の読み込みは例外になっていた）
            Assert.Throws<ArgumentException>(() => first.Position = 12);
        }

        [Fact]
        public void CheckHeaderStartIndex_FindsSignature()
        {
            var signature = PackUtility.FileSignature;

            Assert.Equal(-1, PackUtility.CheckHeaderStartIndex(null));
            Assert.Equal(-1, PackUtility.CheckHeaderStartIndex([1, 2]));
            Assert.Equal(2, PackUtility.CheckHeaderStartIndex([0, 0, .. signature, 0]));
            // 配列の最後で終わるときも見つける
            Assert.Equal(0, PackUtility.CheckHeaderStartIndex([.. signature]));
            Assert.Equal(1, PackUtility.CheckHeaderStartIndex([0, .. signature]));
            Assert.Equal(-1, PackUtility.CheckHeaderStartIndex([0, .. signature[..^1]]));
        }
    }

    public class PPDPackV2Tests
    {
        [Fact]
        public void WriteThenRead_KeepsFiles()
        {
            var stream = new MemoryStream();
            using (var writer = new PPDPack.V2.PackWriter(stream))
            {
                writer.Write("a", s => s.Write(Encoding.ASCII.GetBytes("hello"), 0, 5));
                writer.Write("empty", null);
                writer.Write("名前", s => s.Write(Encoding.ASCII.GetBytes("xy"), 0, 2));
            }
            var bytes = stream.ToArray();

            using var reader = new PPDPack.V2.PackReader(new MemoryStream(bytes));

            Assert.Equal(["a", "empty", "名前"], reader.Files);
            Assert.Equal((uint)bytes.Length, reader.FileSize);
            var buffer = new byte[10];
            Assert.Equal(5, reader.Read("a").Read(buffer, 0, 10));
            Assert.Equal("hello", Encoding.ASCII.GetString(buffer, 0, 5));
            Assert.Equal(0, reader.Read("empty").Length);
            Assert.Equal(2, reader.Read("名前").Read(buffer, 0, 10));
            // 以前はない名前で例外になっていた（null を返すはずだった）
            Assert.Null(reader.Read("missing"));
        }

        [Fact]
        public void File_WriteThenRead()
        {
            using var folder = new TempFolder();
            var path = folder.Combine("v2.pak");
            using (var writer = new PPDPack.V2.PackWriter(path))
            {
                writer.Write("x", s => s.WriteByte(7));
            }

            using var reader = new PPDPack.V2.PackReader(path);
            Assert.Equal(7, reader.Read("x").ReadByte());
        }

        [Fact]
        public void NotAPack_Throws()
        {
            using var folder = new TempFolder();
            folder.Write("bad.pak", "PPDPACKV1xxxxxxxxxx");
            folder.Write("short.pak", "PPD");
            var incomplete = PPDPack.V2.PackUtility.FileSignature.Concat(BitConverter.GetBytes(1000u)).Concat(BitConverter.GetBytes(0)).ToArray();

            Assert.Throws<ArgumentException>(() => new PPDPack.V2.PackReader(folder.Combine("missing.pak")));
            Assert.Throws<Exception>(() => new PPDPack.V2.PackReader(folder.Combine("bad.pak")));
            Assert.Throws<Exception>(() => new PPDPack.V2.PackReader(folder.Combine("short.pak")));
            Assert.Throws<Exception>(() => new PPDPack.V2.PackReader(new MemoryStream(incomplete)));
            File.Delete(folder.Combine("bad.pak"));
        }

        [Fact]
        public void CheckHeaderStartIndex_FindsSignature()
        {
            var signature = PPDPack.V2.PackUtility.FileSignature;

            Assert.Equal(-1, PPDPack.V2.PackUtility.CheckHeaderStartIndex(null));
            Assert.Equal(3, PPDPack.V2.PackUtility.CheckHeaderStartIndex([1, 2, 3, .. signature, 4]));
            Assert.Equal(0, PPDPack.V2.PackUtility.CheckHeaderStartIndex([.. signature]));
        }
    }
}
