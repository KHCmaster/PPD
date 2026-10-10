using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using Xunit;
using Utility = PPDEditor.Utility;

namespace UnitTests.PPDEditor
{
    /// <summary>
    /// エディターの小さな関数（フォルダーの写し、数の読み取り、範囲）のテストです。
    /// </summary>
    public class EditorUtilityTests
    {
        [Fact]
        public void CopyDirectory_SkipsExceptedFilesAndAsksBeforeOverwriting()
        {
            using var folder = new TempFolder();
            folder.Write(Path.Combine("src", "a.txt"), "new a");
            folder.Write(Path.Combine("src", "movie.mp4"), "movie");
            folder.Write(Path.Combine("src", "sub", "b.txt"), "new b");
            folder.Write(Path.Combine("dest", "a.txt"), "old a");
            folder.Write(Path.Combine("dest", "sub", "b.txt"), "old b");
            var asked = new List<string>();

            // 上書きしないと答える
            Utility.CopyDirectory(folder.Combine("src"), folder.Combine("dest"), [new Regex(@"\.mp4$")], path =>
            {
                asked.Add(Path.GetFileName(path));
                return false;
            });

            Assert.Equal("old a", File.ReadAllText(folder.Combine("dest", "a.txt")));
            // 下のフォルダーのファイルも聞く
            Assert.Equal("old b", File.ReadAllText(folder.Combine("dest", "sub", "b.txt")));
            Assert.Equal(["a.txt", "b.txt"], asked);
            Assert.False(File.Exists(folder.Combine("dest", "movie.mp4")));

            // 聞かないときは上書きする
            Utility.CopyDirectory(folder.Combine("src"), folder.Combine("dest") + Path.DirectorySeparatorChar, null);
            Assert.Equal("new b", File.ReadAllText(folder.Combine("dest", "sub", "b.txt")));
            Assert.True(File.Exists(folder.Combine("dest", "movie.mp4")));
        }

        [Fact]
        public void SafeCopyFile_CreatesFolders()
        {
            using var folder = new TempFolder();
            var source = folder.Write("a.txt", "a");

            Utility.SafeCopyFile(source, folder.Combine("x", "y", "a.txt"));

            Assert.Equal("a", File.ReadAllText(folder.Combine("x", "y", "a.txt")));
        }

        [Fact]
        public void ParseFloat_AndClamp()
        {
            Assert.Equal(1.5f, Utility.ParseFloat("1.5"));
            Assert.Throws<Exception>(() => Utility.ParseFloat("abc"));
            Assert.Equal((0, 5, 3), (Utility.Clamp(-1, 0, 5), Utility.Clamp(9, 0, 5), Utility.Clamp(3, 0, 5)));
            Assert.Equal((0f, 1f, 0.5f), (Utility.Clamp(-1f, 0f, 1f), Utility.Clamp(2f, 0f, 1f), Utility.Clamp(0.5f, 0f, 1f)));
        }
    }
}
