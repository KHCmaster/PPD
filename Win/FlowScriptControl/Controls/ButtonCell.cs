using System;
using System.Windows.Forms;

namespace FlowScriptControl.Controls
{
    public class ButtonCell(string sourceName, string propertyName, Type propertyType) : DataGridViewTextBoxCell
    {
        public event EventHandler ButtonClick;
        ButtonEditingControl ctrl;
        bool initialized;

        public string SourceName
        {
            get;
            private set;
        } = sourceName;

        public string PropertyName
        {
            get;
            private set;
        } = propertyName;

        public Type PropertyType
        {
            get;
            private set;
        } = propertyType;

        public ButtonEditingControl Ctrl
        {
            get
            {
                return ctrl;
            }
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
                ctrl.ButtonClick += Ctl_ButtonClick;
                initialized = true;
            }
            ctrl.Initializing = true;
            ctrl.Text = initialFormattedValue.ToString();
            ctrl.Initializing = false;
        }

        void Ctl_ButtonClick()
        {
            if (IsInEditMode)
            {
                ButtonClick?.Invoke(this, EventArgs.Empty);
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
