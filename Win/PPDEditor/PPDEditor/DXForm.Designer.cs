namespace testgame
{
    partial class DXForm
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
            this.timer1 = new System.Windows.Forms.Timer(this.components);
            this.contextMenuStrip1 = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.画面比を169にToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.contextMenuStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // contextMenuStrip1
            // 
            this.contextMenuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.画面比を169にToolStripMenuItem});
            this.contextMenuStrip1.Name = "contextMenuStrip1";
            this.contextMenuStrip1.Size = new System.Drawing.Size(158, 26);
            // 
            // 画面比を169にToolStripMenuItem
            // 
            this.画面比を169にToolStripMenuItem.Enabled = false;
            this.画面比を169にToolStripMenuItem.Name = "画面比を169にToolStripMenuItem";
            this.画面比を169にToolStripMenuItem.Size = new System.Drawing.Size(157, 22);
            this.画面比を169にToolStripMenuItem.Text = "画面比を16:9に";
            this.画面比を169にToolStripMenuItem.Click += new System.EventHandler(this.画面比を169にToolStripMenuItem_Click);
            // 
            // DXForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(792, 417);
            this.HideOnClose = true;
            this.Name = "DXForm";
            this.TabPageContextMenuStrip = this.contextMenuStrip1;
            this.TabText = "ゲームウィンドウ";
            this.Text = "ゲームウィンドウ";
            this.MouseUp += new System.Windows.Forms.MouseEventHandler(this.DXFrom_MouseUp);
            this.MouseDown += new System.Windows.Forms.MouseEventHandler(this.DXFrom_MouseDown);
            this.DockStateChanged += new System.EventHandler(this.DXFrom_DockStateChanged);
            this.PreviewKeyDown += new System.Windows.Forms.PreviewKeyDownEventHandler(this.DXForm_PreviewKeyDown);
            this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.DXForm_KeyDown);
            this.contextMenuStrip1.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Timer timer1;
        private System.Windows.Forms.ContextMenuStrip contextMenuStrip1;
        private System.Windows.Forms.ToolStripMenuItem 画面比を169にToolStripMenuItem;
    }
}