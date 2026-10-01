using System;
using System.Windows.Forms;

namespace FlowScriptControl.Controls
{
    public class ButtonEditingControl : CustomButton, IDataGridViewEditingControl
    {
        DataGridView dataGridView;
        private bool valueChanged;
        int rowIndex;

        public bool Initializing
        {
            get;
            set;
        }

        public object EditingControlFormattedValue
        {
            get
            {
                return this.Text;
            }
            set
            {
                if (value is string v)
                {
                    this.Text = v;
                }
            }
        }

        public object GetEditingControlFormattedValue(DataGridViewDataErrorContexts context)
        {
            return EditingControlFormattedValue;
        }

        public void ApplyCellStyleToEditingControl(DataGridViewCellStyle dataGridViewCellStyle)
        {
            this.Font = dataGridViewCellStyle.Font;
        }

        public int EditingControlRowIndex
        {
            get
            {
                return rowIndex;
            }
            set
            {
                rowIndex = value;
            }
        }

        public bool EditingControlWantsInputKey(Keys key, bool dataGridViewWantsInputKey)
        {
            return (key & Keys.KeyCode) switch
            {
                Keys.Left or Keys.Up or Keys.Down or Keys.Right or Keys.Home or Keys.End or Keys.PageDown or Keys.PageUp => true,
                _ => !dataGridViewWantsInputKey,
            };
        }

        public void PrepareEditingControlForEdit(bool selectAll)
        {
        }

        public bool RepositionEditingControlOnValueChange
        {
            get
            {
                return false;
            }
        }

        public DataGridView EditingControlDataGridView
        {
            get
            {
                return dataGridView;
            }
            set
            {
                dataGridView = value;
            }
        }

        public bool EditingControlValueChanged
        {
            get
            {
                return valueChanged;
            }
            set
            {
                valueChanged = value;
            }
        }

        public Cursor EditingPanelCursor
        {
            get
            {
                return base.Cursor;
            }
        }

        protected override void OnTextChanged(EventArgs e)
        {
            if (!Initializing)
            {
                valueChanged = true;
                this.EditingControlDataGridView.NotifyCurrentCellDirty(true);
            }

            base.OnTextChanged(e);
        }
    }
}
