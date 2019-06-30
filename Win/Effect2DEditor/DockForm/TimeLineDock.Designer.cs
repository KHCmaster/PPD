namespace Effect2DEditor.DockForm
{
    partial class TimeLineDock
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
            this.splitContainer3 = new System.Windows.Forms.SplitContainer();
            this.timeLineItem1 = new Effect2DEditor.TimeLineItem();
            this.timeLine1 = new Effect2DEditor.TimeLine();
            this.toolStrip1 = new System.Windows.Forms.ToolStrip();
            this.toolStripButton1 = new System.Windows.Forms.ToolStripButton();
            this.toolStripButton2 = new System.Windows.Forms.ToolStripButton();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.exToolStripSplitButton1 = new Effect2DEditor.ExToolStripSplitButton();
            this.回再生ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.リバース1回再生ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.ループ再生ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.リバースループ再生ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripButton5 = new System.Windows.Forms.ToolStripButton();
            this.toolStripTextBox1 = new System.Windows.Forms.ToolStripTextBox();
            this.toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
            this.toolStripButton3 = new System.Windows.Forms.ToolStripButton();
            this.toolStripButton4 = new System.Windows.Forms.ToolStripButton();
            this.toolStripButton6 = new System.Windows.Forms.ToolStripButton();
            this.toolStripContainer1 = new System.Windows.Forms.ToolStripContainer();
            this.openFileDialog1 = new System.Windows.Forms.OpenFileDialog();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer3)).BeginInit();
            this.splitContainer3.Panel1.SuspendLayout();
            this.splitContainer3.Panel2.SuspendLayout();
            this.splitContainer3.SuspendLayout();
            this.toolStrip1.SuspendLayout();
            this.toolStripContainer1.ContentPanel.SuspendLayout();
            this.toolStripContainer1.TopToolStripPanel.SuspendLayout();
            this.toolStripContainer1.SuspendLayout();
            this.SuspendLayout();
            // 
            // splitContainer3
            // 
            this.splitContainer3.AllowDrop = true;
            this.splitContainer3.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer3.FixedPanel = System.Windows.Forms.FixedPanel.Panel1;
            this.splitContainer3.IsSplitterFixed = true;
            this.splitContainer3.Location = new System.Drawing.Point(0, 0);
            this.splitContainer3.Name = "splitContainer3";
            // 
            // splitContainer3.Panel1
            // 
            this.splitContainer3.Panel1.Controls.Add(this.timeLineItem1);
            // 
            // splitContainer3.Panel2
            // 
            this.splitContainer3.Panel2.Controls.Add(this.timeLine1);
            this.splitContainer3.Size = new System.Drawing.Size(1239, 522);
            this.splitContainer3.SplitterDistance = 200;
            this.splitContainer3.SplitterWidth = 1;
            this.splitContainer3.TabIndex = 1;
            // 
            // timeLineItem1
            // 
            this.timeLineItem1.BackColor = System.Drawing.SystemColors.Control;
            this.timeLineItem1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.timeLineItem1.EffectManager = null;
            this.timeLineItem1.Location = new System.Drawing.Point(0, 0);
            this.timeLineItem1.MainForm = null;
            this.timeLineItem1.Name = "timeLineItem1";
            this.timeLineItem1.SelectedManager = null;
            this.timeLineItem1.Size = new System.Drawing.Size(200, 522);
            this.timeLineItem1.TabIndex = 0;
            this.timeLineItem1.TimeLine = null;
            // 
            // timeLine1
            // 
            this.timeLine1.CurrentFrame = 0;
            this.timeLine1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.timeLine1.EffectManager = null;
            this.timeLine1.Location = new System.Drawing.Point(0, 0);
            this.timeLine1.MainForm = null;
            this.timeLine1.Name = "timeLine1";
            this.timeLine1.SelectedManager = null;
            this.timeLine1.Size = new System.Drawing.Size(1038, 522);
            this.timeLine1.TabIndex = 0;
            // 
            // toolStrip1
            // 
            this.toolStrip1.Dock = System.Windows.Forms.DockStyle.None;
            this.toolStrip1.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
            this.toolStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripButton1,
            this.toolStripButton2,
            this.toolStripSeparator1,
            this.exToolStripSplitButton1,
            this.toolStripButton5,
            this.toolStripTextBox1,
            this.toolStripSeparator2,
            this.toolStripButton3,
            this.toolStripButton4,
            this.toolStripButton6});
            this.toolStrip1.Location = new System.Drawing.Point(3, 0);
            this.toolStrip1.Name = "toolStrip1";
            this.toolStrip1.Size = new System.Drawing.Size(287, 25);
            this.toolStrip1.TabIndex = 2;
            this.toolStrip1.Text = "toolStrip1";
            // 
            // toolStripButton1
            // 
            this.toolStripButton1.Image = global::Effect2DEditor.Properties.Resources.add;
            this.toolStripButton1.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripButton1.Name = "toolStripButton1";
            this.toolStripButton1.Size = new System.Drawing.Size(23, 22);
            this.toolStripButton1.ToolTipText = "追加";
            this.toolStripButton1.Click += new System.EventHandler(this.toolStripButton1_Click);
            // 
            // toolStripButton2
            // 
            this.toolStripButton2.Image = global::Effect2DEditor.Properties.Resources.remove;
            this.toolStripButton2.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripButton2.Name = "toolStripButton2";
            this.toolStripButton2.Size = new System.Drawing.Size(23, 22);
            this.toolStripButton2.ToolTipText = "削除";
            this.toolStripButton2.Click += new System.EventHandler(this.toolStripButton2_Click);
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            this.toolStripSeparator1.Size = new System.Drawing.Size(6, 25);
            // 
            // exToolStripSplitButton1
            // 
            this.exToolStripSplitButton1.Checked = false;
            this.exToolStripSplitButton1.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.exToolStripSplitButton1.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.回再生ToolStripMenuItem,
            this.リバース1回再生ToolStripMenuItem,
            this.ループ再生ToolStripMenuItem,
            this.リバースループ再生ToolStripMenuItem});
            this.exToolStripSplitButton1.Image = global::Effect2DEditor.Properties.Resources.playonce;
            this.exToolStripSplitButton1.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.exToolStripSplitButton1.Name = "exToolStripSplitButton1";
            this.exToolStripSplitButton1.Size = new System.Drawing.Size(32, 22);
            this.exToolStripSplitButton1.Text = "exToolStripSplitButton1";
            this.exToolStripSplitButton1.ToolTipText = "１回再生";
            this.exToolStripSplitButton1.Click += new System.EventHandler(this.exToolStripSplitButton1_ButtonClick);
            // 
            // 回再生ToolStripMenuItem
            // 
            this.回再生ToolStripMenuItem.Checked = true;
            this.回再生ToolStripMenuItem.CheckState = System.Windows.Forms.CheckState.Checked;
            this.回再生ToolStripMenuItem.Image = global::Effect2DEditor.Properties.Resources.playonce;
            this.回再生ToolStripMenuItem.Name = "回再生ToolStripMenuItem";
            this.回再生ToolStripMenuItem.Size = new System.Drawing.Size(184, 22);
            this.回再生ToolStripMenuItem.Text = "1回再生";
            this.回再生ToolStripMenuItem.Click += new System.EventHandler(this.回再生ToolStripMenuItem_Click);
            // 
            // リバース1回再生ToolStripMenuItem
            // 
            this.リバース1回再生ToolStripMenuItem.Image = global::Effect2DEditor.Properties.Resources.playreverse;
            this.リバース1回再生ToolStripMenuItem.Name = "リバース1回再生ToolStripMenuItem";
            this.リバース1回再生ToolStripMenuItem.Size = new System.Drawing.Size(184, 22);
            this.リバース1回再生ToolStripMenuItem.Text = "リバース1回再生";
            this.リバース1回再生ToolStripMenuItem.Click += new System.EventHandler(this.リバース1回再生ToolStripMenuItem_Click);
            // 
            // ループ再生ToolStripMenuItem
            // 
            this.ループ再生ToolStripMenuItem.Image = global::Effect2DEditor.Properties.Resources.playloop;
            this.ループ再生ToolStripMenuItem.Name = "ループ再生ToolStripMenuItem";
            this.ループ再生ToolStripMenuItem.Size = new System.Drawing.Size(184, 22);
            this.ループ再生ToolStripMenuItem.Text = "ループ再生";
            this.ループ再生ToolStripMenuItem.Click += new System.EventHandler(this.ループ再生ToolStripMenuItem_Click);
            // 
            // リバースループ再生ToolStripMenuItem
            // 
            this.リバースループ再生ToolStripMenuItem.Image = global::Effect2DEditor.Properties.Resources.playreverseloop;
            this.リバースループ再生ToolStripMenuItem.Name = "リバースループ再生ToolStripMenuItem";
            this.リバースループ再生ToolStripMenuItem.Size = new System.Drawing.Size(184, 22);
            this.リバースループ再生ToolStripMenuItem.Text = "リバースループ再生";
            this.リバースループ再生ToolStripMenuItem.Click += new System.EventHandler(this.リバースループ再生ToolStripMenuItem_Click);
            // 
            // toolStripButton5
            // 
            this.toolStripButton5.Image = global::Effect2DEditor.Properties.Resources.stop;
            this.toolStripButton5.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripButton5.Name = "toolStripButton5";
            this.toolStripButton5.Size = new System.Drawing.Size(23, 22);
            this.toolStripButton5.ToolTipText = "停止";
            this.toolStripButton5.Click += new System.EventHandler(this.toolStripButton5_Click);
            // 
            // toolStripTextBox1
            // 
            this.toolStripTextBox1.Name = "toolStripTextBox1";
            this.toolStripTextBox1.Size = new System.Drawing.Size(100, 25);
            this.toolStripTextBox1.Text = "60";
            this.toolStripTextBox1.ToolTipText = "FPS";
            this.toolStripTextBox1.TextChanged += new System.EventHandler(this.toolStripTextBox1_TextChanged);
            // 
            // toolStripSeparator2
            // 
            this.toolStripSeparator2.Name = "toolStripSeparator2";
            this.toolStripSeparator2.Size = new System.Drawing.Size(6, 25);
            // 
            // toolStripButton3
            // 
            this.toolStripButton3.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.toolStripButton3.Image = global::Effect2DEditor.Properties.Resources.swapreverse;
            this.toolStripButton3.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripButton3.Name = "toolStripButton3";
            this.toolStripButton3.Size = new System.Drawing.Size(23, 22);
            this.toolStripButton3.Text = "順番を逆転";
            this.toolStripButton3.ToolTipText = "順番を逆転";
            this.toolStripButton3.Click += new System.EventHandler(this.toolStripButton3_Click);
            // 
            // toolStripButton4
            // 
            this.toolStripButton4.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.toolStripButton4.Image = global::Effect2DEditor.Properties.Resources.copy;
            this.toolStripButton4.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripButton4.Name = "toolStripButton4";
            this.toolStripButton4.Size = new System.Drawing.Size(23, 22);
            this.toolStripButton4.Text = "コピー";
            this.toolStripButton4.Click += new System.EventHandler(this.toolStripButton4_Click);
            // 
            // toolStripButton6
            // 
            this.toolStripButton6.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.toolStripButton6.Image = global::Effect2DEditor.Properties.Resources.delete;
            this.toolStripButton6.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripButton6.Name = "toolStripButton6";
            this.toolStripButton6.Size = new System.Drawing.Size(23, 22);
            this.toolStripButton6.Text = "全てクリア";
            this.toolStripButton6.Click += new System.EventHandler(this.toolStripButton6_Click);
            // 
            // toolStripContainer1
            // 
            // 
            // toolStripContainer1.ContentPanel
            // 
            this.toolStripContainer1.ContentPanel.Controls.Add(this.splitContainer3);
            this.toolStripContainer1.ContentPanel.Size = new System.Drawing.Size(1239, 522);
            this.toolStripContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.toolStripContainer1.Location = new System.Drawing.Point(0, 0);
            this.toolStripContainer1.Name = "toolStripContainer1";
            this.toolStripContainer1.Size = new System.Drawing.Size(1239, 547);
            this.toolStripContainer1.TabIndex = 3;
            this.toolStripContainer1.Text = "toolStripContainer1";
            // 
            // toolStripContainer1.TopToolStripPanel
            // 
            this.toolStripContainer1.TopToolStripPanel.Controls.Add(this.toolStrip1);
            // 
            // openFileDialog1
            // 
            this.openFileDialog1.FileName = "openFileDialog1";
            // 
            // TimeLineDock
            // 
            this.AllowDrop = true;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1239, 547);
            this.Controls.Add(this.toolStripContainer1);
            this.Font = new System.Drawing.Font("MS UI Gothic", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(128)));
            this.HideOnClose = true;
            this.Name = "TimeLineDock";
            this.Text = "TimeLineDock";
            this.DragDrop += new System.Windows.Forms.DragEventHandler(this.TimeLineDock_DragDrop);
            this.DragEnter += new System.Windows.Forms.DragEventHandler(this.TimeLineDock_DragEnter);
            this.splitContainer3.Panel1.ResumeLayout(false);
            this.splitContainer3.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer3)).EndInit();
            this.splitContainer3.ResumeLayout(false);
            this.toolStrip1.ResumeLayout(false);
            this.toolStrip1.PerformLayout();
            this.toolStripContainer1.ContentPanel.ResumeLayout(false);
            this.toolStripContainer1.TopToolStripPanel.ResumeLayout(false);
            this.toolStripContainer1.TopToolStripPanel.PerformLayout();
            this.toolStripContainer1.ResumeLayout(false);
            this.toolStripContainer1.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.SplitContainer splitContainer3;
        private TimeLineItem timeLineItem1;
        private TimeLine timeLine1;
        private System.Windows.Forms.ToolStrip toolStrip1;
        private System.Windows.Forms.ToolStripButton toolStripButton1;
        private System.Windows.Forms.ToolStripButton toolStripButton2;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private ExToolStripSplitButton exToolStripSplitButton1;
        private System.Windows.Forms.ToolStripMenuItem 回再生ToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem リバース1回再生ToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem ループ再生ToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem リバースループ再生ToolStripMenuItem;
        private System.Windows.Forms.ToolStripButton toolStripButton5;
        private System.Windows.Forms.ToolStripTextBox toolStripTextBox1;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator2;
        private System.Windows.Forms.ToolStripButton toolStripButton3;
        private System.Windows.Forms.ToolStripButton toolStripButton4;
        private System.Windows.Forms.ToolStripButton toolStripButton6;
        private System.Windows.Forms.ToolStripContainer toolStripContainer1;
        private System.Windows.Forms.OpenFileDialog openFileDialog1;
    }
}