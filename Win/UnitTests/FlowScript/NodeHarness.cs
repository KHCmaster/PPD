using FlowScriptEngine;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;

namespace UnitTests.FlowScript
{
    /// <summary>
    /// ノードを 1 つだけ、スクリプトの中と同じように動かします（イベントを出すノード用）。
    /// </summary>
    /// <remarks>
    /// 空のスクリプトの FlowSourceManager をノードにつけ、その中でノードの In などを呼ぶ。出たイベントの名前を順に記録し、
    /// Predicate などのイベントでは、テストで決めた処理（入力の Result を設定するなど）を行う。
    /// </remarks>
    public sealed class NodeHarness
    {
        static readonly PropertyInfo managerProperty = typeof(FlowSourceObjectBase).GetProperty(nameof(FlowSourceObjectBase.Manager));
        static readonly ConstructorInfo eventArgsConstructor = typeof(FlowEventArgs).GetConstructors(BindingFlags.Instance | BindingFlags.NonPublic)[0];

        readonly FlowSourceManager manager;
        readonly Dictionary<string, Action> handlers = [];
        int callCount;

        public NodeHarness(string basicNode) : this(BasicValueNodeTests.FindType(basicNode))
        {
        }

        public NodeHarness(Type type)
        {
            ScriptRunner.RegisterNodes();
            var engine = new Engine(new StringReader(""));
            manager = engine.Load(new MemoryStream(Encoding.UTF8.GetBytes("<Root><Sources /><Flows /></Root>")), false);
            manager.Initialize();
            Node = (FlowSourceObjectBase)Activator.CreateInstance(type);
            managerProperty.SetValue(Node, manager);
            foreach (var @event in type.GetEvents())
            {
                if (@event.EventHandlerType != typeof(FlowEventHandler))
                {
                    continue;
                }
                var name = @event.Name;
                @event.AddEventHandler(Node, (FlowEventHandler)(e =>
                {
                    Fired.Add(name);
                    if (handlers.TryGetValue(name, out var handler))
                    {
                        handler();
                    }
                }));
            }
        }

        public FlowSourceObjectBase Node { get; }

        /// <summary>
        /// 出たイベントの名前（順に）です。
        /// </summary>
        public List<string> Fired { get; } = [];

        public NodeHarness Set(string name, object value)
        {
            Node.GetType().GetProperty(name).SetValue(Node, value);
            return this;
        }

        public object Get(string name)
        {
            return Node.GetType().GetProperty(name).GetValue(Node);
        }

        /// <summary>
        /// イベントが出たときの処理を決めます（Predicate で Result を決めるなど）。
        /// </summary>
        public NodeHarness On(string eventName, Action<NodeHarness> handler)
        {
            handlers[eventName] = () => handler(this);
            return this;
        }

        /// <summary>
        /// スクリプトの中で処理を行い、出たイベントを処理し終えるまで待ちます。
        /// </summary>
        public NodeHarness Run(Action<NodeHarness> action)
        {
            var key = "test" + callCount++;
            manager.RegisterCallBack(key, args => action(this));
            manager.Call(key, null);
            return this;
        }

        /// <summary>
        /// イベントを受けるメソッド（In など）を呼びます。
        /// </summary>
        public NodeHarness Invoke(string method = "In")
        {
            return Run(h => Node.GetType().GetMethod(method).Invoke(Node, [eventArgsConstructor.Invoke([Node, false])]));
        }

        /// <summary>
        /// 出力をスクリプトの中で読みます（読むときにイベントを出すノード用）。
        /// </summary>
        public object Read(string name)
        {
            object value = null;
            Run(h => value = Get(name));
            return value;
        }
    }
}
