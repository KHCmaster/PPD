using PPDFramework;
using SharpDX;

namespace PPDShareComponent
{
    public class ButtonComponent : SelectableComponent
    {
        EffectObject select;

        public ButtonComponent(PPDDevice device, PPDFramework.Resource.ResourceManager resourceManager, PathManager pathManager, string text) : base(device)
        {
            TextureString str;
            this.AddChild(str = new TextureString(device, text, 20, PPDColors.White)
            {
                Position = new Vector2(30, 0)
            });

            select = new EffectObject(device, resourceManager, pathManager.Combine("greenflare.etd"))
            {
                Position = new Vector2(13, 13)
            };
            select.PlayType = Effect2D.EffectManager.PlayType.ReverseLoop;
            select.Play();
            select.Alignment = EffectObject.EffectAlignment.Center;
            select.Scale = new Vector2(0.4f, 0.4f);
            this.AddChild(select);
        }

        protected override void UpdateImpl()
        {
            select.Hidden = !Selected;
        }
    }
}
