using System.Globalization;

namespace FlowScriptEngineBasicExtension.FlowSourceObjects.Hashtable
{
    public partial class ParseDoubleValueFlowSourceObject : ParseValueFlowSourceObjectBase<double>
    {
        protected override double ParseValue(string value)
        {
            if (!double.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out double val))
            {
                OnParseFailed();
                return Default;
            }
            return val;
        }
    }
}
