using System;

namespace FlowScriptEngine
{
    /// <summary>
    /// フローそのもの
    /// </summary>
    public class FlowObject
    {
        public Type SourceType { get; set; }
        public Type DestType { get; set; }
        public FlowSourceObjectBase Source { get; set; }
        public FlowSourceObjectBase Dest { get; set; }
        public string SourcePropertyName { get; set; }
        public string DestPropertyName { get; set; }
        public object GetSrcValue()
        {
            if (Source != null)
            {
                Source.Manager.Evaluate(Source);
                var retVal = Source.GetPropertyValue(SourcePropertyName);
                Source.Manager.Evaluate(Source);
                if (SourceType != DestType)
                {
                    var retValType = retVal == null ? typeof(object) : retVal.GetType();
                    if (retValType != SourceType && TypeConverterManager.CanConvert(retValType, DestType))
                    {
                        return TypeConverterManager.Convert(retValType, DestType, retVal);
                    }
                    return TypeConverterManager.Convert(SourceType, DestType, retVal);
                }
                else
                {
                    return retVal;
                }
            }
            else
            {
                return null;
            }
        }
    }
}
