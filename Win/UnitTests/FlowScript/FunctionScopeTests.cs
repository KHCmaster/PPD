using FlowScriptEngine;
using System;
using System.Collections.Generic;
using Xunit;

namespace UnitTests.FlowScript
{
    /// <summary>
    /// スコープ（変数と、スコープの間で送るイベント）のテストです。
    /// </summary>
    public class FunctionScopeTests
    {
        [Fact]
        public void Values_AreReadFromParents()
        {
            var parent = new FunctionScope(ScopeType.Global);
            var child = new FunctionScope(ScopeType.Default);
            typeof(FunctionScope).GetMethod("AddChild", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic).Invoke(parent, [child]);
            parent.SetValue("shared", 1);
            child.SetValue("own", 2);
            child.SetValue("own", 3);

            Assert.Equal(1, child.GetValue("shared"));
            Assert.Equal(3, child.GetValue("own"));
            Assert.True(child.IsDefinedInParents("shared"));
            Assert.False(child.IsDefined("shared"));
            Assert.False(parent.IsDefinedInParents("shared"));
            Assert.Throws<Exception>(() => child.GetValue("none"));
            Assert.Equal(ScopeType.Default, child.ScopeType);

            child.Clear();
            Assert.False(child.IsDefined("own"));
        }

        static (FunctionScope Parent, FunctionScope Child) CreateScopes()
        {
            var parent = new FunctionScope(ScopeType.Global);
            var child = new FunctionScope(ScopeType.Default);
            typeof(FunctionScope).GetMethod("AddChild", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic).Invoke(parent, [child]);
            return (parent, child);
        }

        [Fact]
        public void InvokeEvent_GoesThroughPreviewThenChildrenThenSelf()
        {
            var (parent, child) = CreateScopes();
            var order = new List<string>();
            parent.PreviewEventInvoked += e => { order.Add("parent preview " + e.Arg); e.OnEndInvoked(); };
            child.PreviewEventInvoked += e => { order.Add("child preview"); e.OnEndInvoked(); };
            child.EventInvoked += e => { order.Add("child"); e.OnEndInvoked(); };
            parent.EventInvoked += e => { order.Add("parent"); e.OnEndInvoked(); };

            parent.InvokeEvent("x", 5);

            Assert.Equal(["parent preview 5", "child preview", "child", "parent"], order);
        }

        [Fact]
        public void HandledInPreview_StopsEvent()
        {
            var (parent, child) = CreateScopes();
            var reached = 0;
            parent.PreviewEventInvoked += e => { e.Handled = true; e.OnEndInvoked(); };
            child.EventInvoked += e => { reached++; e.OnEndInvoked(); };

            parent.InvokeEvent("x", null);

            Assert.Equal(0, reached);
        }

        [Fact]
        public void HandledInChild_DoesNotReachParent()
        {
            var (parent, child) = CreateScopes();
            var parentReached = 0;
            child.EventInvoked += e => { e.Handled = true; e.OnEndInvoked(); };
            parent.EventInvoked += e => { parentReached++; e.OnEndInvoked(); };

            parent.InvokeEvent("x", null);

            Assert.Equal(0, parentReached);
        }

        // スコープの中に、別の名前のイベントを受けるノードが 2 つあるとき
        // （以前は、受けるノードがそれぞれ次へ進めたので、子のスコープに受けるノードの数だけ届いていた）
        [Fact]
        public void SeveralHandlers_EventIsDeliveredOnce()
        {
            var (parent, child) = CreateScopes();
            var handled = 0;
            parent.PreviewEventInvoked += e => e.OnEndInvoked();
            parent.PreviewEventInvoked += e => e.OnEndInvoked();
            child.EventInvoked += e =>
            {
                if (e.EventName == "x")
                {
                    handled++;
                }
                e.OnEndInvoked();
            };
            child.EventInvoked += e => e.OnEndInvoked();

            parent.InvokeEvent("x", null);

            Assert.Equal(1, handled);
        }

        [Fact]
        public void SeveralHandlers_AnyHandledStopsEvent()
        {
            var (parent, child) = CreateScopes();
            var reached = 0;
            var parentReached = 0;
            parent.PreviewEventInvoked += e => e.OnEndInvoked();
            parent.PreviewEventInvoked += e => { e.Handled = true; e.OnEndInvoked(); };
            child.EventInvoked += e => { reached++; e.OnEndInvoked(); };
            parent.EventInvoked += e => { parentReached++; e.OnEndInvoked(); };

            parent.InvokeEvent("x", null);

            Assert.Equal((0, 0), (reached, parentReached));
        }

        // 受けるものが後で終わる（スクリプトのノードはイベントの処理が終わってから次へ進める）ときも、すべて終わってから進める
        [Fact]
        public void SeveralHandlers_WaitForAllToEnd()
        {
            var (parent, child) = CreateScopes();
            var pending = new List<FunctionScopeArg>();
            var reached = 0;
            parent.PreviewEventInvoked += pending.Add;
            parent.PreviewEventInvoked += pending.Add;
            child.EventInvoked += e => { reached++; e.OnEndInvoked(); };

            parent.InvokeEvent("x", null);
            Assert.Equal(0, reached);
            pending[0].OnEndInvoked();
            Assert.Equal(0, reached);
            pending[1].OnEndInvoked();

            Assert.Equal(1, reached);
        }

        [Fact]
        public void SeveralChildren_ParentRunsAfterAllChildren()
        {
            var parent = new FunctionScope(ScopeType.Global);
            var order = new List<string>();
            foreach (var name in new[] { "a", "b" })
            {
                var child = new FunctionScope(ScopeType.Default);
                typeof(FunctionScope).GetMethod("AddChild", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic).Invoke(parent, [child]);
                child.EventInvoked += e => { order.Add(name + "1"); e.OnEndInvoked(); };
                child.EventInvoked += e => { order.Add(name + "2"); e.OnEndInvoked(); };
            }
            parent.EventInvoked += e => { order.Add("parent"); e.OnEndInvoked(); };

            parent.InvokeEvent("x", null);

            // 以前は、子の受けるものが 2 つあると、1 つ目の子が 2 回終わったことになり、2 つ目の子より先に親が動いていた
            Assert.Equal(["a1", "a2", "b1", "b2", "parent"], order);
        }
    }
}
