using FlowScriptEngine;
using PPDEditorCommon;

namespace FlowScriptEnginePPDEditor.FlowSourceObjects.Storage
{
    [ToolTipText("Storage_GetValue_Summary")]
    public partial class GetValueFlowSourceObject : FlowSourceObjectBase
    {
        private IStorage storage;

        public override string Name
        {
            get { return "PPDEditor.Storage.GetValue"; }
        }

        protected override void OnInitialize()
        {
            if (Manager.Items.ContainsKey("Storage"))
            {
                storage = (IStorage)Manager.Items["Storage"];
            }
        }

        [ToolTipText("Storage_GetValue_StorageKey")]
        public string StorageKey
        {
            private get;
            set;
        }

        [ToolTipText("Storage_GetValue_Key")]
        public string Key
        {
            private get;
            set;
        }

        [ToolTipText("Storage_GetValue_Value")]
        public string Value
        {
            get
            {
                SetValue(nameof(StorageKey));
                SetValue(nameof(Key));
                if (storage != null)
                {
                    return storage.GetValue(StorageKey, Key);
                }
                return null;
            }
        }
    }
}
