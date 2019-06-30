using PPDFramework;

namespace PPDTest
{
    class NumberImageScene : TestSceneBase
    {
        protected override string Title
        {
            get
            {
                return "数字画像表示テスト";
            }
        }

        public NumberImageScene(TestSceneManager testSceneManager, PPDDevice device) : base(testSceneManager, device)
        {

        }

        protected override void OnInitialize()
        {
            var pathManager = new PathManager(@"img\PPD\main_game");
            contentSprite.AddChild(new NumberPictureObject(device, ResourceManager, pathManager.Combine("num.png"))
            {
                Value = 12345,
                MaxDigit = -1
            });
        }
    }
}
