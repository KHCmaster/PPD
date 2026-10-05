using PPDFramework;
using System;
using System.Collections.Generic;
using System.Reflection;
using Xunit;

namespace UnitTests.Libraries
{
    /// <summary>
    /// PPDFramework の、描画を使わない小さなクラス（起動引数、配列の木、動画の切り取り、メニューの入力）のテストです。
    /// </summary>
    public class PPDFrameworkUtilityTests
    {
        [Fact]
        public void ExecuteArg_ReadsKeysAndValues()
        {
            var args = new PPDExecuteArg(["-a", "1", "-flag", "-b", "-0.5", "loose", "-c"]);

            Assert.Equal(4, args.Count);
            Assert.Equal("1", args["a"]);
            // 次も「-」で始まるキーなら、値は空
            Assert.Equal("", args["flag"]);
            // 負の数は値
            Assert.Equal("-0.5", args["b"]);
            Assert.Equal("", args["c"]);
            Assert.True(args.ContainsKey("flag"));
            Assert.False(args.ContainsKey("loose"));
            Assert.Throws<Exception>(() => args.GetValue("missing"));
        }

        [Fact]
        public void ExecuteArg_DuplicatesAndEmpty()
        {
            var args = new PPDExecuteArg(["value", "-k", "1", "-k", "2"]);

            Assert.Equal("2", args["k"]);
            Assert.Equal(1, args.Count);
            Assert.Equal(0, PPDExecuteArg.Empty.Count);
            Assert.Empty(PPDExecuteArg.Empty.Args);
        }

        // internal なクラスなので、名前で作って使う
        sealed class Tree
        {
            static readonly Type type = typeof(PPDExecuteArg).Assembly.GetType("PPDFramework.ArrayTree`2", true).MakeGenericType(typeof(byte), typeof(string));
            readonly object tree = Activator.CreateInstance(type, true);

            public void Add(byte[] key, string value) => type.GetMethod("Add", [typeof(byte[]), typeof(string)]).Invoke(tree, [key, value]);

            public void Remove(byte[] key) => type.GetMethod("Remove").Invoke(tree, [key]);

            public string Find(byte[] key) => (string)type.GetMethod("Find").Invoke(tree, [key]);
        }

        [Fact]
        public void ArrayTree_AddFindAndRemove()
        {
            var tree = new Tree();
            tree.Add([1, 2, 3], "a");
            tree.Add([1, 2], "b");
            tree.Add([1, 9], "c");

            Assert.Equal("a", tree.Find([1, 2, 3]));
            Assert.Equal("b", tree.Find([1, 2]));
            Assert.Null(tree.Find([1]));
            Assert.Null(tree.Find([1, 2, 4]));

            // 消すとその下も消え、ほかは残る
            tree.Remove([1, 2]);
            Assert.Null(tree.Find([1, 2]));
            Assert.Null(tree.Find([1, 2, 3]));
            Assert.Equal("c", tree.Find([1, 9]));
            tree.Remove([7]);
            tree.Remove([1, 9]);
            Assert.Null(tree.Find([1, 9]));

            tree.Add([1, 9], "d");
            Assert.Equal("d", tree.Find([1, 9]));
        }

        [Theory]
        [InlineData(0.1f, 1000f, 100f)]
        [InlineData(-0.5f, 100f, -50f)]
        [InlineData(20f, 1000f, 20f)]
        [InlineData(1f, 1000f, 1f)]
        public void MovieTrimming_RatioOrPixels(float trim, float size, float expected)
        {
            var data = new MovieTrimmingData(trim, trim, trim, trim);

            Assert.Equal(expected, data.GetTopTrimming(size), 3);
            Assert.Equal(expected, data.GetLeftTrimming(size), 3);
            Assert.Equal(expected, data.GetRightTrimming(size), 3);
            Assert.Equal(expected, data.GetBottomTrimming(size), 3);
        }

        sealed class FakeInput() : InputInfoBase(true)
        {
            public Dictionary<ButtonType, int> Frames { get; } = [];
            public HashSet<ButtonType> Pressed { get; } = [];
            public HashSet<ButtonType> Released { get; } = [];
            public override int GetPressingFrame(ButtonType buttonType) => Frames.GetValueOrDefault(buttonType);
            public override bool IsPressed(ButtonType buttonType) => Pressed.Contains(buttonType);
            public override bool IsReleased(ButtonType buttonType) => Released.Contains(buttonType);
        }

        [Fact]
        public void MenuInput_RepeatsHeldDirections()
        {
            var input = new FakeInput();
            var menu = new MenuInputInfo(input);
            input.Released.Add(ButtonType.Circle);
            input.Pressed.Add(ButtonType.Cross);

            Assert.True(menu.Accurate);
            Assert.True(menu.IsReleased(ButtonType.Circle));
            Assert.True(menu.IsPressed(ButtonType.Cross));
            Assert.False(menu.IsPressed(ButtonType.Down));
            Assert.Empty(menu.Actions);

            // 方向キーを押し続けると、21 フレームを過ぎてから 5 フレームごとに押したことにする
            input.Frames[ButtonType.Down] = 19;
            Assert.False(menu.IsPressed(ButtonType.Down));
            input.Frames[ButtonType.Down] = 24;
            Assert.True(menu.IsPressed(ButtonType.Down));
            Assert.Equal(1, menu.GetPressingInterval(ButtonType.Down));
            input.Frames[ButtonType.Down] = 25;
            Assert.False(menu.IsPressed(ButtonType.Down));
            Assert.Equal(1, menu.GetPressingInterval(ButtonType.Down));
            // 長く押すほど間隔（何個ずつ進むか）が大きい
            input.Frames[ButtonType.Down] = 64;
            Assert.Equal(3, menu.GetPressingInterval(ButtonType.Down));
            Assert.Equal(64, menu.GetPressingFrame(ButtonType.Down));

            // 方向キー以外はくり返さない
            input.Frames[ButtonType.Circle] = 24;
            Assert.False(menu.IsPressed(ButtonType.Circle));
            Assert.Equal(1, menu.GetPressingInterval(ButtonType.Circle));
        }
    }
}
