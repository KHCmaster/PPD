namespace PPDInstaller.Controls
{
    partial class InstallComponentPanel
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
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.linkLabel1 = new System.Windows.Forms.LinkLabel();
            this.ipaFontCheckBox = new System.Windows.Forms.CheckBox();
            this.bmsTOPPDCheckBox = new System.Windows.Forms.CheckBox();
            this.label6 = new System.Windows.Forms.Label();
            this.ppdCheckBox = new System.Windows.Forms.CheckBox();
            this.label5 = new System.Windows.Forms.Label();
            this.lavFiltersCheckBox = new System.Windows.Forms.CheckBox();
            this.groupBox1.SuspendLayout();
            this.SuspendLayout();
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.linkLabel1);
            this.groupBox1.Controls.Add(this.ipaFontCheckBox);
            this.groupBox1.Location = new System.Drawing.Point(17, 51);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(330, 43);
            this.groupBox1.TabIndex = 23;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "インストール済みならチェックされません";
            // 
            // linkLabel1
            // 
            this.linkLabel1.AutoSize = true;
            this.linkLabel1.Location = new System.Drawing.Point(102, 19);
            this.linkLabel1.Name = "linkLabel1";
            this.linkLabel1.Size = new System.Drawing.Size(179, 12);
            this.linkLabel1.TabIndex = 11;
            this.linkLabel1.TabStop = true;
            this.linkLabel1.Text = "使用ライセンス(必ず確認してください)";
            this.linkLabel1.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.linkLabel1_LinkClicked);
            // 
            // ipaFontCheckBox
            // 
            this.ipaFontCheckBox.AutoSize = true;
            this.ipaFontCheckBox.Location = new System.Drawing.Point(6, 18);
            this.ipaFontCheckBox.Name = "ipaFontCheckBox";
            this.ipaFontCheckBox.Size = new System.Drawing.Size(79, 16);
            this.ipaFontCheckBox.TabIndex = 4;
            this.ipaFontCheckBox.Text = "IPA フォント";
            this.ipaFontCheckBox.UseVisualStyleBackColor = true;
            // 
            // bmsTOPPDCheckBox
            // 
            this.bmsTOPPDCheckBox.AutoSize = true;
            this.bmsTOPPDCheckBox.Checked = true;
            this.bmsTOPPDCheckBox.CheckState = System.Windows.Forms.CheckState.Checked;
            this.bmsTOPPDCheckBox.Location = new System.Drawing.Point(16, 136);
            this.bmsTOPPDCheckBox.Name = "bmsTOPPDCheckBox";
            this.bmsTOPPDCheckBox.Size = new System.Drawing.Size(85, 16);
            this.bmsTOPPDCheckBox.TabIndex = 19;
            this.bmsTOPPDCheckBox.Text = "BMSTOPPD";
            this.bmsTOPPDCheckBox.UseVisualStyleBackColor = true;
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(14, 121);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(116, 12);
            this.label6.TabIndex = 17;
            this.label6.Text = "インストールリスト(任意)";
            // 
            // ppdCheckBox
            // 
            this.ppdCheckBox.AutoSize = true;
            this.ppdCheckBox.Checked = true;
            this.ppdCheckBox.CheckState = System.Windows.Forms.CheckState.Checked;
            this.ppdCheckBox.Location = new System.Drawing.Point(17, 29);
            this.ppdCheckBox.Name = "ppdCheckBox";
            this.ppdCheckBox.Size = new System.Drawing.Size(46, 16);
            this.ppdCheckBox.TabIndex = 16;
            this.ppdCheckBox.Text = "PPD";
            this.ppdCheckBox.UseVisualStyleBackColor = true;
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(14, 12);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(116, 12);
            this.label5.TabIndex = 15;
            this.label5.Text = "インストールリスト(必須)";
            // 
            // lavFiltersCheckBox
            // 
            this.lavFiltersCheckBox.AutoSize = true;
            this.lavFiltersCheckBox.Checked = true;
            this.lavFiltersCheckBox.CheckState = System.Windows.Forms.CheckState.Checked;
            this.lavFiltersCheckBox.Location = new System.Drawing.Point(69, 29);
            this.lavFiltersCheckBox.Name = "lavFiltersCheckBox";
            this.lavFiltersCheckBox.Size = new System.Drawing.Size(79, 16);
            this.lavFiltersCheckBox.TabIndex = 25;
            this.lavFiltersCheckBox.Text = "LAVFilters";
            this.lavFiltersCheckBox.UseVisualStyleBackColor = true;
            // 
            // InstallComponentPanel
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.lavFiltersCheckBox);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.bmsTOPPDCheckBox);
            this.Controls.Add(this.label6);
            this.Controls.Add(this.ppdCheckBox);
            this.Controls.Add(this.label5);
            this.Name = "InstallComponentPanel";
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.LinkLabel linkLabel1;
        private System.Windows.Forms.CheckBox ipaFontCheckBox;
        private System.Windows.Forms.CheckBox bmsTOPPDCheckBox;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.CheckBox ppdCheckBox;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.CheckBox lavFiltersCheckBox;
    }
}
