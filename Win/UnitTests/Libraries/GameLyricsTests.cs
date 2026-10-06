using PPDCore;
using PPDFramework;
using System.Collections.Generic;
using Xunit;

namespace UnitTests.Libraries
{
    /// <summary>
    /// ゲームで歌詞（kasi.txt）を時間に合わせて出す KasiManager のテストです。
    /// </summary>
    public class GameLyricsTests
    {
        static KasiManager Create(TempFolder folder)
        {
            var song = new SongInformation();
            typeof(SongInformation).GetProperty(nameof(SongInformation.DirectoryPath)).SetValue(song, folder.Path);
            return new KasiManager(new PPDGameUtility { SongInformation = song });
        }

        [Fact]
        public void Lyrics_ChangeWithTime()
        {
            using var folder = new TempFolder();
            folder.Write("kasi.txt", "2:second\r\n1:Re:Start\r\nbroken\r\n3\r\n");
            var lyrics = Create(folder);
            var changed = new List<string>();
            lyrics.KasiChanged += changed.Add;

            lyrics.Update(0.5f);
            Assert.Empty(changed);
            lyrics.Update(1);
            // 歌詞の中の「:」で切らない
            Assert.Equal("Re:Start", lyrics.Lylics);
            lyrics.Update(3.5f);
            Assert.Equal(["Re:Start", "second", ""], changed);

            // 途中から始めると、その時の歌詞
            changed.Clear();
            lyrics.Seek(2.5f);
            Assert.Equal(["", "Re:Start", "second"], changed);
            // 使う文字（文字のテクスチャを先に作るため）
            Assert.Equal(new HashSet<char>("Re:Startsecond"), new HashSet<char>(lyrics.LyricCharacters));
        }

        [Fact]
        public void WithoutFile_NothingChanges()
        {
            using var folder = new TempFolder();
            var lyrics = Create(folder);
            var changed = new List<string>();
            lyrics.KasiChanged += changed.Add;

            lyrics.Update(10);

            Assert.Empty(changed);
            Assert.Equal("", lyrics.LyricCharacters);
        }
    }
}
