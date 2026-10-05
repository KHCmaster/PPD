using System;
using System.Collections.Generic;
using System.Linq;

namespace FlowScriptEngine
{
    public class FunctionScope(ScopeType scopeType)
    {
        public event Action<FunctionScopeArg> EventInvoked;
        public event Action<FunctionScopeArg> PreviewEventInvoked;
        readonly Dictionary<string, object> dict = [];

        private readonly List<FunctionScope> childScopes = [];

        public ScopeType ScopeType
        {
            get;
            private set;
        } = scopeType;

        internal int Id
        {
            get;
            set;
        }

        internal int ParentId
        {
            get;
            set;
        }

        internal FunctionScope Parent
        {
            get;
            private set;
        }

        internal KeyValuePair<string, object>[] Pairs
        {
            get
            {
                return [.. dict];
            }
        }

        internal void AddChild(FunctionScope scope)
        {
            childScopes.Add(scope);
            scope.Parent = this;
        }

        public void SetValue(string name, object value)
        {
            if (!dict.TryAdd(name, value))
            {
                dict[name] = value;
            }
        }

        public object GetValue(string name)
        {
            if (dict.TryGetValue(name, out object value))
            {
                return value;
            }
            else
            {
                if (RecursiveGetFromParent(name, out object ret))
                {
                    return ret;
                }
                else
                {
                    throw new Exception(String.Format("Not Difined Value:{0}", name));
                }
            }
        }

        private bool RecursiveGetFromParent(string name, out object value)
        {
            value = null;
            FunctionScope parent = Parent;
            while (parent != null)
            {
                if (parent.IsDefined(name))
                {
                    value = parent.GetValue(name);
                    return true;
                }
                parent = parent.Parent;
            }
            return false;
        }

        public bool IsDefined(string name)
        {
            return dict.ContainsKey(name);
        }

        public bool IsDefinedInParents(string name)
        {
            FunctionScope parent = Parent;
            while (parent != null)
            {
                if (parent.IsDefined(name))
                {
                    return true;
                }
                parent = parent.Parent;
            }
            return false;
        }

        public void InvokeEvent(string eventName, object arg)
        {
            InvokeEventImpl(eventName, arg, null);
        }

        private void InvokeEventImpl(string eventName, object arg, Action<bool> allEnd)
        {
            var scopeArg = new FunctionScopeArg(eventName, arg, (e) =>
            {
                if (!e.Handled)
                {
                    bool anyHandled = false;
                    int currentEndCount = 0;
                    void childInvokeEnd()
                    {
                        if (!anyHandled)
                        {
                            var scopeArg2 = new FunctionScopeArg(eventName, arg, (e2) =>
                            {
                                allEnd?.Invoke(anyHandled | e2.Handled);
                            });
                            OnEventInvoked(scopeArg2);
                        }
                    }
                    if (childScopes.Count > 0)
                    {
                        foreach (FunctionScope child in childScopes)
                        {
                            child.InvokeEventImpl(eventName, arg, (handled) =>
                            {
                                anyHandled |= handled;
                                currentEndCount++;
                                if (currentEndCount == childScopes.Count)
                                {
                                    childInvokeEnd();
                                }
                            });
                        }
                    }
                    else
                    {
                        childInvokeEnd();
                    }
                }
            });
            OnPreviewEventInvoked(scopeArg);
        }

        protected void OnPreviewEventInvoked(FunctionScopeArg arg)
        {
            Dispatch(PreviewEventInvoked, arg);
        }

        protected void OnEventInvoked(FunctionScopeArg arg)
        {
            Dispatch(EventInvoked, arg);
        }

        /// <summary>
        /// イベントを受けるもの（ノード）それぞれに渡し、すべてが終わったときに 1 回だけ次へ進めます。どれかが処理したら、処理したことにします。
        /// </summary>
        /// <remarks>
        /// 以前は同じ引数をすべてに渡していたので、受けるものがそれぞれ次へ進め、子のスコープにイベントが何回も届いていた。
        /// </remarks>
        private static void Dispatch(Action<FunctionScopeArg> handlers, FunctionScopeArg arg)
        {
            if (handlers == null)
            {
                arg.OnEndInvoked();
                return;
            }
            var list = handlers.GetInvocationList();
            if (list.Length == 1)
            {
                handlers(arg);
                return;
            }
            var remaining = list.Length;
            var handled = arg.Handled;
            foreach (Action<FunctionScopeArg> handler in list)
            {
                handler(new FunctionScopeArg(arg.EventName, arg.Arg, e =>
                {
                    handled |= e.Handled;
                    remaining--;
                    if (remaining == 0)
                    {
                        arg.Handled = handled;
                        arg.OnEndInvoked();
                    }
                }));
            }
        }

        public void Clear()
        {
            dict.Clear();
        }
    }
}
