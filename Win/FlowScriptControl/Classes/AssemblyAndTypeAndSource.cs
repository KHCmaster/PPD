using FlowScriptEngine;

namespace FlowScriptControl.Classes
{
    class AssemblyAndTypeAndSource
    {
        public AssemblyAndType AssemblyAndType
        {
            get;
            private set;
        }

        public FlowSourceObjectBase Source
        {
            get;
            private set;
        }

        public FlowSourceDumper Dumper
        {
            get;
            private set;
        }

        public AssemblyAndTypeAndSource(AssemblyAndType asmAndType)
        {
            AssemblyAndType = asmAndType;
            Source = (FlowSourceObjectBase)asmAndType.Assembly.CreateInstance(asmAndType.Type.FullName);
            Dumper = new FlowSourceDumper(Source, asmAndType);
        }
    }
}
