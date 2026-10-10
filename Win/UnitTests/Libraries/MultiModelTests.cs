using PPDFrameworkCore;
using PPDMultiCommon.Data;
using PPDMultiCommon.Model;
using PPDMultiCommon.Web;
using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace UnitTests.Libraries
{
    /// <summary>
    /// マルチプレイの画面で使う値（ユーザー、プレイの状態、メッセージ、部屋、譜面の情報）のテストです。
    /// </summary>
    public class MultiModelTests
    {
        static object Different(Type type, object current)
        {
            if (type == typeof(int)) return (int)current + 1;
            if (type == typeof(bool)) return !(bool)current;
            if (type == typeof(string)) return current + "x";
            if (type.IsEnum)
            {
                var values = Enum.GetValues(type).Cast<object>().ToArray();
                return values.First(v => !v.Equals(current));
            }
            if (type == typeof(UserInfo)) return new UserInfo();
            return null;
        }

        public static TheoryData<Type> Models() => [typeof(UserInfo), typeof(UserPlayState), typeof(PPDMultiCommon.Model.Message)];

        [Theory]
        [MemberData(nameof(Models))]
        public void Properties_NotifyChanges(Type type)
        {
            var model = Activator.CreateInstance(type);
            var changed = new List<string>();
            ((IPropertyChanged)model).PropertyChanged += changed.Add;
            var count = 0;

            foreach (var property in type.GetProperties().Where(p => p.CanWrite && p.CanRead))
            {
                var value = Different(property.PropertyType, property.GetValue(model));
                if (value == null)
                {
                    continue;
                }
                changed.Clear();
                property.SetValue(model, value);
                Assert.Equal(value, property.GetValue(model));
                count++;
                // 通知しない（画面に出さない）プロパティもある
                if (changed.Count > 0)
                {
                    Assert.Contains(property.Name, changed);
                }
            }

            Assert.True(count >= 3, $"確かめたプロパティ: {count}");
        }

        [Fact]
        public void UserPlayState_Life()
        {
            var state = new UserPlayState();
            var changed = new List<string>();
            state.PropertyChanged += changed.Add;

            Assert.Equal(1, state.Rank);
            Assert.Equal(511f / 1023 * 100, state.LifeAsFloat, 3);
            state.Life = 1023;
            state.Life = 1023;

            Assert.Equal(100f, state.LifeAsFloat);
            Assert.Equal(["Life", "LifeAsFloat"], changed);
        }

        [Fact]
        public void RoomInfo_FillsDefaultsAndHashesPassword()
        {
            var room = new RoomInfo(" ", "", "secret", 54321);

            Assert.Equal("NONAME", room.UserName);
            Assert.Equal("NOROOMNAME", room.RoomName);
            Assert.True(room.HasPassword);
            Assert.Equal(WebManager.GetPasswordHash("secret"), room.PasswordHash);
            Assert.NotEqual("secret", room.PasswordHash);
            Assert.Equal(64, room.PasswordHash.Length);
            Assert.Equal(54321, room.Port);
            Assert.False(string.IsNullOrEmpty(room.Language));

            var open = new RoomInfo("user", "room", "", 1);
            Assert.False(open.HasPassword);
            Assert.Equal(("user", "room"), (open.UserName, open.RoomName));

            var listed = new RoomInfo("u", "r", "hash", 2, "jpn", "1.2.3.4", 3);
            Assert.Equal(("hash", "jpn", "1.2.3.4", 3), (listed.PasswordHash, listed.Language, listed.IP, listed.PlayerCount));
        }

        [Fact]
        public void ScoreInfo_ParsesHashes()
        {
            var hash = new string('a', 64);
            var info = new PPDScoreInfo { Title = "曲", EasyHash = hash, NormalHash = "", HardHash = hash, Hash = "ABCDEF", MovieUrl = "m" };

            Assert.Equal(Enumerable.Repeat((byte)0xaa, 32), info.Easy);
            Assert.Empty(info.Normal);
            Assert.Same(info.Hard, info.Hard);
            Assert.Empty(info.Extreme);
            Assert.EndsWith("/score/index/id/abcdef", info.CreateUrl());
            Assert.Equal(("曲", "m"), (info.Title, info.MovieUrl));
        }
    }
}
