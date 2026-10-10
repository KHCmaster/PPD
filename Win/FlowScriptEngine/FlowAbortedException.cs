using System;

namespace FlowScriptEngine
{
    /// <summary>
    /// DebugController.Abort で実行の中止が要求されたときに、実行中のフローを抜けるための例外です。
    /// </summary>
    class FlowAbortedException : Exception
    {
        public FlowAbortedException()
            : base("Flow execution was aborted.")
        {
        }
    }
}
