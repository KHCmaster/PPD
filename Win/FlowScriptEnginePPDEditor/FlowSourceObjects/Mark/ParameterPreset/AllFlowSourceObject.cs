using FlowScriptEngine;
using System.Linq;

namespace FlowScriptEnginePPDEditor.FlowSourceObjects.Mark.ParameterPreset
{
    [ToolTipText("Mark_ParameterPreset_All_Summary")]
    public partial class AllFlowSourceObject : FlowSourceObjectBase
    {
        private PPDEditorCommon.ParameterPreset[] all;

        public override string Name
        {
            get { return "PPDEditor.Mark.ParameterPreset.All"; }
        }

        protected override void OnInitialize()
        {
            if (Manager.Items.TryGetValue("ParameterPresets", out object value))
            {
                all = (PPDEditorCommon.ParameterPreset[])value;
            }
        }

        [ToolTipText("Mark_ParameterPreset_All_All")]
        public object[] All
        {
            get
            {
                return all?.Cast<object>().ToArray();
            }
        }
    }
}
