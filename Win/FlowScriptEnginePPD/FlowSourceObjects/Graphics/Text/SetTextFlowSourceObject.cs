using FlowScriptEngine;
using PPDFramework;

namespace FlowScriptEnginePPD.FlowSourceObjects.Graphics.Text
{
    [ToolTipText("Text_SetText_Summary")]
    public partial class SetTextFlowSourceObject : ExecutableFlowSourceObject
    {
        public override string Name
        {
            get { return "PPD.Graphics.Text.SetText"; }
        }

        [ToolTipText("Text_SetText_Object")]
        public TextureString Object
        {
            private get;
            set;
        }

        [ToolTipText("Text_SetText_Text")]
        public string Text
        {
            private get;
            set;
        }

        public override void In(FlowEventArgs e)
        {
            SetValue(nameof(Object));
            SetValue(nameof(Text));
            if (Object != null)
            {
                if (Text == null)
                {
                    Text = "";
                }

                Object.Text = Text;
                OnSuccess();
            }
            else
            {
                OnFailed();
            }
        }
    }
}
