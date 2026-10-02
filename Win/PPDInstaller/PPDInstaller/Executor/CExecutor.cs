using System;
using System.Windows.Threading;

namespace PPDInstaller.Executor
{
    /// <summary>
    /// インストールの処理の基底クラスです。終了と進み具合は UI のスレッドに通知します。
    /// </summary>
    abstract class CExecutor(Dispatcher dispatcher)
    {
        public event EventHandler Finished;
        public event EventHandler Progressed;
        readonly Dispatcher dispatcher = dispatcher;

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
            if (!dispatcher.CheckAccess())
            {
                dispatcher.Invoke(OnFinish);
                return;
            }
            Finished?.Invoke(this, EventArgs.Empty);
        }

        protected void OnProgress()
        {
            if (!dispatcher.CheckAccess())
            {
                dispatcher.Invoke(OnProgress);
                return;
            }
            Progressed?.Invoke(this, EventArgs.Empty);
        }
    }
}
