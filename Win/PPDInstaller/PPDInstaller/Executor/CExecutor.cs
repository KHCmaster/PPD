using System;
using System.Windows.Forms;

namespace PPDInstaller.Executor
{
    abstract class CExecutor(Control control)
    {
        public delegate void VoidDelegate();

        public event EventHandler Finished;
        public event EventHandler Progressed;
        readonly Control control = control;

        public bool Success
        {
            get;
            protected set;
        }

        public string Progress
        {
            get;
            protected set;
        }

        public string ErrorLog
        {
            get;
            protected set;
        }

        public string ProgressText
        {
            get;
            set;
        }

        public string Name
        {
            get;
            set;
        }

        public virtual void Execute()
        {
            Progress = "0";
        }

        protected void OnFinish()
        {
            if (control.InvokeRequired)
            {
                control.Invoke(new VoidDelegate(OnFinish));
                return;
            }
            Finished?.Invoke(this, EventArgs.Empty);
        }

        protected void OnProgress()
        {
            if (control.InvokeRequired)
            {
                control.Invoke(new VoidDelegate(OnProgress));
                return;
            }
            Progressed?.Invoke(this, EventArgs.Empty);
        }
    }
}
