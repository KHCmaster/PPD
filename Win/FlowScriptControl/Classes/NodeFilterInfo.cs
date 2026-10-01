using System.Linq;
using System.Text.RegularExpressions;

namespace FlowScriptControl.Classes
{
    public class NodeFilterInfo : NodeFilterInfoBase
    {
        private static readonly NodeFilterInfo defaultFilter = new("All",
            new Regex[] { new(".") },
            new Regex[] { new(".") });
        private readonly string name;
        private readonly Regex[] includeAssemblyNames;
        private readonly Regex[] includeNodeNames;
        private readonly Regex[] excludeAssemblyNames;
        private readonly Regex[] excludeNodeNames;

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
