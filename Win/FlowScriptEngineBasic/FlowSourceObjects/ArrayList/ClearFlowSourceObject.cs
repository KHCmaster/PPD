using FlowScriptEngine;

namespace FlowScriptEngineBasic.FlowSourceObjects.ArrayList
{
    [ToolTipText("ArrayList_Clear_Summary")]
    public partial class ClearFlowSourceObject : ArrayListFlowSourceObjectBase
    {
        public override string Name
        {
            get { return "ArrayList.Clear"; }
        }

        public override void In(FlowEventArgs e)
        {
            SetArrayList();
            if (ArrayList != null)
            {
                ArrayList.Clear();
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
