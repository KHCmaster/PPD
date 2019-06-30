using System.Linq;

namespace FlowScriptEngineBasic.FlowSourceObjects.Enumerable.Lookup
{
    [ToolTipText("Enumerable_Lookup_Contains_Summary")]
    public partial class ContainsFlowSourceObject : ExecutableFlowSourceObject
    {
        public override string Name
        {
            get { return "Enumerable.Lookup.Contains"; }
        }

        [ToolTipText("Enumerable_Lookup_Contains_Lookup")]
        public ILookup<object, object> Lookup
        {
            private get;
            set;
        }

        [ToolTipText("Enumerable_Lookup_Contains_Key")]
        public object Key
        {
            private get;
            set;
        }

        [ToolTipText("Enumerable_Lookup_Contains_Result")]
        public bool Result
        {
            get;
            private set;
        }

        public override void In(FlowScriptEngine.FlowEventArgs e)
        {
            SetValue(nameof(Lookup));
            if (Lookup != null)
            {
                SetValue(nameof(Key));
                Result = Lookup.Contains(Key);
                OnSuccess();
            }
            else
            {
                OnFailed();
            }
        }
    }
}
