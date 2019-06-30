namespace BezierDrawControl
{
    partial class DXBezierControl
    {
        /// <summary> 
        /// 必要なデザイナー変数です。
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary> 
        /// 使用中のリソースをすべてクリーンアップします。
        /// </summary>
        /// <param name="disposing">マネージ リソースを破棄する場合は true を指定し、その他の場合は false を指定します。</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region コンポーネント デザイナーで生成されたコード

        /// <summary> 
        /// デザイナー サポートに必要なメソッドです。このメソッドの内容を 
        /// コード エディターで変更しないでください。
        /// </summary>
        private void InitializeComponent()
        {
            this.SuspendLayout();
            // 
            // DXBezierControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Name = "DXBezierControl";
            this.SizeChanged += new System.EventHandler(this.DXBezierControl_SizeChanged);
            this.MouseDown += new System.Windows.Forms.MouseEventHandler(this.DXBezierControl_MouseDown);
            this.MouseMove += new System.Windows.Forms.MouseEventHandler(this.DXBezierControl_MouseMove);
            this.MouseUp += new System.Windows.Forms.MouseEventHandler(this.DXBezierControl_MouseUp);
            this.ResumeLayout(false);

        }

        #endregion
    }
}
