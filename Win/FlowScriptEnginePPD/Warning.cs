using FlowScriptEngine;

namespace FlowScriptEnginePPD
{
    public class Warning(string key) : IWarning
    {
        private readonly string key = key;
        private readonly string text = FlowScriptEnginePPD.Properties.Resources.ResourceManager.GetString(key);

        public override string TextKey
        {
            get { return key; }
        }

        public override string Text
        {
            get { return text; }
        }
    }
}
