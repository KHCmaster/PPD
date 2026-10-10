using PPDFramework;
using PPDFramework.Shaders;
using System;

namespace PPDEditor
{
    class DummyDrawComponent(PPDDevice device) : GameComponent(device)
    {
        public event Action Drawing;

        protected override void Draw(AlphaBlendContext alphaBlendContext, int depth, int childIndex)
        {
            Drawing?.Invoke();
        }
    }
}
