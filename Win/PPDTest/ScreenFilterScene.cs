using PPDFramework;
using PPDFramework.ScreenFilters;
using PPDFramework.Shaders;

namespace PPDTest
{
    class ScreenFilterScene(TestSceneManager testSceneManager, PPDDevice device) : TestSceneBase(testSceneManager, device)
    {
        protected override string Title
        {
            get
            {
                return "スクリーンフィルタテスト";
            }
        }

        protected override void OnInitialize()
        {
            var pathManager = new PathManager(@"img\PPD\main_game");
            contentSprite.AddChild(new PictureObject(device, ResourceManager, pathManager.Combine("num.png")));
            contentSprite.PostScreenFilters.Add(new GaussianFilter
            {
                Disperson = 16
            });
            var filter = new ColorScreenFilter();
            filter.Filters.Add(new InvertColorFilter { Weight = 1 });
            this.PostScreenFilters.Add(filter);
        }
    }
}
