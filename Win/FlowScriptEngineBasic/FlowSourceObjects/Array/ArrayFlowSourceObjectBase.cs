namespace FlowScriptEngineBasic.FlowSourceObjects.Array
{
    public abstract class ArrayFlowSourceObjectBase : ExecutableFlowSourceObject
    {
        [ToolTipText("Array_Array")]
        public object[] Array
        {
            protected get;
            set;
        }

        protected void SetArray()
        {
            SetValue(nameof(Array));
        }
    }
}
