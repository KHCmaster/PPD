using FlowScriptDrawControl.Model;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows;

namespace FlowScriptDrawControl.Control
{
    /// <summary>
    /// ScopeControl.xaml の相互作用ロジック
    /// </summary>
    public partial class ScopeControl : PositionableControl
    {
        private const int ScopeMargin = 20;

        public Scope CurrentScope
        {
            get
            {
                return (Scope)DataContext;
            }
        }

        private List<ScopeControl> childScopes;
        private List<PositionableControl> controls;

        public ScopeControl[] ChildScopes
        {
            get
            {
                return childScopes.ToArray();
            }
        }

        void parentScope_TransformChanged(object sender, EventArgs e)
        {
            UpdateSize();
        }

        void parentScope_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            UpdateSize();
        }

        public ScopeControl()
        {
            childScopes = new List<ScopeControl>();
            controls = new List<PositionableControl>();

            InitializeComponent();

            Loaded += ScopeControl_Loaded;
        }

        public void AddScope(ScopeControl scopeControl)
        {
            if (childScopes.Contains(scopeControl))
            {
                return;
            }

            scopeControl.CurrentPositionable.PropertyChanged += CurrentPositionable_PropertyChanged;
            scopeControl.CurrentPositionable.SizeChanged += CurrentPositionable_SizeChanged;
            childScopes.Add(scopeControl);
            UpdateSize();
        }

        public void RemoveScope(ScopeControl scopeControl)
        {
            if (!childScopes.Contains(scopeControl))
            {
                return;
            }

            scopeControl.CurrentPositionable.PropertyChanged -= CurrentPositionable_PropertyChanged;
            scopeControl.CurrentPositionable.SizeChanged -= CurrentPositionable_SizeChanged;
            childScopes.Remove(scopeControl);
            UpdateSize();
        }

        public bool ContainsScope(ScopeControl scopeControl)
        {
            return childScopes.Contains(scopeControl);
        }

        public void AddPositionable(PositionableControl control)
        {
            if (controls.Contains(control))
            {
                return;
            }

            control.CurrentPositionable.PropertyChanged += CurrentPositionable_PropertyChanged;
            control.CurrentPositionable.SizeChanged += CurrentPositionable_SizeChanged;
            controls.Add(control);
            UpdateSize();
        }

        void CurrentPositionable_SizeChanged()
        {
            UpdateSize();
        }

        void CurrentPositionable_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "Transform")
            {
                UpdateSize();
            }
        }

        public void RemovePositionable(PositionableControl control)
        {
            if (!controls.Contains(control))
            {
                return;
            }

            control.CurrentPositionable.PropertyChanged -= CurrentPositionable_PropertyChanged;
            control.CurrentPositionable.SizeChanged -= CurrentPositionable_SizeChanged;
            controls.Remove(control);
            UpdateSize();
        }

        public bool Contains(PositionableControl control)
        {
            return controls.Contains(control);
        }

        private void UpdateSize()
        {
            if ((controls.Count == 0 && childScopes.Count == 0) || Utility.IsLoading)
            {
                mainGrid.Width = mainGrid.Height = ScopeMargin * 2;
                return;
            }

            var topLeft = new Point(double.PositiveInfinity, double.PositiveInfinity);
            var bottomRight = new Point(double.NegativeInfinity, double.NegativeInfinity);

            foreach (ScopeControl control in childScopes)
            {
                topLeft = new Point(Math.Min(control.CurrentPositionable.Position.X, topLeft.X),
                    Math.Min(control.CurrentPositionable.Position.Y, topLeft.Y));
                bottomRight = new Point(Math.Max(control.CurrentPositionable.Position.X + control.ActualWidth, bottomRight.X),
                    Math.Max(control.CurrentPositionable.Position.Y + control.ActualHeight, bottomRight.Y));
            }

            foreach (PositionableControl control in controls)
            {
                topLeft = new Point(Math.Min(control.CurrentPositionable.Position.X, topLeft.X),
                    Math.Min(control.CurrentPositionable.Position.Y, topLeft.Y));
                bottomRight = new Point(Math.Max(control.CurrentPositionable.Position.X + control.ActualWidth, bottomRight.X),
                    Math.Max(control.CurrentPositionable.Position.Y + control.ActualHeight, bottomRight.Y));
            }

            CurrentScope.Position = new Point(topLeft.X - ScopeMargin, topLeft.Y - ScopeMargin);
            mainGrid.Width = bottomRight.X - topLeft.X + ScopeMargin * 2;
            mainGrid.Height = bottomRight.Y - topLeft.Y + ScopeMargin * 2;
        }


        void ScopeControl_Loaded(object sender, RoutedEventArgs e)
        {
            UpdateSize();
        }
    }
}
