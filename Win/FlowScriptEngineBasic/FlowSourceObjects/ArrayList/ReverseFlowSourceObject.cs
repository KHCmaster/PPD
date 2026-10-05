using FlowScriptEngine;

namespace FlowScriptEngineBasic.FlowSourceObjects.ArrayList
{
    [ToolTipText("ArrayList_Reverse_Summary")]
    public partial class ReverseFlowSourceObject : ArrayListFlowSourceObjectBase
    {
        public override string Name
        {
            get { return "ArrayList.Reverse"; }
        }

        public override void In(FlowEventArgs e)
        {
            SetArrayList();
            if (ArrayList != null)
            {
                ArrayList.Reverse();
                OnSuccess();
            }
            else
            {
                // 以前は Success も Failed も出さなかったので、後につないだ処理が動かなかった
                OnFailed();
            }
        }
    }
}
