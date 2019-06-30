using System.Globalization;

namespace FlowScriptEngineBasicExtension.FlowSourceObjects.Hashtable
{
    public partial class ParseInt32ValueFlowSourceObject : ParseValueFlowSourceObjectBase<int>
    {
        protected override int ParseValue(string value)
        {
            if (!int.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out int val))
            {
                OnParseFailed();
                return Default;
            }
            return val;
        }
    }
}
