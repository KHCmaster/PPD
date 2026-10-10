using PPDFramework;

namespace PPDTest
{
    class NumberImageScene(TestSceneManager testSceneManager, PPDDevice device) : TestSceneBase(testSceneManager, device)
    {
        protected override string Title
        {
            get
            {
                return "数字画像表示テスト";
            }
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
