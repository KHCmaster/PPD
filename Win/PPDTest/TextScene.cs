using PPDMath;
using PPDFramework;

namespace PPDTest
{
    class TextScene(TestSceneManager testSceneManager, PPDDevice device) : TestSceneBase(testSceneManager, device)
    {
        protected override string Title
        {
            get
            {
                return "文字列表示テスト";
            }
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
                Position = new System.Numerics.Vector2(100, 100)
            });
        }
    }
}