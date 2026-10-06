using PPDEditor;
using PPDEditor.FlowScript;
using System.IO;
using System.Linq;
using System.Numerics;
using Xunit;

namespace UnitTests.PPDEditor
{
    /// <summary>
    /// マークの位置と角度の並び（posdat の .txt）の読み書きのテストです。
    /// </summary>
    public class PosAndAngleInfoTests
    {
        [Fact]
        public void PositionsAndAngles_RoundTrip()
        {
            using var folder = new TempFolder();
            var path = folder.Combine("both.txt");

            PosAndAngleInfo.Save([new PosAndAngleInfo(new Vector2(1.5f, 2), 0.25f), new PosAndAngleInfo(new Vector2(3, 4), 1)], path);
            var loaded = PosAndAngleInfo.Load(path);

            Assert.Equal("x,y,angle", File.ReadLines(path).First());
            Assert.Equal([new Vector2(1.5f, 2), new Vector2(3, 4)], loaded.Select(i => i.Position.Value));
            Assert.Equal([0.25f, 1f], loaded.Select(i => i.Rotation.Value));
        }

        [Fact]
        public void PositionsOnly_RoundTrip()
        {
            using var folder = new TempFolder();
            var path = folder.Combine("positions.txt");

            PosAndAngleInfo.Save([new PosAndAngleInfo(new Vector2(1, 2)), new PosAndAngleInfo(new Vector2(3, 4))], path);
            var loaded = PosAndAngleInfo.Load(path);

            Assert.Equal("x,y", File.ReadLines(path).First());
            Assert.Equal([new Vector2(1, 2), new Vector2(3, 4)], loaded.Select(i => i.Position.Value));
            Assert.All(loaded, i => Assert.Null(i.Rotation));
        }

        [Fact]
        public void AnglesOnly_RoundTrip()
        {
            using var folder = new TempFolder();
            var path = folder.Combine("angles.txt");

            PosAndAngleInfo.Save([new PosAndAngleInfo(0.5f), new PosAndAngleInfo(1.5f)], path);
            var loaded = PosAndAngleInfo.Load(path);

            // 最後の改行で、角度 0 のものが増えない
            Assert.Equal([0.5f, 1.5f], loaded.Select(i => i.Rotation.Value));
            Assert.All(loaded, i => Assert.Null(i.Position));
        }

        [Fact]
        public void Load_ColumnsInAnyOrderAndBrokenValues()
        {
            using var folder = new TempFolder();
            // 見出しより多い列は無視する
            var path = folder.Write("any.txt", "angle,y,x\r\n1,20,10\r\nbad,5,6,99\r\n\r\n");

            var loaded = PosAndAngleInfo.Load(path);

            Assert.Equal([new Vector2(10, 20), new Vector2(6, 5)], loaded.Select(i => i.Position.Value));
            // 読めない値は 0
            Assert.Equal([1f, 0f], loaded.Select(i => i.Rotation.Value));
            Assert.Throws<InvalidDataException>(() => PosAndAngleInfo.Load(folder.Write("header.txt", "x,y")));
        }

        [Fact]
        public void Loader_ReadsFromPosdatFolder()
        {
            using var folder = new TempFolder(setCurrentDirectory: true);
            folder.Write(Path.Combine("posdat", "circle.txt"), "x,y\n1,2\n");

            // 拡張子は .txt にする。ないファイルは null
            var loaded = PosAndAngleLoader.Instance.Load("circle.dat");
            Assert.Equal(new Vector2(1, 2), Assert.Single(loaded).Position);
            Assert.Null(PosAndAngleLoader.Instance.Load("missing"));
        }
    }
}
