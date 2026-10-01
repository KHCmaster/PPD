using FlowScriptDrawControl.Command;
using FlowScriptDrawControl.Control;
using System;
using System.Linq;
using System.Windows;
using System.Windows.Input;

namespace FlowScriptDrawControl.Model
{
    public class MoveManager(PositionableControl[] controls, Command.CommandManager commandManager)
    {
        struct MoveInfo
        {
            public double OffsetX;
            public double OffsetY;
        }

        private Point mouseDownPos;
        private Point areaShift;
        private MoveInfo[] moveInfos;

        private readonly PositionableControl[] controls = controls;
        private FlowAreaControl areaControl;
        private readonly Command.CommandManager commandManager = commandManager;
        private MovePositionablesCommand command;

        public PositionableControl[] Controls
        {
            get
            {
                return [.. controls];
            }
        }

        public event EventHandler Moved;
        public event EventHandler MoveEnd;
        public event EventHandler ControlPressed;

        public void Initialize()
        {
            controls[0].IsMoved = false;
            mouseDownPos = controls[0].PointToScreen(Mouse.GetPosition(controls[0]));
            moveInfos = [.. controls.Select(c => new MoveInfo
            {
                OffsetX = c.CurrentPositionable.Position.X,
                OffsetY = c.CurrentPositionable.Position.Y
            })];

            areaControl = Utility.GetParent<FlowAreaControl>(controls[0]);
            areaControl.MouseMove += AreaControl_MouseMove;
            areaControl.MouseUp += AreaControl_MouseUp;
            areaControl.MouseLeave += AreaControl_MouseLeave;
            areaControl.KeyDown += AreaControl_KeyDown;
            areaShift = new Point(areaControl.CurrentShiftX, areaControl.CurrentShiftY);
        }

        private void DisableMove()
        {
            areaControl.MouseMove -= AreaControl_MouseMove;
            areaControl.MouseUp -= AreaControl_MouseUp;
            areaControl.MouseLeave -= AreaControl_MouseLeave;
            areaControl.KeyDown -= AreaControl_KeyDown;
            OnMoveEnd(this, EventArgs.Empty);
        }

        private void MoveImpl()
        {
            var currentPos = controls[0].PointToScreen(Mouse.GetPosition(controls[0]));
            if (command == null && mouseDownPos == currentPos)
            {
                return;
            }

            controls[0].IsMoved = true;
            var diff = new Point((currentPos.X - mouseDownPos.X - areaControl.CurrentShiftX + areaShift.X) / areaControl.CurrentScale,
                (currentPos.Y - mouseDownPos.Y - areaControl.CurrentShiftY + areaShift.Y) / areaControl.CurrentScale);
            if (command != null)
            {
                command.NewPoses = [.. moveInfos.Select(m => new Point(diff.X + m.OffsetX, diff.Y + m.OffsetY))];
                command.Execute();
            }
            else
            {
                command = new MovePositionablesCommand([.. controls.Select(c => c.CurrentPositionable)],
                    [.. moveInfos.Select(m => new Point(diff.X + m.OffsetX, diff.Y + m.OffsetY))]);
                commandManager.AddCommand(command);
            }
            OnMoved(this, EventArgs.Empty);
        }

        private void KeyPressImpl(KeyboardEventArgs e)
        {
            if (e.KeyboardDevice.IsKeyDown(Key.LeftCtrl))
            {
                OnControlPressed(this, EventArgs.Empty);
            }
        }

        void AreaControl_MouseLeave(object sender, MouseEventArgs e)
        {
            DisableMove();
        }

        void AreaControl_MouseUp(object sender, MouseButtonEventArgs e)
        {
            DisableMove();
        }

        void AreaControl_MouseMove(object sender, MouseEventArgs e)
        {
            MoveImpl();
        }

        void AreaControl_KeyDown(object sender, KeyEventArgs e)
        {
            KeyPressImpl(e);
        }

        private void OnMoved(object sender, EventArgs e)
        {
            Moved?.Invoke(sender, e);
        }

        private void OnMoveEnd(object sender, EventArgs e)
        {
            MoveEnd?.Invoke(sender, e);
        }

        private void OnControlPressed(object sender, EventArgs e)
        {
            ControlPressed?.Invoke(sender, e);
        }
    }
}
