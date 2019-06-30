namespace PPDEditor.Controls
{
    partial class Seekex
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
            this.SuspendLayout();
            // 
            // Seekex
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.SystemColors.ControlLightLight;
            this.Cursor = System.Windows.Forms.Cursors.SizeWE;
            this.Name = "Seekex";
            this.Size = new System.Drawing.Size(95, 286);
            this.MouseMove += new System.Windows.Forms.MouseEventHandler(this.Seekex_MouseMove);
            this.MouseDown += new System.Windows.Forms.MouseEventHandler(this.Seekex_MouseDown);
            this.MouseUp += new System.Windows.Forms.MouseEventHandler(this.Seekex_MouseUp);
            this.ResumeLayout(false);

        }

        #endregion

    }
}
