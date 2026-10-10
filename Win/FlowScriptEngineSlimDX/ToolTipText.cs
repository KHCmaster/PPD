using FlowScriptEngine;

namespace FlowScriptEngineSlimDX
{
    public class ToolTipText : IToolTipText
    {
        private readonly string toolTipText;
        private readonly string summary;
        private readonly string remark;
        private readonly string key;

        public ToolTipText(string summary)
        {
            this.key = summary;
            this.summary = FlowScriptEngineSlimDX.Properties.Resources.ResourceManager.GetString(summary);
            toolTipText = this.summary;
        }

        public ToolTipText(string summary, string remark)
        {
            this.key = summary;
            this.summary = FlowScriptEngineSlimDX.Properties.Resources.ResourceManager.GetString(summary);
            this.remark = FlowScriptEngineSlimDX.Properties.Resources.ResourceManager.GetString(remark);
            toolTipText = this.summary + "\n" + this.remark;
        }

        public override string Text
        {
            get
            {
                return toolTipText;
            }
        }

        public override string Summary
        {
            get { return summary; }
        }

        public override string TextKey
        {
            get { return key; }
        }
    }
}
