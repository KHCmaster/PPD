using System.Linq;
using System.Text.RegularExpressions;

namespace FlowScriptControl.Classes
{
    public partial class NodeFilterInfo(string name, Regex[] includeAssemblyNames, Regex[] includeNodeNames,
        Regex[] excludeAssemblyNames, Regex[] excludeNodeNames) : NodeFilterInfoBase
    {
        private static readonly NodeFilterInfo defaultFilter = new("All",
            [AnyCharRegex()],
            [AnyCharRegex()]);
        private readonly string name = name;
        private readonly Regex[] includeAssemblyNames = includeAssemblyNames;
        private readonly Regex[] includeNodeNames = includeNodeNames;
        private readonly Regex[] excludeAssemblyNames = excludeAssemblyNames;
        private readonly Regex[] excludeNodeNames = excludeNodeNames;

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

        [GeneratedRegex(".")]
        private static partial Regex AnyCharRegex();
    }
}
