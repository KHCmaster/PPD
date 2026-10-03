using System;
using System.Threading;

namespace PPDUpdater.Executor
{
    /// <summary>
    /// 別のスレッドで処理を行い、終了と進み具合を作ったスレッドの <see cref="SynchronizationContext"/> に通知する処理の基底クラスです。
    /// </summary>
    abstract class CExecutor(SynchronizationContext context)
    {
        public event EventHandler Finished;
        public event EventHandler Progressed;

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
            context.Send(_ => Finished?.Invoke(this, EventArgs.Empty), null);
        }

        protected void OnProgress()
        {
            context.Send(_ => Progressed?.Invoke(this, EventArgs.Empty), null);
        }
    }
}
