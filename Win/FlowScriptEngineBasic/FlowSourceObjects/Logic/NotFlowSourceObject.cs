using FlowScriptEngine;

namespace FlowScriptEngineBasic.FlowSourceObjects.Logic
{
    [ToolTipText("Logic_Not_Summary")]
    public partial class NotFlowSourceObject : FlowSourceObjectBase
    {
        public override string Name
        {
            get { return "Logic.NOT"; }
        }


        private bool _value;
        [ToolTipText("Logic_Not_Value")]
        public bool Value
        {
            get
            {
                SetValue(nameof(Value));
                return !_value;
            }
            set { _value = value; }
        }
    }
}
