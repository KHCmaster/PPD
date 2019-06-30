using PPDFramework;
using PPDFramework.Shaders;
using System;

namespace PPDEditor
{
    class DummyDrawComponent : GameComponent
    {
        public event Action Drawing;

        public DummyDrawComponent(PPDDevice device) : base(device)
        {

        }

        protected override void Draw(AlphaBlendContext alphaBlendContext, int depth, int childIndex)
        {
            Drawing?.Invoke();
        }
    }
}
