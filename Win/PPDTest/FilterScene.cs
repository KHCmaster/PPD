using PPDFramework;
using PPDFramework.Shaders;
using System;

namespace PPDTest
{
    class FilterScene : TestSceneBase
    {
        protected override string Title
        {
            get
            {
                return "フィルター表示テスト";
            }
        }

        public FilterScene(TestSceneManager testSceneManager, PPDDevice device) : base(testSceneManager, device)
        {

        }

        protected override void OnInitialize()
        {
            var pathManager = new PathManager(@"img\PPD\main_game");
            Add(pathManager, new ColorFilter() { Weight = 1, Color = new SharpDX.Color4(1, 0, 0, 1) });
            Add(pathManager, new MaxGrayScaleColorFilter() { Weight = 1 });
            Add(pathManager, new MiddleGrayScaleColorFilter() { Weight = 1 });
            Add(pathManager, new NTSCGrayScaleColorFilter() { Weight = 1 });
            Add(pathManager, new HDTVGrayScaleColorFilter() { Weight = 1 });
            Add(pathManager, new AverageGrayScaleColorFilter() { Weight = 1 });
            Add(pathManager, new GreenGrayScaleColorFilter() { Weight = 1 });
            Add(pathManager, new MedianGrayScaleColorFilter() { Weight = 1 });
            Add(pathManager, new HueColorFilter() { Weight = 1, Rotation = (float)Math.PI });
            Add(pathManager, new SaturationColorFilter() { Weight = 1, Scale = 2 });
            Add(pathManager, new BrightnessColorFilter() { Weight = 1, Scale = 2 });
            Add(pathManager, new InvertColorFilter() { Weight = 1 });
        }

        private void Add(PathManager pathManager, ColorFilterBase filter)
        {
            PictureObject temp;
            var offset = contentSprite.ChildrenCount / 2;
            contentSprite.AddChild(new TextureString(device, filter.GetType().Name, 20, new SharpDX.Color4(1, 1, 1, 1))
            {
                Position = new SharpDX.Vector2(0, offset * 30),
            });
            contentSprite.AddChild(temp = new PictureObject(device, ResourceManager, pathManager.Combine("num.png"))
            {
                Position = new SharpDX.Vector2(400, offset * 30),
            });
            temp.ColorFilters.Add(filter);
        }
    }
}
