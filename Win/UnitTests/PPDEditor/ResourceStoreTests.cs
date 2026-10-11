using Effect2D;
using PPDEditor;
using PPDEditor.Models;
using PPDFramework;
using PPDFramework.PPDStructure;
using PPDFramework.PPDStructure.PPDData;
using System;
using System.Collections.Generic;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using Utility = PPDEditor.Utility;
using Xunit;

namespace UnitTests.PPDEditor
{
    public sealed class ResourceStoreTests : IDisposable
    {
        readonly TempFolder folder = new(setCurrentDirectory: true);
        readonly string projectDir;

        public ResourceStoreTests()
        {
            AppFiles.CopyTo("PPDEditor", folder);
            Utility.ChangeLanguage("jp");
            projectDir = folder.Combine("project");
            Directory.CreateDirectory(projectDir);
        }

        public void Dispose()
        {
            folder.Dispose();
        }

        private string WritePng(string relativePath, int width = 4, int height = 3)
        {
            var path = folder.Combine(relativePath);
            Directory.CreateDirectory(Path.GetDirectoryName(path));
            using var bitmap = new System.Drawing.Bitmap(width, height);
            bitmap.Save(path, ImageFormat.Png);
            return path;
        }

        // 画像を 1 枚使うエフェクトを、Effect2DEditor と同じく画像のフォルダー付きで保存する
        private string WriteEffect(string name)
        {
            var image = WritePng(Path.Combine("src", name + "_image.png"));
            var manager = new EffectManager();
            var effect = new BaseEffect { Filename = image };
            effect.SetDefault();
            manager.Effects.Add(effect);
            var path = folder.Combine("src", name + ".etd");
            global::Effect2DEditor.Models.EffectFile.Save(manager, path);
            return path;
        }

        [Fact]
        public void CreateFoldersAndFolderNames()
        {
            ResourceStore.CreateFolders(projectDir);

            Assert.Equal(["Effect", "Image", "Others", "Sound"], Directory.GetDirectories(Path.Combine(projectDir, "Resource")).Select(Path.GetFileName).Order());
            Assert.Equal("Others", ResourceStore.GetFolderName(ResourceKind.Folder));
        }

        [Fact]
        public void GetFiles_FiltersByKind()
        {
            foreach (var name in new[] { "Image/a.png", "Image/b.JPG", "Image/c.txt", "Sound/s.ogg", "Sound/t.mp3", "Effect/e.etd", "Others/x.bin" })
            {
                folder.Write(Path.Combine("project", "Resource", name), "");
            }

            Assert.Equal(["a.png", "b.JPG"], ResourceStore.GetFiles(projectDir, ResourceKind.Image).Order());
            Assert.Equal(["s.ogg"], ResourceStore.GetFiles(projectDir, ResourceKind.Sound));
            Assert.Equal(["e.etd"], ResourceStore.GetFiles(projectDir, ResourceKind.Effect));
            Assert.Equal(["x.bin"], ResourceStore.GetFiles(projectDir, ResourceKind.Others));
        }

        [Fact]
        public void Add_CopiesFileOrReportsExisting()
        {
            var image = WritePng(Path.Combine("src", "a.png"));
            var added = new List<string>();
            var existing = new List<string>();
            ResourceStore.CreateFolders(projectDir);

            ResourceStore.Add(projectDir, ResourceKind.Image, image, added.Add, existing.Add);
            ResourceStore.Add(projectDir, ResourceKind.Image, image, added.Add, existing.Add);

            Assert.Equal(["a.png"], added);
            Assert.Equal([image], existing);
            Assert.True(File.Exists(Path.Combine(projectDir, "Resource", "Image", "a.png")));
        }

        [Fact]
        public void Add_EffectCopiesImagesAndReferencedEffects()
        {
            WriteEffect("child");
            folder.Write(Path.Combine("src", "parent.etd"), "<Effects><EffectReference FilePath=\"Child.etd\" /><EffectReference FilePath=\"missing.etd\" /></Effects>");
            folder.Write(Path.Combine("src", "parent", "p.png"), "");
            var added = new List<string>();
            ResourceStore.CreateFolders(projectDir);

            ResourceStore.Add(projectDir, ResourceKind.Effect, folder.Combine("src", "parent.etd"), added.Add, _ => { });

            Assert.Equal(["parent.etd", "child.etd"], added);
            Assert.True(File.Exists(Path.Combine(projectDir, "Resource", "Effect", "parent", "p.png")));
            Assert.True(File.Exists(Path.Combine(projectDir, "Resource", "Effect", "child", "child_image.png")));
            Assert.Equal([@"Effect\child\child_image.png"], ResourceStore.GetEffectImages(projectDir, "child.etd"));
        }

        [Fact]
        public void Delete_EffectAlsoDeletesItsImages()
        {
            ResourceStore.CreateFolders(projectDir);
            ResourceStore.Add(projectDir, ResourceKind.Effect, WriteEffect("fx"), _ => { }, _ => { });
            Assert.True(Directory.Exists(Path.Combine(projectDir, "Resource", "Effect", "fx")));

            ResourceStore.Delete(projectDir, @"Effect\fx.etd");

            Assert.False(File.Exists(Path.Combine(projectDir, "Resource", "Effect", "fx.etd")));
            // 以前はフォルダーの名前に拡張子を付けて探していたので、画像のフォルダーが残っていた
            Assert.False(Directory.Exists(Path.Combine(projectDir, "Resource", "Effect", "fx")));
        }

        [Fact]
        public void Delete_ImageAndMissingFile()
        {
            folder.Write(Path.Combine("project", "Resource", "Image", "a.png"), "");

            ResourceStore.Delete(projectDir, @"Image\a.png");
            ResourceStore.Delete(projectDir, @"Image\none.png");

            Assert.False(File.Exists(Path.Combine(projectDir, "Resource", "Image", "a.png")));
        }

        [Fact]
        public void Write_CopiesContent()
        {
            folder.Write(Path.Combine("project", "Resource", "Others", "x.txt"), "content");
            var stream = new MemoryStream();

            ResourceStore.Write(projectDir, stream, @"Others\x.txt");

            Assert.Equal("content", System.Text.Encoding.UTF8.GetString(stream.ToArray()));
        }

        [Fact]
        public void GetImageInfo_ShowsFormatAndSize()
        {
            var info = ResourceStore.GetImageInfo(WritePng("info.png", 5, 7));

            Assert.Equal(("PNG", 5, 7), ((string)info[0].Value, (int)info[1].Value, (int)info[2].Value));
            Assert.Equal(Utility.Language["ImageKind"], info[0].Key);
        }

        [Fact]
        public void GetEffectInfo_CountsLayers()
        {
            var info = ResourceStore.GetEffectInfo(WriteEffect("fx")).ToDictionary(p => (string)p.Key, p => p.Value);

            Assert.Equal(1, info[Utility.Language["LayerCount"]]);
            Assert.Equal("", info[Utility.Language["ReferenceEffect"]]);
        }

        [Fact]
        public void GetOthersInfo_ScoreOrSize()
        {
            var score = folder.Combine("score.ppd");
            PPDWriter.Write(score, [new MarkData(0, 0, 0, 2, ButtonType.Square, 0), new ExMarkData(0, 0, 0, 1, 5, ButtonType.Cross, 1), new MarkData(0, 0, 0, 3, ButtonType.Circle, 2)]);
            folder.Write("other.bin", "12345");
            var empty = folder.Combine("empty.ppd");
            PPDWriter.Write(empty, []);

            var scoreInfo = ResourceStore.GetOthersInfo(score).Select(p => p.Value).ToList();
            Assert.Equal([2, 1, 1f, 5f], scoreInfo);
            Assert.Equal([0, 0, 0f, 0f], ResourceStore.GetOthersInfo(empty).Select(p => p.Value));
            Assert.Equal(5L, Assert.Single(ResourceStore.GetOthersInfo(folder.Combine("other.bin"))).Value);
        }
    }
}
