using PPDFramework.Mod;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using Xunit;

namespace UnitTests.FlowScript
{
    /// <summary>
    /// Mod の設定（ModSettingManager と ModSetting）と、それを使う PPD.Mod のノードのテストです。
    /// </summary>
    public class ModSettingTests
    {
        [Theory]
        [InlineData("10", true, 10)]
        [InlineData("0", true, 0)]
        [InlineData("11", false, 11)]
        [InlineData("-1", false, -1)]
        [InlineData("x", false, null)]
        public void Int32Setting_ValidatesRange(string text, bool valid, object value)
        {
            var setting = new Int32ModSetting("k", "name", "description", 5, 0, 10);

            Assert.Equal(valid, setting.Validate(text, out var parsed));
            Assert.Equal(value, parsed);
        }

        [Fact]
        public void NumberSettings_WithoutLimit_AcceptAnyNumber()
        {
            var manager = new ModSettingManager();
            manager.AddInt32Setting("i", "I", "", 1);
            manager.AddFloatSetting("f", "F", "", 1.5f);
            manager.AddDoubleSetting("d", "D", "", 2.5);

            var settings = manager.ModSettings;
            Assert.True(((Int32ModSetting)settings[0]).IsMinClassMinValue);
            Assert.True(((Int32ModSetting)settings[0]).IsMaxClassMaxValue);
            Assert.True(settings[0].Validate(int.MinValue.ToString(), out _));
            Assert.True(settings[1].Validate("1e30", out var f));
            Assert.Equal(1e30f, f);
            Assert.True(settings[2].Validate("-1e300", out var d));
            Assert.Equal(-1e300, d);
            Assert.Equal([1, 1.5f, 2.5], settings.Select(s => s.Default));
        }

        [Fact]
        public void FloatAndDoubleSettings_ValidateRange()
        {
            var single = new FloatModSetting("f", "F", "", 0.5f, 0, 1);
            Assert.True(single.Validate("1", out _));
            Assert.False(single.Validate("1.5", out _));
            Assert.False(single.Validate("-0.5", out _));
            Assert.False(single.Validate("x", out _));
            Assert.Equal(0.25f.ToString(), single.GetStringValue(0.25f));

            var @double = new DoubleModSetting("d", "D", "", 0.5, 0, 1);
            Assert.True(@double.Validate("0", out _));
            Assert.False(@double.Validate("2", out _));
            Assert.False(@double.Validate("-1", out _));
            Assert.False(@double.Validate("x", out _));
            Assert.Equal(0.25.ToString(), @double.GetStringValue(0.25));
        }

        [Fact]
        public void StringSetting_ValidatesMaxLength()
        {
            var limited = new StringModSetting("s", "S", "", "ab", 3);
            Assert.True(limited.Validate("abc", out var value));
            Assert.Equal("abc", value);
            Assert.False(limited.Validate("abcd", out _));
            Assert.False(limited.IsMaxLengthClassMaxLength);
            Assert.Equal("ab", limited.GetStringValue("ab"));

            var manager = new ModSettingManager();
            manager.AddStringSetting("s", "S", "", "");
            var unlimited = (StringModSetting)manager.ModSettings[0];
            Assert.True(unlimited.IsMaxLengthClassMaxLength);
            Assert.True(unlimited.Validate(new string('x', 10000), out _));
        }

        [Fact]
        public void BooleanSetting()
        {
            var setting = new BooleanModSetting("b", "B", "", true);

            Assert.Equal([false, true], setting.AvailableValues);
            Assert.True(setting.Validate("False", out var value));
            Assert.Equal(false, value);
            Assert.False(setting.Validate("no", out value));
            Assert.Null(value);
            Assert.Equal("True", setting.GetStringValue(true));
            Assert.Null(new Int32ModSetting("i", "I", "", 0, 0, 1).AvailableValues);
        }

        [Theory]
        [InlineData("#FF8000", 1, 128 / 255f, 0)]
        [InlineData("#ff8000", 1, 128 / 255f, 0)]
        [InlineData("#FFF", 1, 1, 1)]
        [InlineData("#F80", 1, 136 / 255f, 0)]
        public void Color4Setting_ReadsColorCodes(string text, float r, float g, float b)
        {
            var setting = new Color4ModSetting("c", "C", "", new Vortice.Mathematics.Color4(0, 0, 0, 1));

            Assert.True(setting.Validate(text, out var value));

            var color = (Vortice.Mathematics.Color4)value;
            Assert.Equal(r, color.R, 5);
            Assert.Equal(g, color.G, 5);
            Assert.Equal(b, color.B, 5);
            Assert.Equal(1, color.A);
        }

        [Theory]
        [InlineData("FF8000")]
        [InlineData("#FF80")]
        [InlineData("#GGGGGG")]
        public void Color4Setting_RejectsOtherText(string text)
        {
            Assert.False(new Color4ModSetting("c", "C", "", default).Validate(text, out var value));
            Assert.Null(value);
        }

        [Fact]
        public void Color4Setting_WritesColorCode()
        {
            var setting = new Color4ModSetting("c", "C", "", default);

            Assert.Equal("#FF8000", setting.GetStringValue(new Vortice.Mathematics.Color4(1, 128 / 255f, 0, 1)));
        }

        [Fact]
        public void Manager_StoresValuesByKey()
        {
            var manager = new ModSettingManager();

            manager["volume"] = 3;

            Assert.Equal(3, manager["volume"]);
            Assert.Equal([new KeyValuePair<string, object>("volume", 3)], manager.Settings);
        }

        static NodeHarness Node(string name, ModSettingManager manager, params (string Name, object Value)[] inputs)
        {
            var type = typeof(FlowScriptEnginePPD.ToolTipText).Assembly.GetType($"FlowScriptEnginePPD.FlowSourceObjects.Mod.{name}FlowSourceObject", true);
            var harness = new NodeHarness(type);
            if (manager != null)
            {
                harness.Items["ModSettingManager"] = manager;
            }
            foreach (var (n, v) in inputs)
            {
                harness.Set(n, v);
            }
            return harness.Initialize();
        }

        [Fact]
        public void AddSettingNodes_AddSettings()
        {
            var manager = new ModSettingManager();
            void Add(string name, params (string, object)[] inputs) =>
                Node(name, manager, [("Key", name), ("DisplayName", "名前"), ("Description", "説明"), .. inputs]).Invoke();

            Add("AddBooleanSetting", ("Default", true));
            Add("AddColor4Setting", ("Default", new Vortice.Mathematics.Color4(1, 0, 0, 1)));
            Add("AddDoubleSetting", ("Default", 1.5));
            Add("AddDoubleSettingWithLimit", ("Default", 1.5), ("Min", 1.0), ("Max", 2.0));
            Add("AddFloatSetting", ("Default", 2.5f));
            Add("AddFloatSettingWithLimit", ("Default", 2.5f), ("Min", 2f), ("Max", 3f));
            Add("AddInt32Setting", ("Default", 3));
            Add("AddInt32SettingWithLimit", ("Default", 3), ("Min", 1), ("Max", 5));
            Add("AddStringSetting", ("Default", "s"));
            Add("AddStringSettingWithMaxLength", ("Default", "s"), ("MaxLength", 4));

            var settings = manager.ModSettings.ToDictionary(s => s.Key);
            Assert.Equal(10, settings.Count);
            Assert.All(settings.Values, s => Assert.Equal(("名前", "説明"), (s.Name, s.Description)));
            Assert.Equal(true, settings["AddBooleanSetting"].Default);
            Assert.IsType<Color4ModSetting>(settings["AddColor4Setting"]);
            Assert.True(((DoubleModSetting)settings["AddDoubleSetting"]).IsMaxClassMaxValue);
            Assert.Equal((1.0, 2.0), (((DoubleModSetting)settings["AddDoubleSettingWithLimit"]).MinValue, ((DoubleModSetting)settings["AddDoubleSettingWithLimit"]).MaxValue));
            Assert.True(((FloatModSetting)settings["AddFloatSetting"]).IsMinClassMinValue);
            Assert.Equal((2f, 3f), (((FloatModSetting)settings["AddFloatSettingWithLimit"]).MinValue, ((FloatModSetting)settings["AddFloatSettingWithLimit"]).MaxValue));
            Assert.True(((Int32ModSetting)settings["AddInt32Setting"]).IsMaxClassMaxValue);
            Assert.Equal((1, 5), (((Int32ModSetting)settings["AddInt32SettingWithLimit"]).MinValue, ((Int32ModSetting)settings["AddInt32SettingWithLimit"]).MaxValue));
            Assert.True(((StringModSetting)settings["AddStringSetting"]).IsMaxLengthClassMaxLength);
            Assert.Equal(4, ((StringModSetting)settings["AddStringSettingWithMaxLength"]).MaxLength);
        }

        [Fact]
        public void AddSettingNodes_OutsideMod_DoNothing()
        {
            // Mod でないスクリプトでは何もしない（エラーにもならない）
            Node("AddInt32Setting", null, ("Key", "k")).Invoke();
        }

        [Fact]
        public void GetSetting_ReadsValue()
        {
            var manager = new ModSettingManager { ["speed"] = 2 };

            Assert.Equal(2, Node("GetSetting", manager, ("Key", "speed")).Get("Value"));
            Assert.Null(Node("GetSetting", null, ("Key", "speed")).Get("Value"));
        }

        [Fact]
        public void EntryPoint_FiresOnModStart()
        {
            var entry = Node("EntryPoint", null);

            entry.Node.Manager.Call(ModSettingManager.MODSTART, null);

            Assert.Equal(["Start"], entry.Fired);
        }

        [Fact]
        public void CurrentAndUsingMods_OutsideGame()
        {
            Assert.Null(Node("Current", null).Get("Value"));
            Assert.Empty((object[])Node("UsingMods", null).Get("Mods"));
        }
    }
}
