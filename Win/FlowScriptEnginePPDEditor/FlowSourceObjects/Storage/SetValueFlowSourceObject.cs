using PPDEditorCommon;

namespace FlowScriptEnginePPDEditor.FlowSourceObjects.Storage
{
    [ToolTipText("Storage_SetValue_Summary")]
    public partial class SetValueFlowSourceObject : ExecutableFlowSourceObject
    {
        private IStorage storage;

        public override string Name
        {
            get { return "PPDEditor.Storage.SetValue"; }
        }

        protected override void OnInitialize()
        {
            if (Manager.Items.ContainsKey("Storage"))
            {
                storage = (IStorage)Manager.Items["Storage"];
            }
        }

        [ToolTipText("Storage_SetValue_StorageKey")]
        public string StorageKey
        {
            private get;
            set;
        }

        [ToolTipText("Storage_SetValue_Key")]
        public string Key
        {
            private get;
            set;
        }

        [ToolTipText("Storage_SetValue_Value")]
        public string Value
        {
            private get;
            set;
        }

        public override void In(FlowScriptEngine.FlowEventArgs e)
        {
            SetValue(nameof(StorageKey));
            SetValue(nameof(Key));
            SetValue(nameof(Value));
            if (storage != null)
            {
                storage.SetValue(StorageKey, Key, Value);
                OnSuccess();
            }
            else
            {
                OnFailed();
            }
        }
    }
}
