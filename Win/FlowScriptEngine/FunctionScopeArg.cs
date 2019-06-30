using System;

namespace FlowScriptEngine
{
    public class FunctionScopeArg
    {
        private Action<FunctionScopeArg> action;

        public string EventName
        {
            get;
            private set;
        }

        public object Arg
        {
            get;
            private set;
        }

        public bool Handled
        {
            get;
            set;
        }

        internal FunctionScopeArg(string eventName, object arg, Action<FunctionScopeArg> action)
        {
            EventName = eventName;
            Arg = arg;
            this.action = action;
        }

        public void OnEndInvoked()
        {
            if (action != null)
            {
                action.Invoke(this);
            }
        }
    }
}
