namespace PPDEditor
{
    partial class LayerManager
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

        #region Windows フォーム デザイナで生成されたコード

        /// <summary>
        /// デザイナ サポートに必要なメソッドです。このメソッドの内容を
        /// コード エディタで変更しないでください。
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.contextMenuStrip1 = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.新規レイヤーToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.contextMenuStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // contextMenuStrip1
            // 
            this.contextMenuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.新規レイヤーToolStripMenuItem});
            this.contextMenuStrip1.Name = "contextMenuStrip1";
            this.contextMenuStrip1.Size = new System.Drawing.Size(133, 26);
            // 
            // 新規レイヤーToolStripMenuItem
            // 
            this.新規レイヤーToolStripMenuItem.Name = "新規レイヤーToolStripMenuItem";
            this.新規レイヤーToolStripMenuItem.Size = new System.Drawing.Size(132, 22);
            this.新規レイヤーToolStripMenuItem.Text = "新規レイヤー";
            this.新規レイヤーToolStripMenuItem.Click += new System.EventHandler(this.新規レイヤーToolStripMenuItem_Click);
            // 
            // layermanager
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(238, 266);
            this.ContextMenuStrip = this.contextMenuStrip1;
            this.HideOnClose = true;
            this.Name = "layermanager";
            this.TabText = "レイヤーマネージャー";
            this.Text = "レイヤーマネージャー";
            this.contextMenuStrip1.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.ContextMenuStrip contextMenuStrip1;
        private System.Windows.Forms.ToolStripMenuItem 新規レイヤーToolStripMenuItem;
    }
}