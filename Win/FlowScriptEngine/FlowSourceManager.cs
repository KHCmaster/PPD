using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;

namespace FlowScriptEngine
{
    public class FlowSourceManager
    {
        public event Action<FlowExecutionException> Error;

        public const string START = "FlowScriptEngine.Start";
        public delegate void FlowCallEventHandler(object[] values);

        Dictionary<string, List<FlowCallEventHandler>> callBackDictionary;
        EventQueue eventQueue;
        EventQueue currentQueue;

        Dictionary<int, FlowSourceObjectBase> sources;
        List<FlowInfo> flowInfos;

        Engine engine;
        FunctionScope scope;
        FunctionScope contextScope;
        FunctionScope globalScope;

        bool debugMode;
        bool initialized;
        KeyValuePair<FlowSourceObjectBase, KeyValuePair<string, string>[]>[] defaultValues;

        List<FunctionScope> scopes;
        HashSet<int> breakPoints;
        private readonly object breakPointLock = new object();

        private bool evaluating;
        private DebugController debugController;
        private OperationType nextOperation;
        private FlowSourceObjectBase lastEvaluateSource;

        public Dictionary<string, object> Items
        {
            get;
            private set;
        }

        public bool IsAborted
        {
            get { return nextOperation == OperationType.Abort; }
        }

        public TextReader ConsoleReader
        {
            get { return engine.ConsoleReader; }
        }

        internal FlowSourceManager(Engine engine, bool debugMode, FunctionScope globalScope, FunctionScope contextScope, DebugController debugController)
        {
            this.engine = engine;
            this.debugMode = debugMode;
            this.contextScope = contextScope;
            this.globalScope = globalScope;
            this.debugController = debugController;

            callBackDictionary = new Dictionary<string, List<FlowCallEventHandler>>();
            sources = new Dictionary<int, FlowSourceObjectBase>();
            flowInfos = new List<FlowInfo>();
            eventQueue = new EventQueue();
            currentQueue = eventQueue;

            scope = new FunctionScope(ScopeType.Default);
            contextScope.AddChild(scope);
            scopes = new List<FunctionScope>();
            breakPoints = new HashSet<int>();

            Items = new Dictionary<string, object>();
        }

        internal void AddScope(FunctionScope scope)
        {
            scopes.Add(scope);
        }

        public void Start()
        {
            Call(START, null);
        }

        public void RegisterCallBack(string callBackName, FlowCallEventHandler func)
        {
            if (!callBackDictionary.TryGetValue(callBackName, out List<FlowCallEventHandler> callBacks))
            {
                callBacks = new List<FlowCallEventHandler>();
                callBackDictionary.Add(callBackName, callBacks);
            }
            callBacks.Add(func);
        }

        public void Call(string callBackName, object[] args)
        {
            if (callBackDictionary.TryGetValue(callBackName, out List<FlowCallEventHandler> callBacks))
            {
                if (args == null)
                {
                    args = new object[0];
                }
                foreach (FlowCallEventHandler callBack in callBacks)
                {
                    var source = GetTargetSource(callBack);
                    Evaluate(source);
                    callBack.Invoke(args);
                    Evaluate(source);
                }
            }
            ProcessEventStack();
        }

        internal void BreakLoop()
        {
            if (!currentQueue.HasLoopEndInParents)
            {
                throw new Exception("Break was used out of loop event!!");
            }

            currentQueue = currentQueue.GetLoopEndParent();
            currentQueue.ClearChildEvents();
            var eventSet = currentQueue.GetEventSet();
            if (eventSet == null)
            {
                return;
            }
            ProcessEvent(eventSet, new FlowEventArgs(eventSet.Source, true));
        }

        internal void ContinueLoop()
        {
            if (!currentQueue.HasLoopEndInParents)
            {
                throw new Exception("Continue was used out of loop event!!");
            }

            currentQueue = currentQueue.GetLoopEndParent();
            currentQueue.ClearChildEvents();
            var eventSet = currentQueue.GetEventSet();
            if (eventSet == null)
            {
                return;
            }
            ProcessEvent(eventSet, new FlowEventArgs(eventSet.Source, false));
        }

        internal void ProcessOnlyCurrentQueue()
        {
            ProcessEventStack(currentQueue);
        }

        internal void AddEventExecute(FlowEventHandler eventHandler, FlowSourceObjectBase source, bool doContinuously, bool isLoopEnd)
        {
            if (eventHandler != null)
            {
                if (!doContinuously)
                {
                    foreach (FlowEventHandler dele in eventHandler.GetInvocationList())
                    {
                        currentQueue.AddEventSet(new EventSet { EventHandler = dele, Source = source, IsLoopEnd = isLoopEnd });
                    }
                }
                else
                {
                    foreach (FlowEventHandler dele in eventHandler.GetInvocationList())
                    {
                        currentQueue.AddEventSetToDepth(new EventSet { EventHandler = dele, Source = source, IsLoopEnd = isLoopEnd });
                    }
                }
            }
        }

        private void ProcessEventStack()
        {
            ProcessEventStack(currentQueue);
        }

        private void ProcessEventStack(EventQueue rootQueue)
        {
            if (nextOperation == OperationType.Abort)
            {
                return;
            }
            while ((currentQueue = rootQueue.GetEvent()) != null)
            {
                var eventSet = currentQueue.GetEventSet();
                if (eventSet == null)
                {
                    break;
                }
                ProcessEvent(eventSet);
            }
        }

        private FlowSourceObjectBase GetTargetSource(Delegate del)
        {
            var source = del.Target as FlowSourceObjectBase;
            if (source == null)
            {
                if (del.Target is Delegate temp)
                {
                    source = temp.Target as FlowSourceObjectBase;
                }
            }
            return source;
        }

        private void ProcessEvent(EventSet eventSet)
        {
            ProcessEvent(eventSet, new FlowEventArgs(eventSet.Source, false));
        }

        private void ProcessEvent(EventSet eventSet, FlowEventArgs eventArgs)
        {
            if (nextOperation == OperationType.Abort)
            {
                return;
            }

            try
            {
                var source = GetTargetSource(eventSet.EventHandler);
                Evaluate(source);
                eventSet.EventHandler.Invoke(eventArgs);
                Evaluate(source);
            }
            catch (FlowExecutionException e)
            {
                Error?.Invoke(e);
                ProcessError(e);
                if (debugMode)
                {
                    throw;
                }
            }
            catch (ThreadAbortException)
            {
            }
            catch (Exception e)
            {
                var error = new FlowExecutionException(e, eventSet.Source);
                Error?.Invoke(error);
                ProcessError(error);
            }
        }

        private void ProcessError(FlowExecutionException e)
        {
            if (debugController != null)
            {
                debugController.Error(this, e);
                Evaluate(e.SourceObject, false);
            }
        }

        public bool HasEvent
        {
            get
            {
                return eventQueue.DeeplyHasEventSet;
            }
        }

        public void Update()
        {
            ProcessEventStack();
        }

        internal void AddFlowSourceObject(FlowSourceObjectBase source)
        {
            sources.Add(source.Id, source);
        }

        internal void AddFlowInfo(FlowInfo flowInfo)
        {
            flowInfos.Add(flowInfo);
        }

        internal void Connect(KeyValuePair<FlowSourceObjectBase, KeyValuePair<string, string>[]>[] defaultValues)
        {
            foreach (FlowInfo flowInfo in flowInfos)
            {
                FlowSourceObjectBase src = sources[flowInfo.SrcID], dest = sources[flowInfo.DestID];
                src.Connect(src, dest, flowInfo);

#if DEBUG
                //Console.WriteLine("Connect {0}'s{1} to {2}'s{3}", src.GetType().Name, flowInfo.SrcName, dest.GetType().Name, flowInfo.DestName);
#endif
            }

            this.defaultValues = defaultValues;
        }

        public void Initialize()
        {
            if (initialized)
            {
                return;
            }

            foreach (FunctionScope scope in scopes)
            {
                FunctionScope parent = null;
                if (scope.ParentId < 0)
                {
                    parent = this.scope;
                }
                else
                {
                    parent = scopes.FirstOrDefault(s => s.Id == scope.ParentId);
                }
                parent.AddChild(scope);
            }

            foreach (FlowSourceObjectBase source in sources.Values)
            {
                source.Manager = this;
                source.Scope = GetScopeById(source.ScopeId);
                source.ContextScope = contextScope;
                source.GlobalScope = globalScope;
                source.Initialize();
            }

            foreach (KeyValuePair<FlowSourceObjectBase, KeyValuePair<string, string>[]> kvp in defaultValues)
            {
                foreach (KeyValuePair<string, string> defaultValue in kvp.Value)
                {
                    kvp.Key.SetValue(defaultValue.Key, defaultValue.Value);
                }
            }

            initialized = true;
        }

        private FunctionScope GetScopeById(int scopeId)
        {
            if (scopeId < 0)
            {
                return scope;
            }
            return scopes.FirstOrDefault(s => s.Id == scopeId);
        }

        public bool ContainsAttribute(Type attributeType)
        {
            return sources.Values.Any(s => s.ContainsAttribute(attributeType));
        }

        public bool ContainsNode(string nodeName)
        {
            return sources.Values.Any(s => s.Name == nodeName);
        }

        public void Reset()
        {
            eventQueue.Clear();
            currentQueue = eventQueue;
            scope.Clear();
            foreach (FlowSourceObjectBase source in sources.Values)
            {
                source.Reset();
            }
        }

        internal void Evaluate(FlowSourceObjectBase source)
        {
            Evaluate(source, true);
        }

        internal void Evaluate(FlowSourceObjectBase source, bool checkBreakPoints)
        {
            if (evaluating || debugController == null || lastEvaluateSource == source || source == null || nextOperation == OperationType.Abort)
            {
                return;
            }
            lastEvaluateSource = source;
            if (nextOperation != OperationType.StepIn && checkBreakPoints)
            {
                lock (breakPointLock)
                {
                    if (!breakPoints.Contains(source.Id))
                    {
                        return;
                    }
                }
            }

            evaluating = true;
#if DEBUG
            Console.WriteLine("Evaluate: {0}", source.GetType().FullName);
#endif
            var serializedText = SerializeSource(source);
#if DEBUG
            Console.WriteLine(serializedText);
#endif
            debugController.OnSourceChanged(serializedText, this);
            nextOperation = debugController.WaitOperation(this);
            evaluating = false;
        }

        internal string SerializeSource(int sourceId)
        {
            if (sources.TryGetValue(sourceId, out FlowSourceObjectBase source))
            {
                return SerializeSource(source);
            }
            return null;
        }

        internal string SerializeSource(FlowSourceObjectBase source)
        {
            var serializer = new Serializer();
            serializer.Serialize(source);
            serializer.Serialize(source.Scope);
            return serializer.ToString();
        }

        public void AddBreakPoint(int sourceId)
        {
            lock (breakPointLock)
            {
                breakPoints.Add(sourceId);
            }
        }

        public void RemoveBreakPoint(int sourceId)
        {
            lock (breakPointLock)
            {
                breakPoints.Remove(sourceId);
            }
        }
    }
}
