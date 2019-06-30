using PPDFramework.Shaders;

namespace PPDFramework.RenderMask
{
    class BorderMask : RenderMaskBase
    {
        Border border;

        public override bool Enabled
        {
            get { return border.Thickness > 0; }
        }

        public override int Priority
        {
            get { return 0; }
        }

        public BorderMask(Border border)
        {
            this.border = border;
        }

        public override void Draw(PPDDevice device, WorkspaceTexture maskTexture)
        {
            device.GetModule<BorderFilter>().Draw(device, maskTexture, border);
        }
    }
}
