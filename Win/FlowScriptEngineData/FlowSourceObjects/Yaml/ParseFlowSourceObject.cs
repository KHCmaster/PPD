using System.IO;
using YamlDotNet.Serialization;

namespace FlowScriptEngineData.FlowSourceObjects.Yaml
{
    [ToolTipText("Yaml_Parse_Summary")]
    public partial class ParseFlowSourceObject : ExecutableFlowSourceObject
    {
        public override string Name
        {
            get { return "Yaml.Parse"; }
        }

        [ToolTipText("Yaml_Parse_Yaml")]
        public string Yaml
        {
            private get;
            set;
        }

        [ToolTipText("Yaml_Parse_Data")]
        public object Data
        {
            get;
            private set;
        }

        public override void In(FlowScriptEngine.FlowEventArgs e)
        {
            try
            {
                SetValue(nameof(Yaml));
                var deserializer = new Deserializer();
                using (var reader = new StringReader(Yaml))
                {
                    Data = deserializer.Deserialize(reader);
                }
                OnSuccess();
            }
            catch
            {
                OnFailed();
            }
        }
    }
}
