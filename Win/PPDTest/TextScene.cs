using PPDFramework;

namespace PPDTest
{
    class TextScene : TestSceneBase
    {
        protected override string Title
        {
            get
            {
                return "文字列表示テスト";
            }
        }

        public TextScene(TestSceneManager testSceneManager, PPDDevice device) : base(testSceneManager, device)
        {

        }

        protected override void OnInitialize()
        {
            contentSprite.AddChild(new TextureString(device, "ボ", 400, PPDColors.White)
            {
                Border = new Border
                {
                    Thickness = 1,
                    Color = PPDColors.Black
                },
                Position = new SharpDX.Vector2(100, 100)
            });
        }
    }
}