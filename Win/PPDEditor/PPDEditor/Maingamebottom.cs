using PPDFramework;
using SharpDX;

namespace PPDEditor
{
    class MainGameBottom : GameComponent
    {
        enum State
        {
            downing = 0,
            still = 1
        }

        State state = State.downing;
        PictureObject bottom;

        public MainGameBottom(PPDDevice device, PPDFramework.Resource.ResourceManager resourceManager) : base(device)
        {
            AddChild(bottom = new PictureObject(device, resourceManager, Utility.Path.Combine("game", "gamebottom.png"))
            {
                Position = new Vector2(0, 450)
            });
        }

        protected override void UpdateImpl()
        {
            if (state == State.downing)
            {
                if (bottom.Position.Y <= 450 - bottom.Height)
                {
                    bottom.Position = new Vector2(0, 450 - bottom.Height);

                    state = State.still;
                }
                else
                {
                    bottom.Position = new Vector2(0, bottom.Position.Y - 2);

                }
            }
        }
    }
}