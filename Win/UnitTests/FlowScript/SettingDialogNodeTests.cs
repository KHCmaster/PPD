using PPDEditorCommon.Dialog.ViewModel;
using System;
using System.Linq;
using System.Windows.Input;
using Xunit;

namespace UnitTests.FlowScript
{
    /// <summary>
    /// エディターのスクリプトで設定画面の中身を作るノード（PPDEditor.Dialog.Setting）と、その ViewModel のテストです。
    /// 画面は出さない（出すのは Dialog.Show）。
    /// </summary>
    public class SettingDialogNodeTests
    {
        static NodeHarness Node(string name, params (string Name, object Value)[] inputs)
        {
            var type = typeof(FlowScriptEnginePPDEditor.ToolTipText).Assembly.GetType($"FlowScriptEnginePPDEditor.FlowSourceObjects.Dialog.Setting.{name}FlowSourceObject", true);
            var harness = new NodeHarness(type);
            foreach (var (n, v) in inputs)
            {
                harness.Set(n, v);
            }
            return harness;
        }

        static SettingWindowViewModel Build()
        {
            var setting = (SettingWindowViewModel)Node("Value", ("Title", "設定")).Invoke().Get("Value");
            void Add(string name, string key, params (string, object)[] inputs) =>
                Assert.Equal(["Success"], Node(name, [("Setting", setting), ("Key", key), ("Description", key + " の説明"), .. inputs]).Invoke().Fired);

            Add("AddBool", "bool", ("Value", true));
            Add("AddDouble", "double", ("Value", 1.5));
            Add("AddDoubleWithLimit", "doubleLimit", ("Value", 1.5), ("MinValue", 1.0), ("MaxValue", 2.0));
            Add("AddFloat", "float", ("Value", 2.5f));
            Add("AddFloatWithLimit", "floatLimit", ("Value", 2.5f), ("MinValue", 2f), ("MaxValue", 3f));
            Add("AddInt", "int", ("Value", 3));
            Add("AddIntWithLimit", "intLimit", ("Value", 3), ("MinValue", 1), ("MaxValue", 5));
            Add("AddString", "string", ("Value", "abc"));
            Add("AddStringWithLimit", "stringLimit", ("Value", "ab"), ("MaxLength", 3));
            Add("AddEnumerable", "enum", ("Values", new object[] { "x", "y" }), ("Value", "y"));
            Add("AddEnumerable", "enumDefault", ("Values", new object[] { "x", "y" }), ("Value", "z"));
            setting.Initialize();
            return setting;
        }

        [Fact]
        public void AddNodes_BuildSettingsWithDefaults()
        {
            var setting = Build();

            Assert.Equal("設定", setting.Title);
            Assert.True(setting.IsOkEnabled);
            Assert.Equal(11, setting.Settings.Length);
            var values = setting.SettingPairs.ToDictionary(p => (string)p.Key, p => p.Value);
            Assert.Equal(true, values["bool"]);
            Assert.Equal(1.5, values["double"]);
            Assert.Equal(2.5f, values["floatLimit"]);
            Assert.Equal(3, values["intLimit"]);
            Assert.Equal("abc", values["string"]);
            Assert.Equal("y", values["enum"]);
            // 既定の値が選べないときは最初のもの
            Assert.Equal("x", values["enumDefault"]);
            Assert.Equal("ab", Node("GetValue", ("Setting", setting), ("Key", "stringLimit")).Get("Value"));
            Assert.Null(Node("GetValue", ("Key", "stringLimit")).Get("Value"));
        }

        [Theory]
        [InlineData("AddBool")]
        [InlineData("AddDouble")]
        [InlineData("AddDoubleWithLimit")]
        [InlineData("AddEnumerable")]
        [InlineData("AddFloat")]
        [InlineData("AddFloatWithLimit")]
        [InlineData("AddInt")]
        [InlineData("AddIntWithLimit")]
        [InlineData("AddString")]
        [InlineData("AddStringWithLimit")]
        public void AddNodes_WithoutSetting_Fail(string name)
        {
            Assert.Equal(["Failed"], Node(name).Invoke().Fired);
        }

        // 画面で文字を入れたときと同じく、Text を変えて ChangedCommand を実行する
        static void Type(SettingWindowViewModel setting, string key, string text)
        {
            var viewModel = setting.Settings.Single(s => (string)s.GetType().GetProperty("Name").GetValue(s) == key);
            viewModel.GetType().GetProperty("Text").SetValue(viewModel, text);
            ((ICommand)viewModel.GetType().GetProperty("ChangedCommand").GetValue(viewModel)).Execute(null);
        }

        static string Error(SettingWindowViewModel setting, string key)
        {
            var viewModel = setting.Settings.Single(s => (string)s.GetType().GetProperty("Name").GetValue(s) == key);
            return (string)viewModel.GetType().GetProperty("ValidateErrorText").GetValue(viewModel);
        }

        [Theory]
        [InlineData("intLimit", "5", true)]
        [InlineData("intLimit", "6", false)]
        [InlineData("intLimit", "0", false)]
        [InlineData("intLimit", "x", false)]
        [InlineData("doubleLimit", "2", true)]
        [InlineData("doubleLimit", "2.5", false)]
        [InlineData("doubleLimit", "x", false)]
        [InlineData("floatLimit", "3", true)]
        [InlineData("floatLimit", "1", false)]
        [InlineData("floatLimit", "x", false)]
        [InlineData("double", "1e300", true)]
        // 最大長ちょうどは使える
        [InlineData("stringLimit", "abc", true)]
        [InlineData("stringLimit", "abcd", false)]
        public void TypedText_IsValidated(string key, string text, bool valid)
        {
            var setting = Build();

            Type(setting, key, text);

            Assert.Equal(valid, setting.IsOkEnabled);
            if (valid)
            {
                Assert.Null(Error(setting, key));
            }
        }

        [Fact]
        public void FixingText_EnablesOkAgain()
        {
            var setting = Build();

            Type(setting, "intLimit", "9");
            Assert.False(setting.IsOkEnabled);
            Assert.Equal("1~5", Error(setting, "intLimit"));
            Type(setting, "int", "x");
            Assert.Equal("Not Proper Number", Error(setting, "int"));

            Type(setting, "intLimit", "4");
            Type(setting, "int", "7");

            Assert.True(setting.IsOkEnabled);
            Assert.Equal(4, setting["intLimit"]);
            Assert.Equal(7, setting["int"]);
        }

        [Fact]
        public void IsType()
        {
            var isType = typeof(FlowScriptEnginePPDEditor.ToolTipText).Assembly.GetType("FlowScriptEnginePPDEditor.FlowSourceObjects.Dialog.Setting.IsTypeFlowSourceObject", true);

            Assert.Equal(true, new NodeHarness(isType).Set("Target", new SettingWindowViewModel("t")).Get("Value"));
            Assert.Equal(false, new NodeHarness(isType).Set("Target", "t").Get("Value"));
        }

        [Fact]
        public void UnknownSettingType_IsIgnored()
        {
            var setting = new SettingWindowViewModel("t");

            setting.AddSetting(null);
            setting.Initialize();

            Assert.Empty(setting.Settings);
            Assert.Throws<InvalidOperationException>(() => setting["missing"]);
        }
    }
}
