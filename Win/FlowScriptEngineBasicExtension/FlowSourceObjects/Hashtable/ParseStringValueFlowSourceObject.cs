namespace FlowScriptEngineBasicExtension.FlowSourceObjects.Hashtable
{
    public partial class ParseStringValueFlowSourceObject : ParseValueFlowSourceObjectBase<string>
    {
        protected override string ParseValue(string value)
        {
            return value;
        }
    }
}
