using System;

namespace FlowScriptEngine
{
    public abstract class TemplateTypeConverter<F, T> : TypeConverterBase
    {
        public override Type From
        {
            get { return typeof(F); }
        }

        public override Type To
        {
            get { return typeof(T); }
        }

        public abstract override object Convert(object data);
    }
}
