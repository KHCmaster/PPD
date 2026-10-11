using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;
using Extension = FlowScriptEngineBasicExtension.FlowSourceObjects;
using Json = FlowScriptEngineData.FlowSourceObjects.Json;
using Yaml = FlowScriptEngineData.FlowSourceObjects.Yaml;

namespace UnitTests.FlowScript
{
    /// <summary>
    /// FlowScriptEngineData（Json と Yaml）と FlowScriptEngineBasicExtension のノードのテストです。
    /// </summary>
    public class DataNodeTests
    {
        static NodeHarness Ok(Type node, params (string Name, object Value)[] inputs)
        {
            var harness = new NodeHarness(node);
            foreach (var (name, value) in inputs)
            {
                harness.Set(name, value);
            }
            harness.Invoke();
            Assert.Equal(["Success"], harness.Fired);
            return harness;
        }

        [Fact]
        public void JsonParse_ConvertsToScriptTypes()
        {
            var data = (Dictionary<object, object>)Ok(typeof(Json.ParseFlowSourceOjbect),
                ("Json", """{"int":3,"big":10000000000,"half":0.5,"tenth":0.1,"text":"譜面","flag":true,"none":null,"list":[1,"a",{"x":2}]}""")).Get("Data");

            Assert.Equal(3, data["int"]);
            Assert.Equal(10000000000L, data["big"]);
            // float で正しく表せる値は float、表せない値は double
            Assert.Equal(0.5f, data["half"]);
            Assert.Equal(0.1, data["tenth"]);
            Assert.Equal("譜面", data["text"]);
            Assert.Equal(true, data["flag"]);
            Assert.Null(data["none"]);
            var list = (List<object>)data["list"];
            Assert.Equal(1, list[0]);
            Assert.Equal("a", list[1]);
            Assert.Equal(2, ((Dictionary<object, object>)list[2])["x"]);
        }

        [Fact]
        public void JsonParse_InvalidText_Fails()
        {
            var harness = new NodeHarness(typeof(Json.ParseFlowSourceOjbect)).Set("Json", "{not json").Invoke();

            Assert.Equal(["Failed"], harness.Fired);
        }

        [Fact]
        public void JsonDump_WritesScriptTypes()
        {
            var data = new Dictionary<object, object>
            {
                ["list"] = new List<object> { 1, 2.5, 1.5f, "s", false, 'c', 7L, 8u, 9ul, 1.25m },
                [3] = new Dictionary<object, object> { ["nested"] = new object[] { "x" } },
                ["time"] = TimeSpan.FromSeconds(90),
                ["guid"] = Guid.Empty,
                ["uri"] = new Uri("https://example.com/"),
                ["date"] = new DateTime(2020, 1, 2, 3, 4, 5, DateTimeKind.Utc),
                ["offset"] = new DateTimeOffset(2020, 1, 2, 3, 4, 5, TimeSpan.Zero),
                ["other"] = DayOfWeek.Monday,
            };

            var json = (string)Ok(typeof(Json.DumpFlowSourceObject), ("Data", data)).Get("Json");

            Assert.Equal("""{"list":[1,2.5,1.5,"s",false,"c",7,8,9,1.25],"3":{"nested":["x"]},"time":"00:01:30","guid":"00000000-0000-0000-0000-000000000000","uri":"https://example.com/","date":"2020-01-02T03:04:05Z","offset":"2020-01-02T03:04:05+00:00","other":"Monday"}""", json);
        }

        [Fact]
        public void Json_RoundTrip()
        {
            var json = """{"a":[1,2],"b":{"c":"d"}}""";
            var data = Ok(typeof(Json.ParseFlowSourceOjbect), ("Json", json)).Get("Data");

            Assert.Equal(json, Ok(typeof(Json.DumpFlowSourceObject), ("Data", data)).Get("Json"));
        }

        [Fact]
        public void JsonDump_Null_Fails()
        {
            Assert.Equal(["Failed"], new NodeHarness(typeof(Json.DumpFlowSourceObject)).Invoke().Fired);
        }

        [Fact]
        public void Yaml_ParseAndDump()
        {
            var data = (Dictionary<object, object>)Ok(typeof(Yaml.ParseFlowSourceObject), ("Yaml", "name: 譜面\nlevels:\n  - 1\n  - 2\n")).Get("Data");

            Assert.Equal("譜面", data["name"]);
            Assert.Equal(new object[] { "1", "2" }, [.. ((List<object>)data["levels"])]);

            var yaml = (string)Ok(typeof(Yaml.DumpFlowSourceObject), ("Data", new Dictionary<object, object> { ["a"] = 1, ["b"] = new List<object> { "x" } })).Get("Yaml");
            Assert.Equal("a: 1\nb:\n- x\n", yaml.ReplaceLineEndings("\n"));
        }

        [Fact]
        public void Yaml_InvalidText_Fails()
        {
            Assert.Equal(["Failed"], new NodeHarness(typeof(Yaml.ParseFlowSourceObject)).Set("Yaml", "a: [1, 2").Invoke().Fired);
            Assert.Equal(["Failed"], new NodeHarness(typeof(Yaml.ParseFlowSourceObject)).Invoke().Fired);
        }

        public static TheoryData<Type, string, object, object> ParseValueCases() => new()
        {
            { typeof(Extension.Hashtable.ParseInt32ValueFlowSourceObject), "42", -1, 42 },
            { typeof(Extension.Hashtable.ParseDoubleValueFlowSourceObject), "1.5", -1.0, 1.5 },
            { typeof(Extension.Hashtable.ParseFloatValueFlowSourceObject), "2.25", -1f, 2.25f },
            { typeof(Extension.Hashtable.ParseBooleanValueFlowSourceObject), "True", false, true },
            { typeof(Extension.Hashtable.ParseStringValueFlowSourceObject), "text", "default", "text" },
        };

        [Theory]
        [MemberData(nameof(ParseValueCases), DisableDiscoveryEnumeration = true)]
        public void HashtableParseValue(Type node, string text, object @default, object expected)
        {
            var table = new Dictionary<object, object> { ["key"] = text, ["bad"] = "x,y" };
            NodeHarness Parse(string key) => new NodeHarness(node).Set("Hashtable", table).Set("Key", key).Set("Default", @default);

            var ok = Parse("key");
            Assert.Equal(expected, ok.Read("Value"));
            Assert.Empty(ok.Fired);

            // キーがなければ Default（ParseFailed は出さない）
            var missing = Parse("missing");
            Assert.Equal(@default, missing.Read("Value"));
            Assert.Empty(missing.Fired);
            Assert.Equal(@default, new NodeHarness(node).Set("Default", @default).Read("Value"));

            if (node != typeof(Extension.Hashtable.ParseStringValueFlowSourceObject))
            {
                var bad = Parse("bad");
                Assert.Equal(@default, bad.Read("Value"));
                Assert.Equal(["ParseFailed"], bad.Fired);
            }
        }

        [Fact]
        public void HashtableParseValue_UsesInvariantCulture()
        {
            var table = new Dictionary<object, object> { ["v"] = "1,5" };

            var harness = new NodeHarness(typeof(Extension.Hashtable.ParseDoubleValueFlowSourceObject)).Set("Hashtable", table).Set("Key", "v");

            // 小数点はいつもピリオド（カンマは桁区切りとしても使わない）
            Assert.Equal(0.0, harness.Read("Value"));
            Assert.Equal(["ParseFailed"], harness.Fired);
        }

        [Fact]
        public void EnumerableRange()
        {
            static object[] Range(Type node, params (string, object)[] inputs)
            {
                var harness = new NodeHarness(node);
                foreach (var (name, value) in inputs)
                {
                    harness.Set(name, value);
                }
                return [.. ((IEnumerable<object>)harness.Get("Range"))];
            }

            Assert.Equal([0, 1, 2], Range(typeof(Extension.Enumerable.RangeFlowSourceObject), ("EndIndex", 3)));
            Assert.Equal([2, 3], Range(typeof(Extension.Enumerable.RangeWithStartIndexFlowSourceObject), ("StartIndex", 2), ("EndIndex", 4)));
            Assert.Equal([1, 4, 7], Range(typeof(Extension.Enumerable.RangeWithStepFlowSourceObject), ("StartIndex", 1), ("EndIndex", 8), ("Step", 3)));
            Assert.Equal([5, 3, 1], Range(typeof(Extension.Enumerable.RangeWithStepFlowSourceObject), ("StartIndex", 5), ("EndIndex", 0), ("Step", -2)));
            Assert.Empty(Range(typeof(Extension.Enumerable.RangeWithStepFlowSourceObject), ("StartIndex", 0), ("EndIndex", 5), ("Step", 0)));
            Assert.Empty(Range(typeof(Extension.Enumerable.RangeFlowSourceObject), ("EndIndex", -1)));
        }
    }
}
