using PPDEditor;
using System;
using System.IO;
using System.Numerics;
using Xunit;
using static UnitTests.Graphics.GraphicsDevice;

namespace UnitTests.Graphics
{
    /// <summary>
    /// エディターのゲームの画面（MainGame）の部品（上下の帯、方眼、位置と角度の読み込みの表示、歌詞）のテストです。
    /// </summary>
    /// <remarks>
    /// エディターの画像（img\PPDEditor）は、一時フォルダーに赤い画像として作る。
    /// </remarks>
    [Collection(GraphicsCollection.Name)]
    public sealed class EditorGameViewTests : IDisposable
    {
        readonly GraphicsDevice graphics;
        readonly TempFolder folder = new(setCurrentDirectory: true);
        readonly PPDFramework.Resource.ResourceManager resources = new();

        public EditorGameViewTests(GraphicsDevice graphics)
        {
            this.graphics = graphics;
            foreach (var name in new[] { "assist/up.png", "assist/upselect.png", "assist/right.png", "assist/rightselect.png", "assist/area.png",
                "assist/circle.png", "assist/arrow.png", "game/gametop.png", "game/gamebottom.png" })
            {
                var path = folder.Combine("img", "PPDEditor", name);
                Directory.CreateDirectory(Path.GetDirectoryName(path));
                using var bitmap = new System.Drawing.Bitmap(8, 8);
                using (var g = System.Drawing.Graphics.FromImage(bitmap))
                {
                    g.Clear(System.Drawing.Color.Red);
                }
                bitmap.Save(path, System.Drawing.Imaging.ImageFormat.Png);
            }
        }

        public void Dispose()
        {
            resources.Dispose();
            folder.Dispose();
        }

        [Fact]
        public void TopAndBottom_SlideIntoPlace()
        {
            // 方眼は画面より大きくして、線を出さない
            var grid = new SquareGrid { Width = 1000, Height = 1000 };
            using var game = new MainGame(graphics.Device, resources, grid);
            game.MarkPointDrawer.Hidden = true;
            // 選んだマークがない（タイムラインがない）ときは、補助の矢印などを隠す
            game.UpdateAssist(MarkSelectMode.Angle, MarkSelectMode.Angle);

            // 最初は画面の外
            var pixels = graphics.Render(game);
            Assert.Equal((0, 0, 0), Rgb(Pixel(pixels, 4, 4)));
            Assert.Equal((0, 0, 0), Rgb(Pixel(pixels, 4, 443)));

            for (var i = 0; i < 20; i++)
            {
                game.Update();
            }

            // 上の帯は上から、下の帯は下から出てきて止まる
            pixels = graphics.Render(game);
            Assert.Equal((255, 0, 0), Rgb(Pixel(pixels, 4, 4)));
            Assert.Equal((255, 0, 0), Rgb(Pixel(pixels, 4, 443)));
            Assert.Equal((0, 0, 0), Rgb(Pixel(pixels, 4, 20)));
            Assert.Same(grid, typeof(GridComponent).GetField("grid", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic).GetValue(game.Grid));
            Assert.Null(game.Movie);
            Assert.Equal("", game.Kasi.Text);
        }

        [Fact]
        public void Grid_FollowsChanges()
        {
            var grid = new SquareGrid { Width = 100, Height = 100, OffsetX = 50, OffsetY = 50, GridColor = System.Drawing.Color.Lime };
            using var component = new GridComponent(graphics.Device, resources, grid);

            var pixels = graphics.Render(component);
            // 縦の線は 50、150、… に、横の線は 50、150、… にある（偶数番目は太さ 2、奇数番目は太さ 1）
            Assert.Equal((0, 255, 0), Rgb(Pixel(pixels, 50, 10)));
            Assert.Equal((0, 255, 0), Rgb(Pixel(pixels, 149, 10)));
            Assert.Equal((0, 255, 0), Rgb(Pixel(pixels, 10, 50)));
            Assert.Equal((0, 0, 0), Rgb(Pixel(pixels, 100, 10)));

            // 方眼を変えたら作り直す（オフセットは幅で割った余り）
            grid.OffsetX = -20;
            grid.GridColor = System.Drawing.Color.Blue;
            component.Update();
            pixels = graphics.Render(component);
            Assert.Equal((0, 0, 255), Rgb(Pixel(pixels, 80, 10)));
            Assert.Equal((0, 0, 0), Rgb(Pixel(pixels, 50, 10)));
            Assert.Equal(80, grid.NormalizedOffsetX);
        }

        [Fact]
        public void MarkPoints_ShowPositionsOverAWhiteVeil()
        {
            using var drawer = new MarkPointDrawer(graphics.Device, resources);

            drawer.ChangeData([new Vector2(100, 100), new Vector2(300, 200)], [0, MathF.PI / 2]);
            var pixels = graphics.Render(drawer);

            // 半透明の白の上に、印（赤）を出す
            Assert.Equal((204, 204, 204), Rgb(Pixel(pixels, 600, 400)));
            Assert.Equal(255, Rgb(Pixel(pixels, 100, 100)).R);
            Assert.Equal(255, Rgb(Pixel(pixels, 300, 200)).R);

            // 角度の矢印は、角度も読み込むときだけ
            drawer.DrawAngle = true;
            Assert.True(drawer.DrawAngle);
            drawer.DrawAngle = false;
            Assert.False(drawer.DrawAngle);

            // 読み込み直すと、前の印は消える
            drawer.ChangeData([], []);
            Assert.Equal((204, 204, 204), Rgb(Pixel(graphics.Render(drawer), 100, 100)));
        }
    }
}
