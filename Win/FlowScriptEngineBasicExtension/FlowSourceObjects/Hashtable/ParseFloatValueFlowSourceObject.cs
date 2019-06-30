using System.Globalization;

namespace FlowScriptEngineBasicExtension.FlowSourceObjects.Hashtable
{
    public partial class ParseFloatValueFlowSourceObject : ParseValueFlowSourceObjectBase<float>
    {
        protected override float ParseValue(string value)
        {
            if (!float.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out float val))
            {
                OnParseFailed();
                return Default;
            }
            return val;
        }
    }
}
