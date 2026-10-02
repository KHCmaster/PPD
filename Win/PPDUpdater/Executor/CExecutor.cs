using System;
using System.Windows.Threading;

namespace PPDUpdater.Executor
{
    /// <summary>
    /// 別のスレッドで処理を行い、終了と進み具合を UI のスレッドに通知する処理の基底クラスです。
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

        public virtual void Execute()
        {
            Progress = 0;
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
