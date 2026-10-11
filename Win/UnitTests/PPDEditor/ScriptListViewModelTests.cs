using PPDEditor;
using PPDEditor.ViewModels.Docks;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Xunit;

namespace UnitTests.PPDEditor
{
    /// <summary>
    /// スクリプトの一覧のテストです。
    /// </summary>
    /// <remarks>
    /// コマンドと関数はエディターのフォルダー（テストでは出力のフォルダー）にあるので、読むだけにする。作ったり消したりするのは、一時フォルダーのプロジェクトの譜面のスクリプトだけ。
    /// </remarks>
    public sealed class ScriptListViewModelTests : IDisposable
    {
        sealed class FakeScriptListHost(string projectDir) : IScriptListHost
        {
            public bool IsProjectLoaded { get; set; } = true;
            public string ProjectDir { get; } = projectDir;
            public AvailableDifficulty CurrentDifficulty { get; set; } = AvailableDifficulty.Hard;
            public List<string> Messages { get; } = [];
            public bool ConfirmAnswer { get; set; } = true;
            public int Confirmed { get; private set; }

            public void ShowMessage(string text) => Messages.Add(text);

            public bool Confirm(string text, string caption)
            {
                Confirmed++;
                return ConfirmAnswer;
            }
        }

        readonly TempFolder folder = new(setCurrentDirectory: true);
        readonly FakeScriptListHost host;
        readonly ScriptListViewModel viewModel;

        public ScriptListViewModelTests()
        {
            AppFiles.CopyTo("PPDEditor", folder);
            Utility.ChangeLanguage("jp");
            host = new FakeScriptListHost(folder.Combine("project"));
            folder.Write(Path.Combine("project", "Hard_Scripts", "Main.fsml"), "main");
            folder.Write(Path.Combine("project", "Hard_Scripts", "Sub", "Helper.FSML"), "helper");
            folder.Write(Path.Combine("project", "Hard_Scripts", "Sub", "notes.txt"), "");
            viewModel = new ScriptListViewModel(host);
            viewModel.ReadScript(host.ProjectDir, AvailableDifficulty.Hard, @"Scripts\Sub\Helper|Scripts\Sub|Scripts\Missing");
        }

        public void Dispose()
        {
            folder.Dispose();
        }

        private ScriptNode Find(params string[] names)
        {
            var node = viewModel.ScriptRoot;
            foreach (var name in names)
            {
                node = node.FindChild(name);
            }
            return node;
        }

        [Fact]
        public void Startup_ReadsEditorFoldersAndChartScripts()
        {
            Assert.Equal(["Commands", "Functions", "Scripts"], viewModel.Roots.Select(r => r.Name));
            Assert.NotEmpty(viewModel.CommandRoot.Children);
            Assert.NotEmpty(viewModel.FunctionRoot.Children);
            Assert.Equal(["Sub", "Main"], viewModel.ScriptRoot.Children.Select(c => c.Name));
            Assert.Equal(["Helper"], Find("Sub").Children.Select(c => c.Name));

            // フォルダーは無効にしない
            Assert.False(Find("Sub").IsDisabled);
            Assert.True(Find("Sub", "Helper").IsDisabled);
            Assert.Equal([@"Scripts\Main"], viewModel.GetEnabledScriptList());
            Assert.Equal(@"Scripts\Sub\Helper", viewModel.GetDisabledScriptList());
            Assert.Equal(folder.Combine("project", @"Hard_Scripts\Sub\Helper"), Find("Sub", "Helper").FullPath);
            Assert.True(Find("Main").IsUnderScripts);
            Assert.False(viewModel.CommandRoot.IsUnderScripts);
        }

        [Fact]
        public void SelectionAndOpen()
        {
            var opened = new List<string>();
            viewModel.ScriptSelected += opened.Add;
            Assert.Null(viewModel.CurrentScriptPath);
            Assert.False(viewModel.ChildNodeSelected);

            viewModel.ScriptRoot.IsSelected = true;
            Assert.False(viewModel.ChildNodeSelected);
            viewModel.OpenSelectedCommand.Execute(null);
            Find("Main").IsSelected = true;
            Assert.True(viewModel.ChildNodeSelected);
            Assert.Equal(folder.Combine("project", @"Hard_Scripts\Main") + ".fsml", viewModel.CurrentScriptPath);
            viewModel.OpenSelectedCommand.Execute(null);

            Assert.Equal([folder.Combine("project", @"Hard_Scripts\Main") + ".fsml"], opened);
        }

        [Fact]
        public void WriteScript_CopiesContent()
        {
            var stream = new MemoryStream();
            viewModel.WriteScript(stream, @"Scripts\Sub\Helper");
            Assert.Equal("helper", System.Text.Encoding.UTF8.GetString(stream.ToArray()));

            var missing = new MemoryStream();
            viewModel.WriteScript(missing, @"Scripts\None");
            Assert.Equal(0, missing.Length);
        }

        [Fact]
        public void CreateFolderAndScript_UseFreeNames()
        {
            var modified = 0;
            viewModel.Modified += (sender, e) => modified++;
            viewModel.CreateFolderCommand.Execute(null);

            Find("Sub").IsSelected = true;
            viewModel.CreateFolderCommand.Execute(null);
            viewModel.CreateFolderCommand.Execute(null);
            viewModel.CreateScriptCommand.Execute(null);
            viewModel.CreateScriptCommand.Execute(null);

            Assert.Equal(["Helper", "New Folder", "New Folder(1)", "New Script", "New Script(1)"], Find("Sub").Children.Select(c => c.Name));
            Assert.True(Directory.Exists(folder.Combine("project", "Hard_Scripts", "Sub", "New Folder(1)")));
            Assert.True(File.Exists(folder.Combine("project", "Hard_Scripts", "Sub", "New Script(1).fsml")));
            Assert.True(Find("Sub").IsExpanded);
            Assert.Equal(2, modified);

            // スクリプトを選んでいるときは、同じフォルダーに作る
            Find("Main").IsSelected = true;
            viewModel.CreateFolderCommand.Execute(null);
            Assert.NotNull(Find("New Folder"));
        }

        [Fact]
        public void CreateInChartScripts_NeedsProject()
        {
            host.IsProjectLoaded = false;
            Find("Sub").IsSelected = true;

            viewModel.CreateFolderCommand.Execute(null);
            viewModel.CreateScriptCommand.Execute(null);

            Assert.Single(Find("Sub").Children);
        }

        [Fact]
        public void Delete_RemovesFileOrFolder()
        {
            var deleted = new List<string>();
            viewModel.Deleted += deleted.Add;
            viewModel.DeleteCommand.Execute(null);

            Find("Main").IsSelected = true;
            host.ConfirmAnswer = false;
            viewModel.DeleteCommand.Execute(null);
            Assert.NotNull(Find("Main"));
            host.ConfirmAnswer = true;
            viewModel.DeleteCommand.Execute(null);
            Assert.Null(Find("Main"));
            Assert.False(File.Exists(folder.Combine("project", "Hard_Scripts", "Main.fsml")));

            Find("Sub").IsSelected = true;
            viewModel.DeleteCommand.Execute(null);
            Assert.False(Directory.Exists(folder.Combine("project", "Hard_Scripts", "Sub")));
            Assert.Equal([folder.Combine("project", @"Hard_Scripts\Main.fsml"), folder.Combine("project", @"Hard_Scripts\Sub")], deleted);
        }

        [Fact]
        public void Delete_RootIsIgnored()
        {
            // 一番上のフォルダーは確かめもしないで、何もしない
            viewModel.ScriptRoot.IsSelected = true;

            viewModel.DeleteCommand.Execute(null);

            Assert.Equal(0, host.Confirmed);
            Assert.True(Directory.Exists(folder.Combine("project", "Hard_Scripts")));
        }

        [Fact]
        public void ToggleDisabled()
        {
            viewModel.ToggleDisabledCommand.Execute(null);
            Find("Main").IsSelected = true;

            viewModel.ToggleDisabledCommand.Execute(null);

            Assert.True(Find("Main").IsDisabled);
            Assert.Empty(viewModel.GetEnabledScriptList());
        }

        [Fact]
        public void Rename_ChecksNamesAndMovesFiles()
        {
            var renamed = new List<(string Before, string After)>();
            viewModel.Renamed += (before, after) => renamed.Add((before, after));
            var main = Find("Main");

            viewModel.BeginRenameCommand.Execute(null);
            viewModel.ScriptRoot.IsSelected = true;
            viewModel.BeginRenameCommand.Execute(null);
            Assert.False(viewModel.ScriptRoot.IsEditing);

            main.IsSelected = true;
            viewModel.BeginRenameCommand.Execute(null);
            Assert.Equal(("Main", true), (main.EditingName, main.IsEditing));
            main.EditingName = "Renamed";
            main.EndRenameCommand.Execute(false);
            Assert.Equal("Main", main.Name);

            foreach (var bad in new[] { "", "Main", "bad|name" })
            {
                viewModel.BeginRenameCommand.Execute(null);
                main.EditingName = bad;
                main.EndRenameCommand.Execute(true);
                Assert.Equal("Main", main.Name);
            }
            main.EndRenameCommand.Execute(true);

            folder.Write(Path.Combine("project", "Hard_Scripts", "Taken.fsml"), "");
            viewModel.BeginRenameCommand.Execute(null);
            main.EditingName = "Taken";
            main.EndRenameCommand.Execute(true);
            Assert.Equal([Utility.Language["AlreadyExistName"]], host.Messages);

            viewModel.BeginRenameCommand.Execute(null);
            main.EditingName = "Renamed";
            main.EndRenameCommand.Execute(true);
            Assert.Equal("Renamed", main.Name);
            Assert.True(File.Exists(folder.Combine("project", "Hard_Scripts", "Renamed.fsml")));

            var sub = Find("Sub");
            sub.IsSelected = true;
            Directory.CreateDirectory(folder.Combine("project", "Hard_Scripts", "Other"));
            viewModel.BeginRenameCommand.Execute(null);
            sub.EditingName = "Other";
            sub.EndRenameCommand.Execute(true);
            Assert.Equal(2, host.Messages.Count);
            viewModel.BeginRenameCommand.Execute(null);
            sub.EditingName = "Folder";
            sub.EndRenameCommand.Execute(true);
            Assert.True(File.Exists(folder.Combine("project", "Hard_Scripts", "Folder", "Helper.FSML")));
            Assert.Equal(2, renamed.Count);
        }

        [Fact]
        public void Reload_KeepsDisabledAndFindsNewFiles()
        {
            var reloaded = 0;
            viewModel.Reloaded += () => reloaded++;
            folder.Write(Path.Combine("project", "Hard_Scripts", "Added.fsml"), "");

            viewModel.ReloadCommand.Execute(null);

            Assert.Equal(1, reloaded);
            Assert.NotNull(Find("Added"));
            Assert.True(Find("Sub", "Helper").IsDisabled);

            host.IsProjectLoaded = false;
            viewModel.ReloadCommand.Execute(null);
            Assert.Equal(1, reloaded);
            viewModel.Clear();
            Assert.Empty(viewModel.ScriptRoot.Children);
        }

        [Fact]
        public void CopyFunction_ToChartScripts()
        {
            var samples = viewModel.FunctionRoot.FindChild("Samples");
            var sampleScript = samples.GetScripts().First();
            Assert.True(viewModel.CanDragFunction(sampleScript));
            Assert.False(viewModel.CanDragFunction(viewModel.FunctionRoot));
            Assert.False(viewModel.CanDragFunction(Find("Main")));
            Assert.True(viewModel.CanDropFunction(sampleScript, Find("Sub")));
            Assert.False(viewModel.CanDropFunction(Find("Main"), Find("Sub")));
            Assert.False(viewModel.CanDropFunction(sampleScript, sampleScript));

            // スクリプトの上に落としたら、そのフォルダーに入れる
            viewModel.CopyFunction(sampleScript, Find("Sub", "Helper"));
            Assert.NotNull(Find("Sub", sampleScript.Name));

            host.ConfirmAnswer = false;
            viewModel.CopyFunction(sampleScript, Find("Sub"));
            Assert.Equal(1, host.Confirmed);

            // フォルダーごとコピーする
            var sampleFolder = samples.Children.First(c => c.IsFolder);
            viewModel.CopyFunction(sampleFolder, viewModel.ScriptRoot);
            Assert.NotEmpty(Find(sampleFolder.Name).GetScripts());
        }
    }
}
