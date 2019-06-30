using System;
using System.Windows.Forms;

namespace FlowScriptControl.Controls
{
    public class ButtonCell : DataGridViewTextBoxCell
    {
        public event EventHandler ButtonClick;
        ButtonEditingControl ctrl;
        bool initialized;

        public string SourceName
        {
            get;
            private set;
        }

        public string PropertyName
        {
            get;
            private set;
        }

        public Type PropertyType
        {
            get;
            private set;
        }

        public ButtonEditingControl Ctrl
        {
            get
            {
                return ctrl;
            }
        }

        public ButtonCell(string sourceName, string propertyName, Type propertyType)
        {
            SourceName = sourceName;
            PropertyName = propertyName;
            PropertyType = propertyType;
        }

        protected override void OnDataGridViewChanged()
        {
            base.OnDataGridViewChanged();
        }

        public override void InitializeEditingControl(int rowIndex, object initialFormattedValue, DataGridViewCellStyle dataGridViewCellStyle)
        {
            base.InitializeEditingControl(rowIndex, initialFormattedValue, dataGridViewCellStyle);
            ctrl = DataGridView.EditingControl as ButtonEditingControl;
            if (!initialized)
            {
                ctrl.ButtonClick += ctl_ButtonClick;
                initialized = true;
            }
            ctrl.Initializing = true;
            ctrl.Text = initialFormattedValue.ToString();
            ctrl.Initializing = false;
        }

        void ctl_ButtonClick()
        {
            if (IsInEditMode)
            {
                if (ButtonClick != null)
                {
                    ButtonClick.Invoke(this, EventArgs.Empty);
                }
            }
        }

        public override Type EditType
        {
            get
            {
                return typeof(ButtonEditingControl);
            }
        }

        public override Type ValueType
        {
            get
            {
                return typeof(string);
            }
        }
    }
}
