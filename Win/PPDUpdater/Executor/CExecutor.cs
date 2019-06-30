using System;
using System.Windows.Forms;

namespace PPDUpdater.Executor
{
    abstract class CExecutor
    {
        public delegate void VoidDelegate();

        public event EventHandler Finished;
        public event EventHandler Progressed;
        Control control;

        public bool Success
        {
            get;
            protected set;
        }

        public int Progress
        {
            get;
            protected set;
        }

        public string ErrorLog
        {
            get;
            protected set;
        }

        protected CExecutor(Control control)
        {
            this.control = control;
        }

        public virtual void Execute()
        {
            Progress = 0;
        }

        protected void OnFinish()
        {
            if (control.InvokeRequired)
            {
                control.Invoke(new VoidDelegate(OnFinish));
                return;
            }
            if (Finished != null)
            {
                Finished.Invoke(this, EventArgs.Empty);
            }
        }

        protected void OnProgress()
        {
            if (control.InvokeRequired)
            {
                control.Invoke(new VoidDelegate(OnProgress));
                return;
            }
            if (Progressed != null)
            {
                Progressed.Invoke(this, EventArgs.Empty);
            }
        }
    }
}
