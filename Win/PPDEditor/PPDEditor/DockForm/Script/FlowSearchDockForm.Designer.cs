namespace PPDEditor.DockForm.Script
{
    partial class FlowSearchDockForm
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
            this.flowSearchPanel1 = new FlowScriptControl.Controls.FlowSearchPanel();
            this.SuspendLayout();
            // 
            // flowSearchPanel1
            // 
            this.flowSearchPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.flowSearchPanel1.Location = new System.Drawing.Point(0, 0);
            this.flowSearchPanel1.Name = "flowSearchPanel1";
            this.flowSearchPanel1.Size = new System.Drawing.Size(284, 262);
            this.flowSearchPanel1.TabIndex = 0;
            // 
            // FlowSearchDockForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(284, 262);
            this.Controls.Add(this.flowSearchPanel1);
            this.Font = new System.Drawing.Font("MS UI Gothic", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(128)));
            this.HideOnClose = true;
            this.Name = "FlowSearchDockForm";
            this.Text = "検索";
            this.ResumeLayout(false);

        }

        #endregion

        private FlowScriptControl.Controls.FlowSearchPanel flowSearchPanel1;
    }
}