using System.IO;
using YamlDotNet.Serialization;

namespace FlowScriptEngineData.FlowSourceObjects.Yaml
{
    [ToolTipText("Yaml_Dump_Summary")]
    public partial class DumpFlowSourceObject : ExecutableFlowSourceObject
    {
        public override string Name
        {
            get { return "Yaml.Dump"; }
        }

        [ToolTipText("Yaml_Dump_Data")]
        public object Data
        {
            private get;
            set;
        }

        [ToolTipText("Yaml_Dump_Yaml")]
        public string Yaml
        {
            get;
            private set;
        }

        public override void In(FlowScriptEngine.FlowEventArgs e)
        {
            try
            {
                SetValue(nameof(Data));
                var serializer = new Serializer();
                using (var writer = new StringWriter())
                {
                    serializer.Serialize(writer, Data);
                    Yaml = writer.ToString();
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
