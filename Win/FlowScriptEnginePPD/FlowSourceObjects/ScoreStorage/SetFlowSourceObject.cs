using FlowScriptEngine;

namespace FlowScriptEnginePPD.FlowSourceObjects.ScoreStorage
{
    [ToolTipText("ScoreStorage_Set_Summary")]
    public partial class SetFlowSourceObject : ScoreStorageFlowSourceObjectBase
    {
        public override string Name
        {
            get { return "PPD.ScoreStorage.Set"; }
        }

        [ToolTipText("ScoreStorage_Set_Key")]
        public string Key
        {
            private get;
            set;
        }

        [ToolTipText("ScoreStorage_Set_Value")]
        public string Value
        {
            private get;
            set;
        }

        [ToolTipText("Execute_In")]
        public void In(FlowEventArgs e)
        {
            SetValue(nameof(Key));
            SetValue(nameof(Value));
            if (scoreStorage != null)
            {
                scoreStorage[Key] = Value;
            }
        }
    }
}
