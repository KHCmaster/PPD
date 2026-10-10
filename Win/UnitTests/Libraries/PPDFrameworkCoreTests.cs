using PPDFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using Xunit;

namespace UnitTests.Libraries
{
    /// <summary>
    /// PPDFrameworkCore（ハッシュ、値のバインディング、タイマー、スレッド）のテストです。
    /// </summary>
    public class PPDFrameworkCoreTests
    {
        [Fact]
        public void Hashes()
        {
            Assert.Equal("900150983cd24fb0d6963f7d28e17f72", CryptographyUtility.CalcMd5HashString("abc"));
            Assert.Equal("ba7816bf8f01cfea414140de5dae2223b00361a396177a9cb410ff61f20015ad", CryptographyUtility.CalcSha256HashString("abc"));
            Assert.Equal(CryptographyUtility.CalcSha256HashString("譜面", Encoding.UTF8), CryptographyUtility.Getx2Encoding(CryptographyUtility.CalcSha256Hash("譜面", Encoding.UTF8)));
            Assert.Equal(CryptographyUtility.CalcMd5HashString("譜面", Encoding.UTF8), CryptographyUtility.Getx2Encoding(CryptographyUtility.CalcMd5Hash("譜面", Encoding.UTF8)));
            Assert.Equal(16, CryptographyUtility.CalcMd5Hash("abc").Length);
            Assert.Equal(32, CryptographyUtility.CalcSha256Hash("abc").Length);
        }

        [Fact]
        public void HexAndBase64()
        {
            var bytes = new byte[] { 0x00, 0xab, 0xff };

            Assert.Equal("00abff", CryptographyUtility.Getx2Encoding(bytes));
            Assert.Equal(bytes, CryptographyUtility.Parsex2String("00abff"));
            Assert.Equal(bytes, CryptographyUtility.Parsex2String("00ABFF"));
            Assert.Empty(CryptographyUtility.Parsex2String(""));
            Assert.Empty(CryptographyUtility.Parsex2String(null));
            Assert.Equal(bytes, CryptographyUtility.GetBytesFromBase64String(CryptographyUtility.GetBase64String(bytes)));
            Assert.Equal("", CryptographyUtility.GetBase64String(null));
        }

        class Source : IPropertyChanged
        {
            int value;
            public Source Child { get; set; }
            public string Name { get; set; }
            public int Value
            {
                get => value;
                set
                {
                    this.value = value;
                    PropertyChanged?.Invoke(nameof(Value));
                }
            }
            public void Rename(string name)
            {
                Name = name;
                PropertyChanged?.Invoke(nameof(Name));
            }
            public event PropertyChangedEventHandler PropertyChanged;
        }

        class Target
        {
            public object Value { get; set; }
            public Target Inner { get; set; }
        }

        [Fact]
        public void Binding_CopiesValueWhenChanged()
        {
            var source = new Source { Value = 1 };
            var target = new Target();

            var binding = new Binding(source, "Value", target, "Value");
            Assert.Equal(1, target.Value);

            source.Value = 2;
            Assert.Equal(2, target.Value);

            // 別のプロパティの変更では変えない
            source.Rename("x");
            Assert.Equal(2, target.Value);

            binding.Release();
            source.Value = 3;
            Assert.Equal(2, target.Value);
        }

        [Fact]
        public void Binding_FollowsPropertyPaths()
        {
            var source = new Source { Child = new Source { Name = "a" } };
            var target = new Target { Inner = new Target() };

            _ = new Binding(source, "Child.Name", target, "Inner.Value");
            Assert.Equal("a", target.Inner.Value);

            source.Child.Rename("b");
            Assert.Equal("b", target.Inner.Value);

            // null の値は写さない
            source.Child.Rename(null);
            Assert.Equal("b", target.Inner.Value);
        }

        [Fact]
        public void Binding_MissingPropertyDoesNothing()
        {
            var source = new Source { Value = 5 };
            var target = new Target();

            var binding = new Binding(source, "Missing.Value", target, "Nothing.Value");
            source.Value = 6;
            binding.Release();
            _ = new Binding(source, "Value", target, "Unknown");

            Assert.Null(target.Value);
        }

        [Fact]
        public void TimerManager_CallsRepeatedlyAndOnce()
        {
            var timer = new GameTimer();
            var manager = new TimerManager(timer);
            var repeated = new List<int>();
            var once = new List<int>();
            var removed = new List<int>();

            manager.AddTimerCallBack(repeated.Add, 20, false, false);
            manager.AddTimerCallBack(once.Add, 20, true, false);
            var id = manager.AddTimerCallBack(removed.Add, 20, false, false);
            // 足したものは次の Update から動く
            manager.Update();
            manager.RemoveTimerCallBack(id);
            Thread.Sleep(70);
            manager.Update();
            Thread.Sleep(30);
            manager.Update();

            Assert.True(repeated.Count >= 3, $"呼んだ回数: {repeated.Count}");
            // 何回目の呼び出しかを渡す
            Assert.Equal(0, repeated[0]);
            Assert.Equal(repeated.Count - 1, repeated[^1]);
            Assert.Equal([0], once);
            Assert.Empty(removed);
            Assert.True(timer.ElapsedTime >= 100);
            Assert.True(timer.ElapsedTickTime >= timer.ElapsedTime);
        }

        [Fact]
        public void TimerManager_ImmediateStartsFromZero()
        {
            var timer = new GameTimer();
            Thread.Sleep(30);
            var manager = new TimerManager(timer);
            var calls = new List<int>();

            // immediate は時刻 0 から数えるので、すでに過ぎた分もすぐに呼ぶ
            manager.AddTimerCallBack(calls.Add, 10, false, true);
            manager.Update();
            manager.Update();

            Assert.True(calls.Count >= 3, $"呼んだ回数: {calls.Count}");
        }

        [Fact]
        public void ThreadManager_KeepsUntilFinished()
        {
            using var finished = new ManualResetEventSlim();
            object received = null;
            var thread = ThreadManager.Instance.GetThread(() => finished.Set());
            var withParameter = ThreadManager.Instance.GetThread(p => received = p);

            Assert.True(thread.IsBackground);
            thread.Start();
            withParameter.Start("x");
            Assert.True(finished.Wait(5000));
            thread.Join();
            withParameter.Join();
            ThreadManager.Instance.Update();

            Assert.Equal("x", received);
        }
    }
}
