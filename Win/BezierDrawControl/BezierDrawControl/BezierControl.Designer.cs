namespace BezierDrawControl
{
    partial class BezierControl
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
            // BezierControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Name = "BezierControl";
            this.Size = new System.Drawing.Size(256, 256);
            this.Paint += new System.Windows.Forms.PaintEventHandler(this.BezierControl_Paint);
            this.MouseMove += new System.Windows.Forms.MouseEventHandler(this.BezierControl_MouseMove);
            this.MouseDown += new System.Windows.Forms.MouseEventHandler(this.BezierControl_MouseDown);
            this.MouseUp += new System.Windows.Forms.MouseEventHandler(this.BezierControl_MouseUp);
            this.SizeChanged += new System.EventHandler(this.BezierControl_SizeChanged);
            this.ResumeLayout(false);

        }

        #endregion
    }
}
