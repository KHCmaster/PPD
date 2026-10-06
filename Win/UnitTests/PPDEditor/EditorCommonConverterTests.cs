using CommunityToolkit.Mvvm.Messaging;
using PPDConfiguration;
using PPDEditorCommon;
using PPDEditorCommon.Dialog.Converter;
using PPDEditorCommon.Dialog.Message;
using PPDEditorCommon.Dialog.ViewModel;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Windows;
using Xunit;

namespace UnitTests.PPDEditor
{
    /// <summary>
    /// エディターの共通の部品（変換、翻訳、新しいプロジェクトのテンプレートの一覧）のテストです。画面は作らない。
    /// </summary>
    public class EditorCommonConverterTests
    {
        [Theory]
        [InlineData(true, false)]
        [InlineData(false, true)]
        public void InvertBool_InvertsTheValue(bool value, bool expected)
        {
            Assert.Equal(expected, new InvertBoolConverter().Convert(value, typeof(bool), null, CultureInfo.InvariantCulture));
            // bool でなければそのまま
            Assert.Equal("x", new InvertBoolConverter().Convert("x", typeof(bool), true, CultureInfo.InvariantCulture));
            Assert.Throws<NotImplementedException>(() => new InvertBoolConverter().ConvertBack(value, typeof(bool), null, CultureInfo.InvariantCulture));
        }

        [Theory]
        [InlineData(true, null, Visibility.Visible)]
        [InlineData(false, null, Visibility.Collapsed)]
        [InlineData(true, "Invert", Visibility.Collapsed)]
        [InlineData("text", null, Visibility.Visible)]
        [InlineData(null, null, Visibility.Collapsed)]
        [InlineData(null, "Invert", Visibility.Visible)]
        public void Visibility_FromBoolOrNull(object value, string parameter, Visibility expected)
        {
            Assert.Equal(expected, new VisibilityConverter().Convert(value, typeof(Visibility), parameter, CultureInfo.InvariantCulture));
        }

        [Fact]
        public void Translate_UsesLanguageAndFormat()
        {
            var saved = TranslateExtension.Language;
            try
            {
                TranslateExtension.Language = null;
                Assert.Equal("Key", new TranslateExtension("Key").ProvideValue(null));
                Assert.Equal("[Key]", new TranslateExtension("Key", "[{0}]").ProvideValue(null));

                var language = new LanguageReader("PPDEditor", "jp");
                TranslateExtension.Language = language;
                Assert.Equal(language["Cancel"], new TranslateExtension("Cancel").ProvideValue(null));
                Assert.Equal("<" + language["Cancel"] + ">", new TranslateExtension("Cancel", "<{0}>").ProvideValue(null));
            }
            finally
            {
                TranslateExtension.Language = saved;
            }
        }

        [Fact]
        public void NewProject_BuildsTemplateTreeFromFolder()
        {
            using var folder = new TempFolder(setCurrentDirectory: true);
            folder.Write(Path.Combine("ProjectTemplates", "a.xml"), """<Template Type="Basic\Song" Name="A" />""");
            folder.Write(Path.Combine("ProjectTemplates", "b.xml"), """<Template Type="Basic\Song" Name="B" />""");
            folder.Write(Path.Combine("ProjectTemplates", "broken.xml"), "<Template");
            folder.Write(Path.Combine("ProjectTemplates", "c.xml"), """<Template Type="Other" Name="C" />""");
            var vm = new NewProjectWindowViewModel(new LanguageReader("PPDEditor", "jp"));
            var messages = new List<string>();
            var recipient = new object();
            WeakReferenceMessenger.Default.Register<ShowMessageBoxMessage>(recipient, (r, m) => messages.Add(m.Text));
            try
            {
                vm.LoadedCommand.Execute(null);
            }
            finally
            {
                WeakReferenceMessenger.Default.Unregister<ShowMessageBoxMessage>(recipient);
            }

            // 種類の「\」で木にする。最初の枝を開いて選ぶ
            Assert.Equal(["Basic", "Other"], vm.Root.Children.Select(c => c.Text));
            var basic = vm.Root.Children[0];
            Assert.True(basic.IsExpanded && basic.IsSelected);
            var song = Assert.Single(basic.Children);
            Assert.Equal(["A", "B"], song.Children.Select(c => c.Text));
            Assert.Equal("C", Assert.Single(vm.Root.Children[1].Children).Value.Name);
            // 読めないテンプレートは知らせて飛ばす
            Assert.Equal(2, messages.Count);
            Assert.Contains("broken.xml", messages[0]);
        }
    }
}
