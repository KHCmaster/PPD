namespace PPDEditor
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
            this.標準解像度にするToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.contextMenuStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // contextMenuStrip1
            // 
            this.contextMenuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.画面比を169にToolStripMenuItem,
            this.標準解像度にするToolStripMenuItem});
            this.contextMenuStrip1.Name = "contextMenuStrip1";
            this.contextMenuStrip1.Size = new System.Drawing.Size(165, 70);
            // 
            // 画面比を169にToolStripMenuItem
            // 
            this.画面比を169にToolStripMenuItem.Enabled = false;
            this.画面比を169にToolStripMenuItem.Name = "画面比を169にToolStripMenuItem";
            this.画面比を169にToolStripMenuItem.Size = new System.Drawing.Size(164, 22);
            this.画面比を169にToolStripMenuItem.Text = "画面比を16:9に";
            this.画面比を169にToolStripMenuItem.Click += new System.EventHandler(this.画面比を169にToolStripMenuItem_Click);
            // 
            // 標準解像度にするToolStripMenuItem
            // 
            this.標準解像度にするToolStripMenuItem.Enabled = false;
            this.標準解像度にするToolStripMenuItem.Name = "標準解像度にするToolStripMenuItem";
            this.標準解像度にするToolStripMenuItem.Size = new System.Drawing.Size(164, 22);
            this.標準解像度にするToolStripMenuItem.Text = "標準解像度にする";
            this.標準解像度にするToolStripMenuItem.Click += new System.EventHandler(this.標準解像度にするToolStripMenuItem_Click);
            // 
            // DXForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(792, 417);
            this.Font = new System.Drawing.Font("MS UI Gothic", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(128)));
            this.HideOnClose = true;
            this.Name = "DXForm";
            this.TabPageContextMenuStrip = this.contextMenuStrip1;
            this.TabText = "ゲームウィンドウ";
            this.Text = "ゲームウィンドウ";
            this.MouseUp += new System.Windows.Forms.MouseEventHandler(this.DXForm_MouseUp);
            this.MouseDown += new System.Windows.Forms.MouseEventHandler(this.DXForm_MouseDown);
            this.MouseLeave += new System.EventHandler(this.DXForm_MouseLeave);
            this.DockStateChanged += new System.EventHandler(this.DXForm_DockStateChanged);
            this.MouseMove += new System.Windows.Forms.MouseEventHandler(this.DXForm_MouseMove);
            this.PreviewKeyDown += new System.Windows.Forms.PreviewKeyDownEventHandler(this.DXForm_PreviewKeyDown);
            this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.DXForm_KeyDown);
            this.contextMenuStrip1.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Timer timer1;
        private System.Windows.Forms.ContextMenuStrip contextMenuStrip1;
        private System.Windows.Forms.ToolStripMenuItem 画面比を169にToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem 標準解像度にするToolStripMenuItem;
    }
}