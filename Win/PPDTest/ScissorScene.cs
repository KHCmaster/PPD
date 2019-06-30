using PPDFramework;

namespace PPDTest
{
    class ScissorScene : TestSceneBase
    {
        protected override string Title
        {
            get
            {
                return "シザーテスト";
            }
        }

        public ScissorScene(TestSceneManager testSceneManager, PPDDevice device) : base(testSceneManager, device)
        {

        }

        protected override void OnInitialize()
        {
            var pathManager = new PathManager(@"img\PPD\main_game");
            contentSprite.AddChild(new PictureObject(device, ResourceManager, pathManager.Combine("num.png")));
            contentSprite.Clip = new ClipInfo(GameHost)
            {
                Height = 200,
                Width = 200
            };
        }
    }
}
