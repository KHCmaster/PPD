using PPDFramework;
using PPDFramework.Scene;

namespace PPDTest
{
    abstract class TestSceneBase : SceneBase
    {
        TestSceneManager testSceneManager;
        protected SpriteObject contentSprite;

        protected abstract string Title
        {
            get;
        }

        protected TestSceneBase(TestSceneManager testSceneManager, PPDDevice device) : base(device)
        {
            this.testSceneManager = testSceneManager;
        }

        public override bool Load()
        {
            this.AddChild(new TextureString(device, Title, 20, new SharpDX.Color4(1, 1, 1, 1))
            {
                Position = new SharpDX.Vector2(10, 10)
            });
            this.AddChild(contentSprite = new SpriteObject(device)
            {
                Position = new SharpDX.Vector2(10, 40)
            });
            OnInitialize();
            this.AddChild(new RectangleComponent(device, ResourceManager, PPDColors.Selection)
            {
                RectangleHeight = 450,
                RectangleWidth = 800
            });
            return true;
        }

        public override void Update(InputInfoBase inputInfo, MouseInfo mouseInfo)
        {
            base.Update(inputInfo, mouseInfo);
            if (inputInfo.IsPressed(ButtonType.R))
            {
                testSceneManager.Next(this);
            }
            else if (inputInfo.IsPressed(ButtonType.L))
            {
                testSceneManager.Previous(this);
            }
            base.Update();
        }

        protected virtual void OnInitialize()
        {

        }
    }
}
