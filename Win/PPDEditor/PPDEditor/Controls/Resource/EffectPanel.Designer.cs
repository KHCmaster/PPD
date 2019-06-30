namespace PPDEditor.Controls.Resource
{
    partial class EffectPanel
    {
        /// <summary> 
        /// 必要なデザイナー変数です。
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

        #region コンポーネント デザイナーで生成されたコード

        /// <summary> 
        /// デザイナー サポートに必要なメソッドです。このメソッドの内容を 
        /// コード エディターで変更しないでください。
        /// </summary>
        private void InitializeComponent()
        {
            this.toolStrip1 = new System.Windows.Forms.ToolStrip();
            this.toolStripSplitButton1 = new PPDEditor.Controls.ExToolStripSplitButton();
            this.playToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.reversePlayToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.loopPlayToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.reverseLoopPlayToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripButton1 = new System.Windows.Forms.ToolStripButton();
            this.toolStripComboBox1 = new System.Windows.Forms.ToolStripComboBox();
            this.toolStripComboBox2 = new System.Windows.Forms.ToolStripComboBox();
            this.panel1 = new System.Windows.Forms.Panel();
            this.canvasPanel = new PPDEditor.Controls.CustomUserControl();
            this.colorDialog1 = new System.Windows.Forms.ColorDialog();
            this.toolStrip1.SuspendLayout();
            this.panel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // toolStrip1
            // 
            this.toolStrip1.Dock = System.Windows.Forms.DockStyle.None;
            this.toolStrip1.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
            this.toolStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripSplitButton1,
            this.toolStripButton1,
            this.toolStripComboBox1,
            this.toolStripComboBox2});
            this.toolStrip1.Location = new System.Drawing.Point(9, 100);
            this.toolStrip1.Name = "toolStrip1";
            this.toolStrip1.RenderMode = System.Windows.Forms.ToolStripRenderMode.Professional;
            this.toolStrip1.Size = new System.Drawing.Size(293, 26);
            this.toolStrip1.TabIndex = 0;
            this.toolStrip1.Text = "toolStrip1";
            // 
            // toolStripSplitButton1
            // 
            this.toolStripSplitButton1.Checked = false;
            this.toolStripSplitButton1.CheckOnClick = true;
            this.toolStripSplitButton1.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.toolStripSplitButton1.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.playToolStripMenuItem,
            this.reversePlayToolStripMenuItem,
            this.loopPlayToolStripMenuItem,
            this.reverseLoopPlayToolStripMenuItem});
            this.toolStripSplitButton1.Image = global::PPDEditor.Properties.Resources.playonce;
            this.toolStripSplitButton1.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripSplitButton1.Name = "toolStripSplitButton1";
            this.toolStripSplitButton1.Size = new System.Drawing.Size(32, 23);
            // 
            // playToolStripMenuItem
            // 
            this.playToolStripMenuItem.Checked = true;
            this.playToolStripMenuItem.CheckState = System.Windows.Forms.CheckState.Checked;
            this.playToolStripMenuItem.Image = global::PPDEditor.Properties.Resources.playonce;
            this.playToolStripMenuItem.Name = "playToolStripMenuItem";
            this.playToolStripMenuItem.Size = new System.Drawing.Size(184, 22);
            this.playToolStripMenuItem.Text = "1回再生";
            this.playToolStripMenuItem.Click += new System.EventHandler(this.playToolStripMenuItem_Click);
            // 
            // reversePlayToolStripMenuItem
            // 
            this.reversePlayToolStripMenuItem.Image = global::PPDEditor.Properties.Resources.playreverse;
            this.reversePlayToolStripMenuItem.Name = "reversePlayToolStripMenuItem";
            this.reversePlayToolStripMenuItem.Size = new System.Drawing.Size(184, 22);
            this.reversePlayToolStripMenuItem.Text = "リバース1回再生";
            this.reversePlayToolStripMenuItem.Click += new System.EventHandler(this.reversePlayToolStripMenuItem_Click);
            // 
            // loopPlayToolStripMenuItem
            // 
            this.loopPlayToolStripMenuItem.Image = global::PPDEditor.Properties.Resources.playloop;
            this.loopPlayToolStripMenuItem.Name = "loopPlayToolStripMenuItem";
            this.loopPlayToolStripMenuItem.Size = new System.Drawing.Size(184, 22);
            this.loopPlayToolStripMenuItem.Text = "ループ再生";
            this.loopPlayToolStripMenuItem.Click += new System.EventHandler(this.loopPlayToolStripMenuItem_Click);
            // 
            // reverseLoopPlayToolStripMenuItem
            // 
            this.reverseLoopPlayToolStripMenuItem.Image = global::PPDEditor.Properties.Resources.playreverseloop;
            this.reverseLoopPlayToolStripMenuItem.Name = "reverseLoopPlayToolStripMenuItem";
            this.reverseLoopPlayToolStripMenuItem.Size = new System.Drawing.Size(184, 22);
            this.reverseLoopPlayToolStripMenuItem.Text = "リバースループ再生";
            this.reverseLoopPlayToolStripMenuItem.Click += new System.EventHandler(this.reverseLoopPlayToolStripMenuItem_Click);
            // 
            // toolStripButton1
            // 
            this.toolStripButton1.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.toolStripButton1.Image = global::PPDEditor.Properties.Resources.stop;
            this.toolStripButton1.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripButton1.Name = "toolStripButton1";
            this.toolStripButton1.Size = new System.Drawing.Size(23, 27);
            this.toolStripButton1.Text = "toolStripButton1";
            this.toolStripButton1.Click += new System.EventHandler(this.toolStripButton1_Click);
            // 
            // toolStripComboBox1
            // 
            this.toolStripComboBox1.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.toolStripComboBox1.Items.AddRange(new object[] {
            "256 x 256",
            "384 x 384",
            "512 x 512",
            "768 x 768",
            "1024 x 1024"});
            this.toolStripComboBox1.Name = "toolStripComboBox1";
            this.toolStripComboBox1.Size = new System.Drawing.Size(100, 30);
            this.toolStripComboBox1.SelectedIndexChanged += new System.EventHandler(this.toolStripComboBox1_SelectedIndexChanged);
            // 
            // toolStripComboBox2
            // 
            this.toolStripComboBox2.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.toolStripComboBox2.Items.AddRange(new object[] {
            "デフォルト",
            "黒",
            "白",
            "カスタム"});
            this.toolStripComboBox2.Name = "toolStripComboBox2";
            this.toolStripComboBox2.Size = new System.Drawing.Size(100, 30);
            this.toolStripComboBox2.SelectedIndexChanged += new System.EventHandler(this.toolStripComboBox2_SelectedIndexChanged);
            // 
            // panel1
            // 
            this.panel1.AutoScroll = true;
            this.panel1.Controls.Add(this.canvasPanel);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel1.Location = new System.Drawing.Point(0, 0);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(343, 272);
            this.panel1.TabIndex = 1;
            // 
            // canvasPanel
            // 
            this.canvasPanel.Location = new System.Drawing.Point(0, 0);
            this.canvasPanel.Name = "canvasPanel";
            this.canvasPanel.Size = new System.Drawing.Size(256, 256);
            this.canvasPanel.TabIndex = 0;
            // 
            // EffectPanel
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.White;
            this.Controls.Add(this.toolStrip1);
            this.Controls.Add(this.panel1);
            this.Name = "EffectPanel";
            this.Size = new System.Drawing.Size(343, 272);
            this.toolStrip1.ResumeLayout(false);
            this.toolStrip1.PerformLayout();
            this.panel1.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ToolStrip toolStrip1;
        private System.Windows.Forms.Panel panel1;
        private CustomUserControl canvasPanel;
        private ExToolStripSplitButton toolStripSplitButton1;
        private System.Windows.Forms.ToolStripMenuItem playToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem reversePlayToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem loopPlayToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem reverseLoopPlayToolStripMenuItem;
        private System.Windows.Forms.ToolStripButton toolStripButton1;
        private System.Windows.Forms.ToolStripComboBox toolStripComboBox1;
        private System.Windows.Forms.ToolStripComboBox toolStripComboBox2;
        private System.Windows.Forms.ColorDialog colorDialog1;
    }
}
