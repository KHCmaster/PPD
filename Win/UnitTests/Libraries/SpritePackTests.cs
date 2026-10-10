using PPDFramework.Sprites;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using Xunit;

namespace UnitTests.Libraries
{
    /// <summary>
    /// 画像を 1 枚の大きな画像（スプライト）に詰める処理のテストです。詰めた位置が重ならないことを確かめる。
    /// </summary>
    /// <remarks>
    /// スプライトは今のフォルダーの sprites に書くので、今のフォルダーを一時フォルダーにする。
    /// </remarks>
    public class SpritePackTests
    {
        static void WriteImage(TempFolder folder, string name, int width, int height)
        {
            var path = folder.Combine("img", name);
            Directory.CreateDirectory(Path.GetDirectoryName(path));
            using var bitmap = new Bitmap(width, height);
            bitmap.Save(path, ImageFormat.Png);
        }

        [Fact]
        public void Pack_PlacesImagesWithoutOverlap()
        {
            using var folder = new TempFolder(setCurrentDirectory: true);
            // 幅も高さも違う画像をたくさん（最初の大きさに収まらず、大きくし直す）
            WriteImage(folder, "wide.png", 300, 10);
            WriteImage(folder, "tall.png", 30, 200);
            WriteImage(folder, Path.Combine("sub", "square.png"), 100, 100);
            for (var i = 0; i < 20; i++)
            {
                WriteImage(folder, $"small{i}.png", 20 + i, 15 + i % 5);
            }
            File.WriteAllText(folder.Combine("img", "note.txt"), "not an image");

            var manager = new DirSpriteManager("img");
            manager.Pack();

            var sprites = manager.Dicts.SelectMany(d => d.SpritesWithActualPath.Values.Select(s => (Dict: d, Sprite: s))).ToArray();
            Assert.Equal(23, sprites.Length);
            foreach (var group in sprites.GroupBy(s => s.Dict))
            {
                var rects = group.Select(s => new Rectangle(s.Sprite.X, s.Sprite.Y, s.Sprite.SpaceWidth, s.Sprite.SpaceHeight)).ToArray();
                for (var i = 0; i < rects.Length; i++)
                {
                    for (var j = i + 1; j < rects.Length; j++)
                    {
                        Assert.False(rects[i].IntersectsWith(rects[j]), $"{rects[i]} と {rects[j]} が重なる");
                    }
                }
            }
            Assert.Contains(sprites, s => s.Sprite.Width == 300 && s.Sprite.Height == 10);

            // 変わっていなければ、詰め直さない
            var written = File.GetLastWriteTimeUtc(manager.MetaXmlPath);
            var again = new DirSpriteManager("img");
            again.Pack();
            Assert.Equal(written, File.GetLastWriteTimeUtc(again.MetaXmlPath));
            Assert.Equal(manager.Dicts.Length, again.Dicts.Length);
        }
    }
}
