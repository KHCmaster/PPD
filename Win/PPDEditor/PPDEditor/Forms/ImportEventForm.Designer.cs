namespace PPDEditor.Forms
{
    partial class ImportEventForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.readOnlyEventDataGrid = new System.Windows.Forms.DataGridView();
            this.dataGridViewTextBoxColumn1 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.dataGridViewTextBoxColumn2 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.deleteButton = new System.Windows.Forms.Button();
            this.importButton = new System.Windows.Forms.Button();
            this.currentEventDataGrid = new System.Windows.Forms.DataGridView();
            this.Column1 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Column2 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.cancelButton = new System.Windows.Forms.Button();
            this.okButton = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.readOnlyEventDataGrid)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.currentEventDataGrid)).BeginInit();
            this.SuspendLayout();
            // 
            // splitContainer1
            // 
            this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer1.FixedPanel = System.Windows.Forms.FixedPanel.Panel2;
            this.splitContainer1.IsSplitterFixed = true;
            this.splitContainer1.Location = new System.Drawing.Point(0, 0);
            this.splitContainer1.Name = "splitContainer1";
            this.splitContainer1.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // splitContainer1.Panel1
            // 
            this.splitContainer1.Panel1.Controls.Add(this.readOnlyEventDataGrid);
            this.splitContainer1.Panel1.Controls.Add(this.deleteButton);
            this.splitContainer1.Panel1.Controls.Add(this.importButton);
            this.splitContainer1.Panel1.Controls.Add(this.currentEventDataGrid);
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.Controls.Add(this.cancelButton);
            this.splitContainer1.Panel2.Controls.Add(this.okButton);
            this.splitContainer1.Size = new System.Drawing.Size(580, 394);
            this.splitContainer1.SplitterDistance = 354;
            this.splitContainer1.SplitterWidth = 1;
            this.splitContainer1.TabIndex = 0;
            // 
            // readOnlyEventDataGrid
            // 
            this.readOnlyEventDataGrid.AllowUserToAddRows = false;
            this.readOnlyEventDataGrid.AllowUserToDeleteRows = false;
            this.readOnlyEventDataGrid.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
            this.readOnlyEventDataGrid.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.readOnlyEventDataGrid.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.dataGridViewTextBoxColumn1,
            this.dataGridViewTextBoxColumn2});
            this.readOnlyEventDataGrid.Dock = System.Windows.Forms.DockStyle.Right;
            this.readOnlyEventDataGrid.Location = new System.Drawing.Point(351, 0);
            this.readOnlyEventDataGrid.MultiSelect = false;
            this.readOnlyEventDataGrid.Name = "readOnlyEventDataGrid";
            this.readOnlyEventDataGrid.ReadOnly = true;
            this.readOnlyEventDataGrid.RowHeadersVisible = false;
            this.readOnlyEventDataGrid.RowTemplate.Height = 21;
            this.readOnlyEventDataGrid.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.readOnlyEventDataGrid.Size = new System.Drawing.Size(229, 354);
            this.readOnlyEventDataGrid.TabIndex = 4;
            this.readOnlyEventDataGrid.CellDoubleClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.readOnlyEventDataGrid_CellDoubleClick);
            // 
            // dataGridViewTextBoxColumn1
            // 
            this.dataGridViewTextBoxColumn1.FillWeight = 52.58883F;
            this.dataGridViewTextBoxColumn1.HeaderText = "時間(秒)";
            this.dataGridViewTextBoxColumn1.Name = "dataGridViewTextBoxColumn1";
            this.dataGridViewTextBoxColumn1.ReadOnly = true;
            // 
            // dataGridViewTextBoxColumn2
            // 
            this.dataGridViewTextBoxColumn2.FillWeight = 87.41116F;
            this.dataGridViewTextBoxColumn2.HeaderText = "詳細";
            this.dataGridViewTextBoxColumn2.Name = "dataGridViewTextBoxColumn2";
            this.dataGridViewTextBoxColumn2.ReadOnly = true;
            // 
            // deleteButton
            // 
            this.deleteButton.Location = new System.Drawing.Point(244, 149);
            this.deleteButton.Name = "deleteButton";
            this.deleteButton.Size = new System.Drawing.Size(75, 23);
            this.deleteButton.TabIndex = 3;
            this.deleteButton.Text = "削除";
            this.deleteButton.UseVisualStyleBackColor = true;
            this.deleteButton.Click += new System.EventHandler(this.deleteButton_Click);
            // 
            // importButton
            // 
            this.importButton.Location = new System.Drawing.Point(244, 120);
            this.importButton.Name = "importButton";
            this.importButton.Size = new System.Drawing.Size(75, 23);
            this.importButton.TabIndex = 2;
            this.importButton.Text = "<=インポート";
            this.importButton.UseVisualStyleBackColor = true;
            this.importButton.Click += new System.EventHandler(this.importButton_Click);
            // 
            // currentEventDataGrid
            // 
            this.currentEventDataGrid.AllowUserToAddRows = false;
            this.currentEventDataGrid.AllowUserToDeleteRows = false;
            this.currentEventDataGrid.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
            this.currentEventDataGrid.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.currentEventDataGrid.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.Column1,
            this.Column2});
            this.currentEventDataGrid.Dock = System.Windows.Forms.DockStyle.Left;
            this.currentEventDataGrid.Location = new System.Drawing.Point(0, 0);
            this.currentEventDataGrid.MultiSelect = false;
            this.currentEventDataGrid.Name = "currentEventDataGrid";
            this.currentEventDataGrid.ReadOnly = true;
            this.currentEventDataGrid.RowHeadersVisible = false;
            this.currentEventDataGrid.RowTemplate.Height = 21;
            this.currentEventDataGrid.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.currentEventDataGrid.Size = new System.Drawing.Size(229, 354);
            this.currentEventDataGrid.TabIndex = 0;
            this.currentEventDataGrid.CellDoubleClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.currentEventDataGrid_CellDoubleClick);
            // 
            // Column1
            // 
            this.Column1.FillWeight = 52.58883F;
            this.Column1.HeaderText = "時間(秒)";
            this.Column1.Name = "Column1";
            this.Column1.ReadOnly = true;
            // 
            // Column2
            // 
            this.Column2.FillWeight = 87.41116F;
            this.Column2.HeaderText = "詳細";
            this.Column2.Name = "Column2";
            this.Column2.ReadOnly = true;
            // 
            // cancelButton
            // 
            this.cancelButton.Location = new System.Drawing.Point(294, 10);
            this.cancelButton.Name = "cancelButton";
            this.cancelButton.Size = new System.Drawing.Size(75, 23);
            this.cancelButton.TabIndex = 1;
            this.cancelButton.Text = "Cancel";
            this.cancelButton.UseVisualStyleBackColor = true;
            this.cancelButton.Click += new System.EventHandler(this.cancelButton_Click);
            // 
            // okButton
            // 
            this.okButton.Location = new System.Drawing.Point(199, 10);
            this.okButton.Name = "okButton";
            this.okButton.Size = new System.Drawing.Size(75, 23);
            this.okButton.TabIndex = 0;
            this.okButton.Text = "OK";
            this.okButton.UseVisualStyleBackColor = true;
            this.okButton.Click += new System.EventHandler(this.okButton_Click);
            // 
            // ImportEventForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(580, 394);
            this.Controls.Add(this.splitContainer1);
            this.Name = "ImportEventForm";
            this.Text = "ImportEventForm";
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
            this.splitContainer1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.readOnlyEventDataGrid)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.currentEventDataGrid)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.SplitContainer splitContainer1;
        private System.Windows.Forms.Button deleteButton;
        private System.Windows.Forms.Button importButton;
        private System.Windows.Forms.DataGridView currentEventDataGrid;
        private System.Windows.Forms.Button cancelButton;
        private System.Windows.Forms.Button okButton;
        private System.Windows.Forms.DataGridView readOnlyEventDataGrid;
        private System.Windows.Forms.DataGridViewTextBoxColumn dataGridViewTextBoxColumn1;
        private System.Windows.Forms.DataGridViewTextBoxColumn dataGridViewTextBoxColumn2;
        private System.Windows.Forms.DataGridViewTextBoxColumn Column1;
        private System.Windows.Forms.DataGridViewTextBoxColumn Column2;
    }
}