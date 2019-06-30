using PPDFramework;
using SharpDX;

namespace FlowScriptEnginePPD.FlowSourceObjects.Graphics.Image
{
    [ToolTipText("Image_SetRect")]
    public partial class SetRectFlowSourceObject : ExecutableFlowSourceObject
    {
        public override string Name
        {
            get { return "PPD.Graphics.Image.SetRect"; }
        }

        [ToolTipText("Image_SetRect_Object")]
        public PictureObject Object
        {
            private get;
            set;
        }

        [ToolTipText("Image_SetRect_X")]
        public float X
        {
            private get;
            set;
        }

        [ToolTipText("Image_SetRect_Y")]
        public float Y
        {
            private get;
            set;
        }

        [ToolTipText("Image_SetRect_Width")]
        public float Width
        {
            private get;
            set;
        }

        [ToolTipText("Image_SetRect_Height")]
        public float Height
        {
            private get;
            set;
        }

        public override void In(FlowScriptEngine.FlowEventArgs e)
        {
            SetValue(nameof(Object));
            SetValue(nameof(X));
            SetValue(nameof(Y));
            SetValue(nameof(Width));
            SetValue(nameof(Height));

            if (Object != null)
            {
                Object.Rectangle = new RectangleF(X, Y, Width, Height);
                OnSuccess();
            }
            else
            {
                OnFailed();
            }
        }
    }
}
