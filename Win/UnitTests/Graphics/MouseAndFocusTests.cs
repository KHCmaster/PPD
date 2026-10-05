using PPDFramework;
using System.Collections.Generic;
using System.Numerics;
using System.Reflection;
using System.Windows.Forms;
using Vortice.Mathematics;
using Xunit;
using static PPDFramework.MouseEvent;

namespace UnitTests.Graphics
{
    /// <summary>
    /// マウスの操作（GameComponent のイベントと MouseManager）、フォーカス（FocusManager）、StackObject の並べ方のテストです。
    /// </summary>
    [Collection(GraphicsCollection.Name)]
    public class MouseAndFocusTests(GraphicsDevice graphics)
    {
        RectangleComponent Rectangle(float x, float y, float width, float height)
        {
            return new RectangleComponent(graphics.Device, graphics.ResourceManager, new Color4(1, 1, 1, 1))
            {
                Position = new Vector2(x, y),
                RectangleWidth = width,
                RectangleHeight = height,
                AcceptMouseOperation = true,
            };
        }

        // 起きたイベントを名前で記録する
        static List<string> Record(GameComponent component)
        {
            var log = new List<string>();
            component.MouseEnter += (s, e) => log.Add("enter");
            component.MouseLeave += (s, e) => log.Add("leave");
            component.MouseMove += (s, e) => log.Add("move");
            component.MouseLeftDown += (s, e) => log.Add("left down");
            component.MouseLeftUp += (s, e) => log.Add("left up");
            component.MouseLeftClick += (s, e) => log.Add("left click");
            component.MouseRightDown += (s, e) => log.Add("right down");
            component.MouseRightUp += (s, e) => log.Add("right up");
            component.MouseRightClick += (s, e) => log.Add("right click");
            component.MouseMiddleDown += (s, e) => log.Add("middle down");
            component.MouseMiddleUp += (s, e) => log.Add("middle up");
            component.MouseMiddleClick += (s, e) => log.Add("middle click");
            component.MouseWheel += (s, e) => log.Add($"wheel {e.WheelValue}");
            return log;
        }

        static MouseInfo At(float x, float y, params MouseEvent[] events) => new(new Vector2(x, y), events);

        static MouseEvent Button(float x, float y, bool up, MouseEventType type) => new(new Vector2(x, y), up, type);

        [Fact]
        public void EnterMoveAndLeave()
        {
            using var rectangle = Rectangle(10, 10, 100, 50);
            var log = Record(rectangle);

            rectangle.UpdateMouseInfo(At(0, 0));
            rectangle.UpdateMouseInfo(At(20, 20));
            rectangle.UpdateMouseInfo(At(20, 20));
            rectangle.UpdateMouseInfo(At(30, 20));
            rectangle.UpdateMouseInfo(At(200, 20));

            Assert.Equal(["enter", "move", "move", "leave"], log);
        }

        [Fact]
        public void ClicksNeedDownAndUpInside()
        {
            using var rectangle = Rectangle(0, 0, 100, 100);
            var log = Record(rectangle);

            rectangle.UpdateMouseInfo(At(50, 50, Button(50, 50, false, MouseEventType.Left), Button(50, 50, true, MouseEventType.Left)));
            rectangle.UpdateMouseInfo(At(50, 50, Button(50, 50, false, MouseEventType.Right), Button(60, 60, true, MouseEventType.Right)));
            rectangle.UpdateMouseInfo(At(50, 50, Button(50, 50, false, MouseEventType.Middle), Button(50, 50, true, MouseEventType.Middle)));
            rectangle.UpdateMouseInfo(At(50, 50, new MouseEvent(new Vector2(50, 50), 120, MouseEventType.Wheel)));

            Assert.Equal([
                "enter", "move",
                "left down", "left up", "left click",
                "right down", "right up", "right click",
                "middle down", "middle up", "middle click",
                "wheel 120",
            ], log);
        }

        [Fact]
        public void ReleasedOutside_IsNotClick()
        {
            using var rectangle = Rectangle(0, 0, 100, 100);
            var log = Record(rectangle);

            rectangle.UpdateMouseInfo(At(50, 50, Button(50, 50, false, MouseEventType.Left)));
            rectangle.UpdateMouseInfo(At(500, 50, Button(500, 50, true, MouseEventType.Left)));
            // 外で離したので、次に中で離してもクリックにはならない
            rectangle.UpdateMouseInfo(At(50, 50, Button(50, 50, true, MouseEventType.Left)));
            rectangle.UpdateMouseInfo(At(500, 50, Button(500, 50, false, MouseEventType.Right), Button(500, 50, true, MouseEventType.Middle)));

            Assert.DoesNotContain("left click", log);
            Assert.Equal(["enter", "move", "left down", "leave", "enter", "move", "left up", "leave"], log);
        }

        [Fact]
        public void Children_UseTheirOwnPosition()
        {
            using var parent = new SpriteObject(graphics.Device) { Position = new Vector2(100, 100), AcceptMouseOperation = true };
            var child = Rectangle(50, 0, 20, 20);
            parent.AddChild(child);
            var log = Record(child);

            parent.UpdateMouseInfo(At(160, 110, Button(160, 110, false, MouseEventType.Left)));
            parent.UpdateMouseInfo(At(60, 10, Button(60, 10, true, MouseEventType.Left)));

            Assert.Equal(["enter", "move", "left down", "leave"], log);
        }

        [Fact]
        public void NotAccepting_GetsNothing()
        {
            using var rectangle = Rectangle(0, 0, 100, 100);
            rectangle.AcceptMouseOperation = false;
            var log = Record(rectangle);

            rectangle.UpdateMouseInfo(At(50, 50, Button(50, 50, false, MouseEventType.Left)));

            Assert.Empty(log);
        }

        static void Raise(Control control, string method, MouseEventArgs e)
        {
            typeof(Control).GetMethod(method, BindingFlags.Instance | BindingFlags.NonPublic).Invoke(control, [e]);
        }

        [Fact]
        public void MouseManager_ConvertsControlEvents()
        {
            using var control = new Control();
            _ = control.Handle;
            // 画面の左上が (10, 20)、2 倍に拡大されているとき
            var manager = new MouseManager(control, new Vector2(10, 20), new Vector2(2, 2));

            Raise(control, "OnMouseDown", new MouseEventArgs(MouseButtons.Left, 1, 30, 40, 0));
            Raise(control, "OnMouseUp", new MouseEventArgs(MouseButtons.Right, 1, 50, 60, 0));
            Raise(control, "OnMouseDown", new MouseEventArgs(MouseButtons.Middle, 1, 10, 20, 0));
            Raise(control, "OnMouseDown", new MouseEventArgs(MouseButtons.XButton1, 1, 10, 20, 0));
            Raise(control, "OnMouseWheel", new MouseEventArgs(MouseButtons.None, 0, 10, 20, -120));

            var events = manager.GetMouseEvents().Events;

            Assert.Equal(5, events.Length);
            Assert.Equal((new Vector2(10, 10), false, MouseEventType.Left), (events[0].Position, events[0].Up, events[0].EventType));
            Assert.Equal((new Vector2(20, 20), true, MouseEventType.Right), (events[1].Position, events[1].Up, events[1].EventType));
            Assert.Equal((Vector2.Zero, MouseEventType.Middle), (events[2].Position, events[2].EventType));
            Assert.Equal(MouseEventType.None, events[3].EventType);
            Assert.Equal((-120f, MouseEventType.Wheel), (events[4].WheelValue, events[4].EventType));
            // 一度読んだイベントは消える
            Assert.Empty(manager.GetMouseEvents().Events);
            Assert.Empty(MouseInfo.Empty.Events);
        }

        sealed class Focusable(PPDDevice device, string name, List<string> log, bool handles) : FocusableGameComponent(device)
        {
            protected override void OnFocus(IFocusable focusObject)
            {
                log.Add($"{name} focus");
                base.OnFocus(focusObject);
            }

            protected override void OnLostFocus(IFocusable focusObject)
            {
                log.Add($"{name} lost");
                base.OnLostFocus(focusObject);
            }

            protected override bool OnProcessInput(InputInfoBase inputInfo)
            {
                log.Add($"{name} input");
                base.OnProcessInput(inputInfo);
                return handles;
            }
        }

        sealed class NoInput() : InputInfoBase(false)
        {
            public override int GetPressingFrame(ButtonType buttonType) => 0;
            public override bool IsPressed(ButtonType buttonType) => buttonType == ButtonType.Circle;
            public override bool IsReleased(ButtonType buttonType) => false;
        }

        [Fact]
        public void FocusManager_StackOfFocus()
        {
            var log = new List<string>();
            var manager = new FocusManager();
            using var menu = new Focusable(graphics.Device, "menu", log, handles: false) { HandleOverFocusInput = true };
            using var dialog = new Focusable(graphics.Device, "dialog", log, handles: false);

            manager.Focus(menu);
            manager.Focus(dialog);
            Assert.Same(dialog, manager.CurrentFocusObject);
            Assert.True(dialog.Focused);
            Assert.False(menu.Focused);
            Assert.True(menu.OverFocused);

            // 一番上が処理しなければ、HandleOverFocusInput のものにも渡す
            manager.ProcessInput(new NoInput());

            manager.RemoveFocus();
            Assert.Same(menu, manager.CurrentFocusObject);
            Assert.True(menu.Focused);
            Assert.Null(dialog.FocusManager);
            manager.RemoveFocus();
            manager.RemoveFocus();
            Assert.Null(manager.CurrentFocusObject);
            Assert.Null(manager.BaseScene);

            Assert.Equal(["menu focus", "dialog focus", "menu lost", "dialog input", "menu input", "dialog lost", "menu focus", "menu lost"], log);
        }

        [Fact]
        public void Focusable_InputedEventCanHandle()
        {
            var manager = new FocusManager();
            using var top = new FocusableGameComponent(graphics.Device);
            using var under = new FocusableGameComponent(graphics.Device) { HandleOverFocusInput = true };
            var underInputs = 0;
            InputEventArgs received = null;
            top.Inputed += (s, e) =>
            {
                received = e;
                e.Handled = true;
            };
            under.Inputed += (s, e) => underInputs++;
            var focused = new List<IFocusable>();
            top.GotFocused += (s, e) => focused.Add(e.FocusObject);
            manager.Focus(under);
            manager.Focus(top);

            manager.ProcessInput(new NoInput());

            Assert.True(received.AnyPressed);
            Assert.False(received.AnyReleased);
            // 上が処理したので、下には渡さない
            Assert.Equal(0, underInputs);
            Assert.Same(under, Assert.Single(focused));
        }

        [Fact]
        public void StackObject_LaysOutChildren()
        {
            using var vertical = new StackObject(graphics.Device, Rectangle(0, 0, 10, 20), Rectangle(0, 0, 30, 5), Rectangle(0, 0, 1, 1));
            vertical.Update();
            Assert.Equal([new Vector2(0, 0), new Vector2(0, 20), new Vector2(0, 25)], [.. System.Linq.Enumerable.Select(vertical.Children, c => c.Position)]);

            using var horizontal = new StackObject(graphics.Device, [Rectangle(0, 0, 10, 20), Rectangle(0, 0, 30, 5)]) { IsHorizontal = true };
            horizontal.Update();
            Assert.Equal([new Vector2(0, 0), new Vector2(10, 0)], [.. System.Linq.Enumerable.Select(horizontal.Children, c => c.Position)]);

            using var empty = new StackObject(graphics.Device);
            empty.Update();
            Assert.Equal(0, empty.ChildrenCount);
        }
    }
}
