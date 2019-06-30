using PPDEditor.Controls;
namespace PPDEditor
{
    partial class TimeLineForm
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
            this.splitContainer3 = new System.Windows.Forms.SplitContainer();
            this.seekex1 = new PPDEditor.Controls.Seekex();
            this.seekmain1 = new PPDEditor.Controls.Seekmain();
            this.contextMenuStrip1 = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.選択中のマークにIDを割り当てToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.選択中のマークのID割り当てを解除ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.位置を線形補完ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.角度を線形補完ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.時計回りToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.反時計回りToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.パラメーターToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer3)).BeginInit();
            this.splitContainer3.Panel1.SuspendLayout();
            this.splitContainer3.Panel2.SuspendLayout();
            this.splitContainer3.SuspendLayout();
            this.contextMenuStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // splitContainer3
            // 
            this.splitContainer3.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer3.FixedPanel = System.Windows.Forms.FixedPanel.Panel1;
            this.splitContainer3.IsSplitterFixed = true;
            this.splitContainer3.Location = new System.Drawing.Point(0, 0);
            this.splitContainer3.Name = "splitContainer3";
            // 
            // splitContainer3.Panel1
            // 
            this.splitContainer3.Panel1.Controls.Add(this.seekex1);
            // 
            // splitContainer3.Panel2
            // 
            this.splitContainer3.Panel2.AutoScroll = true;
            this.splitContainer3.Panel2.Controls.Add(this.seekmain1);
            this.splitContainer3.Size = new System.Drawing.Size(952, 304);
            this.splitContainer3.SplitterDistance = 95;
            this.splitContainer3.SplitterWidth = 1;
            this.splitContainer3.TabIndex = 1;
            // 
            // seekex1
            // 
            this.seekex1.BackColor = System.Drawing.SystemColors.ControlLightLight;
            this.seekex1.Cursor = System.Windows.Forms.Cursors.SizeWE;
            this.seekex1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.seekex1.Location = new System.Drawing.Point(0, 0);
            this.seekex1.Name = "seekex1";
            this.seekex1.Size = new System.Drawing.Size(95, 304);
            this.seekex1.TabIndex = 0;
            // 
            // seekmain1
            // 
            this.seekmain1.BackColor = System.Drawing.SystemColors.ControlLightLight;
            this.seekmain1.BeatType = PPDEditor.DisplayBeatType.Second;
            this.seekmain1.BPM = 100F;
            this.seekmain1.BPMSTART = 0F;
            this.seekmain1.ContextMenuStrip = this.contextMenuStrip1;
            this.seekmain1.Currenttime = 0D;
            this.seekmain1.DisplayMode = PPDEditor.DisplayLineMode.Fourth;
            this.seekmain1.DisplayWidth = 240;
            this.seekmain1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.seekmain1.Length = 10D;
            this.seekmain1.Location = new System.Drawing.Point(0, 0);
            this.seekmain1.MaximumSize = new System.Drawing.Size(66666, 1000);
            this.seekmain1.Name = "seekmain1";
            this.seekmain1.Size = new System.Drawing.Size(856, 304);
            this.seekmain1.SpeedScale = 1F;
            this.seekmain1.TabIndex = 0;
            // 
            // contextMenuStrip1
            // 
            this.contextMenuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.選択中のマークにIDを割り当てToolStripMenuItem,
            this.選択中のマークのID割り当てを解除ToolStripMenuItem,
            this.パラメーターToolStripMenuItem,
            this.位置を線形補完ToolStripMenuItem,
            this.角度を線形補完ToolStripMenuItem});
            this.contextMenuStrip1.Name = "contextMenuStrip1";
            this.contextMenuStrip1.Size = new System.Drawing.Size(271, 136);
            this.contextMenuStrip1.Opening += new System.ComponentModel.CancelEventHandler(this.contextMenuStrip1_Opening);
            // 
            // 選択中のマークにIDを割り当てToolStripMenuItem
            // 
            this.選択中のマークにIDを割り当てToolStripMenuItem.Name = "選択中のマークにIDを割り当てToolStripMenuItem";
            this.選択中のマークにIDを割り当てToolStripMenuItem.Size = new System.Drawing.Size(270, 22);
            this.選択中のマークにIDを割り当てToolStripMenuItem.Text = "選択中のマークにIDを割り当て";
            this.選択中のマークにIDを割り当てToolStripMenuItem.Click += new System.EventHandler(this.選択中のマークにIDを割り当てToolStripMenuItem_Click);
            // 
            // 選択中のマークのID割り当てを解除ToolStripMenuItem
            // 
            this.選択中のマークのID割り当てを解除ToolStripMenuItem.Name = "選択中のマークのID割り当てを解除ToolStripMenuItem";
            this.選択中のマークのID割り当てを解除ToolStripMenuItem.Size = new System.Drawing.Size(270, 22);
            this.選択中のマークのID割り当てを解除ToolStripMenuItem.Text = "選択中のマークのID割り当てを解除";
            this.選択中のマークのID割り当てを解除ToolStripMenuItem.Click += new System.EventHandler(this.選択中のマークのID割り当てを解除ToolStripMenuItem_Click);
            // 
            // 位置を線形補完ToolStripMenuItem
            // 
            this.位置を線形補完ToolStripMenuItem.Name = "位置を線形補完ToolStripMenuItem";
            this.位置を線形補完ToolStripMenuItem.Size = new System.Drawing.Size(270, 22);
            this.位置を線形補完ToolStripMenuItem.Text = "位置を線形補完";
            this.位置を線形補完ToolStripMenuItem.Click += new System.EventHandler(this.位置を線形補完ToolStripMenuItem_Click);
            // 
            // 角度を線形補完ToolStripMenuItem
            // 
            this.角度を線形補完ToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.時計回りToolStripMenuItem,
            this.反時計回りToolStripMenuItem});
            this.角度を線形補完ToolStripMenuItem.Name = "角度を線形補完ToolStripMenuItem";
            this.角度を線形補完ToolStripMenuItem.Size = new System.Drawing.Size(270, 22);
            this.角度を線形補完ToolStripMenuItem.Text = "角度を線形補完";
            // 
            // 時計回りToolStripMenuItem
            // 
            this.時計回りToolStripMenuItem.Name = "時計回りToolStripMenuItem";
            this.時計回りToolStripMenuItem.Size = new System.Drawing.Size(136, 22);
            this.時計回りToolStripMenuItem.Text = "時計回り";
            this.時計回りToolStripMenuItem.Click += new System.EventHandler(this.時計回りToolStripMenuItem_Click);
            // 
            // 反時計回りToolStripMenuItem
            // 
            this.反時計回りToolStripMenuItem.Name = "反時計回りToolStripMenuItem";
            this.反時計回りToolStripMenuItem.Size = new System.Drawing.Size(136, 22);
            this.反時計回りToolStripMenuItem.Text = "反時計回り";
            this.反時計回りToolStripMenuItem.Click += new System.EventHandler(this.反時計回りToolStripMenuItem_Click);
            // 
            // パラメーターToolStripMenuItem
            // 
            this.パラメーターToolStripMenuItem.Name = "パラメーターToolStripMenuItem";
            this.パラメーターToolStripMenuItem.Size = new System.Drawing.Size(270, 22);
            this.パラメーターToolStripMenuItem.Text = "パラメーター";
            // 
            // TimeLineForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(952, 304);
            this.Controls.Add(this.splitContainer3);
            this.Font = new System.Drawing.Font("MS UI Gothic", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(128)));
            this.HideOnClose = true;
            this.Name = "TimeLineForm";
            this.TabText = "タイムライン";
            this.Text = "タイムライン";
            this.DockStateChanged += new System.EventHandler(this.TimeLineForm_DockStateChanged);
            this.BackgroundImageChanged += new System.EventHandler(this.TimeLineForm_BackgroundImageChanged);
            this.splitContainer3.Panel1.ResumeLayout(false);
            this.splitContainer3.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer3)).EndInit();
            this.splitContainer3.ResumeLayout(false);
            this.contextMenuStrip1.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.SplitContainer splitContainer3;
        private Seekex seekex1;
        private Seekmain seekmain1;
        private System.Windows.Forms.ContextMenuStrip contextMenuStrip1;
        private System.Windows.Forms.ToolStripMenuItem 選択中のマークにIDを割り当てToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem 選択中のマークのID割り当てを解除ToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem 角度を線形補完ToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem 時計回りToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem 反時計回りToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem 位置を線形補完ToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem パラメーターToolStripMenuItem;
    }
}