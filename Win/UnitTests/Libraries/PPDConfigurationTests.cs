using PPDConfiguration;
using System;
using System.IO;
using Xunit;

namespace UnitTests.Libraries
{
    public class SettingReaderTests
    {
        enum Mode
        {
            First = 1,
            Second = 2,
        }

        [Fact]
        public void Normal_ReadsKeysAndMultiLineValues()
        {
            var reader = new SettingReader("[a]1\r\n[b] text \r[multi]line1\nline2\n[empty]\n[dup]x\n[dup]y\nno key line\n");

            Assert.False(reader.IsIniFormat);
            Assert.Equal("1", reader["a"]);
            // 値の前後の空白は取る
            Assert.Equal("text", reader["b"]);
            Assert.Equal("line1\nline2", reader["multi"]);
            Assert.Equal("", reader["empty"]);
            Assert.Equal("y\nno key line", reader["dup"]);
            Assert.Equal("", reader["missing"]);
            Assert.False(reader.ContainsKey("missing"));
            Assert.True(reader.ContainsKey("empty"));
        }

        [Fact]
        public void Ini_ReadsKeyValuesAndSkipsCommentsAndSections()
        {
            var reader = new SettingReader("[formattype]\nisini=1\n[setting]\n# comment\nkey = value with spaces\nnum=3\nbroken line\n");

            Assert.True(reader.IsIniFormat);
            Assert.Equal("value with spaces", reader["key"]);
            Assert.Equal(3, reader.ReadInt("num", 0));
            Assert.Equal("1", reader["isini"]);
            Assert.False(reader.ContainsKey("broken line"));
            Assert.False(reader.ContainsKey("# comment"));
        }

        [Fact]
        public void TypedReads_UseDefaultsWhenInvalid()
        {
            var reader = new SettingReader("[i]42\n[f]1.5\n[d]2.25\n[b]1\n[nb]0\n[bad]x\n[mode]2\n[unknown]9\n");

            Assert.Equal((42, 7), (reader.ReadInt("i", 0), reader.ReadInt("bad", 7)));
            Assert.Equal((1.5f, 3f), (reader.ReadFloat("f", 0), reader.ReadFloat("bad", 3)));
            Assert.Equal((2.25, 4.0), (reader.ReadDouble("d", 0), reader.ReadDouble("bad", 4)));
            Assert.True(reader.ReadBoolean("b"));
            Assert.False(reader.ReadBoolean("nb") || reader.ReadBoolean("missing"));
            Assert.Equal(Mode.Second, reader.ReadEnum("mode", Mode.First));
            Assert.Equal(Mode.First, reader.ReadEnum("unknown", Mode.First));
            Assert.Equal(Mode.Second, reader.ReadEnum("missing", Mode.Second));
        }

        [Fact]
        public void ReplaceOrAdd_ConvertsInvariant()
        {
            var reader = new SettingReader("");

            reader.ReplaceOrAdd("i", 5);
            reader.ReplaceOrAdd("f", 0.5f);
            reader.ReplaceOrAdd("d", 1.25);
            reader.ReplaceOrAdd("t", true);
            reader.ReplaceOrAdd("i", 6);
            reader["s"] = "text";

            Assert.Equal(("6", "0.5", "1.25", "1", "text"), (reader["i"], reader["f"], reader["d"], reader["t"], reader["s"]));
            Assert.Equal(5, reader.Dictionary.Count);
        }
    }

    public class SettingWriterTests
    {
        [Fact]
        public void Normal_WritesAndReadsBack()
        {
            using var folder = new TempFolder();
            var path = folder.Combine("a.ini");

            using (var writer = new SettingWriter(path, false))
            {
                writer.Write("s", "text");
                writer.Write("i", 3);
                writer.Write("f", 1.5f);
                writer.Write("d", 2.5);
                writer.Write("b", false);
                writer.Write("multi", "l1\nl2");
            }

            Assert.StartsWith("[s]text\r\n[i]3\r\n", File.ReadAllText(path));
            var reader = new SettingReader(File.ReadAllText(path));
            Assert.Equal(("text", "3", "1.5", "2.5", "0", "l1\nl2"), (reader["s"], reader["i"], reader["f"], reader["d"], reader["b"], reader["multi"]));
        }

        [Fact]
        public void Ini_WritesSectionsAndReadsBack()
        {
            using var folder = new TempFolder();
            var path = folder.Combine("a.ini");

            var writer = new SettingWriter(path, true);
            writer.Write("key", "first");
            writer.Write("key", "second");
            writer.Write("other", 1);
            writer.Close();
            writer.Close();
            writer.Write("after", "ignored");
            writer.Dispose();
            writer.Dispose();

            var text = File.ReadAllText(path);
            Assert.Equal(string.Join(Environment.NewLine, "[formattype]", "isini=1", "[setting]", "key=second", "other=1", ""), text);
            var reader = new SettingReader(text);
            Assert.True(reader.IsIniFormat);
            Assert.Equal(("second", "1"), (reader["key"], reader["other"]));
            Assert.True(writer.disposed);
        }
    }

    public class LanguageReaderTests
    {
        [Fact]
        public void FallsBackThroughAppendedAndEnglishFiles()
        {
            using var folder = new TempFolder(setCurrentDirectory: true);
            folder.Write(Path.Combine("Lang", "lang_Test_jp.ini"), "[a]jp a\n[b]jp b\n");
            folder.Write(Path.Combine("Lang", "lang_Test_jp_extra.ini"), "[a]jp extra a\n");
            folder.Write(Path.Combine("Lang", "lang_Test_en.ini"), "[b]en b\n[c]en c\n[d]en d\n");
            folder.Write(Path.Combine("Lang", "lang_Test_en_extra.ini"), "[c]en extra c\n");

            var reader = new LanguageReader("Test", "jp");

            // 追加の言語ファイル、言語ファイル、英語の追加、英語の順に探す
            Assert.Equal("jp extra a", reader["a"]);
            Assert.Equal("jp b", reader["b"]);
            Assert.Equal("en extra c", reader["c"]);
            Assert.Equal("en d", reader.ReadValue("d"));
            Assert.Equal("", reader["none"]);
        }

        [Fact]
        public void MissingFiles_ReturnEmpty()
        {
            using var folder = new TempFolder(setCurrentDirectory: true);

            var reader = new LanguageReader("Test", "kr");

            Assert.Equal("", reader["a"]);
        }
    }
}
