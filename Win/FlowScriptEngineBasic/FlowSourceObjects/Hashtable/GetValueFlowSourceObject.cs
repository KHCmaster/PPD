namespace FlowScriptEngineBasic.FlowSourceObjects.Hashtable
{
    [ToolTipText("Hashtable_GetValue_Summary")]
    public partial class GetValueFlowSourceObject : HashtableFlowSourceBase
    {
        public override string Name
        {
            get { return "Hashtable.GetValue"; }
        }

        [ToolTipText("Hashtable_GetValue_Key")]
        public object Key
        {
            private get;
            set;
        }

        [ToolTipText("Hashtable_GetValue_Value")]
        public object Value
        {
            get
            {
                SetHashtable();
                if (Hashtable != null)
                {
                    SetValue(nameof(Key));
                    // ない鍵は null にする（以前は値を読んだときにエラーになっていた）
                    return Key != null && Hashtable.TryGetValue(Key, out var value) ? value : null;
                }
                return null;
            }
        }

        /// <summary>
        /// 鍵があれば Success、なければ Failed にします（以前は鍵がなくても Success だった）。
        /// </summary>
        public override void In(FlowScriptEngine.FlowEventArgs e)
        {
            SetHashtable();
            if (Hashtable != null)
            {
                SetValue(nameof(Key));
                if (Key != null && Hashtable.ContainsKey(Key))
                {
                    OnSuccess();
                    return;
                }
            }
            OnFailed();
        }
    }
}
