using PPDFramework;
using SharpDX;

namespace PPDEditor
{
    class MarkPointDrawer : GameComponent
    {
        const int FontHeight = 16;
        bool drawAngle;
        SpriteObject positionSprite;
        SpriteObject angleSprite;

        PPDFramework.Resource.ResourceManager resourceManager;

        public bool DrawAngle
        {
            get { return drawAngle; }
            set
            {
                drawAngle = value;
                angleSprite.Hidden = !drawAngle;
            }
        }

        public MarkPointDrawer(PPDDevice device, PPDFramework.Resource.ResourceManager resourceManager) : base(device)
        {
            this.resourceManager = resourceManager;
            AddChild(positionSprite = new SpriteObject(device));
            AddChild(angleSprite = new SpriteObject(device));
            AddChild(new RectangleComponent(device, resourceManager, PPDColors.White)
            {
                RectangleHeight = 450,
                RectangleWidth = 800,
                Alpha = 0.8f
            });
        }

        public void ChangeData(Vector2[] positions, float[] angles)
        {
            positionSprite.ClearChildren();
            angleSprite.ClearChildren();
            for (var i = 0; i < positions.Length; i++)
            {
                positionSprite.AddChild(new PictureObject(device, resourceManager, Utility.Path.Combine("assist", "circle.png"), true)
                {
                    Position = positions[i]
                });
                angleSprite.AddChild(new PictureObject(device, resourceManager, Utility.Path.Combine("assist", "arrow.png"), true)
                {
                    Rotation = -angles[i],
                    Position = positions[i]
                });
                var str = $"{i + 1}";
                positionSprite.AddChild(new TextureString(device, str, FontHeight, PPDColors.Black)
                {
                    Position = positions[i] - new Vector2(FontHeight * str.Length / 4, FontHeight / 2)
                });
            }
        }
    }
}
