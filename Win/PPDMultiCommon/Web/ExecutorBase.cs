using PPDFrameworkCore;
using System;

namespace PPDMultiCommon.Web
{
    public abstract class ExecutorBase
    {
        public event EventHandler Finished;

        public bool Success
        {
            get;
            private set;
        }

        public bool IsFinished
        {
            get;
            private set;
        }

        public void Start()
        {
            var thread = ThreadManager.Instance.GetThread(InnerStart);
            thread.IsBackground = true;
            thread.Start();
        }

        protected virtual void InnerStart()
        {
            Success = false;
            IsFinished = false;
        }

        protected void OnSuccess()
        {
            Success = true;
            OnFinished();
        }

        protected void OnFail()
        {
            Success = false;
            OnFinished();
        }

        private void OnFinished()
        {
            IsFinished = true;
            if (Finished != null)
            {
                Finished.Invoke(this, EventArgs.Empty);
            }
        }
    }
}
