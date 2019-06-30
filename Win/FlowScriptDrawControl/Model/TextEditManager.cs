using FlowScriptDrawControl.Control;
using System;
using System.Windows.Controls;
using System.Windows.Input;

namespace FlowScriptDrawControl.Model
{
    class TextEditManager
    {
        private bool isMouseDown;
        private bool isInEdit;

        private UserControl ownerControl;
        private TextBlock textBlock;
        private TextBox textBox;

        public event Func<object, bool> EnterEdit;

        public TextEditManager(UserControl ownerControl, TextBlock textBlock, TextBox textBox)
        {
            this.ownerControl = ownerControl;
            this.textBlock = textBlock;
            this.textBox = textBox;

            textBox.IsEnabled = false;
            textBox.Visibility = System.Windows.Visibility.Hidden;
            textBlock.MouseDown += textBlock_MouseDown;
            textBox.PreviewKeyDown += textBox_PreviewKeyDown;
            textBox.LostFocus += textBox_LostFocus;
            ownerControl.KeyDown += ownerControl_KeyDown;
        }

        void textBox_LostFocus(object sender, System.Windows.RoutedEventArgs e)
        {
            EndEdit();
        }

        void textBox_PreviewKeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                if (Keyboard.Modifiers.HasFlag(ModifierKeys.Control))
                {
                    int start = textBox.SelectionStart;
                    textBox.SelectedText = "\n";
                    textBox.CaretIndex = textBox.SelectionStart = start + 1;
                    textBox.SelectionLength = 0;
                }
                else
                {
                    EndEdit();
                    e.Handled = true;
                }
            }
        }

        void textBlock_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (!Keyboard.Modifiers.HasFlag(ModifierKeys.Control))
            {
                EnableEdit();
            }
        }


        private void EnableEdit()
        {
            isMouseDown = true;
            var flowAreaControl = Utility.GetParent<FlowAreaControl>(ownerControl);
            flowAreaControl.MouseUp += flowAreaControl_MouseUp;
            flowAreaControl.MouseLeave += flowAreaControl_MouseLeave;
        }

        private void DisableEdit()
        {
            isMouseDown = false;
            var flowAreaControl = Utility.GetParent<FlowAreaControl>(ownerControl);
            flowAreaControl.MouseUp -= flowAreaControl_MouseUp;
            flowAreaControl.MouseLeave -= flowAreaControl_MouseLeave;
        }

        public void BeginEdit()
        {
            isInEdit = true;
            textBox.IsEnabled = true;
            textBox.Visibility = System.Windows.Visibility.Visible;
            textBox.Focus();
            textBox.SelectAll();
        }

        public void EndEdit()
        {
            isInEdit = false;
            textBox.IsEnabled = false;
            textBox.Visibility = System.Windows.Visibility.Hidden;
        }

        void flowAreaControl_MouseLeave(object sender, MouseEventArgs e)
        {
            DisableEdit();
        }

        void flowAreaControl_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (isMouseDown && OnEnterEdit())
            {
                BeginEdit();
            }
            DisableEdit();
        }

        void ownerControl_KeyDown(object sender, KeyEventArgs e)
        {
            if (!isInEdit && e.Key == Key.Enter)
            {
                BeginEdit();
                e.Handled = true;
            }
        }

        private bool OnEnterEdit()
        {
            if (EnterEdit != null)
            {
                return EnterEdit(this);
            }
            return true;
        }
    }
}
