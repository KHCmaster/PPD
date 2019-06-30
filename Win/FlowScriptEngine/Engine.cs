using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Xml;

namespace FlowScriptEngine
{
    public class Engine
    {
        public event Action<FlowExecutionException> Error;

        private List<FlowSourceManager> managers;
        private FunctionScope globalScope;

        private Dictionary<Guid, FunctionScope> contexts;

        public TextReader ConsoleReader
        {
            get;
            private set;
        }

        public Engine()
            : this(Console.In)
        {
        }

        public Engine(TextReader consoleReader)
        {
            ConsoleReader = consoleReader;
            managers = new List<FlowSourceManager>();
            globalScope = new FunctionScope(ScopeType.Global);
            contexts = new Dictionary<Guid, FunctionScope>();
        }

        public FlowSourceManager Load(Stream stream, bool debugMode)
        {
            return Load(stream, debugMode, Guid.NewGuid(), null);
        }

        public FlowSourceManager Load(Stream stream, bool debugMode, DebugController debugController)
        {
            return Load(stream, debugMode, Guid.NewGuid(), debugController);
        }

        public FlowSourceManager Load(Stream stream, bool debugMode, Guid contextGuid, DebugController debugController)
        {
            if (!contexts.TryGetValue(contextGuid, out FunctionScope contextScope))
            {
                contextScope = new FunctionScope(ScopeType.Context);
                contexts.Add(contextGuid, contextScope);
                globalScope.AddChild(contextScope);
            }

            var manager = new FlowSourceManager(this, debugMode, globalScope, contextScope, debugController);
            var defaultValues = LoadSync(stream, manager);

            manager.Connect(defaultValues);
            manager.Error += e =>
            {
                Error?.Invoke(e);
            };
            managers.Add(manager);

            return manager;
        }

        private KeyValuePair<FlowSourceObjectBase, KeyValuePair<string, string>[]>[] LoadSync(Stream stream, FlowSourceManager manager)
        {
            var reader = XmlReader.Create(stream);
            KeyValuePair<FlowSourceObjectBase, KeyValuePair<string, string>[]>[] defaultValues = null;
            while (reader.Read())
            {
                if (reader.IsStartElement())
                {
                    switch (reader.Name)
                    {
                        case "Sources":
                            defaultValues = ReadSources(reader.ReadSubtree(), manager);
                            break;
                        case "Flows":
                            ReadFlows(reader.ReadSubtree(), manager);
                            break;
                        case "Scope":
                            ReadScopes(reader.ReadSubtree(), manager);
                            break;
                    }
                }
            }
            reader.Close();
            return defaultValues;
        }

        private KeyValuePair<FlowSourceObjectBase, KeyValuePair<string, string>[]>[] ReadSources(XmlReader reader, FlowSourceManager manager)
        {
            var ret = new List<KeyValuePair<FlowSourceObjectBase, KeyValuePair<string, string>[]>>();
            while (reader.Read())
            {
                if (reader.IsStartElement())
                {
                    switch (reader.Name)
                    {
                        case "Source":
                            var fullName = reader.GetAttribute("FullName");
                            var ID = int.Parse(reader.GetAttribute("ID"), CultureInfo.InvariantCulture);
                            var scopeId = ParseId(reader.GetAttribute("ScopeID"));
                            var source = FlowSourceObjectManager.CreateSource(fullName);
                            if (source == null)
                            {
                                throw new ArgumentException(String.Format("{0} not found", fullName));
                            }
                            source.Id = ID;
                            source.ScopeId = scopeId;
                            var defaultValues = ReadValues(reader.ReadSubtree(), source);
                            ret.Add(new KeyValuePair<FlowSourceObjectBase, KeyValuePair<string, string>[]>(source, defaultValues));
                            manager.AddFlowSourceObject(source);
                            break;
                    }
                }
            }
            reader.Close();

            return ret.ToArray();
        }

        private KeyValuePair<string, string>[] ReadValues(XmlReader reader, FlowSourceObjectBase source)
        {
            var ret = new List<KeyValuePair<string, string>>();
            while (reader.Read())
            {
                if (reader.IsStartElement())
                {
                    switch (reader.Name)
                    {
                        case "Value":
                            string name = reader.GetAttribute("Name"), value = reader.GetAttribute("Value");
                            ret.Add(new KeyValuePair<string, string>(name, value));
                            break;
                    }
                }
            }
            reader.Close();

            return ret.ToArray();
        }

        private void ReadFlows(XmlReader reader, FlowSourceManager manager)
        {
            while (reader.Read())
            {
                switch (reader.Name)
                {
                    case "Flow":
                        int srcID = int.Parse(reader.GetAttribute("SrcID"), CultureInfo.InvariantCulture), destID = int.Parse(reader.GetAttribute("DestID"), CultureInfo.InvariantCulture);
                        var flowInfo = new FlowInfo
                        {
                            SrcID = srcID,
                            DestID = destID,
                            SrcName = reader.GetAttribute("SrcName"),
                            DestName = reader.GetAttribute("DestName")
                        };
                        manager.AddFlowInfo(flowInfo);
                        break;
                }
            }
            reader.Close();
        }

        private void ReadScopes(XmlReader reader, FlowSourceManager manager)
        {
            while (reader.Read())
            {
                switch (reader.Name)
                {
                    case "Scope":
                        var ID = int.Parse(reader.GetAttribute("ID"), CultureInfo.InvariantCulture);
                        var scopeId = ParseId(reader.GetAttribute("ScopeID"));
                        var scope = new FunctionScope(ScopeType.User)
                        {
                            Id = ID,
                            ParentId = scopeId
                        };
                        manager.AddScope(scope);
                        break;
                }
            }
            reader.Close();
        }

        private int ParseId(string idStr)
        {
            if (!int.TryParse(idStr, NumberStyles.Integer, CultureInfo.InvariantCulture, out int ret))
            {
                ret = -1;
            }
            return ret;
        }

        public void Update()
        {
            while (true)
            {
                foreach (FlowSourceManager manager in managers)
                {
                    manager.Update();
                }

                if (!HasEvent)
                {
                    break;
                }
            }
        }

        private bool HasEvent
        {
            get
            {
                foreach (FlowSourceManager manager in managers)
                {
                    if (manager.HasEvent && !manager.IsAborted)
                    {
                        return true;
                    }
                }

                return false;
            }
        }

        public void Call(string callBackName, object[] args)
        {
            foreach (FlowSourceManager manager in managers)
            {
                manager.Call(callBackName, args);
            }

            Update();
        }

        public void Start()
        {
            Call(FlowSourceManager.START, null);
        }

        public void Clear()
        {
            managers.Clear();
        }

        public void Reset()
        {
            foreach (FlowSourceManager manager in managers)
            {
                manager.Reset();
            }
            foreach (var contextScope in contexts.Values)
            {
                contextScope.Clear();
            }
            globalScope.Clear();
        }

        public bool ContainsAttribute(Type attributeType)
        {
            return managers.Any(m => m.ContainsAttribute(attributeType));
        }

        public bool ContainsNode(string nodeName)
        {
            return managers.Any(m => m.ContainsNode(nodeName));
        }
    }

    internal class FlowInfo
    {
        public int SrcID;
        public int DestID;
        public string SrcName;
        public string DestName;
    }
}
