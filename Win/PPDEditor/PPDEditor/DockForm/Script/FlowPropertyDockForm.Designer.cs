namespace PPDEditor.DockForm.Script
{
    partial class FlowPropertyDockForm
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
            this.flowPropertyPanel1 = new PPDEditor.Controls.CustomFlowPropertyPanel();
            this.flowPropertyPanel1Host = new System.Windows.Forms.Integration.ElementHost();
            this.SuspendLayout();
            // 
            // flowPropertyPanel1
            // 
            this.flowPropertyPanel1Host.Child = this.flowPropertyPanel1;
            this.flowPropertyPanel1Host.Dock = System.Windows.Forms.DockStyle.Fill;
            this.flowPropertyPanel1Host.Location = new System.Drawing.Point(0, 0);
            this.flowPropertyPanel1Host.Name = "flowPropertyPanel1Host";
            this.flowPropertyPanel1Host.Size = new System.Drawing.Size(292, 267);
            this.flowPropertyPanel1Host.TabIndex = 0;
            // 
            // FlowPropertyDockForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(292, 267);
            this.Controls.Add(this.flowPropertyPanel1Host);
            this.Font = new System.Drawing.Font("MS UI Gothic", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(128)));
            this.HideOnClose = true;
            this.Name = "FlowPropertyDockForm";
            this.Text = "FlowPropertyDockForm";
            this.ResumeLayout(false);

        }

        #endregion

        private PPDEditor.Controls.CustomFlowPropertyPanel flowPropertyPanel1;
        private System.Windows.Forms.Integration.ElementHost flowPropertyPanel1Host;
    }
}