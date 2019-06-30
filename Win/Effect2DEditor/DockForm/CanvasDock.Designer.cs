namespace Effect2DEditor.DockForm
{
    partial class CanvasDock
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
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(CanvasDock));
            this.bezierControl1 = new BezierDrawControl.DXBezierControl();
            this.contextMenuStrip1 = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.選択したアンカーを削除ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.変形を開始するToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.変形を終了するToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.リセットするToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.contextMenuStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // bezierControl1
            // 
            this.bezierControl1.Controller.AllowMouseOperation = true;
            this.bezierControl1.Controller.BCPS = new BezierCaliculator.BezierControlPoint[0];
            this.bezierControl1.Controller.Center = new System.Drawing.Point(0, 0);
            this.bezierControl1.ContextMenuStrip = this.contextMenuStrip1;
            this.bezierControl1.Location = new System.Drawing.Point(0, 0);
            this.bezierControl1.Margin = new System.Windows.Forms.Padding(0);
            this.bezierControl1.Name = "bezierControl1";
            this.bezierControl1.Controller.RestrictAngleSplit = 0;
            this.bezierControl1.Size = new System.Drawing.Size(256, 256);
            this.bezierControl1.TabIndex = 1;
            this.bezierControl1.Controller.TransformRotation = 0F;
            this.bezierControl1.Controller.TransformScale = ((System.Drawing.PointF)(resources.GetObject("bezierControl1.TransformScale")));
            this.bezierControl1.Controller.TransformTranslation = ((System.Drawing.PointF)(resources.GetObject("bezierControl1.TransformTranslation")));
            // 
            // contextMenuStrip1
            // 
            this.contextMenuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.選択したアンカーを削除ToolStripMenuItem,
            this.変形を開始するToolStripMenuItem,
            this.変形を終了するToolStripMenuItem,
            this.リセットするToolStripMenuItem});
            this.contextMenuStrip1.Name = "contextMenuStrip1";
            this.contextMenuStrip1.Size = new System.Drawing.Size(209, 114);
            this.contextMenuStrip1.Opening += new System.ComponentModel.CancelEventHandler(this.contextMenuStrip1_Opening);
            // 
            // 選択したアンカーを削除ToolStripMenuItem
            // 
            this.選択したアンカーを削除ToolStripMenuItem.Name = "選択したアンカーを削除ToolStripMenuItem";
            this.選択したアンカーを削除ToolStripMenuItem.Size = new System.Drawing.Size(208, 22);
            this.選択したアンカーを削除ToolStripMenuItem.Text = "選択したアンカーを削除";
            this.選択したアンカーを削除ToolStripMenuItem.Click += new System.EventHandler(this.選択したアンカーを削除ToolStripMenuItem_Click);
            // 
            // 変形を開始するToolStripMenuItem
            // 
            this.変形を開始するToolStripMenuItem.Name = "変形を開始するToolStripMenuItem";
            this.変形を開始するToolStripMenuItem.Size = new System.Drawing.Size(208, 22);
            this.変形を開始するToolStripMenuItem.Text = "変形を開始する";
            this.変形を開始するToolStripMenuItem.Click += new System.EventHandler(this.変形を開始するToolStripMenuItem_Click);
            // 
            // 変形を終了するToolStripMenuItem
            // 
            this.変形を終了するToolStripMenuItem.Name = "変形を終了するToolStripMenuItem";
            this.変形を終了するToolStripMenuItem.Size = new System.Drawing.Size(208, 22);
            this.変形を終了するToolStripMenuItem.Text = "変形を終了する";
            this.変形を終了するToolStripMenuItem.Click += new System.EventHandler(this.変形を終了するToolStripMenuItem_Click);
            // 
            // リセットするToolStripMenuItem
            // 
            this.リセットするToolStripMenuItem.Name = "リセットするToolStripMenuItem";
            this.リセットするToolStripMenuItem.Size = new System.Drawing.Size(208, 22);
            this.リセットするToolStripMenuItem.Text = "リセットする";
            this.リセットするToolStripMenuItem.Click += new System.EventHandler(this.リセットするToolStripMenuItem_Click);
            // 
            // CanvasDock
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoScroll = true;
            this.ClientSize = new System.Drawing.Size(284, 262);
            this.Controls.Add(this.bezierControl1);
            this.Font = new System.Drawing.Font("MS UI Gothic", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(128)));
            this.HideOnClose = true;
            this.Name = "CanvasDock";
            this.Text = "CanvasForm";
            this.contextMenuStrip1.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private BezierDrawControl.DXBezierControl bezierControl1;
        private System.Windows.Forms.ContextMenuStrip contextMenuStrip1;
        private System.Windows.Forms.ToolStripMenuItem 選択したアンカーを削除ToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem 変形を開始するToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem 変形を終了するToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem リセットするToolStripMenuItem;
    }
}