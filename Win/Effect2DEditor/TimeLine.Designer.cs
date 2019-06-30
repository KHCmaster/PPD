namespace Effect2DEditor
{
    partial class TimeLine
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
            this.components = new System.ComponentModel.Container();
            this.hScrollBar1 = new System.Windows.Forms.HScrollBar();
            this.vScrollBar1 = new System.Windows.Forms.VScrollBar();
            this.contextMenuStrip1 = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.キーステートを挿入ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.ノーマルステートを挿入ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.ステートを削除ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.contextMenuStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // hScrollBar1
            // 
            this.hScrollBar1.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.hScrollBar1.Location = new System.Drawing.Point(0, 133);
            this.hScrollBar1.Name = "hScrollBar1";
            this.hScrollBar1.Size = new System.Drawing.Size(150, 17);
            this.hScrollBar1.TabIndex = 0;
            // 
            // vScrollBar1
            // 
            this.vScrollBar1.Dock = System.Windows.Forms.DockStyle.Right;
            this.vScrollBar1.Location = new System.Drawing.Point(133, 0);
            this.vScrollBar1.Name = "vScrollBar1";
            this.vScrollBar1.Size = new System.Drawing.Size(17, 133);
            this.vScrollBar1.TabIndex = 1;
            // 
            // contextMenuStrip1
            // 
            this.contextMenuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.キーステートを挿入ToolStripMenuItem,
            this.ノーマルステートを挿入ToolStripMenuItem,
            this.ステートを削除ToolStripMenuItem});
            this.contextMenuStrip1.Name = "contextMenuStrip1";
            this.contextMenuStrip1.Size = new System.Drawing.Size(177, 92);
            this.contextMenuStrip1.Opening += new System.ComponentModel.CancelEventHandler(this.contextMenuStrip1_Opening);
            // 
            // キーステートを挿入ToolStripMenuItem
            // 
            this.キーステートを挿入ToolStripMenuItem.Name = "キーステートを挿入ToolStripMenuItem";
            this.キーステートを挿入ToolStripMenuItem.Size = new System.Drawing.Size(176, 22);
            this.キーステートを挿入ToolStripMenuItem.Text = "キーステートを挿入";
            this.キーステートを挿入ToolStripMenuItem.Click += new System.EventHandler(this.キーステートを挿入ToolStripMenuItem_Click);
            // 
            // ノーマルステートを挿入ToolStripMenuItem
            // 
            this.ノーマルステートを挿入ToolStripMenuItem.Name = "ノーマルステートを挿入ToolStripMenuItem";
            this.ノーマルステートを挿入ToolStripMenuItem.Size = new System.Drawing.Size(176, 22);
            this.ノーマルステートを挿入ToolStripMenuItem.Text = "ノーマルステートを挿入";
            this.ノーマルステートを挿入ToolStripMenuItem.Click += new System.EventHandler(this.ノーマルステートを挿入ToolStripMenuItem_Click);
            // 
            // ステートを削除ToolStripMenuItem
            // 
            this.ステートを削除ToolStripMenuItem.Name = "ステートを削除ToolStripMenuItem";
            this.ステートを削除ToolStripMenuItem.Size = new System.Drawing.Size(176, 22);
            this.ステートを削除ToolStripMenuItem.Text = "ステートを削除";
            this.ステートを削除ToolStripMenuItem.Click += new System.EventHandler(this.ステートを削除ToolStripMenuItem_Click);
            // 
            // TimeLine
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ContextMenuStrip = this.contextMenuStrip1;
            this.Controls.Add(this.vScrollBar1);
            this.Controls.Add(this.hScrollBar1);
            this.Name = "TimeLine";
            this.MouseMove += new System.Windows.Forms.MouseEventHandler(this.TimeLine_MouseMove);
            this.MouseDown += new System.Windows.Forms.MouseEventHandler(this.TimeLine_MouseDown);
            this.MouseUp += new System.Windows.Forms.MouseEventHandler(this.TimeLine_MouseUp);
            this.contextMenuStrip1.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.HScrollBar hScrollBar1;
        private System.Windows.Forms.VScrollBar vScrollBar1;
        private System.Windows.Forms.ContextMenuStrip contextMenuStrip1;
        private System.Windows.Forms.ToolStripMenuItem キーステートを挿入ToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem ノーマルステートを挿入ToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem ステートを削除ToolStripMenuItem;
    }
}
