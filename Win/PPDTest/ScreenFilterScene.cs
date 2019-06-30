using PPDFramework;
using PPDFramework.ScreenFilters;
using PPDFramework.Shaders;

namespace PPDTest
{
    class ScreenFilterScene : TestSceneBase
    {
        protected override string Title
        {
            get
            {
                return "スクリーンフィルタテスト";
            }
        }

        public ScreenFilterScene(TestSceneManager testSceneManager, PPDDevice device) : base(testSceneManager, device)
        {

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
