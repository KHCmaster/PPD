using PPDFramework;
using PPDFramework.Scene;
using SharpDX;

namespace PPDSingle
{
    public class TestMenu : SceneBase
    {
        public TestMenu(PPDDevice device) : base(device)
        {

        }

        public override bool Load()
        {
            SpriteObject sprite;
            AddChild(sprite = new SpriteObject(device)
            {
                Position = new SharpDX.Vector2(100, 100),
                Alpha = 0.5f
            });
            sprite.AddChild(new PictureObject(device, ResourceManager, Utility.Path.Combine("confirm.png"))
            {
                Position = new SharpDX.Vector2(100, 100),
                Alpha = 0.5f
            });
            sprite.AddChild(new TextureString(device, "AAAAAAAAAAAAAAAAAA", 20, 200, PPDColors.White)
            {
                Position = new Vector2(0, 200)
            });
            return true;
        }

        public override void Update(InputInfoBase inputInfo, MouseInfo mouseInfo)
        {
            base.Update();
        }
    }
}
