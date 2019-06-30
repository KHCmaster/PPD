using System;

namespace FlowScriptEngine
{
    public abstract class SubClassTemplateTypeConverter<F, T> : TemplateTypeConverter<F, T> where F : T where T : class
    {
        public override Type From
        {
            get { return typeof(F); }
        }

        public override Type To
        {
            get { return typeof(T); }
        }

        public override object Convert(object data)
        {
            return (T)(F)data;
        }
    }
}
