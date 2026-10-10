using System;
using System.Threading;

namespace PPDInstaller.Executor
{
    /// <summary>
    /// インストールの処理の基底クラスです。終了と進み具合は作ったスレッドの <see cref="SynchronizationContext"/> に通知します。
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

        /// <summary>
        /// 進み具合（%）です。始める前に表示しても数字になるように、最初は 0 にしておきます。
        /// </summary>
        public string Progress
        {
            get;
            protected set;
        } = "0";

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
            context.Send(_ => Finished?.Invoke(this, EventArgs.Empty), null);
        }

        protected void OnProgress()
        {
            context.Send(_ => Progressed?.Invoke(this, EventArgs.Empty), null);
        }
    }
}
