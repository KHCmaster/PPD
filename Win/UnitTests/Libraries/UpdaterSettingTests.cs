using PPDUpdater;
using System.IO;
using Xunit;

namespace UnitTests.Libraries
{
    /// <summary>
    /// PPDUpdater の設定の読み書き（PPDConfiguration と同じ形）のテストです。本物の PPDUpdater.ini は使わない。
    /// </summary>
    public sealed class UpdaterSettingTests : System.IDisposable
    {
        readonly string path = Path.GetTempFileName();

        public void Dispose()
        {
            File.Delete(path);
        }

        [Fact]
        public void Reader_NormalFormat()
        {
            var reader = new SettingReader("[a]1\r\n[b] text \r[multi]line1\nline2\n[empty]\n[dup]x\n[dup]y\nno key line\n");

            Assert.False(reader.IsIniFormat);
            Assert.Equal("1", reader["a"]);
            // 前後の空白は除く
            Assert.Equal("text", reader["b"]);
            Assert.Equal("line1", reader["multi"]);
            Assert.Equal("", reader["empty"]);
            Assert.Equal("y", reader["dup"]);
            Assert.Equal("", reader["missing"]);
            Assert.True(reader.ContainsKey("a"));
            Assert.False(reader.ContainsKey("missing"));
        }

        [Fact]
        public void Reader_IniFormat()
        {
            var reader = new SettingReader("[formattype]\nisini=1\n[setting]\n# comment\nkey = value with spaces\nnum=3\nbroken line\n");

            Assert.True(reader.IsIniFormat);
            Assert.Equal("value with spaces", reader["key"]);
            Assert.Equal(3, reader.ReadInt("num", 0));
            Assert.False(reader.ContainsKey("# comment"));
        }

        [Fact]
        public void Reader_TypedValuesAndChanges()
        {
            var reader = new SettingReader("[i]42\n[f]1.5\n[d]2.25\n[b]1\n[nb]0\n[bad]x\n");

            Assert.Equal(42, reader.ReadInt("i", -1));
            Assert.Equal(-1, reader.ReadInt("bad", -1));
            Assert.Equal(1.5f, reader.ReadFloat("f", 0));
            Assert.Equal(9f, reader.ReadFloat("bad", 9));
            Assert.Equal(2.25, reader.ReadDouble("d", 0));
            Assert.Equal(8.0, reader.ReadDouble("bad", 8));
            Assert.True(reader.ReadBoolean("b"));
            Assert.False(reader.ReadBoolean("nb"));

            reader["new"] = "v";
            reader.ReplaceOrAdd("i", 7);
            reader.ReplaceOrAdd("f", 0.5f);
            reader.ReplaceOrAdd("d", 0.25);
            reader.ReplaceOrAdd("b", false);
            Assert.Equal("v", reader["new"]);
            Assert.Equal(7, reader.ReadInt("i", -1));
            Assert.Equal(0.5f, reader.ReadFloat("f", 0));
            Assert.Equal(0.25, reader.ReadDouble("d", 0));
            Assert.False(reader.ReadBoolean("b"));
            Assert.Equal(7, reader.Dictionary.Count);
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public void Writer_ThenReader(bool ini)
        {
            using (var writer = new SettingWriter(path, ini))
            {
                writer.Write("s", "text");
                writer.Write("i", 3);
                writer.Write("f", 1.5f);
                writer.Write("d", 2.5);
                writer.Write("b", true);
                writer.Write("s", "again");
            }

            var reader = new SettingReader(File.ReadAllText(path));

            Assert.Equal(ini, reader.IsIniFormat);
            // ini はキーを上書き、そうでなければ後の行が勝つ
            Assert.Equal("again", reader["s"]);
            Assert.Equal(3, reader.ReadInt("i", 0));
            Assert.Equal(1.5f, reader.ReadFloat("f", 0));
            Assert.Equal(2.5, reader.ReadDouble("d", 0));
            Assert.True(reader.ReadBoolean("b"));
        }

        [Fact]
        public void Writer_CloseTwiceIsSafe()
        {
            var writer = new SettingWriter(path, true);
            writer.Write("k", "v");
            writer.Close();
            writer.Close();
            writer.Write("after", "x");
            writer.Dispose();
            writer.Dispose();

            Assert.Contains("k=v", File.ReadAllText(path));
            Assert.True(writer.disposed);
        }
    }
}
