using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace FlowScriptEngine
{
    public abstract class FlowSourceObjectBase
    {
        private static Dictionary<Type, InnerData> dataDictionary;

        class InnerData
        {
            public Dictionary<string, CustomMemberInfo<PropertyInfo>> InProperty;
            public Dictionary<string, CustomMemberInfo<PropertyInfo>> OutProperty;
            public Dictionary<string, CustomMemberInfo<MethodInfo>> MethodList;
            public Dictionary<string, CustomMemberInfo<EventInfo>> EventList;
            public Dictionary<string, string> ReplacedInPropertyNames;
            public Dictionary<string, string> ReplacedOutPropertyNames;
            public Dictionary<string, string> ReplacedMethodNames;
            public Dictionary<string, string> ReplacedEventNames;
            public string[] inPropertyNameList;
            public string[] outPropertyNameList;
            public string[] methodNameList;
            public string[] eventNameList;
            public object[] attributes;
            public CustomMemberInfo<PropertyInfo>[] inPropertyList;
            public CustomMemberInfo<PropertyInfo>[] outPropertyList;
            public CustomMemberInfo<MethodInfo>[] methodList;
            public CustomMemberInfo<EventInfo>[] eventList;
            public string toolTipText = "";
            public string toolTipTextKey = "";
            public string summary = "";
            public string warningKey = "";
            public string warning = "";
        }

        private Dictionary<string, CustomMemberInfo<PropertyInfo>> inProperty;
        private Dictionary<string, CustomMemberInfo<PropertyInfo>> outProperty;
        private Dictionary<string, CustomMemberInfo<MethodInfo>> methodList;
        private Dictionary<string, CustomMemberInfo<EventInfo>> eventList;
        private Dictionary<string, string> replacedInPropertyNames;
        private Dictionary<string, string> replacedOutPropertyNames;
        private Dictionary<string, string> replacedMethodNames;
        private Dictionary<string, string> replacedEventNames;
        private Dictionary<string, FlowObject> inFlowList;
        private Dictionary<string, bool> inIsConnected;
        private Dictionary<string, bool> outIsConnected;

        InnerData innerData;
        protected FlowSourceObjectBase()
        {
            inFlowList = new Dictionary<string, FlowObject>();
            inIsConnected = new Dictionary<string, bool>();
            outIsConnected = new Dictionary<string, bool>();

            SetDefaultInfos();
        }

        static FlowSourceObjectBase()
        {
            dataDictionary = new Dictionary<Type, InnerData>();
        }

        private void SetDefaultInfos()
        {
            var type = this.GetType();
            innerData = GetInnerData(this, type);

            inProperty = innerData.InProperty;
            outProperty = innerData.OutProperty;
            methodList = innerData.MethodList;
            eventList = innerData.EventList;
            replacedInPropertyNames = innerData.ReplacedInPropertyNames;
            replacedOutPropertyNames = innerData.ReplacedOutPropertyNames;
            replacedMethodNames = innerData.ReplacedMethodNames;
            replacedEventNames = innerData.ReplacedEventNames;
        }

        private static InnerData GetInnerData(FlowSourceObjectBase source, Type type)
        {
            if (!dataDictionary.TryGetValue(type, out InnerData ret))
            {
                ret = ParseInnerData(source, type);
                dataDictionary.Add(type, ret);
            }

            return ret;
        }

        private static InnerData ParseInnerData(FlowSourceObjectBase source, Type type)
        {
            string summary = "", flowToolTipText = "", flowToolTipTextKey = "", warningKey = "", warning = "";
#if DEBUG
            bool attributeFound = false;
#endif
            var attributes = type.GetCustomAttributes(true);
            foreach (Object attribute in attributes)
            {
                if (attribute is IToolTipText)
                {
                    summary = ((IToolTipText)attribute).Summary;
                    flowToolTipText = ((IToolTipText)attribute).Text;
                    flowToolTipTextKey = ((IToolTipText)attribute).TextKey;
#if DEBUG
                    attributeFound = true;
                    if (String.IsNullOrEmpty(flowToolTipText))
                    {
                        Console.WriteLine("[{0}]'s summary is null or empty", type);
                    }
#endif
                }
                else if (attribute is IWarning)
                {
                    warningKey = ((IWarning)attribute).TextKey;
                    warning = ((IWarning)attribute).Text;
                }
            }

#if DEBUG
            if (!attributeFound)
            {
                Console.WriteLine("[{0}]'s summary is not implemented", type);
            }
#endif

            var inProperty = new Dictionary<string, CustomMemberInfo<PropertyInfo>>();
            var outProperty = new Dictionary<string, CustomMemberInfo<PropertyInfo>>();
            var methodList = new Dictionary<string, CustomMemberInfo<MethodInfo>>();
            var eventList = new Dictionary<string, CustomMemberInfo<EventInfo>>();

            string toolTipText, toolTipTextKey;
            string[] replacedNames;
            foreach (PropertyInfo propertyinfo in type.GetProperties())
            {
                if (propertyinfo.Name == "Name") continue;
                if (propertyinfo.CanRead && propertyinfo.GetGetMethod() != null && propertyinfo.GetGetMethod().IsPublic)
                {
                    if (CheckAttribute(source, propertyinfo, out toolTipText, out toolTipTextKey, out replacedNames))
                    {
                        outProperty.Add(propertyinfo.Name, new CustomMemberInfo<PropertyInfo>(propertyinfo, toolTipText, toolTipTextKey, replacedNames));
                    }
                }
                if (propertyinfo.CanWrite && propertyinfo.GetSetMethod() != null && propertyinfo.GetSetMethod().IsPublic)
                {
                    if (CheckAttribute(source, propertyinfo, out toolTipText, out toolTipTextKey, out replacedNames))
                    {
                        inProperty.Add(propertyinfo.Name, new CustomMemberInfo<PropertyInfo>(propertyinfo, toolTipText, toolTipTextKey, replacedNames));
                    }
                }
            }
            foreach (MethodInfo methodinfo in type.GetMethods())
            {
                if (methodinfo.IsPublic && !methodinfo.IsAbstract && methodinfo.ReturnType == typeof(void))
                {
                    var parameterinfos = methodinfo.GetParameters();
                    if (parameterinfos.Length == 1 && parameterinfos[0].ParameterType == typeof(FlowEventArgs))
                    {
                        if (CheckAttribute(source, methodinfo, out toolTipText, out toolTipTextKey, out replacedNames))
                        {
                            methodList.Add(methodinfo.Name, new CustomMemberInfo<MethodInfo>(methodinfo, toolTipText, toolTipTextKey, replacedNames));
                        }
                    }
                }
            }
            foreach (EventInfo eventinfo in type.GetEvents())
            {
                if (eventinfo.EventHandlerType == typeof(FlowEventHandler) && eventinfo.GetAddMethod() != null && eventinfo.GetAddMethod().IsPublic
                    && eventinfo.GetRemoveMethod() != null && eventinfo.GetRemoveMethod().IsPublic)
                {
                    if (CheckAttribute(source, eventinfo, out toolTipText, out toolTipTextKey, out replacedNames))
                    {
                        eventList.Add(eventinfo.Name, new CustomMemberInfo<EventInfo>(eventinfo, toolTipText, toolTipTextKey, replacedNames));
                    }
                }
            }

            // copy to innerlist
            CustomMemberInfo<PropertyInfo>[] inPropertyList = new CustomMemberInfo<PropertyInfo>[inProperty.Count];
            string[] inPropertyNameList = new string[inProperty.Count];
            CustomMemberInfo<PropertyInfo>[] outPropertyList = new CustomMemberInfo<PropertyInfo>[outProperty.Count];
            string[] outPropertyNameList = new string[outProperty.Count];
            string[] methodNameList = new string[methodList.Count];
            CustomMemberInfo<MethodInfo>[] methods = new CustomMemberInfo<MethodInfo>[methodList.Count];
            string[] eventNameList = new string[eventList.Count];
            CustomMemberInfo<EventInfo>[] events = new CustomMemberInfo<EventInfo>[eventList.Count];

            var replacedInPropertyNames = new Dictionary<string, string>();
            var replacedOutPropertyNames = new Dictionary<string, string>();
            var replacedMethodNames = new Dictionary<string, string>();
            var replacedEventNames = new Dictionary<string, string>();
            int iter = 0;
            foreach (string name in inProperty.Keys)
            {
                inPropertyList[iter] = inProperty[name];
                inPropertyNameList[iter] = name;
                foreach (var replacedName in inPropertyList[iter].ReplacedNames)
                {
                    replacedInPropertyNames[replacedName] = name;
                }
                iter++;
            }
            Array.Sort(inPropertyNameList, inPropertyList, StringComparer.InvariantCulture);

            iter = 0;
            foreach (string name in outProperty.Keys)
            {
                outPropertyList[iter] = outProperty[name];
                outPropertyNameList[iter] = name;
                foreach (var replacedName in outPropertyList[iter].ReplacedNames)
                {
                    replacedOutPropertyNames[replacedName] = name;
                }
                iter++;
            }
            Array.Sort(outPropertyNameList, outPropertyList, StringComparer.InvariantCulture);

            iter = 0;
            foreach (string name in methodList.Keys)
            {
                methods[iter] = methodList[name];
                methodNameList[iter] = name;
                foreach (var replacedName in methods[iter].ReplacedNames)
                {
                    replacedMethodNames[replacedName] = name;
                }
                iter++;
            }
            Array.Sort(methodNameList, methods, StringComparer.InvariantCulture);

            iter = 0;
            foreach (string name in eventList.Keys)
            {
                events[iter] = eventList[name];
                eventNameList[iter] = name;
                foreach (var replacedName in events[iter].ReplacedNames)
                {
                    replacedEventNames[replacedName] = name;
                }
                iter++;
            }
            Array.Sort(eventNameList, events, StringComparer.InvariantCulture);

            return new InnerData
            {
                EventList = eventList,
                MethodList = methodList,
                InProperty = inProperty,
                OutProperty = outProperty,
                ReplacedEventNames = replacedEventNames,
                ReplacedInPropertyNames = replacedInPropertyNames,
                ReplacedOutPropertyNames = replacedOutPropertyNames,
                ReplacedMethodNames = replacedMethodNames,
                eventList = events,
                eventNameList = eventNameList,
                inPropertyList = inPropertyList,
                inPropertyNameList = inPropertyNameList,
                methodList = methods,
                methodNameList = methodNameList,
                outPropertyList = outPropertyList,
                outPropertyNameList = outPropertyNameList,
                toolTipText = flowToolTipText,
                toolTipTextKey = flowToolTipTextKey,
                summary = summary,
                warningKey = warningKey,
                warning = warning,
                attributes = attributes
            };
        }

#if DEBUG
        public void AutoTest()
        {
            var type = this.GetType();
            var attributes = type.GetCustomAttributes(true);
            foreach (Object attribute in attributes)
            {
                if (attribute is IgnoreTestAttribute)
                {
                    Console.WriteLine("{0}'s test is ignored.", type);
                    return;
                }
            }
            Scope = new FunctionScope(ScopeType.Default);
            ContextScope = new FunctionScope(ScopeType.Context);
            GlobalScope = new FunctionScope(ScopeType.Global);
            Manager = new FlowSourceManager(new Engine(), true, GlobalScope, ContextScope, null);
            try
            {
                OnInitialize();
                foreach (CustomMemberInfo<PropertyInfo> info in outProperty.Values)
                {
                    try
                    {
                        GetPropertyValue(info.MemberInfo.Name);
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine("Property Test:{2}\n{0} in {1}", e.InnerException != null ? e.InnerException.GetType().Name : e.GetType().Name, type, info.MemberInfo.PropertyType);
                        Console.WriteLine();
                    }
                }

                foreach (CustomMemberInfo<MethodInfo> memberInfo in methodList.Values)
                {
                    try
                    {
                        memberInfo.MemberInfo.Invoke(this, new object[] { new FlowEventArgs(this, false) });
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine("Method Test:{2}\n{0} in {1}", e.InnerException.GetType().Name, type, memberInfo.MemberInfo.Name);
                        Console.WriteLine();
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message + "\n" + e.StackTrace);
            }
        }
#endif

        private static bool CheckAttribute(FlowSourceObjectBase source, MemberInfo memberInfo, out string toolTipText, out string toolTipTextKey, out string[] replacedNames)
        {
            toolTipText = "";
            toolTipTextKey = "";
            Object[] objects = Attribute.GetCustomAttributes(memberInfo, true);
            var replaced = new HashSet<string>();
#if DEBUG
            bool attributeFound = false;
#endif
            for (int i = 0; i < objects.Length; i++)
            {
                if (objects[i] is IgnoreAttribute)
                {
                    replacedNames = replaced.ToArray();
                    return false;
                }
                else if (objects[i] is IToolTipText)
                {
                    toolTipText = (objects[i] as IToolTipText).Text;
                    toolTipTextKey = (objects[i] as IToolTipText).TextKey;
#if DEBUG
                    attributeFound = true;
                    if (String.IsNullOrEmpty(toolTipText))
                    {
                        Console.WriteLine("[{0}]'s [{1}]'s summary is null or empty", source.GetType(), memberInfo);
                    }
#endif
                }
                else if (objects[i] is ReplacedAttribute)
                {
                    var name = (objects[i] as ReplacedAttribute).Name;
                    replaced.Add(name);
                }
            }

#if DEBUG
            if (!attributeFound)
            {
                Console.WriteLine("[{0}]'s [{1}]'s summary is not implemented", source.GetType(), memberInfo);
            }
#endif
            replacedNames = replaced.ToArray();
            return true;
        }

        [Ignore]
        public IList<string> InPropertyNames
        {
            get
            {
                return innerData.inPropertyNameList;
            }
        }
        [Ignore]
        public IList<CustomMemberInfo<PropertyInfo>> InProperties
        {
            get
            {
                return innerData.inPropertyList;
            }
        }
        [Ignore]
        public IList<string> OutPropertyNames
        {
            get
            {
                return innerData.outPropertyNameList;
            }
        }
        [Ignore]
        public IList<CustomMemberInfo<PropertyInfo>> OutProperties
        {
            get
            {
                return innerData.outPropertyList;
            }
        }
        [Ignore]
        public IList<string> InMethodNames
        {
            get
            {
                return innerData.methodNameList;
            }
        }
        [Ignore]
        public IList<CustomMemberInfo<MethodInfo>> InMethods
        {
            get
            {
                return innerData.methodList;
            }
        }
        [Ignore]
        public IList<string> OutMethodNames
        {
            get
            {
                return innerData.eventNameList;
            }
        }
        [Ignore]
        public IList<CustomMemberInfo<EventInfo>> OutMethods
        {
            get
            {
                return innerData.eventList;
            }
        }
        [Ignore]
        public string ToolTipText
        {
            get
            {
                return innerData.toolTipText;
            }
        }
        [Ignore]
        public string ToolTipTextKey
        {
            get
            {
                return innerData.toolTipTextKey;
            }
        }
        [Ignore]
        public string Summary
        {
            get
            {
                return innerData.summary;
            }
        }
        [Ignore]
        public string WarningKey
        {
            get
            {
                return innerData.warningKey;
            }
        }
        [Ignore]
        public string Warning
        {
            get
            {
                return innerData.warning;
            }
        }
        [Ignore]
        public abstract string Name
        {
            get;
        }
        [Ignore]
        public FlowSourceManager Manager
        {
            get;
            internal set;
        }
        [Ignore]
        public FunctionScope Scope
        {
            get;
            internal set;
        }
        [Ignore]
        public FunctionScope ContextScope
        {
            get;
            internal set;
        }
        [Ignore]
        public FunctionScope GlobalScope
        {
            get;
            internal set;
        }
        [Ignore]
        public int Id
        {
            get;
            internal set;
        }
        [Ignore]
        public int ScopeId
        {
            get;
            internal set;
        }

        protected bool IsConnected(string memberName, bool isIn)
        {
            if (isIn)
            {
                if (inIsConnected.TryGetValue(memberName, out bool ret))
                {
                    return ret;
                }
            }
            else
            {
                if (outIsConnected.TryGetValue(memberName, out bool ret))
                {
                    return ret;
                }
            }
            return false;
        }

        public virtual object GetPropertyValue(string propertyName)
        {
            return null;
        }

        protected virtual void SetPropertyValue(string propertyName, object value)
        {
            return;
        }

        protected void SetValue(string propertyName)
        {
            try
            {
                if (inFlowList.TryGetValue(propertyName, out FlowObject flowObject))
                {
                    var value = flowObject.GetSrcValue();
                    SetPropertyValue(propertyName, value);
                }
#if DEBUG
                else if (!inProperty.ContainsKey(propertyName))
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("[{0}] doesn't have {1}'s property", this.GetType().FullName, propertyName);
                    Console.ForegroundColor = ConsoleColor.White;
                }
#endif
            }
            catch (Exception e)
            {
                throw new FlowExecutionException(e, this);
            }
        }

        internal void SetValue(string propertyName, string value)
        {
            CustomMemberInfo<PropertyInfo> info = inProperty[propertyName];
            if (info != null)
            {
                var formatter = TypeFormatterManager.GetFormatter(info.MemberInfo.PropertyType);
                if (formatter.Format(value, out object convert))
                {
                    SetPropertyValue(propertyName, convert);
                }
            }
        }

        internal void Connect(FlowSourceObjectBase src, FlowSourceObjectBase dest, FlowInfo flowInfo)
        {
            try
            {
                string actualSrcName = flowInfo.SrcName;
                string actualDestName = flowInfo.DestName;
                if (src.outProperty.ContainsKey(flowInfo.SrcName) || src.replacedOutPropertyNames.TryGetValue(flowInfo.SrcName, out actualSrcName))
                {
                    //property
                    var flowObject = new FlowObject
                    {
                        Source = src,
                        Dest = dest,
                        SourcePropertyName = actualSrcName,
                        SourceType = src.outProperty[actualSrcName].MemberInfo.PropertyType,
                        DestPropertyName = flowInfo.DestName
                    };
                    dest.ConnectProperty(flowObject, out actualDestName);
                    flowObject.DestPropertyName = actualDestName;
                    flowObject.DestType = dest.inProperty[actualDestName].MemberInfo.PropertyType;
                }
                else
                {
                    //event
                    ConnectMethod(flowInfo.SrcName, dest, flowInfo.DestName, out actualSrcName, out actualDestName);
                }
                src.outIsConnected[actualSrcName] = true;
                dest.inIsConnected[actualDestName] = true;
            }
            catch (Exception e)
            {
                throw new ConnectErrorException(e, src.Name, dest.Name, flowInfo.SrcName, flowInfo.DestName);
            }
        }


        private void ConnectProperty(FlowObject flow, out string actualDestName)
        {
            actualDestName = flow.DestPropertyName;
            if (!inProperty.ContainsKey(flow.DestPropertyName) && !replacedInPropertyNames.TryGetValue(flow.DestPropertyName, out actualDestName))
            {
                return;
            }
            if (inFlowList.ContainsKey(actualDestName))
            {
                //disconnect current
                // error
                throw new Exception("error in connection");
                /*InFlowList[flow.DestPropertyName].DisConnect();
                InFlowList.Add(flow.DestPropertyName, flow);*/
            }
            else
            {
                inFlowList.Add(actualDestName, flow);
            }
        }

        private void ConnectMethod(string methodNameOfSource, FlowSourceObjectBase destObject, string methodNameOfDest, out string actualSrcName, out string actualDestName)
        {
            actualSrcName = methodNameOfSource;
            actualDestName = methodNameOfDest;
            if (eventList.ContainsKey(methodNameOfSource) || replacedEventNames.TryGetValue(methodNameOfSource, out actualSrcName))
            {
                if (destObject.methodList.ContainsKey(methodNameOfDest) || replacedMethodNames.TryGetValue(methodNameOfDest, out actualDestName))
                {
                    MethodInfo mi = destObject.methodList[actualDestName].MemberInfo;
                    ConnectEvent(actualSrcName, (FlowEventHandler)Delegate.CreateDelegate(typeof(FlowEventHandler), destObject, mi));
                }
            }
        }

        public virtual void ConnectEvent(string eventName, FlowEventHandler eventHandler)
        {
            EventInfo ei = eventList[eventName].MemberInfo;
            ei.AddEventHandler(this, eventHandler);
        }

        protected void ProcessChildEvent()
        {
            Manager.ProcessOnlyCurrentQueue();
        }

        protected void FireEvent(FlowEventHandler eventHandler)
        {
            FireEvent(eventHandler, false);
        }

        protected void FireEvent(FlowEventHandler eventHandler, bool doContinuously)
        {
#if DEBUG
            //Console.WriteLine("{0} Fire", this.GetType().Name);
#endif
            FireEvent(eventHandler, doContinuously, false);
        }

        protected void FireEvent(FlowEventHandler eventHandler, bool doContinuously, bool isLoopEnd)
        {
            Manager.AddEventExecute(eventHandler, this, doContinuously, isLoopEnd);
        }

        protected void BreakLoop()
        {
            Manager.BreakLoop();
        }

        protected void ContinueLoop()
        {
            Manager.ContinueLoop();
        }

        internal void Initialize()
        {
            OnInitialize();
        }

        protected virtual void OnInitialize()
        {

        }

        internal void Reset()
        {
            OnReset();
        }

        protected virtual void OnReset()
        {

        }

        public bool ContainsAttribute(Type attributeType)
        {
            foreach (var attribute in innerData.attributes)
            {
                if (attribute.GetType().IsSubclassOf(attributeType) || attribute.GetType() == attributeType)
                {
                    return true;
                }
            }

            return false;
        }
    }
}
