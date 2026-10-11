using FlowScriptEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Reflection;
using Xunit;

namespace UnitTests.FlowScript
{
    /// <summary>
    /// ノードのプロパティの読み書き（AutoFast で作った GetPropertyValue と SetPropertyValue）が、名前どおりのプロパティにつながっているかを確かめます。
    /// </summary>
    /// <remarks>
    /// スクリプトはプロパティの名前でこの 2 つを呼ぶので、違うプロパティにつながっていると、エラーにならずに違う値で動いてしまう。
    /// </remarks>
    public class AutoFastTests
    {
        static readonly MethodInfo setPropertyValue = typeof(FlowSourceObjectBase).GetMethod("SetPropertyValue", BindingFlags.Instance | BindingFlags.NonPublic);

        static IEnumerable<Type> NodeTypes()
        {
            Type[] anchors =
            [
                typeof(FlowScriptEngineBasic.FlowSourceObjects.Misc.EntryPointFlowSourceObject),
                typeof(FlowScriptEngineConsole.FlowSourceObjects.Console.WriteLineFlowSourceObject),
                typeof(FlowScriptEngineBasicExtension.ToolTipText),
                typeof(FlowScriptEngineData.ToolTipText),
                typeof(FlowScriptEngineSlimDX.ToolTipText),
                typeof(FlowScriptEnginePPD.ToolTipText),
                typeof(FlowScriptEnginePPDEditor.ToolTipText),
            ];
            return anchors.Select(t => t.Assembly).Distinct()
                .SelectMany(a => a.GetTypes())
                .Where(t => t.IsPublic && t.IsClass && !t.IsAbstract && t.IsSubclassOf(typeof(FlowSourceObjectBase)));
        }

        // 既定の値と違う、型に合う値（作れないときは null）
        static object Sample(Type type)
        {
            if (type == typeof(int)) return 7;
            if (type == typeof(float)) return 1.5f;
            if (type == typeof(double)) return 2.5;
            if (type == typeof(bool)) return true;
            if (type == typeof(string)) return "sample";
            if (type == typeof(long)) return 9L;
            if (type == typeof(object)) return new object();
            if (type == typeof(Vector2)) return new Vector2(1, 2);
            if (type == typeof(Vector3)) return new Vector3(1, 2, 3);
            if (type == typeof(Vector4)) return new Vector4(1, 2, 3, 4);
            if (type == typeof(Quaternion)) return new Quaternion(1, 2, 3, 4);
            if (type == typeof(Matrix4x4)) return Matrix4x4.CreateTranslation(1, 2, 3);
            if (type == typeof(Vortice.Mathematics.Color4)) return new Vortice.Mathematics.Color4(0.1f, 0.2f, 0.3f, 0.4f);
            if (type == typeof(TimeSpan)) return TimeSpan.FromSeconds(3);
            if (type == typeof(DateTime)) return new DateTime(2020, 1, 2);
            if (type == typeof(KeyValuePair<object, object>)) return new KeyValuePair<object, object>("k", "v");
            if (type.IsEnum)
            {
                var values = Enum.GetValues(type);
                return values.Length > 1 ? values.GetValue(values.Length - 1) : null;
            }
            if (type.IsArray)
            {
                return Array.CreateInstance(type.GetElementType(), 1);
            }
            if (type == typeof(IEnumerable<object>)) return new object[] { 1 };
            if (type == typeof(IOrderedEnumerable<object>)) return new object[] { 1 }.OrderBy(o => o);
            if (type == typeof(ILookup<object, object>)) return new object[] { 1 }.ToLookup(o => o);
            if (type == typeof(IGrouping<object, object>)) return new object[] { 1 }.GroupBy(o => o).First();
            if (type.IsInterface || type.IsAbstract || type.IsValueType)
            {
                return null;
            }
            try
            {
                return type.GetConstructor(Type.EmptyTypes) != null ? Activator.CreateInstance(type) : null;
            }
            catch
            {
                return null;
            }
        }

        static bool TryRead(Func<object> read, out object value)
        {
            try
            {
                value = read();
                return true;
            }
            catch
            {
                value = null;
                return false;
            }
        }

        [Fact]
        public void PropertyNames_ReachTheirProperties()
        {
            var problems = new List<string>();
            var checkedNodes = 0;
            var checkedProperties = 0;
            foreach (var type in NodeTypes())
            {
                FlowSourceObjectBase node;
                try
                {
                    node = (FlowSourceObjectBase)Activator.CreateInstance(type);
                }
                catch (Exception e)
                {
                    problems.Add($"{type.FullName}: 作れない ({e.InnerException?.GetType().Name ?? e.GetType().Name})");
                    continue;
                }
                checkedNodes++;
                foreach (var property in type.GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic).GroupBy(p => p.Name).Select(g => g.First()))
                {
                    // スクリプトから使わないもの（エンジンと同じく、Ignore を付けたものは除く）
                    if (property.Name == "Name" || Attribute.IsDefined(property, typeof(IgnoreAttribute), true))
                    {
                        continue;
                    }
                    // 入力: 名前で設定した値が、そのプロパティに入るか（出力でもあるプロパティは、読むと計算した値になるので除く）
                    if (property.GetSetMethod() is { IsPublic: true } && property.GetGetMethod(true) is { IsPublic: false } getter)
                    {
                        var sample = Sample(property.PropertyType);
                        if (sample != null)
                        {
                            try
                            {
                                setPropertyValue.Invoke(node, [property.Name, sample]);
                            }
                            catch (TargetInvocationException e)
                            {
                                problems.Add($"{type.FullName}.{property.Name}: 設定できない ({e.InnerException.GetType().Name})");
                                continue;
                            }
                            if (TryRead(() => getter.Invoke(node, null), out var stored) && !Equals(stored, sample))
                            {
                                problems.Add($"{type.FullName}.{property.Name}: 名前で設定した値が入らない");
                            }
                            checkedProperties++;
                        }
                    }
                    // 出力: 名前で読んだ値が、そのプロパティの値か
                    if (property.GetGetMethod() is { IsPublic: true } publicGetter)
                    {
                        var byName = TryRead(() => node.GetPropertyValue(property.Name), out var named);
                        var direct = TryRead(() => publicGetter.Invoke(node, null), out var value);
                        if (byName && direct && !Equals(named, value) && !(named != null && value != null && named.GetType() == value.GetType()))
                        {
                            // 読むたびに変わる値（乱数や時間）は除く
                            if (TryRead(() => publicGetter.Invoke(node, null), out var again) && Equals(again, value))
                            {
                                problems.Add($"{type.FullName}.{property.Name}: 名前で読んだ値が違う ({named} / {value})");
                            }
                        }
                        else if (byName && named == null && direct && value != null)
                        {
                            problems.Add($"{type.FullName}.{property.Name}: 名前で読めない");
                        }
                        checkedProperties++;
                    }
                }
            }

            Assert.True(checkedNodes > 800, $"確かめたノード: {checkedNodes}");
            Assert.True(checkedProperties > 2000, $"確かめたプロパティ: {checkedProperties}");
            Assert.True(problems.Count == 0, string.Join(Environment.NewLine, problems.Take(60)) + Environment.NewLine + $"（全部で {problems.Count} 件）");
        }

        static IEnumerable<FieldInfo> InstanceFields(Type type)
        {
            for (var t = type; t != null && t != typeof(object); t = t.BaseType)
            {
                foreach (var field in t.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly))
                {
                    yield return field;
                }
            }
        }

        // 作ったばかりのものどうしで同じ値になるフィールド（作るたびに変わるリストなどは比べられない）
        static FieldInfo[] StableFields(Type type, object a, object b)
        {
            return [.. InstanceFields(type).Where(f => Equals(f.GetValue(a), f.GetValue(b)))];
        }

        /// <summary>
        /// 名前で設定すると、プロパティに直接設定したときと同じフィールドに値が入ります（入力にも出力にもなるプロパティも含む）。
        /// </summary>
        [Fact]
        public void SettersByName_MatchDirectSetters()
        {
            var problems = new List<string>();
            var checkedProperties = 0;
            foreach (var type in NodeTypes())
            {
                foreach (var property in type.GetProperties(BindingFlags.Instance | BindingFlags.Public).GroupBy(p => p.Name).Select(g => g.First()))
                {
                    if (property.GetSetMethod() == null || Attribute.IsDefined(property, typeof(IgnoreAttribute), true))
                    {
                        continue;
                    }
                    var sample = Sample(property.PropertyType);
                    if (sample == null)
                    {
                        continue;
                    }
                    FlowSourceObjectBase byName, direct, fresh, fresh2;
                    try
                    {
                        byName = (FlowSourceObjectBase)Activator.CreateInstance(type);
                        direct = (FlowSourceObjectBase)Activator.CreateInstance(type);
                        fresh = (FlowSourceObjectBase)Activator.CreateInstance(type);
                        fresh2 = (FlowSourceObjectBase)Activator.CreateInstance(type);
                        setPropertyValue.Invoke(byName, [property.Name, sample]);
                        property.SetValue(direct, sample);
                    }
                    catch (Exception e)
                    {
                        problems.Add($"{type.FullName}.{property.Name}: 設定できない ({(e.InnerException ?? e).GetType().Name})");
                        continue;
                    }
                    // 名前で設定したときと直接設定したときで、フィールドの値が同じか（値を変えて入れるプロパティもあるので、フィールドごとに比べる）
                    var fields = StableFields(type, fresh, fresh2);
                    var wrong = fields.Where(f => !Equals(f.GetValue(byName), f.GetValue(direct))).Select(f => f.Name).ToArray();
                    if (wrong.Length > 0)
                    {
                        problems.Add($"{type.FullName}.{property.Name}: 名前で設定したときと違う ({string.Join(",", wrong)})");
                    }
                    if (fields.Any(f => !Equals(f.GetValue(direct), f.GetValue(fresh))))
                    {
                        checkedProperties++;
                    }
                }
            }

            Assert.True(checkedProperties > 1300, $"確かめたプロパティ: {checkedProperties}");
            Assert.True(problems.Count == 0, string.Join(Environment.NewLine, problems.Take(60)) + Environment.NewLine + $"（全部で {problems.Count} 件）");
        }

        /// <summary>
        /// ConnectEvent（AutoFast で作る）で名前を指定してつなぐと、その名前のイベントにつながります。
        /// </summary>
        [Fact]
        public void EventsByName_ConnectToTheirEvents()
        {
            var problems = new List<string>();
            var checkedEvents = 0;
            foreach (var type in NodeTypes())
            {
                var events = type.GetEvents().Where(e => e.EventHandlerType == typeof(FlowEventHandler)).ToArray();
                if (events.Length == 0)
                {
                    continue;
                }
                var node = (FlowSourceObjectBase)Activator.CreateInstance(type);
                var connected = new Dictionary<string, FlowEventHandler>();
                foreach (var @event in events)
                {
                    // 名前を使い、イベントごとに別のデリゲートにする（何も使わないラムダは 1 つを使い回す）
                    var name = @event.Name;
                    void handler(FlowEventArgs e) => GC.KeepAlive(name);
                    connected[@event.Name] = handler;
                    node.ConnectEvent(@event.Name, handler);
                }
                foreach (var @event in events)
                {
                    // イベントの中身（同じ名前のフィールド）につないだものが、名前どおりか
                    var field = InstanceFields(type).FirstOrDefault(f => f.Name == @event.Name && f.FieldType == typeof(FlowEventHandler));
                    if (field == null)
                    {
                        continue;
                    }
                    // つなぐときに new FlowEventHandler(handler) で包むので、包んだものか handler そのもの
                    var handlers = ((Delegate)field.GetValue(node))?.GetInvocationList() ?? [];
                    var targets = handlers.Select(d => d.Target is Delegate inner ? inner : d).ToArray();
                    if (targets.Length != 1 || !Equals(targets[0], connected[@event.Name]))
                    {
                        problems.Add($"{type.FullName}.{@event.Name}: つながっていない（{targets.Length} 個）");
                    }
                    checkedEvents++;
                }
            }

            Assert.True(checkedEvents > 600, $"確かめたイベント: {checkedEvents}");
            Assert.True(problems.Count == 0, string.Join(Environment.NewLine, problems.Take(60)) + Environment.NewLine + $"（全部で {problems.Count} 件）");
        }
    }
}
