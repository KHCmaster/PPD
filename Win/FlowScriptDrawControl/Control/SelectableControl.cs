using FlowScriptDrawControl.Model;
using System;
using System.Collections.Generic;
using System.Windows.Controls;
using System.Windows.Input;

namespace FlowScriptDrawControl.Control
{
    /// <summary>
    /// SelectableControl.xaml の相互作用ロジック
    /// </summary>
    public class SelectableControl : PositionableControl
    {
        public Selectable CurrentSelectable
        {
            get
            {
                return (Selectable)DataContext;
            }
        }

        public SelectableManager SelectableManager
        {
            get;
            set;
        }

        public MoveManager MoveManager
        {
            get;
            private set;
        }

        public event EventHandler MoveStarted;

        public SelectableControl()
        {
            FocusVisualStyle = null;
            Focusable = true;
            PreviewMouseDown += SelectableControl_PreviewMouseDown;
            MouseDown += SelectableControl_MouseDown;
        }

        private void EnableMove()
        {
            var areaControl = Utility.GetParent<FlowAreaControl>(this);
            var selectables = new List<SelectableControl>();
            foreach (Selectable selectable in SelectableManager.SelectedSelectables)
            {
                if (selectable == CurrentSelectable)
                {
                    continue;
                }
                selectables.Add(areaControl.GetSelectableControl(selectable));
            }
            selectables.Insert(0, this);

            MoveManager = new MoveManager(selectables.ToArray(), areaControl.CommandManager);
            MoveManager.Initialize();
            OnMoveStarted(this, EventArgs.Empty);
        }

        private void BringFront()
        {
            if (this.Parent is Canvas canvas)
            {
                int tail = canvas.Children.Count - 1;
                canvas.Children.Remove(this);
                canvas.Children.Insert(tail, this);
            }
        }

        void SelectableControl_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left || e.ChangedButton == MouseButton.Right)
            {
                if (Keyboard.Modifiers.HasFlag(ModifierKeys.Control))
                {
                    if (CurrentSelectable.IsSelected)
                    {
                        SelectableManager.RemoveSelect(CurrentSelectable);
                    }
                    else
                    {
                        SelectableManager.AddSelect(CurrentSelectable);
                    }
                }
                else
                {
                    if (!CurrentSelectable.IsSelected)
                    {
                        SelectableManager.Select(CurrentSelectable);
                    }
                }
                BringFront();
                this.Focus();
            }
        }

        void SelectableControl_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
            {
                if (CurrentSelectable.IsSelected)
                {
                    EnableMove();
                }
                e.Handled = true;
            }
        }

        private void OnMoveStarted(object sender, EventArgs e)
        {
            MoveStarted?.Invoke(sender, e);
        }
    }
}
