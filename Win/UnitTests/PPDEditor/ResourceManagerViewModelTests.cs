using PPDEditor;
using PPDEditor.Models;
using PPDEditor.ViewModels.Docks;
using System;
using System.Collections.Generic;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using Xunit;

namespace UnitTests.PPDEditor
{
    /// <summary>
    /// リソースのドックのテストです。プロジェクトは一時フォルダーに作る。
    /// </summary>
    public sealed class ResourceManagerViewModelTests : IDisposable
    {
        sealed class FakeResourceHost(string projectDir) : IResourceHost
        {
            public bool IsProjectLoaded { get; set; } = true;
            public string ProjectDir { get; } = projectDir;
            public Queue<string[]> Files { get; } = new();
            public List<string> Messages { get; } = [];
            public bool ConfirmAnswer { get; set; } = true;
            public List<string> Clipboard { get; } = [];

            public string[] AskFiles(string filter) => Files.Count > 0 ? Files.Dequeue() : [];

            public void ShowMessage(string text) => Messages.Add(text);

            public bool Confirm(string text, string caption) => ConfirmAnswer;

            public void SetClipboardText(string text) => Clipboard.Add(text);

            public List<KeyValuePair<object, object>> GetSoundInfo(string path) => [new("sound", Path.GetFileName(path))];
        }

        readonly TempFolder folder = new(setCurrentDirectory: true);
        readonly FakeResourceHost host;
        readonly ResourceManagerViewModel viewModel;

        public ResourceManagerViewModelTests()
        {
            AppFiles.CopyTo("PPDEditor", folder);
            Utility.ChangeLanguage("jp");
            host = new FakeResourceHost(folder.Combine("project"));
            viewModel = new ResourceManagerViewModel(host);
            viewModel.CreateResourceFolders();
        }

        public void Dispose()
        {
            folder.Dispose();
        }

        private string WritePng(string name)
        {
            var path = folder.Combine("src", name);
            Directory.CreateDirectory(Path.GetDirectoryName(path));
            using var bitmap = new System.Drawing.Bitmap(2, 2);
            bitmap.Save(path, ImageFormat.Png);
            return path;
        }

        [Fact]
        public void ReadResource_ListsFilesAndDisabled()
        {
            foreach (var name in new[] { "Image/a.png", "Image/b.png", "Sound/s.wav", "Others/o.bin" })
            {
                folder.Write(Path.Combine("project", "Resource", name), "");
            }

            viewModel.ReadResource(host.ProjectDir, @"Image\b.png|Image|Missing\x");

            Assert.Equal(["Image", "Effect", "Sound", "Others"], viewModel.Roots.Select(r => r.Name));
            Assert.Equal(["a.png", "b.png"], viewModel.GetImageList());
            Assert.Equal(["s.wav"], viewModel.GetSoundList());
            Assert.Equal(["o.bin"], viewModel.GetOthersList());
            Assert.Empty(viewModel.GetEffectList());
            // フォルダーは無効にしない
            Assert.False(viewModel.Roots[0].IsDisabled);
            Assert.Equal(@"Image\b.png", viewModel.GetDisableList());
            Assert.Equal([@"Image\a.png", @"Sound\s.wav", @"Others\o.bin"], viewModel.GetEnabledList());

            viewModel.Clear();
            Assert.All(viewModel.Roots, r => Assert.Empty(r.Children));
        }

        [Fact]
        public void AddFiles_AddsOrReportsExisting()
        {
            var image = WritePng("a.png");
            folder.Write(Path.Combine("src", "s.wav"), "");
            folder.Write(Path.Combine("src", "o.bin"), "");
            var changed = 0;
            viewModel.Changed += (sender, e) => changed++;

            viewModel.AddImageCommand.Execute(null);
            host.Files.Enqueue([image]);
            viewModel.AddImageCommand.Execute(null);
            host.Files.Enqueue([image]);
            viewModel.AddImageCommand.Execute(null);
            host.Files.Enqueue([folder.Combine("src", "s.wav")]);
            viewModel.AddSoundCommand.Execute(null);
            host.Files.Enqueue([folder.Combine("src", "o.bin")]);
            viewModel.AddOthersCommand.Execute(null);

            Assert.Equal(["a.png"], viewModel.GetImageList());
            Assert.Equal(["s.wav"], viewModel.GetSoundList());
            Assert.Equal(["o.bin"], viewModel.GetOthersList());
            Assert.Single(host.Messages);
            Assert.StartsWith(Utility.Language["AlreadyExistResource"], host.Messages[0]);
            Assert.Equal(4, changed);
        }

        [Fact]
        public void AddEffect_IncludesImagesInEnabledList()
        {
            folder.Write(Path.Combine("src", "fx.etd"), "<Effects />");
            WritePng(Path.Combine("fx", "img.png"));
            host.Files.Enqueue([folder.Combine("src", "fx.etd")]);

            viewModel.AddEffectCommand.Execute(null);

            Assert.Equal(["fx.etd"], viewModel.GetEffectList());
            Assert.Equal([@"Effect\fx.etd", @"Effect\fx\img.png"], viewModel.GetEnabledList());
        }

        [Fact]
        public void DeleteToggleAndCopyName()
        {
            folder.Write(Path.Combine("project", "Resource", "Others", "o.bin"), "");
            viewModel.ReadResource(host.ProjectDir, "");
            var node = viewModel.Roots[3].Children[0];

            viewModel.DeleteCommand.Execute(null);
            viewModel.ToggleDisabledCommand.Execute(null);
            viewModel.CopyNameCommand.Execute(null);
            viewModel.SelectedNode = node;
            viewModel.ToggleDisabledCommand.Execute(null);
            Assert.True(node.IsDisabled);
            viewModel.CopyNameCommand.Execute(null);
            Assert.Equal(["o.bin"], host.Clipboard);

            host.ConfirmAnswer = false;
            viewModel.DeleteCommand.Execute(null);
            Assert.Single(viewModel.Roots[3].Children);
            host.ConfirmAnswer = true;
            viewModel.DeleteCommand.Execute(null);
            Assert.Empty(viewModel.Roots[3].Children);
            Assert.False(File.Exists(Path.Combine(host.ProjectDir, "Resource", "Others", "o.bin")));
        }

        [Fact]
        public void SelectNode_ShowsInfoAndPreview()
        {
            File.Copy(WritePng("a.png"), Path.Combine(host.ProjectDir, "Resource", "Image", "a.png"));
            folder.Write(Path.Combine("project", "Resource", "Sound", "s.wav"), "");
            folder.Write(Path.Combine("project", "Resource", "Others", "o.bin"), "12");
            viewModel.ReadResource(host.ProjectDir, "");

            viewModel.SelectedNode = viewModel.Roots[0].Children[0];
            Assert.Equal("PNG", viewModel.InfoRows[0].Value);
            Assert.Equal(new ResourcePreview(ResourceKind.Image, Path.Combine(host.ProjectDir, "Resource", @"Image\a.png")), viewModel.Preview);

            viewModel.SelectedNode = viewModel.Roots[2].Children[0];
            Assert.Equal("s.wav", viewModel.InfoRows.Single().Value);
            // 以前と同じく、画像やエフェクトでないものを選んでも、前の中身の表示はそのまま
            Assert.Equal(ResourceKind.Image, viewModel.Preview.Kind);

            viewModel.SelectedNode = viewModel.Roots[3].Children[0];
            Assert.Equal(2L, viewModel.InfoRows.Single().Value);

            viewModel.SelectedNode = viewModel.Roots[0];
            Assert.Empty(viewModel.InfoRows);
            host.IsProjectLoaded = false;
            viewModel.SelectedNode = viewModel.Roots[0].Children[0];
            Assert.Empty(viewModel.InfoRows);
        }

        [Fact]
        public void SelectNode_BrokenImage_ShowsError()
        {
            folder.Write(Path.Combine("project", "Resource", "Image", "broken.png"), "not an image");
            viewModel.ReadResource(host.ProjectDir, "");

            viewModel.SelectedNode = viewModel.Roots[0].Children[0];

            Assert.Single(host.Messages);
            Assert.Empty(viewModel.InfoRows);
        }

        [Fact]
        public void CanAddAndWriteResource()
        {
            host.IsProjectLoaded = false;
            viewModel.RefreshCanAdd();
            Assert.False(viewModel.CanAdd);
            host.IsProjectLoaded = true;
            viewModel.RefreshCanAdd();
            Assert.True(viewModel.CanAdd);

            folder.Write(Path.Combine("project", "Resource", "Others", "x.txt"), "abc");
            var stream = new MemoryStream();
            viewModel.WriteResource(stream, @"Others\x.txt");
            Assert.Equal(3, stream.Length);
            Assert.True(viewModel.Roots[0].IsFolder);
        }
    }
}
