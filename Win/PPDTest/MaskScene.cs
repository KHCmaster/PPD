using PPDMath;
using PPDFramework;

namespace PPDTest
{
    class MaskScene(TestSceneManager testSceneManager, PPDDevice device) : TestSceneBase(testSceneManager, device)
    {
        protected override string Title
        {
            get
            {
                return "画像マスクテスト";
            }
        }

        protected override void OnInitialize()
        {
            var pathManager = new PathManager(@"img\PPD\main_game");
            contentSprite.AddChild(new PictureObject(device, ResourceManager, pathManager.Combine("num.png"))
            {
                Mask = new PictureObject(device, ResourceManager, pathManager.Combine("num.png"))
                {
                    Position = new System.Numerics.Vector2(10, 10)
                }
            });
            contentSprite.AddChild(new PictureObject(device, ResourceManager, pathManager.Combine("num.png"))
            {
                Position = new System.Numerics.Vector2(0, 40),
                Mask = new PictureObject(device, ResourceManager, pathManager.Combine("num.png"))
                {
                    Position = new System.Numerics.Vector2(10, 10)
                },
                MaskType = PPDFramework.Shaders.MaskType.Exclude
            });
        }
    }
}
