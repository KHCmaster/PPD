namespace FlowScriptEngineBasicExtension.FlowSourceObjects.Hashtable
{
    public partial class ParseBooleanValueFlowSourceObject : ParseValueFlowSourceObjectBase<bool>
    {
        protected override bool ParseValue(string value)
        {
            if (!bool.TryParse(value, out bool val))
            {
                OnParseFailed();
                return Default;
            }
            return val;
        }
    }
}
