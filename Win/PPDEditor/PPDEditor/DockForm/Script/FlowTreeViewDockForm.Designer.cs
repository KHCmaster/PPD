namespace PPDEditor.DockForm.Script
{
    partial class FlowTreeViewDockForm
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
            this.flowTreeView1 = new FlowScriptControl.Controls.FlowTreeView();
            this.flowTreeView1Host = new System.Windows.Forms.Integration.ElementHost();
            this.SuspendLayout();
            // 
            // flowTreeView1
            // 
            this.flowTreeView1Host.Child = this.flowTreeView1;
            this.flowTreeView1Host.Dock = System.Windows.Forms.DockStyle.Fill;
            this.flowTreeView1Host.Location = new System.Drawing.Point(0, 0);
            this.flowTreeView1Host.Name = "flowTreeView1Host";
            this.flowTreeView1Host.Size = new System.Drawing.Size(292, 267);
            this.flowTreeView1Host.TabIndex = 0;
            // 
            // FlowTreeViewDockForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(292, 267);
            this.Controls.Add(this.flowTreeView1Host);
            this.Font = new System.Drawing.Font("MS UI Gothic", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(128)));
            this.HideOnClose = true;
            this.Name = "FlowTreeViewDockForm";
            this.Text = "FlowTreeViewDockForm";
            this.ResumeLayout(false);

        }

        #endregion

        private FlowScriptControl.Controls.FlowTreeView flowTreeView1;
        private System.Windows.Forms.Integration.ElementHost flowTreeView1Host;
    }
}