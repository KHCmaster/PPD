using FlowScriptEngine;
using System;

namespace FlowScriptDrawControl.Control
{
    public class ChangeSourceEventArgs(AssemblyAndType asmAndType) : EventArgs
    {
        public AssemblyAndType AssemblyAndType
        {
            get;
            private set;
        } = asmAndType;
    }
}
