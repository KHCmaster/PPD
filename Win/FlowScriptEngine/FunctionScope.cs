using System;
using System.Collections.Generic;
using System.Linq;

namespace FlowScriptEngine
{
    public class FunctionScope
    {
        public event Action<FunctionScopeArg> EventInvoked;
        public event Action<FunctionScopeArg> PreviewEventInvoked;
        Dictionary<string, object> dict;

        private List<FunctionScope> childScopes;

        public ScopeType ScopeType
        {
            get;
            private set;
        }

        public FunctionScope(ScopeType scopeType)
        {
            ScopeType = scopeType;
            childScopes = new List<FunctionScope>();
            dict = new Dictionary<string, object>();
        }

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
                return dict.ToArray();
            }
        }

        internal void AddChild(FunctionScope scope)
        {
            childScopes.Add(scope);
            scope.Parent = this;
        }

        public void SetValue(string name, object value)
        {
            if (dict.ContainsKey(name))
            {
                dict[name] = value;
            }
            else
            {
                dict.Add(name, value);
            }
        }

        public object GetValue(string name)
        {
            if (dict.ContainsKey(name))
            {
                return dict[name];
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
                    Action childInvokeEnd = () =>
                    {
                        if (!anyHandled)
                        {
                            var scopeArg2 = new FunctionScopeArg(eventName, arg, (e2) =>
                            {
                                if (allEnd != null)
                                {
                                    allEnd.Invoke(anyHandled | e2.Handled);
                                }
                            });
                            OnEventInvoked(scopeArg2);
                        }
                    };
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
                                    childInvokeEnd.Invoke();
                                }
                            });
                        }
                    }
                    else
                    {
                        childInvokeEnd.Invoke();
                    }
                }
            });
            OnPreviewEventInvoked(scopeArg);
        }

        protected void OnPreviewEventInvoked(FunctionScopeArg arg)
        {
            if (PreviewEventInvoked != null)
            {
                PreviewEventInvoked.Invoke(arg);
            }
            else
            {
                arg.OnEndInvoked();
            }
        }

        protected void OnEventInvoked(FunctionScopeArg arg)
        {
            if (EventInvoked != null)
            {
                EventInvoked.Invoke(arg);
            }
            else
            {
                arg.OnEndInvoked();
            }
        }

        public void Clear()
        {
            dict.Clear();
        }
    }
}
