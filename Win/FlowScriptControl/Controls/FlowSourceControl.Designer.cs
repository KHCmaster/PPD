namespace FlowScriptControl.Controls
{
    partial class FlowSourceControl
    {
        /// <summary> 
        /// 必要なデザイナ変数です。
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary> 
        /// 使用中のリソースをすべてクリーンアップします。
        /// </summary>
        /// <param name="disposing">マネージ リソースが破棄される場合 true、破棄されない場合は false です。</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region コンポーネント デザイナで生成されたコード

        /// <summary> 
        /// デザイナ サポートに必要なメソッドです。このメソッドの内容を 
        /// コード エディタで変更しないでください。
        /// </summary>
        private void InitializeComponent()
        {
            this.flowSourceControlHeader1 = new FlowScriptControl.Controls.FlowSourceControlHeader();
            this.SuspendLayout();
            // 
            // flowSourceControlHeader1
            // 
            this.flowSourceControlHeader1.Dock = System.Windows.Forms.DockStyle.Top;
            this.flowSourceControlHeader1.HeaderText = "label1";
            this.flowSourceControlHeader1.Location = new System.Drawing.Point(0, 0);
            this.flowSourceControlHeader1.MaximumSize = new System.Drawing.Size(0, 20);
            this.flowSourceControlHeader1.MinimumSize = new System.Drawing.Size(80, 20);
            this.flowSourceControlHeader1.Name = "flowSourceControlHeader1";
            this.flowSourceControlHeader1.Size = new System.Drawing.Size(200, 20);
            this.flowSourceControlHeader1.TabIndex = 0;
            // 
            // FlowSourceControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.flowSourceControlHeader1);
            this.MinimumSize = new System.Drawing.Size(200, 0);
            this.Name = "FlowSourceControl";
            this.Size = new System.Drawing.Size(200, 78);
            this.ResumeLayout(false);

        }

        #endregion

        private FlowSourceControlHeader flowSourceControlHeader1;

    }
}
