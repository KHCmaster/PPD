using System.Windows.Forms;

namespace FlowScriptControl.Controls
{
    class CustomDataGridView : DataGridView
    {
        protected override bool ProcessDataGridViewKey(KeyEventArgs e)
        {
            if ((e.KeyData == Keys.Left || e.KeyData == Keys.Right) &&
                this.EditingControl != null)
                if (this.EditingControl.GetType() == typeof(DataGridViewComboBoxEditingControl))
                {
                    var control = this.EditingControl as ComboBox;
                    if (control.DropDownStyle != ComboBoxStyle.DropDownList)
                    {
                        switch (e.KeyData)
                        {
                            case Keys.Left:
                                if (control.SelectionStart > 0)
                                {
                                    control.SelectionStart--;
                                    return true;
                                }
                                break;
                            case Keys.Right:
                                if (control.SelectionStart <
                                control.Text.Length)
                                {
                                    control.SelectionStart++;
                                    return true;
                                }
                                break;
                        }
                    }
                }
                else if (this.EditingControl.GetType() == typeof(DataGridViewTextBoxEditingControl))
                {
                    var textBox = this.EditingControl as TextBox;
                    switch (e.KeyData)
                    {
                        case Keys.Left:
                            if (textBox.SelectionStart > 0)
                            {
                                textBox.SelectionStart--;
                                return true;
                            }
                            break;
                        case Keys.Right:
                            if (textBox.SelectionStart < textBox.Text.Length)
                            {
                                textBox.SelectionStart++;
                                return true;
                            }
                            break;
                    }
                }
            return false;
        }
    }
}
