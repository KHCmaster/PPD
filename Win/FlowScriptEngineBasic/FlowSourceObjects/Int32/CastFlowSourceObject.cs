using System;

namespace FlowScriptEngineBasic.FlowSourceObjects.Int32
{
    public partial class CastFlowSourceObject : TemplateCastValueFlowSourceObject
    {
        public override string Name
        {
            get { return "Int32.Cast"; }
        }

        [ToolTipText("Int32_Cast_Value")]
        public int Value
        {
            get
            {
                SetValue(nameof(CastValue));
                return Convert.ToInt32(CastValue);
            }
        }
    }
}
