using MessagePack;
using PPDMultiCommon.Data;
using PPDMultiCommon.Model;
using PPDMultiCommon.Tcp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using Xunit;

namespace UnitTests.Libraries
{
    /// <summary>
    /// マルチプレイで送る値（NetworkData）と、受け取ったバイト列から値に戻す TcpByteReader のテストです。ネットワークは使わない。
    /// </summary>
    public class MultiNetworkTests
    {
        static int counter;

        // MessagePack で送るプロパティ（Key の付いたもの）に、型に合う値を入れる
        static object Sample(Type type)
        {
            counter++;
            if (type == typeof(int)) return counter;
            if (type == typeof(long)) return (long)counter * 1000000000L;
            if (type == typeof(float)) return counter + 0.5f;
            if (type == typeof(bool)) return true;
            if (type == typeof(string)) return "文字" + counter;
            if (type.IsEnum)
            {
                var values = Enum.GetValues(type);
                return values.GetValue(values.Length - 1);
            }
            if (type.IsArray)
            {
                var array = Array.CreateInstance(type.GetElementType(), 2);
                for (var i = 0; i < array.Length; i++)
                {
                    array.SetValue(Sample(type.GetElementType()), i);
                }
                return array;
            }
            if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Tuple<,>))
            {
                var arguments = type.GetGenericArguments();
                return Activator.CreateInstance(type, Sample(arguments[0]), Sample(arguments[1]));
            }
            if (type.GetCustomAttribute<MessagePackObjectAttribute>() != null)
            {
                return Fill(Activator.CreateInstance(type));
            }
            if (type == typeof(byte))
            {
                return (byte)counter;
            }
            throw new NotSupportedException(type.FullName);
        }

        static T Fill<T>(T value)
        {
            foreach (var property in value.GetType().GetProperties().Where(p => p.GetCustomAttribute<KeyAttribute>() != null && p.CanWrite && p.Name != nameof(NetworkData.MethodType)))
            {
                property.SetValue(value, Sample(property.PropertyType));
            }
            return value;
        }

        // Parser が読める種類（MethodType ごとに 1 つ）
        public static TheoryData<Type> MessageTypes()
        {
            var data = new TheoryData<Type>();
            foreach (var type in typeof(NetworkData).Assembly.GetTypes().Where(t => t.IsSubclassOf(typeof(NetworkData)) && !t.IsAbstract))
            {
                var instance = (NetworkData)Activator.CreateInstance(type);
                if (instance.MethodType != default || type == typeof(AddUserNetworkData))
                {
                    data.Add(type);
                }
            }
            return data;
        }

        static List<NetworkData> Read(TcpByteReader reader, byte[] bytes, int id = 0)
        {
            var received = new List<NetworkData>();
            void Handler(ReadInfo info, int from)
            {
                Assert.Equal(id, from);
                received.Add(info.NetworkData);
            }
            reader.ByteReaded += Handler;
            reader.Read(bytes, id);
            reader.ByteReaded -= Handler;
            return received;
        }

        static byte[] Serialize(NetworkData data) => MessagePackSerializer.Serialize(data.GetType(), data);

        [Theory]
        [MemberData(nameof(MessageTypes))]
        public void EveryMessage_IsReadBack(Type type)
        {
            var sent = Fill((NetworkData)Activator.CreateInstance(type));
            var bytes = Serialize(sent);

            var received = Read(new TcpByteReader(), bytes);

            var data = Assert.Single(received);
            Assert.IsType(type, data);
            Assert.Equal(bytes, Serialize(data));
        }

        [Fact]
        public void SeveralMessagesInOneRead()
        {
            var bytes = Serialize(new ChangeScoreNetworkData { Score = 10, Id = 1 })
                .Concat(Serialize(new AddMessageNetworkData { Message = "こんにちは", Id = 2 }))
                .Concat(Serialize(new ChangeLifeNetworkData { Life = 50, Id = 3 })).ToArray();

            var received = Read(new TcpByteReader(), bytes);

            Assert.Equal([1, 2, 3], received.Select(d => d.Id));
            Assert.Equal("こんにちは", ((AddMessageNetworkData)received[1]).Message);
        }

        [Fact]
        public void SplitMessage_WaitsForRest()
        {
            var reader = new TcpByteReader();
            var bytes = Serialize(new AddMessageNetworkData { Message = new string('あ', 100), Id = 7 });

            Assert.Empty(Read(reader, bytes[..20], id: 1));
            // 別の接続の途中のデータとは混ぜない
            Assert.Empty(Read(reader, bytes[..10], id: 2));
            var received = Read(reader, bytes[20..], id: 1);

            Assert.Equal(new string('あ', 100), ((AddMessageNetworkData)Assert.Single(received)).Message);
            Assert.Single(Read(reader, bytes[10..], id: 2));
        }

        [Fact]
        public void OldProtocol_IsDetected()
        {
            var bytes = Encoding.ASCII.GetBytes("PPDPACK and more");

            var data = Assert.Single(Read(new TcpByteReader(), bytes));

            Assert.IsType<OldProtocolNetworkData>(data);
        }

        [Fact]
        public void UnknownMessage_IsSkipped()
        {
            // 知らない種類のあとのデータも読める（以前は例外になり、受け取るスレッドごと止まっていた）
            var bytes = Serialize(Fill(new CloseNetworkData { Id = 5 }))
                .Concat(Serialize(new ChangeScoreNetworkData { Score = 1, Id = 6 })).ToArray();

            var received = Read(new TcpByteReader(), bytes);

            Assert.Equal(6, Assert.Single(received).Id);
        }

        [Fact]
        public void NotMessagePack_IsKeptUntilMoreData()
        {
            var reader = new TcpByteReader();

            Assert.Empty(Read(reader, [0xc1]));
            Assert.Empty(Read(reader, [0xc1]));
        }
    }
}
