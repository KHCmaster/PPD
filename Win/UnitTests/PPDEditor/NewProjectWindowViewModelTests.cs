using PPDConfiguration;
using PPDEditorCommon.Dialog.Model;
using PPDEditorCommon.Dialog.ViewModel;
using PPDFramework;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Windows;
using Xunit;

namespace UnitTests.PPDEditor
{
    /// <summary>
    /// 新しいプロジェクトの画面の ViewModel と、プロジェクトのテンプレート（TemplateInfo）のテストです。画面は作らない。
    /// </summary>
    public sealed class NewProjectWindowViewModelTests : IDisposable
    {
        readonly string dir = Path.Combine(Path.GetTempPath(), "PPDNewProjectTest_" + Guid.NewGuid().ToString("N"));
        readonly LanguageReader language = new("PPDEditor", "jp");

        public NewProjectWindowViewModelTests()
        {
            Directory.CreateDirectory(dir);
        }

        public void Dispose()
        {
            Directory.Delete(dir, true);
        }

        string Write(string name, string content)
        {
            var path = Path.Combine(dir, name);
            File.WriteAllText(path, content);
            return path;
        }

        const string FullTemplate = """
            <Template Type="Basic\Song" Name="Normal" Description="line1\nline2" IconPath="none.png">
              <SoundManager>
                <Sounds><Sound>a.wav</Sound><Sound>b.wav</Sound></Sounds>
                <Changes>
                  <Change><Time>$(Epsilon)</Time><Data>0,1,2</Data></Change>
                  <Change><Time>2.5</Time><Data>3,3,3,3,3,3,3,3,3,3,3,3</Data></Change>
                  <Change><Time>4</Time></Change>
                </Changes>
              </SoundManager>
              <EventManager>
                <Events>
                  <Event><Time>0</Time><NoteType>AC</NoteType><BPM>150</BPM><InitializeOrders>9,8,7,6,5,4,3,2,1,0</InitializeOrders></Event>
                  <Event><Time>10</Time></Event>
                  <Event><BPM>1</BPM></Event>
                </Events>
              </EventManager>
              <TimeLine>
                <RowOrders>1,0,2,3,4,5,6,7,8,9</RowOrders>
                <RowVisibilities>1,0,1,1,1,1,1,1,1,0</RowVisibilities>
                <RowLimited>1</RowLimited>
              </TimeLine>
              <ScriptManager>
                <Scripts>
                  <Script><Src>a.fsml</Src><Dest>Scripts\a.fsml</Dest></Script>
                  <Script><Src>only-src.fsml</Src></Script>
                </Scripts>
              </ScriptManager>
            </Template>
            """;

        [Fact]
        public void TemplateInfo_LoadsAllSections()
        {
            _ = new NewProjectWindowViewModel(language);
            var template = new TemplateInfo();

            template.Load(Write("full.xml", FullTemplate));

            Assert.Equal(@"Basic\Song", template.Type);
            Assert.Equal("Normal", template.Name);
            Assert.Equal("line1\nline2", template.Description);
            Assert.Null(template.Image);
            Assert.Equal(["a.wav", "b.wav"], template.SoundManagerInfo.Sounds);
            var changes = template.SoundManagerInfo.Changes;
            Assert.Equal(2, changes.Length);
            Assert.Equal(float.Epsilon, changes[0].Key);
            Assert.Equal(new ushort[] { 0, 1, 2, 0, 0, 0, 0, 0, 0, 0 }, changes[0].Value);
            // 10 個より多いものは使わない
            Assert.Equal(10, changes[1].Value.Length);

            var events = template.EventManagerInfo.Events;
            Assert.Equal([0f, 10f], events.Select(e => e.Key));
            Assert.Equal(NoteType.AC, events[0].Value.NoteType);
            Assert.Equal(150f, events[0].Value.BPM);
            Assert.Equal([9, 8, 7, 6, 5, 4, 3, 2, 1, 0], events[0].Value.InitializeOrders);
            Assert.Equal(-1f, events[1].Value.BPM);
            Assert.Equal(Enumerable.Range(0, 10), events[1].Value.InitializeOrders);

            Assert.Equal([1, 0, 2, 3, 4, 5, 6, 7, 8, 9], template.TimeLineInfo.RowOrders);
            Assert.Equal([true, false, true, true, true, true, true, true, true, false], template.TimeLineInfo.RowVisibilities);
            Assert.True(template.TimeLineInfo.RowLimited);
            var script = Assert.Single(template.ScriptManagerInfo.Scripts);
            Assert.Equal(("a.fsml", @"Scripts\a.fsml"), (script.Src, script.Dest));
        }

        [Fact]
        public void TemplateInfo_MissingAttributesAndSections()
        {
            _ = new NewProjectWindowViewModel(language);
            var template = new TemplateInfo();

            // 属性がなくても読める
            template.Load(Write("empty.xml", "<Template Type=\"Basic\" Name=\"Empty\" />"));

            Assert.Equal("", template.Description);
            Assert.Equal("", template.IconPath);
            Assert.Empty(template.SoundManagerInfo.Sounds);
            Assert.Empty(template.EventManagerInfo.Events);
            Assert.Equal(Enumerable.Range(0, 10), template.TimeLineInfo.RowOrders);
            Assert.All(template.TimeLineInfo.RowVisibilities, Assert.True);
            Assert.False(template.TimeLineInfo.RowLimited);
            Assert.Empty(template.ScriptManagerInfo.Scripts);
        }

        [Fact]
        public void TemplateInfo_ReplacesLanguageKeys()
        {
            _ = new NewProjectWindowViewModel(language);
            var template = new TemplateInfo();

            template.Load(Write("lang.xml", "<Template Type=\"{lang:NameNotInput}\" Name=\"x{lang:MovieNotExist}y\" />"));

            Assert.Equal(language["NameNotInput"], template.Type);
            Assert.Equal("x" + language["MovieNotExist"] + "y", template.Name);
        }

        [Fact]
        public void TemplateInfo_Parsers()
        {
            var template = new TemplateInfo();

            Assert.Equal(1.5f, template.ParseFloat("1.5"));
            Assert.Equal(float.Epsilon, template.ParseFloat("$(Epsilon)"));
            Assert.Equal([1, 2, 0], template.ParseIntList("1,2", 3));
            Assert.Equal([1, 2], template.ParseIntList("1,2,3", 2));
            Assert.Equal(NoteType.ACFT, template.ParseEnum<NoteType>("ACFT"));
        }

        [Fact]
        public void Validate_ChecksEachInputInOrder()
        {
            var vm = new NewProjectWindowViewModel(language);
            var changed = new List<string>();
            vm.PropertyChanged += (s, e) => changed.Add(e.PropertyName);
            void Expect(string key)
            {
                Assert.False(vm.IsOkEnabled);
                Assert.Equal(language[key], vm.ValidateResultMessage);
            }

            vm.Name = "";
            Expect("NameNotInput");
            vm.Name = "bad|name";
            Expect("InvalidCharInName");
            vm.Name = "song";
            Expect("LocationNotExist");
            vm.FolderPath = dir;
            Expect("MovieNotInput");
            vm.MoviePath = Path.Combine(dir, "missing.mp4");
            Expect("MovieNotExist");
            vm.MoviePath = Write("movie.mp4", "");
            Expect("TemplateNotSelected");
            vm.SelectedItem = new ListViewModel<TemplateInfo> { Value = new TemplateInfo() };
            vm.Name = "song2";
            Assert.True(vm.IsOkEnabled);
            Assert.Equal("", vm.ValidateResultMessage);

            // 同じ名前のフォルダーかプロジェクトがあれば使えない
            Directory.CreateDirectory(Path.Combine(dir, "song3"));
            vm.Name = "song3";
            Expect("AlreadyExistProject");
            Write("song4.ppdproj", "");
            vm.Name = "song4";
            Expect("AlreadyExistProject");

            Assert.Contains(nameof(vm.IsOkEnabled), changed);
            Assert.Contains(nameof(vm.SelectedItem), changed);
        }

        [Fact]
        public void Loaded_ChoosesUnusedDefaultName()
        {
            Directory.CreateDirectory(Path.Combine(dir, "NewPPDProject"));
            Write("NewPPDProject2.ppdproj", "");
            var vm = new NewProjectWindowViewModel(language) { FolderPath = dir };

            vm.LoadedCommand.Execute(null);

            Assert.Equal("NewPPDProject3", vm.Name);
            // テスト用のフォルダーには ProjectTemplates がないので、テンプレートはない
            Assert.Empty(vm.Root.Children);
            Assert.False(vm.IsOkEnabled);
        }

        [Fact]
        public void Loaded_WithoutFolder_KeepsName()
        {
            var vm = new NewProjectWindowViewModel(language) { Name = "keep" };

            vm.LoadedCommand.Execute(null);

            Assert.Equal("keep", vm.Name);
        }

        [Fact]
        public void SelectingTreeNode_ListsItsTemplates()
        {
            var vm = new NewProjectWindowViewModel(language);
            static TemplateInfo Template() => new();
            var root = new TreeNodeViewModel<TemplateInfo> { Text = "Basic" };
            var child = new TreeNodeViewModel<TemplateInfo> { Text = "Song" };
            var first = Template();
            var second = Template();
            child.Children.Add(new TreeNodeViewModel<TemplateInfo> { Value = first });
            root.Children.Add(child);
            root.Children.Add(new TreeNodeViewModel<TemplateInfo> { Value = second });

            vm.SelectedItemChangedCommand.Execute(new RoutedPropertyChangedEventArgs<object>(null, root));

            // 子のテンプレートもすべて並べ、最初のものを選ぶ
            Assert.Equal([first, second], vm.Templates.Select(t => t.Value));
            Assert.True(vm.Templates[0].IsSelected);

            vm.SelectedItemChangedCommand.Execute(new RoutedPropertyChangedEventArgs<object>(root, "not a node"));
            Assert.Equal(2, vm.Templates.Count);

            vm.SelectedItemChangedCommand.Execute(new RoutedPropertyChangedEventArgs<object>(root, new TreeNodeViewModel<TemplateInfo>()));
            Assert.Empty(vm.Templates);
        }

        [Fact]
        public void DialogCommands_WithoutAnswer_ChangeNothing()
        {
            var vm = new NewProjectWindowViewModel(language) { FolderPath = dir, MoviePath = "m.mp4" };

            // 画面がないので、フォルダーやファイルを選ぶメッセージには誰も答えない
            vm.ReferLocationCommand.Execute(null);
            vm.ReferMovieCommand.Execute(null);
            vm.OkCommand.Execute(null);
            vm.CancelCommand.Execute(null);

            Assert.Equal(dir, vm.FolderPath);
            Assert.Equal("m.mp4", vm.MoviePath);
        }

        [Fact]
        public void TreeAndListNodes_NotifyChanges()
        {
            var node = new TreeNodeViewModel<TemplateInfo>();
            var item = new ListViewModel<TemplateInfo>();
            var changes = new List<string>();
            void record(object s, PropertyChangedEventArgs e) => changes.Add(e.PropertyName);
            node.PropertyChanged += record;
            item.PropertyChanged += record;
            var value = new TemplateInfo();

            node.Text = "a";
            node.Text = "a";
            node.Value = value;
            node.Value = value;
            node.Value = new TemplateInfo();
            node.IsExpanded = true;
            node.IsSelected = true;
            item.Text = "b";
            item.Value = value;
            item.Value = value;
            item.Value = new TemplateInfo();
            item.IsSelected = true;

            Assert.Equal(["Text", "Value", "Value", "IsExpanded", "IsSelected", "Text", "Value", "Value", "IsSelected"], changes);
            Assert.Equal([node.Value], node.Values);
        }
    }
}
