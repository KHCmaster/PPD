using FlowScriptEngine;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Windows.Forms;

namespace FlowScriptControl.Controls
{
    public partial class FlowPropertyPanel : UserControl
    {
        private FlowDrawPanel currentFlowDrawPanel;
        private string lastEditValue;

        public FlowPropertyPanel()
        {
            InitializeComponent();
            dataGridView1.CurrentCellDirtyStateChanged += dataGridView1_CurrentCellDirtyStateChanged;
        }

        void dataGridView1_CurrentCellDirtyStateChanged(object sender, EventArgs e)
        {
            if (dataGridView1.IsCurrentCellDirty)
            {
                dataGridView1.CommitEdit(DataGridViewDataErrorContexts.Commit);
            }
        }

        public void MakeEmpty()
        {
            ChangeFlowSource(null, null, null);
        }

        public void ChangeFlowSource(FlowDrawPanel flowDrawPanel, FlowSourceObjectBase source, Dictionary<string, string> data)
        {
            this.currentFlowDrawPanel = flowDrawPanel;
            try
            {
                while (dataGridView1.RowCount > 0)
                {
                    if (dataGridView1[1, dataGridView1.RowCount - 1] is ExDataGridViewComboBoxCell exCell)
                    {
                        exCell.ResetEvent();
                    }
                    dataGridView1.RowCount--;
                }
                if (flowDrawPanel != null && source != null && data != null)
                {
                    foreach (CustomMemberInfo<PropertyInfo> propertyInfo in source.InProperties)
                    {
                        var row = new DataGridViewRow();
                        var cell = new DataGridViewTextBoxCell
                        {
                            Value = propertyInfo.MemberInfo.Name,
                            ToolTipText = propertyInfo.ToolTipText
                        };
                        row.Cells.Add(cell);
                        var formatter = TypeFormatterManager.GetFormatter(propertyInfo.MemberInfo.PropertyType);
                        DataGridViewCell cell2 = null;
                        if (formatter != null)
                        {
                            if (formatter.AllowedPropertyString != null && formatter.AllowedPropertyString.Length > 0)
                            {
                                var exCell = new ExDataGridViewComboBoxCell();
                                cell2 = exCell;
                                exCell.Items.AddRange(formatter.AllowedPropertyString);
                                exCell.Value = formatter.AllowedPropertyString[0];
                                row.Cells.Add(cell2);
                                this.dataGridView1.Rows.Add(row);
                                exCell.SetEvent();
                            }
                            else
                            {
                                if (CanValueButtonClick(source.Name, propertyInfo.MemberInfo.Name, propertyInfo.MemberInfo.PropertyType))
                                {
                                    var buttonCell = new ButtonCell(source.Name, propertyInfo.MemberInfo.Name, propertyInfo.MemberInfo.PropertyType);
                                    buttonCell.ButtonClick += buttonCell_ButtonClick;
                                    cell2 = buttonCell;
                                }
                                else
                                {
                                    cell2 = new DataGridViewTextBoxCell();
                                }
                                row.Cells.Add(cell2);
                                this.dataGridView1.Rows.Add(row);
                            }
                            if (data.ContainsKey(propertyInfo.MemberInfo.Name))
                            {
                                cell2.Value = data[propertyInfo.MemberInfo.Name];
                            }
                            cell2.ReadOnly = false;
                            cell2.Tag = formatter;
                        }
                        else
                        {
                            cell2 = new DataGridViewTextBoxCell();
                            row.Cells.Add(cell2);
                            cell2.ReadOnly = true;
                            this.dataGridView1.Rows.Add(row);
                        }
                    }
                }
            }
            catch (Exception)
            {
            }
        }

        void buttonCell_ButtonClick(object sender, EventArgs e)
        {
            var cell = sender as ButtonCell;
            var val = ValueButtonClicked(cell.SourceName, cell.PropertyName, cell.PropertyType);
            if (val != null)
            {
                cell.Ctrl.Text = val;
            }
        }

        private void dataGridView1_CellEndEdit(object sender, DataGridViewCellEventArgs e)
        {
            if (currentFlowDrawPanel != null)
            {
                var formatter = dataGridView1[1, e.RowIndex].Tag as TypeFormatterBase;
                string targetText = dataGridView1[1, e.RowIndex].Value == null ? "" : dataGridView1[1, e.RowIndex].Value.ToString();
                if (String.IsNullOrEmpty(targetText))
                {
                    currentFlowDrawPanel.ChangeCurrentSourceProperty(dataGridView1[0, e.RowIndex].Value.ToString(), null);
                }
                else
                {
                    if (formatter != null && formatter.Format(dataGridView1[1, e.RowIndex].Value == null ? "" : dataGridView1[1, e.RowIndex].Value.ToString(), out object temp))
                    {
                        currentFlowDrawPanel.ChangeCurrentSourceProperty(dataGridView1[0, e.RowIndex].Value.ToString(),
                            dataGridView1[1, e.RowIndex].Value?.ToString());
                    }
                    else
                    {
                        MessageBox.Show(String.Format("{0}\nex)\n{1}", currentFlowDrawPanel.LanguageProvider.FormatError, formatter.CorrentFormat));
                        dataGridView1[1, e.RowIndex].Value = lastEditValue;
                    }
                }
            }
        }

        private void dataGridView1_CellBeginEdit(object sender, DataGridViewCellCancelEventArgs e)
        {
            lastEditValue = dataGridView1[1, e.RowIndex].Value == null ? null : dataGridView1[e.ColumnIndex, e.RowIndex].Value.ToString();
        }

        private void dataGridView1_CellMouseClick(object sender, DataGridViewCellMouseEventArgs e)
        {
            if (e.ColumnIndex == 1 && !dataGridView1[e.ColumnIndex, e.RowIndex].IsInEditMode)
            {
                dataGridView1.BeginEdit(false);
            }
        }

        protected virtual bool CanValueButtonClick(string sourceName, string propertyName, Type propertyType)
        {
            return false;
        }

        protected virtual string ValueButtonClicked(string sourceName, string propertyName, Type propertyType)
        {
            return null;
        }
    }
}
