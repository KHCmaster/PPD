using PPDFramework;
using SharpDX;

namespace PPDEditor
{
    class MainGameTop : GameComponent
    {
        enum State
        {
            downing = 0,
            still = 1
        }

        State state = State.downing;
        PictureObject top;

        public MainGameTop(PPDDevice device, PPDFramework.Resource.ResourceManager resourceManager) : base(device)
        {
            AddChild(top = new PictureObject(device, resourceManager, Utility.Path.Combine("game", "gametop.png")));
            top.Position = new Vector2(0, -top.Height);
        }

        protected override void UpdateImpl()
        {
            if (state == State.downing)
            {
                if (top.Position.Y >= 0)
                {
                    top.Position = Vector2.Zero;;

                    state = State.still;
                }
                else
                {
                    top.Position = new Vector2(0, top.Position.Y + 1);
                }
            }
        }
    }
}
