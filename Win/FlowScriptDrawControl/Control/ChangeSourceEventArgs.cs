using FlowScriptEngine;
using System;

namespace FlowScriptDrawControl.Control
{
    public class ChangeSourceEventArgs : EventArgs
    {
        public AssemblyAndType AssemblyAndType
        {
            get;
            private set;
        }

        public ChangeSourceEventArgs(AssemblyAndType asmAndType)
        {
            AssemblyAndType = asmAndType;
        }
    }
}
