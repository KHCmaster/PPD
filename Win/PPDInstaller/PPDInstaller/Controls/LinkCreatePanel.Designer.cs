namespace PPDInstaller.Controls
{
    partial class LinkCreatePanel
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
            this.registerStartMenuCheckBox = new System.Windows.Forms.CheckBox();
            this.SuspendLayout();
            // 
            // registerStartMenuCheckBox
            // 
            this.registerStartMenuCheckBox.AutoSize = true;
            this.registerStartMenuCheckBox.Checked = true;
            this.registerStartMenuCheckBox.CheckState = System.Windows.Forms.CheckState.Checked;
            this.registerStartMenuCheckBox.Location = new System.Drawing.Point(32, 129);
            this.registerStartMenuCheckBox.Name = "registerStartMenuCheckBox";
            this.registerStartMenuCheckBox.Size = new System.Drawing.Size(146, 16);
            this.registerStartMenuCheckBox.TabIndex = 0;
            this.registerStartMenuCheckBox.Text = "スタートメニューに登録する";
            this.registerStartMenuCheckBox.UseVisualStyleBackColor = true;
            // 
            // LinkCreatePanel
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.registerStartMenuCheckBox);
            this.Name = "LinkCreatePanel";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.CheckBox registerStartMenuCheckBox;
    }
}
