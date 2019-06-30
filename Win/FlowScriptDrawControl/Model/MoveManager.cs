using FlowScriptDrawControl.Command;
using FlowScriptDrawControl.Control;
using System;
using System.Linq;
using System.Windows;
using System.Windows.Input;

namespace FlowScriptDrawControl.Model
{
    public class MoveManager
    {
        struct MoveInfo
        {
            public double OffsetX;
            public double OffsetY;
        }

        private Point mouseDownPos;
        private Point areaShift;
        private MoveInfo[] moveInfos;

        private PositionableControl[] controls;
        private FlowAreaControl areaControl;
        private Command.CommandManager commandManager;
        private MovePositionablesCommand command;

        public PositionableControl[] Controls
        {
            get
            {
                return controls.ToArray();
            }
        }

        public event EventHandler Moved;
        public event EventHandler MoveEnd;
        public event EventHandler ControlPressed;

        public MoveManager(PositionableControl[] controls, Command.CommandManager commandManager)
        {
            this.commandManager = commandManager;
            this.controls = controls;
        }

        public void Initialize()
        {
            controls[0].IsMoved = false;
            mouseDownPos = controls[0].PointToScreen(Mouse.GetPosition(controls[0]));
            moveInfos = controls.Select(c => new MoveInfo
            {
                OffsetX = c.CurrentPositionable.Position.X,
                OffsetY = c.CurrentPositionable.Position.Y
            }).ToArray();

            areaControl = Utility.GetParent<FlowAreaControl>(controls[0]);
            areaControl.MouseMove += areaControl_MouseMove;
            areaControl.MouseUp += areaControl_MouseUp;
            areaControl.MouseLeave += areaControl_MouseLeave;
            areaControl.KeyDown += areaControl_KeyDown;
            areaShift = new Point(areaControl.CurrentShiftX, areaControl.CurrentShiftY);
        }

        private void DisableMove()
        {
            areaControl.MouseMove -= areaControl_MouseMove;
            areaControl.MouseUp -= areaControl_MouseUp;
            areaControl.MouseLeave -= areaControl_MouseLeave;
            areaControl.KeyDown -= areaControl_KeyDown;
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
                command.NewPoses = moveInfos.Select(m => new Point(diff.X + m.OffsetX, diff.Y + m.OffsetY)).ToArray();
                command.Execute();
            }
            else
            {
                command = new MovePositionablesCommand(controls.Select(c => c.CurrentPositionable).ToArray(),
                    moveInfos.Select(m => new Point(diff.X + m.OffsetX, diff.Y + m.OffsetY)).ToArray());
                commandManager.AddCommand(command);
            }
            OnMoved(this, EventArgs.Empty);
        }

        private void keyPressImpl(KeyboardEventArgs e)
        {
            if (e.KeyboardDevice.IsKeyDown(Key.LeftCtrl))
            {
                OnControlPressed(this, EventArgs.Empty);
            }
        }

        void areaControl_MouseLeave(object sender, MouseEventArgs e)
        {
            DisableMove();
        }

        void areaControl_MouseUp(object sender, MouseButtonEventArgs e)
        {
            DisableMove();
        }

        void areaControl_MouseMove(object sender, MouseEventArgs e)
        {
            MoveImpl();
        }

        void areaControl_KeyDown(object sender, KeyEventArgs e)
        {
            keyPressImpl(e);
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
