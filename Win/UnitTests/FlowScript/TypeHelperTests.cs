using FlowScriptEngine;
using PPDFramework;
using PPDFramework.PPDStructure.PPDData;
using PPDFramework.ScreenFilters;
using PPDFramework.Shaders;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Xml.Linq;
using Xunit;

namespace UnitTests.FlowScript
{
    /// <summary>
    /// エディターとデバッガーが使う、型の別名（TypeAlias）、色（TypeColors）、値の書き出し（TypeSerializer）のテストです。
    /// </summary>
    public class TypeHelperTests
    {
        static IEnumerable<Type> Subclasses<T>()
        {
            return NodeEventTests.NodeTypes().Select(t => t.Assembly).Distinct().SelectMany(a => a.GetTypes())
                .Where(t => t.IsPublic && t.IsClass && !t.IsAbstract && t.IsSubclassOf(typeof(T)));
        }

        [Fact]
        public void TypeAliases_AreUniqueAndNamed()
        {
            var aliases = Subclasses<TypeAliasBase>().SelectMany(t => ((TypeAliasBase)Activator.CreateInstance(t)).EnumerateAlias()).ToArray();

            Assert.True(aliases.Length > 80, $"別名: {aliases.Length}");
            Assert.All(aliases, a => Assert.False(string.IsNullOrWhiteSpace(a.Value)));
            // 同じ型を別のアセンブリでも決めているもの（NoteType）は、エディターが先に読んだほうを使う
            foreach (var type in Subclasses<TypeAliasBase>())
            {
                var own = ((TypeAliasBase)Activator.CreateInstance(type)).EnumerateAlias().ToArray();
                Assert.Empty(own.GroupBy(a => a.Key).Where(g => g.Count() > 1).Select(g => g.Key.FullName));
            }
            Assert.Empty(aliases.GroupBy(a => a.Value).Where(g => g.Count() > 1).Select(g => g.Key));
            Assert.Contains(aliases, a => a.Key == typeof(PictureObject) && a.Value == "PPD.Graphics.Image");
        }

        [Fact]
        public void TypeColors_AreUniquePerType()
        {
            var colors = Subclasses<TypeColorBase>().SelectMany(t => ((TypeColorBase)Activator.CreateInstance(t)).EnumerateColors()).ToArray();

            Assert.True(colors.Length > 80, $"色: {colors.Length}");
            // 同じ型を別のアセンブリでも決めているときは、同じ色
            Assert.Empty(colors.GroupBy(c => c.Key).Where(g => g.Select(c => c.Value).Distinct().Count() > 1).Select(g => g.Key.FullName));
            Assert.All(colors, c => Assert.Equal(255, c.Value.A));
        }

        // 書き出す型ごとの例（作るのにデバイスなどが要る型はない）
        static readonly Dictionary<Type, object> samples = new()
        {
            [typeof(List<object>)] = new List<object> { 1, "a" },
            [typeof(object[])] = new object[] { 2.5, null },
            [typeof(bool)] = true,
            [typeof(DateTime)] = new DateTime(2020, 1, 2, 3, 4, 5),
            [typeof(KeyValuePair<object, object>)] = new KeyValuePair<object, object>("k", 1),
            [typeof(double)] = 1.5,
            [typeof(float)] = 2.5f,
            [typeof(HashSet<object>)] = new HashSet<object> { "x" },
            [typeof(Dictionary<object, object>)] = new Dictionary<object, object> { ["k"] = "v" },
            [typeof(IEnumerable<object>)] = new object[] { 1, 2 },
            [typeof(IGrouping<object, object>)] = new object[] { 1, 3 }.GroupBy(i => (object)"odd").First(),
            [typeof(ILookup<object, object>)] = new object[] { 1, 2 }.ToLookup(i => i),
            [typeof(IOrderedEnumerable<object>)] = new object[] { 2, 1 }.OrderBy(i => i),
            [typeof(int)] = 3,
            [typeof(object)] = new object(),
            [typeof(Stopwatch)] = new Stopwatch(),
            [typeof(Stream)] = new MemoryStream(new byte[5]),
            [typeof(string)] = "text",
            [typeof(TimeSpan)] = TimeSpan.FromSeconds(1.5),
            [typeof(Vortice.Mathematics.Color4)] = new Vortice.Mathematics.Color4(0.1f, 0.2f, 0.3f, 0.4f),
            [typeof(Matrix4x4)] = Matrix4x4.CreateTranslation(1, 2, 3),
            [typeof(Quaternion)] = new Quaternion(1, 2, 3, 4),
            [typeof(Vector2)] = new Vector2(1, 2),
            [typeof(Vector3)] = new Vector3(1, 2, 3),
            [typeof(Vector4)] = new Vector4(1, 2, 3, 4),
            [typeof(ColoredTexturedVertex)] = new ColoredTexturedVertex(new Vector3(1, 2, 3)),
            [typeof(MarkData)] = new MarkData(100, 200, 45, 1.5f, ButtonType.Circle, 7),
            [typeof(MarkDataBase)] = new MarkData(100, 200, 45, 1.5f, ButtonType.Circle, 7),
            [typeof(ExMarkData)] = new ExMarkData(100, 200, 45, 1.5f, 2.5f, ButtonType.Square, 8),
            [typeof(ResultInfo)] = new ResultInfo(),
            [typeof(ColorScreenFilter)] = new ColorScreenFilter(),
            [typeof(GaussianFilter)] = new GaussianFilter(),
            [typeof(ScreenFilterBase)] = new GaussianFilter(),
            [typeof(ColorFilterBase)] = new HueColorFilter(),
            [typeof(PPDEditorCommon.EventData)] = new PPDEditorCommon.EventData(),
            [typeof(PPDEditorCommon.Dialog.ViewModel.SettingWindowViewModel)] = new PPDEditorCommon.Dialog.ViewModel.SettingWindowViewModel("title"),
        };

        static object Sample(Type type)
        {
            if (type == typeof(PPDEditorCommon.ParameterPreset))
            {
                // プリセットはファイルから読む
                var path = Path.GetTempFileName();
                try
                {
                    File.WriteAllText(path, "<Root><Parameter Key=\"a\" Value=\"1\" /></Root>");
                    return new PPDEditorCommon.ParameterPreset(path, "preset");
                }
                finally
                {
                    File.Delete(path);
                }
            }
            if (samples.TryGetValue(type, out var sample))
            {
                return sample;
            }
            // 色のフィルターは引数のないコンストラクターで作れる
            return typeof(ColorFilterBase).IsAssignableFrom(type) && !type.IsAbstract ? Activator.CreateInstance(type) : null;
        }

        static XElement Serialize(TypeSerializerBase typeSerializer, object value)
        {
            var element = new XElement("Value");
            typeSerializer.Serialize(new Serializer(), element, value);
            return element;
        }

        static string[] Names(XElement element) => [.. element.Elements("Property").Select(e => e.Attribute("Name").Value)];

        [Fact]
        public void EverySerializer_WritesItsValue()
        {
            var skipped = new List<string>();
            var count = 0;
            foreach (var type in Subclasses<TypeSerializerBase>())
            {
                var typeSerializer = (TypeSerializerBase)Activator.CreateInstance(type);
                var sample = Sample(typeSerializer.Type);
                if (sample == null)
                {
                    skipped.Add(typeSerializer.Type.Name);
                    continue;
                }
                var element = Serialize(typeSerializer, sample);
                // 値の文字列か、子の値のどちらかを書く（空の設定画面と、ScreenFilter の基底クラスは書くものがない）
                if (sample is not PPDEditorCommon.Dialog.ViewModel.SettingWindowViewModel && typeSerializer.Type != typeof(ScreenFilterBase))
                {
                    Assert.True(element.HasElements || element.Value.Length > 0, type.Name);
                }
                count++;
            }

            // デバイスが要るもの（画像など）と、エディターのインターフェースのものは作れない
            Assert.True(count >= 45, $"確かめた数: {count}, 作れなかった型: {string.Join(", ", skipped)}");
        }

        static TypeSerializerBase SerializerFor(Type type)
        {
            return Subclasses<TypeSerializerBase>().Select(t => (TypeSerializerBase)Activator.CreateInstance(t)).Single(s => s.Type == type);
        }

        [Fact]
        public void Serializers_WriteExpectedParts()
        {
            Assert.Equal(["X", "Y", "Z"], Names(Serialize(SerializerFor(typeof(Vector3)), new Vector3(1, 2, 3))));
            Assert.Equal(["Length", "Position"], Names(Serialize(SerializerFor(typeof(Stream)), new MemoryStream(new byte[5]))));
            Assert.Equal(["[0]", "[1]"], Names(Serialize(SerializerFor(typeof(List<object>)), new List<object> { 1, 2 })));
            Assert.Equal(["Weight", "Rotation"], Names(Serialize(SerializerFor(typeof(HueColorFilter)), new HueColorFilter())));
            Assert.Equal(["ID", "Difficulty", "ResultType", "Score", "CoolCount", "GoodCount", "SafeCount", "SadCount", "WorstCount", "MaxCombo", "FinishTime", "PlayDateTime"],
                Names(Serialize(SerializerFor(typeof(ResultInfo)), new ResultInfo())));

            var eventData = Serialize(SerializerFor(typeof(PPDEditorCommon.EventData)), new PPDEditorCommon.EventData());
            var volumes = eventData.Elements("Property").Single(e => e.Attribute("Name").Value == "VolumePercents").Element("Value");
            Assert.Equal(11, volumes.Elements("Property").Count());
            Assert.Equal(["PresetName", "Parameters"], Names(Serialize(SerializerFor(typeof(PPDEditorCommon.ParameterPreset)), Sample(typeof(PPDEditorCommon.ParameterPreset)))));

            // 中の値は、Serializer が型の名前と一緒に書く
            var pair = Serialize(SerializerFor(typeof(KeyValuePair<object, object>)), new KeyValuePair<object, object>("k", 1));
            Assert.Equal("System.String", pair.Elements("Property").First().Element("Value").Attribute("Type").Value);
        }
    }
}
