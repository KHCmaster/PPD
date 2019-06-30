using FlowScriptDrawControl.Model;
using System;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace FlowScriptDrawControl.Control
{
    /// <summary>
    /// SourceItemControl.xaml の相互作用ロジック
    /// </summary>
    public partial class SourceItemControl : UserControl
    {
        public event EventHandler Selected;
        public event EventHandler TryConnected;

        public Item CurrentItem
        {
            get
            {
                return (Item)DataContext;
            }
        }

        public SourceItemControl()
        {
            InitializeComponent();
        }

        private void UpdateBackground()
        {
            this.Background = new SolidColorBrush(CurrentItem.IsMouseOver ? Color.FromArgb(128, 170, 170, 170) : Colors.Transparent);
        }

        private void UserControl_MouseEnter(object sender, MouseEventArgs e)
        {
            CurrentItem.IsMouseOver = true;
            UpdateBackground();
        }

        private void UserControl_MouseLeave(object sender, MouseEventArgs e)
        {
            CurrentItem.IsMouseOver = false;
            UpdateBackground();
        }

        private void UserControl_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
            {
                OnSelected();
                e.Handled = true;
            }
        }

        protected virtual void OnSelected()
        {
            Selected?.Invoke(this, new EventArgs());
        }

        private void UserControl_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
            {
                OnTryConnected();
            }
        }

        protected virtual void OnTryConnected()
        {
            TryConnected?.Invoke(this, new EventArgs());
        }
    }
}
