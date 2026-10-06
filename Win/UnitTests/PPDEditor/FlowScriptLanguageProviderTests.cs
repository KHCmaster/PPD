using PPDConfiguration;
using System;
using System.IO;
using System.Linq;
using System.Reflection;
using Xunit;

namespace UnitTests.PPDEditor
{
    /// <summary>
    /// スクリプトの編集の画面に出す文字（FlowScriptLanguageProvider）のテストです。
    /// </summary>
    public class FlowScriptLanguageProviderTests
    {
        static readonly Type providerType = typeof(global::PPDEditor.Utility).Assembly.GetType("PPDEditor.FlowScriptLanguageProvider", true);

        static PropertyInfo[] Texts() => [.. providerType.GetProperties().Where(p => p.PropertyType == typeof(string))];

        [Fact]
        public void Texts_ComeFromEditorLanguage()
        {
            var tooltips = new SettingReader("[Node.Summary]説明\n");
            var provider = Activator.CreateInstance(providerType, tooltips);

            Assert.Equal(20, Texts().Length);
            foreach (var property in Texts())
            {
                var text = (string)property.GetValue(provider);
                Assert.Equal(global::PPDEditor.Utility.Language[property.Name], text);
                Assert.False(string.IsNullOrEmpty(text), property.Name);
            }
            // ノードの説明は、渡した設定から
            Assert.Equal("説明", providerType.GetMethod("GetFlowSourceToolTipText").Invoke(provider, ["Node.Summary"]));
        }

        [Theory]
        [InlineData("jp")]
        [InlineData("en")]
        public void Texts_AreInLanguageFile(string language)
        {
            // 主な言語のファイルには、すべての文字がある（ほかの言語は、ないものは日本語になる）
            var file = new SettingReader(File.ReadAllText(Path.Combine(AppContext.BaseDirectory, "Lang", $"lang_PPDEditor_{language}.ini")));

            Assert.All(Texts(), property => Assert.True(file.ContainsKey(property.Name), property.Name));
        }
    }
}
