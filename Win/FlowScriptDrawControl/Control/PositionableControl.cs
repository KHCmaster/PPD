using FlowScriptDrawControl.Model;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;

namespace FlowScriptDrawControl.Control
{
    public class PositionableControl : UserControl
    {
        public bool IsMoved
        {
            get;
            set;
        }

        public Positionable CurrentPositionable
        {
            get
            {
                return (Positionable)DataContext;
            }
        }

        public PositionableControl()
        {
            DataContextChanged += PositionableControl_DataContextChanged;
        }

        public void UpdateTransform()
        {
            if (!Utility.IsLoading)
            {
                this.UpdateLayout();
            }
        }

        void PositionableControl_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (e.OldValue != null)
            {
                ((Positionable)e.OldValue).PropertyChanged -= PositionableControl_PropertyChanged;
            }
            if (e.NewValue != null)
            {
                ((Positionable)e.NewValue).PropertyChanged += PositionableControl_PropertyChanged;
            }
        }

        void PositionableControl_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "Transform2")
            {
                UpdateTransform();
            }
        }
    }
}
