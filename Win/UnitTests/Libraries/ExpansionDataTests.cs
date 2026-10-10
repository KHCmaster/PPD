using PPDExpansionCore;
using System;
using System.IO;
using System.Linq;
using System.Reflection;
using Xunit;

namespace UnitTests.Libraries
{
    /// <summary>
    /// 拡張機能（PPDExpansion）へ送る値（PackableBase とその種類）を、PPDPack に書いて読み戻すテストです。ネットワークは使わない。
    /// </summary>
    public class ExpansionDataTests
    {
        static int counter;

        static object Sample(Type type)
        {
            counter++;
            if (type == typeof(int)) return counter;
            if (type == typeof(float)) return counter + 0.25f;
            if (type == typeof(bool)) return true;
            if (type == typeof(byte)) return (byte)counter;
            if (type == typeof(string)) return "文字" + counter;
            if (type.IsEnum)
            {
                var values = Enum.GetValues(type);
                return values.GetValue(values.Length - 1);
            }
            if (type == typeof(byte[])) return new byte[] { 1, 2, (byte)counter };
            throw new NotSupportedException(type.FullName);
        }

        static PropertyInfo[] Stored(Type type) =>
            [.. type.GetProperties().Where(p => p.CanWrite && p.GetCustomAttribute<IgnoreAttribute>() == null)];

        public static TheoryData<DataType> DataTypes() => [.. Enum.GetValues<DataType>()];

        [Theory]
        [MemberData(nameof(DataTypes))]
        public void WriteThenRead_KeepsValues(DataType dataType)
        {
            var written = PackableBase.Create(dataType);
            Assert.Equal(dataType, written.DataType);
            foreach (var property in Stored(written.GetType()))
            {
                property.SetValue(written, Sample(property.PropertyType));
            }
            var stream = new MemoryStream();
            using (var writer = new PPDPack.V2.PackWriter(stream))
            {
                written.Write(writer);
            }

            var read = PackableBase.Create(dataType);
            using (var reader = new PPDPack.V2.PackReader(new MemoryStream(stream.ToArray())))
            {
                read.Read(reader);
            }

            var properties = Stored(written.GetType());
            Assert.NotEmpty(properties);
            foreach (var property in properties)
            {
                Assert.Equal(property.GetValue(written), property.GetValue(read));
            }
        }

        [Fact]
        public void ScoreInfo_DerivedValues()
        {
            var info = new ScoreInfo { ScoreHash = [0x0a, 0xff], StartTime = 1.5f, EndTime = 4 };

            Assert.Equal("0AFF", info.ScoreHashAsString);
            Assert.Equal(2.5f, info.Length);
        }
    }
}
