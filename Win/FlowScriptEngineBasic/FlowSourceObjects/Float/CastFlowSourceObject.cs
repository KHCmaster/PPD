using System;

namespace FlowScriptEngineBasic.FlowSourceObjects.Float
{
    public partial class CastFlowSourceObject : TemplateCastValueFlowSourceObject
    {
        public override string Name
        {
            get { return "Float.Cast"; }
        }

        [ToolTipText("Float_Cast_Value")]
        public float Value
        {
            get
            {
                SetValue(nameof(CastValue));
                return Convert.ToSingle(CastValue);
            }
        }
    }
}
