using FlowScriptEngine;
using System.Collections.Generic;

namespace FlowScriptEngineBasicExtension.FlowSourceObjects.Hashtable
{
    [ToolTipText("Hashtable_ParseValue_Summary")]
    public abstract class ParseValueFlowSourceObjectBase<T> : FlowSourceObjectBase
    {
        [ToolTipText("Hashtable_ParseValue_ParseFailed")]
        public event FlowEventHandler ParseFailed;

        public override string Name
        {
            get { return "Hashtable.ParseValue"; }
        }

        [ToolTipText("Hashtable_ParseValue_Hashtable")]
        public Dictionary<object, object> Hashtable
        {
            protected get;
            set;
        }

        [ToolTipText("Hashtable_ParseValue_Key")]
        public string Key
        {
            private get;
            set;
        }

        [ToolTipText("Hashtable_ParseValue_Default")]
        public T Default
        {
            protected get;
            set;
        }

        [ToolTipText("Hashtable_ParseValue_Value")]
        public T Value
        {
            get
            {
                SetValue(nameof(Hashtable));
                SetValue(nameof(Default));
                if (Hashtable != null)
                {
                    SetValue(nameof(Key));
                    if (Hashtable.ContainsKey(Key))
                    {
                        return ParseValue(Hashtable[Key].ToString());
                    }
                }
                return Default;
            }
        }

        protected abstract T ParseValue(string value);

        protected void OnParseFailed()
        {
            FireEvent(ParseFailed);
        }
    }
}
