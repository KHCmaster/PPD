using PPDMath;
using PPDFramework;
using System;

namespace PPDTest
{
    class ImageScene(TestSceneManager testSceneManager, PPDDevice device) : TestSceneBase(testSceneManager, device)
    {
        protected override string Title
        {
            get
            {
                return "画像表示テスト";
            }
        }

        protected override void OnInitialize()
        {
            var pathManager = new PathManager(@"img\PPD\main_game");
            foreach (var blendMode in Enum.GetValues<Effect2D.BlendMode>())
            {
                contentSprite.AddChild(new TextureString(device, blendMode.ToString(), 20, new Vortice.Mathematics.Color4(1, 1, 1, 1))
                {
                    Position = new System.Numerics.Vector2(0, ((int)blendMode) * 30),
                });
                contentSprite.AddChild(new PictureObject(device, ResourceManager, pathManager.Combine("num.png"))
                {
                    Position = new System.Numerics.Vector2(160, ((int)blendMode) * 30),
                    BlendMode = blendMode,
                });
            }
        }

        public override void Update(InputInfoBase inputInfo, MouseInfo mouseInfo)
        {
            base.Update(inputInfo, mouseInfo);
            if (inputInfo.GetPressingFrame(ButtonType.Up) > 0)
            {
                if (contentSprite.Alpha < 1)
                {
                    contentSprite.Alpha += 0.01f;
                }
            }
            else if (inputInfo.GetPressingFrame(ButtonType.Down) > 0)
            {
                if (contentSprite.Alpha > 0)
                {
                    contentSprite.Alpha -= 0.01f;
                }
            }
        }
    }
}