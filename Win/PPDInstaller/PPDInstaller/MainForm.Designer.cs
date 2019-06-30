namespace PPDInstaller
{
    partial class MainForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainForm));
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.pictureBox1 = new System.Windows.Forms.PictureBox();
            this.splitContainer2 = new System.Windows.Forms.SplitContainer();
            this.previousButton = new System.Windows.Forms.Button();
            this.nextButton = new System.Windows.Forms.Button();
            this.cancelButton = new System.Windows.Forms.Button();
            this.startPanel1 = new PPDInstaller.Controls.StartPanel();
            this.installPanel1 = new PPDInstaller.Controls.InstallPanel();
            this.installComponentPanel1 = new PPDInstaller.Controls.InstallComponentPanel();
            this.linkCreatePanel1 = new PPDInstaller.Controls.LinkCreatePanel();
            this.confirmPanel1 = new PPDInstaller.Controls.ConfirmPanel();
            this.finishPanel1 = new PPDInstaller.Controls.FinishPanel();
            this.installAbortedPanel1 = new PPDInstaller.Controls.InstallAbortedPanel();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer2)).BeginInit();
            this.splitContainer2.Panel2.SuspendLayout();
            this.splitContainer2.SuspendLayout();
            this.SuspendLayout();
            // 
            // splitContainer1
            // 
            this.splitContainer1.BackColor = System.Drawing.SystemColors.Window;
            this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer1.IsSplitterFixed = true;
            this.splitContainer1.Location = new System.Drawing.Point(0, 0);
            this.splitContainer1.Name = "splitContainer1";
            // 
            // splitContainer1.Panel1
            // 
            this.splitContainer1.Panel1.Controls.Add(this.pictureBox1);
            this.splitContainer1.Panel1MinSize = 200;
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.Controls.Add(this.splitContainer2);
            this.splitContainer1.Size = new System.Drawing.Size(600, 300);
            this.splitContainer1.SplitterDistance = 200;
            this.splitContainer1.SplitterWidth = 1;
            this.splitContainer1.TabIndex = 0;
            // 
            // pictureBox1
            // 
            this.pictureBox1.Image = global::PPDInstaller.Properties.Resources.inst;
            this.pictureBox1.Location = new System.Drawing.Point(0, 0);
            this.pictureBox1.Margin = new System.Windows.Forms.Padding(0);
            this.pictureBox1.Name = "pictureBox1";
            this.pictureBox1.Size = new System.Drawing.Size(200, 300);
            this.pictureBox1.TabIndex = 0;
            this.pictureBox1.TabStop = false;
            // 
            // splitContainer2
            // 
            this.splitContainer2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer2.IsSplitterFixed = true;
            this.splitContainer2.Location = new System.Drawing.Point(0, 0);
            this.splitContainer2.Name = "splitContainer2";
            this.splitContainer2.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // splitContainer2.Panel2
            // 
            this.splitContainer2.Panel2.Controls.Add(this.previousButton);
            this.splitContainer2.Panel2.Controls.Add(this.nextButton);
            this.splitContainer2.Panel2.Controls.Add(this.cancelButton);
            this.splitContainer2.Size = new System.Drawing.Size(399, 300);
            this.splitContainer2.SplitterDistance = 270;
            this.splitContainer2.SplitterWidth = 1;
            this.splitContainer2.TabIndex = 0;
            // 
            // previousButton
            // 
            this.previousButton.Enabled = false;
            this.previousButton.Location = new System.Drawing.Point(61, 3);
            this.previousButton.Name = "previousButton";
            this.previousButton.Size = new System.Drawing.Size(85, 23);
            this.previousButton.TabIndex = 2;
            this.previousButton.Text = "前へ";
            this.previousButton.UseVisualStyleBackColor = true;
            this.previousButton.Click += new System.EventHandler(this.button3_Click);
            // 
            // nextButton
            // 
            this.nextButton.Location = new System.Drawing.Point(152, 3);
            this.nextButton.Name = "nextButton";
            this.nextButton.Size = new System.Drawing.Size(87, 23);
            this.nextButton.TabIndex = 1;
            this.nextButton.Text = "次へ";
            this.nextButton.UseVisualStyleBackColor = true;
            this.nextButton.Click += new System.EventHandler(this.button2_Click);
            // 
            // cancelButton
            // 
            this.cancelButton.AutoSize = true;
            this.cancelButton.Location = new System.Drawing.Point(276, 3);
            this.cancelButton.Name = "cancelButton";
            this.cancelButton.Size = new System.Drawing.Size(90, 23);
            this.cancelButton.TabIndex = 0;
            this.cancelButton.Text = "キャンセル";
            this.cancelButton.UseVisualStyleBackColor = true;
            this.cancelButton.Click += new System.EventHandler(this.button1_Click);
            // 
            // startPanel1
            // 
            this.startPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.startPanel1.Location = new System.Drawing.Point(0, 0);
            this.startPanel1.Name = "startPanel1";
            this.startPanel1.PanelManager = null;
            this.startPanel1.Size = new System.Drawing.Size(399, 270);
            this.startPanel1.TabIndex = 0;
            // 
            // installPanel1
            // 
            this.installPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.installPanel1.Location = new System.Drawing.Point(0, 0);
            this.installPanel1.Name = "installPanel1";
            this.installPanel1.PanelManager = null;
            this.installPanel1.Size = new System.Drawing.Size(399, 270);
            this.installPanel1.TabIndex = 5;
            this.installPanel1.Visible = false;
            // 
            // installComponentPanel1
            // 
            this.installComponentPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.installComponentPanel1.Location = new System.Drawing.Point(0, 0);
            this.installComponentPanel1.Name = "installComponentPanel1";
            this.installComponentPanel1.PanelManager = null;
            this.installComponentPanel1.Size = new System.Drawing.Size(399, 270);
            this.installComponentPanel1.TabIndex = 4;
            this.installComponentPanel1.Visible = false;
            // 
            // linkCreatePanel1
            // 
            this.linkCreatePanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.linkCreatePanel1.Location = new System.Drawing.Point(0, 0);
            this.linkCreatePanel1.Name = "linkCreatePanel1";
            this.linkCreatePanel1.PanelManager = null;
            this.linkCreatePanel1.Size = new System.Drawing.Size(399, 270);
            this.linkCreatePanel1.TabIndex = 6;
            this.linkCreatePanel1.Visible = false;
            // 
            // confirmPanel1
            // 
            this.confirmPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.confirmPanel1.Location = new System.Drawing.Point(0, 0);
            this.confirmPanel1.Name = "confirmPanel1";
            this.confirmPanel1.PanelManager = null;
            this.confirmPanel1.Size = new System.Drawing.Size(399, 270);
            this.confirmPanel1.TabIndex = 1;
            this.confirmPanel1.Visible = false;
            // 
            // finishPanel1
            // 
            this.finishPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.finishPanel1.Location = new System.Drawing.Point(0, 0);
            this.finishPanel1.Name = "finishPanel1";
            this.finishPanel1.PanelManager = null;
            this.finishPanel1.Size = new System.Drawing.Size(399, 270);
            this.finishPanel1.TabIndex = 2;
            this.finishPanel1.Visible = false;
            // 
            // installAbortedPanel1
            // 
            this.installAbortedPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.installAbortedPanel1.Location = new System.Drawing.Point(0, 0);
            this.installAbortedPanel1.Name = "installAbortedPanel1";
            this.installAbortedPanel1.PanelManager = null;
            this.installAbortedPanel1.Size = new System.Drawing.Size(399, 270);
            this.installAbortedPanel1.TabIndex = 3;
            this.installAbortedPanel1.Visible = false;
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(600, 300);
            this.Controls.Add(this.splitContainer1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.Name = "MainForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "PPDInstaller";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.Form1_FormClosing);
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
            this.splitContainer1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
            this.splitContainer2.Panel2.ResumeLayout(false);
            this.splitContainer2.Panel2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer2)).EndInit();
            this.splitContainer2.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.SplitContainer splitContainer1;
        private System.Windows.Forms.PictureBox pictureBox1;
        private System.Windows.Forms.SplitContainer splitContainer2;
        private System.Windows.Forms.Button previousButton;
        private System.Windows.Forms.Button nextButton;
        private System.Windows.Forms.Button cancelButton;
        private PPDInstaller.Controls.StartPanel startPanel1;
        private PPDInstaller.Controls.LinkCreatePanel linkCreatePanel1;
        private PPDInstaller.Controls.InstallPanel installPanel1;
        private PPDInstaller.Controls.InstallComponentPanel installComponentPanel1;
        private PPDInstaller.Controls.InstallAbortedPanel installAbortedPanel1;
        private PPDInstaller.Controls.FinishPanel finishPanel1;
        private PPDInstaller.Controls.ConfirmPanel confirmPanel1;
    }
}

