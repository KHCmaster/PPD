using System.Linq;
using System.Text.RegularExpressions;

namespace FlowScriptControl.Classes
{
    public class NodeFilterInfo : NodeFilterInfoBase
    {
        private static NodeFilterInfo defaultFilter = new NodeFilterInfo("All",
            new Regex[] { new Regex(".") },
            new Regex[] { new Regex(".") });
        private string name;
        private Regex[] includeAssemblyNames;
        private Regex[] includeNodeNames;
        private Regex[] excludeAssemblyNames;
        private Regex[] excludeNodeNames;

        public static NodeFilterInfo DefaultFilter
        {
            get { return defaultFilter; }
        }

        public override string Name
        {
            get { return name; }
        }

        public NodeFilterInfo(string name, Regex[] includeAssemblyNames, Regex[] includeNodeNames)
            : this(name, includeAssemblyNames, includeNodeNames, null, null)
        {
        }

        public NodeFilterInfo(string name, Regex[] includeAssemblyNames, Regex[] includeNodeNames,
            Regex[] excludeAssemblyNames, Regex[] excludeNodeNames)
        {
            this.name = name;
            this.includeAssemblyNames = includeAssemblyNames;
            this.includeNodeNames = includeNodeNames;
            this.excludeAssemblyNames = excludeAssemblyNames;
            this.excludeNodeNames = excludeNodeNames;
        }

        public override bool IsHide(FlowSourceDumper dumper)
        {
            if (excludeAssemblyNames != null && excludeAssemblyNames.Any(r => r.IsMatch(dumper.AssemblyAndType.FullName)))
            {
                return true;
            }
            if (excludeNodeNames != null && excludeNodeNames.Any(r => r.IsMatch(dumper.SourceName)))
            {
                return true;
            }

            if (includeAssemblyNames != null && includeAssemblyNames.Any(r => r.IsMatch(dumper.AssemblyAndType.FullName)))
            {
                return false;
            }
            if (includeNodeNames != null && includeNodeNames.Any(r => r.IsMatch(dumper.SourceName)))
            {
                return false;
            }

            return true;
        }
    }
}
