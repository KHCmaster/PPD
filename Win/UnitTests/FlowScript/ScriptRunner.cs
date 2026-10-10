using FlowScriptEngine;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Linq;

namespace UnitTests.FlowScript
{
    /// <summary>
    /// テストで使うノードです。In が呼ばれたときの値を記録します。
    /// </summary>
    public class ProbeFlowSourceObject : FlowSourceObjectBase
    {
        public static List<(string Label, object Value, object Value2)> Records { get; } = [];

        public override string Name
        {
            get { return "Test.Probe"; }
        }

        public string Label { private get; set; }

        public object Value { private get; set; }

        public object Value2 { private get; set; }

        public void In(FlowEventArgs e)
        {
            SetValue(nameof(Label));
            SetValue(nameof(Value));
            SetValue(nameof(Value2));
            Records.Add((Label, Value, Value2));
        }

        // ほかのノードは AutoFast で作るプロパティの読み書き（スクリプトからはこれで値を設定する）
        protected override void SetPropertyValue(string propertyName, object value)
        {
            switch (propertyName)
            {
                case nameof(Label):
                    Label = (string)value;
                    break;
                case nameof(Value):
                    Value = value;
                    break;
                case nameof(Value2):
                    Value2 = value;
                    break;
            }
        }
    }

    /// <summary>
    /// ノードとつなぎ方からスクリプト（.fsml と同じ形）を作り、動かします。
    /// </summary>
    public class ScriptRunner
    {
        static readonly object registerLock = new();
        static bool registered;

        readonly XElement sources = new("Sources");
        readonly XElement flows = new("Flows");
        readonly XElement scopes = new("Scopes");
        int nextId = 1;

        /// <summary>
        /// スクリプトで使うノード（Basic と Console とこのテストのノード）を使えるようにします。
        /// </summary>
        public static void RegisterNodes()
        {
            lock (registerLock)
            {
                if (registered)
                {
                    return;
                }
                foreach (var type in new[] { typeof(FlowScriptEngineBasic.FlowSourceObjects.Misc.EntryPointFlowSourceObject), typeof(FlowScriptEngineConsole.FlowSourceObjects.Console.WriteLineFlowSourceObject), typeof(ProbeFlowSourceObject) })
                {
                    _ = FlowSourceEnumerator.EnumerateFromFile(type.Assembly.Location, [typeof(FlowSourceObjectBase)]).ToList();
                }
                registered = true;
            }
        }

        public int Entry()
        {
            return Node(typeof(FlowScriptEngineBasic.FlowSourceObjects.Misc.EntryPointFlowSourceObject));
        }

        /// <summary>
        /// Basic のノードを足します（"Logic.If" など）。値は .fsml と同じく文字列で書きます。
        /// </summary>
        public int Node(string basicNode, params (string Name, string Value)[] values)
        {
            return Node(BasicValueNodeTests.FindType(basicNode), values);
        }

        public int Node(Type type, params (string Name, string Value)[] values)
        {
            var id = nextId++;
            var source = new XElement("Source", new XAttribute("Name", type.FullName), new XAttribute("FullName", type.FullName),
                new XAttribute("ID", id), new XAttribute("X", 0), new XAttribute("Y", 0));
            foreach (var (name, value) in values)
            {
                source.Add(new XElement("Value", new XAttribute("Name", name), new XAttribute("Value", value)));
            }
            sources.Add(source);
            return id;
        }

        /// <summary>
        /// 記録するノードを足します。
        /// </summary>
        public int Probe(string label)
        {
            return Node(typeof(ProbeFlowSourceObject), ("Label", label));
        }

        /// <summary>
        /// 文字列のリスト（ArrayList）を作ります。Logic.Sequential の Out0 から順に使い、使った次の番号を返します。
        /// </summary>
        public (int List, int NextOut) StringList(int sequential, params string[] items)
        {
            var list = Node("ArrayList.Value");
            Flow(sequential, "Out0", list, "In");
            for (int i = 0; i < items.Length; i++)
            {
                var add = Node("ArrayList.Add");
                Flow(sequential, "Out" + (i + 1), add, "In").Flow(list, "Value", add, "ArrayList").Flow(Node("String.Value", ("Value", items[i])), "Value", add, "Value");
            }
            return (list, items.Length + 1);
        }

        /// <summary>
        /// スコープ（エディターで囲んだ範囲）を足します。parent がなければ一番外のスコープの中です。
        /// </summary>
        public int Scope(int? parent = null)
        {
            var id = nextId++;
            var scope = new XElement("Scope", new XAttribute("ID", id));
            if (parent != null)
            {
                scope.Add(new XAttribute("ScopeID", parent.Value));
            }
            scopes.Add(scope);
            return id;
        }

        /// <summary>
        /// ノードをスコープの中に入れます。
        /// </summary>
        public ScriptRunner InScope(int scope, params int[] nodes)
        {
            foreach (var node in nodes)
            {
                sources.Elements("Source").Single(e => (int)e.Attribute("ID") == node).SetAttributeValue("ScopeID", scope);
            }
            return this;
        }

        public ScriptRunner Flow(int srcId, string srcName, int destId, string destName)
        {
            flows.Add(new XElement("Flow", new XAttribute("SrcID", srcId), new XAttribute("SrcName", srcName), new XAttribute("DestID", destId), new XAttribute("DestName", destName)));
            return this;
        }

        public string ToXml()
        {
            return new XElement("Root", sources, flows, new XElement("Comments"), scopes).ToString();
        }

        /// <summary>
        /// スクリプトを始めから動かし、記録した値を返します。エラーがあれば例外にします。
        /// </summary>
        public List<(string Label, object Value, object Value2)> Run(string stdin = "")
        {
            RegisterNodes();
            ProbeFlowSourceObject.Records.Clear();
            var errors = new List<FlowExecutionException>();
            var engine = new Engine(new StringReader(stdin));
            using var stream = new MemoryStream(Encoding.UTF8.GetBytes(ToXml()));
            var manager = engine.Load(stream, false);
            manager.Error += errors.Add;
            manager.Initialize();
            engine.Start();
            if (errors.Count > 0)
            {
                throw new InvalidOperationException("スクリプトでエラーになりました", errors[0]);
            }
            return [.. ProbeFlowSourceObject.Records];
        }

        /// <summary>
        /// いくつかのスクリプトを 1 つのエンジンで（読んだ順に）動かし、記録した値を返します。
        /// GlobalScope はすべてで同じで、ContextScope は context が同じスクリプトどうしで同じです。
        /// </summary>
        public static List<(string Label, object Value, object Value2)> RunTogether(params (ScriptRunner Script, Guid Context)[] scripts)
        {
            RegisterNodes();
            ProbeFlowSourceObject.Records.Clear();
            var errors = new List<FlowExecutionException>();
            var engine = new Engine(new StringReader(""));
            engine.Error += errors.Add;
            foreach (var (script, context) in scripts)
            {
                using var stream = new MemoryStream(Encoding.UTF8.GetBytes(script.ToXml()));
                engine.Load(stream, false, context, null).Initialize();
            }
            engine.Start();
            if (errors.Count > 0)
            {
                throw new InvalidOperationException("スクリプトでエラーになりました", errors[0]);
            }
            return [.. ProbeFlowSourceObject.Records];
        }

        /// <summary>
        /// 記録した値のうち、ラベルが同じものの Value です。
        /// </summary>
        public static object[] Values(List<(string Label, object Value, object Value2)> records, string label)
        {
            return [.. records.Where(r => r.Label == label).Select(r => r.Value)];
        }
    }
}
