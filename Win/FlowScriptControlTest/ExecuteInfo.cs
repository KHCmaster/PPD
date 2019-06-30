using FlowScriptDrawControl.Model;
using System;
using System.IO;

namespace FlowScriptControlTest
{
    class ExecuteInfo
    {
        public Stream Stream
        {
            get;
            private set;
        }

        public int[] BreakPoints
        {
            get;
            private set;
        }

        public FlowDrawTab FlowDrawTab
        {
            get;
            private set;
        }

        public Source[] Sources
        {
            get;
            private set;
        }

        public event Action<ExecuteInfo, int> BreakPointAdded;
        public event Action<ExecuteInfo, int> BreakPointRemoved;

        public ExecuteInfo(Stream stream, int[] breakPoints, FlowDrawTab flowDrawTab, Source[] sources)
        {
            Stream = stream;
            BreakPoints = breakPoints;
            FlowDrawTab = flowDrawTab;
            Sources = sources;

            foreach (var source in sources)
            {
                source.PropertyChanged += source_PropertyChanged;
            }
        }

        public void RemoveEventHandler()
        {
            foreach (var source in Sources)
            {
                source.PropertyChanged -= source_PropertyChanged;
            }
        }

        void source_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "IsBreakPointSet")
            {
                var source = (Source)sender;
                if (source.IsBreakPointSet && BreakPointAdded != null)
                {
                    BreakPointAdded(this, source.Id);
                }
                else
                {
                    BreakPointRemoved(this, source.Id);
                }
            }
        }
    }
}
