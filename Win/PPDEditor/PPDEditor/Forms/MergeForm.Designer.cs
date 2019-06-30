namespace PPDEditor.Forms
{
    partial class MergeForm
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
            this.label1 = new System.Windows.Forms.Label();
            this.button1 = new System.Windows.Forms.Button();
            this.button2 = new System.Windows.Forms.Button();
            this.openFileDialog1 = new System.Windows.Forms.OpenFileDialog();
            this.textBox1 = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.button3 = new System.Windows.Forms.Button();
            this.saveFileDialog1 = new System.Windows.Forms.SaveFileDialog();
            this.mergeGroupBox5 = new PPDEditor.Controls.MergeGroupBox();
            this.mergeGroupBox4 = new PPDEditor.Controls.MergeGroupBox();
            this.mergeGroupBox3 = new PPDEditor.Controls.MergeGroupBox();
            this.mergeGroupBox2 = new PPDEditor.Controls.MergeGroupBox();
            this.mergeGroupBox1 = new PPDEditor.Controls.MergeGroupBox();
            this.comboBox1 = new System.Windows.Forms.ComboBox();
            this.label3 = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.Location = new System.Drawing.Point(12, 9);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(645, 25);
            this.label1.TabIndex = 0;
            this.label1.Text = "プロジェクトをマージ(結合)します。古いバージョンのプロジェクトは自動的に最新のバージョンのプロジェクトに変換された後にマージされます。";
            // 
            // button1
            // 
            this.button1.Enabled = false;
            this.button1.Location = new System.Drawing.Point(244, 240);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(75, 23);
            this.button1.TabIndex = 6;
            this.button1.Text = "OK";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // button2
            // 
            this.button2.Location = new System.Drawing.Point(354, 240);
            this.button2.Name = "button2";
            this.button2.Size = new System.Drawing.Size(75, 23);
            this.button2.TabIndex = 7;
            this.button2.Text = "Cancel";
            this.button2.UseVisualStyleBackColor = true;
            this.button2.Click += new System.EventHandler(this.button2_Click);
            // 
            // textBox1
            // 
            this.textBox1.Location = new System.Drawing.Point(444, 146);
            this.textBox1.Name = "textBox1";
            this.textBox1.ReadOnly = true;
            this.textBox1.Size = new System.Drawing.Size(183, 19);
            this.textBox1.TabIndex = 8;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(444, 131);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(41, 12);
            this.label2.TabIndex = 9;
            this.label2.Text = "保存先";
            // 
            // button3
            // 
            this.button3.Location = new System.Drawing.Point(633, 144);
            this.button3.Name = "button3";
            this.button3.Size = new System.Drawing.Size(22, 23);
            this.button3.TabIndex = 10;
            this.button3.Text = "...";
            this.button3.UseVisualStyleBackColor = true;
            this.button3.Click += new System.EventHandler(this.button3_Click);
            // 
            // mergeGroupBox5
            // 
            this.mergeGroupBox5.GroupName = "Base";
            this.mergeGroupBox5.Label = "マージする難易度";
            this.mergeGroupBox5.Location = new System.Drawing.Point(231, 131);
            this.mergeGroupBox5.Name = "mergeGroupBox5";
            this.mergeGroupBox5.ProjectPath = "";
            this.mergeGroupBox5.ProjectVersion = 0;
            this.mergeGroupBox5.Size = new System.Drawing.Size(211, 88);
            this.mergeGroupBox5.TabIndex = 5;
            // 
            // mergeGroupBox4
            // 
            this.mergeGroupBox4.GroupName = "Extreme";
            this.mergeGroupBox4.Label = "マージする難易度";
            this.mergeGroupBox4.Location = new System.Drawing.Point(14, 131);
            this.mergeGroupBox4.Name = "mergeGroupBox4";
            this.mergeGroupBox4.ProjectPath = "";
            this.mergeGroupBox4.ProjectVersion = 0;
            this.mergeGroupBox4.Size = new System.Drawing.Size(211, 88);
            this.mergeGroupBox4.TabIndex = 4;
            // 
            // mergeGroupBox3
            // 
            this.mergeGroupBox3.GroupName = "Hard";
            this.mergeGroupBox3.Label = "マージする難易度";
            this.mergeGroupBox3.Location = new System.Drawing.Point(446, 37);
            this.mergeGroupBox3.Name = "mergeGroupBox3";
            this.mergeGroupBox3.ProjectPath = "";
            this.mergeGroupBox3.ProjectVersion = 0;
            this.mergeGroupBox3.Size = new System.Drawing.Size(211, 88);
            this.mergeGroupBox3.TabIndex = 3;
            // 
            // mergeGroupBox2
            // 
            this.mergeGroupBox2.GroupName = "Normal";
            this.mergeGroupBox2.Label = "マージする難易度";
            this.mergeGroupBox2.Location = new System.Drawing.Point(229, 37);
            this.mergeGroupBox2.Name = "mergeGroupBox2";
            this.mergeGroupBox2.ProjectPath = "";
            this.mergeGroupBox2.ProjectVersion = 0;
            this.mergeGroupBox2.Size = new System.Drawing.Size(211, 88);
            this.mergeGroupBox2.TabIndex = 2;
            // 
            // mergeGroupBox1
            // 
            this.mergeGroupBox1.GroupName = "Easy";
            this.mergeGroupBox1.Label = "マージする難易度";
            this.mergeGroupBox1.Location = new System.Drawing.Point(12, 37);
            this.mergeGroupBox1.Name = "mergeGroupBox1";
            this.mergeGroupBox1.ProjectPath = "";
            this.mergeGroupBox1.ProjectVersion = 0;
            this.mergeGroupBox1.Size = new System.Drawing.Size(211, 88);
            this.mergeGroupBox1.TabIndex = 1;
            // 
            // comboBox1
            // 
            this.comboBox1.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBox1.FormattingEnabled = true;
            this.comboBox1.Location = new System.Drawing.Point(446, 200);
            this.comboBox1.Name = "comboBox1";
            this.comboBox1.Size = new System.Drawing.Size(121, 20);
            this.comboBox1.TabIndex = 11;
            // 
            // label3
            // 
            this.label3.Location = new System.Drawing.Point(444, 170);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(213, 27);
            this.label3.TabIndex = 12;
            this.label3.Text = "プロジェクトの基本情報を取得する難易度";
            // 
            // MergeForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(669, 275);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.comboBox1);
            this.Controls.Add(this.button3);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.textBox1);
            this.Controls.Add(this.button2);
            this.Controls.Add(this.button1);
            this.Controls.Add(this.mergeGroupBox5);
            this.Controls.Add(this.mergeGroupBox4);
            this.Controls.Add(this.mergeGroupBox3);
            this.Controls.Add(this.mergeGroupBox2);
            this.Controls.Add(this.mergeGroupBox1);
            this.Controls.Add(this.label1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "MergeForm";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.Text = "プロジェクトのマージ";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private PPDEditor.Controls.MergeGroupBox mergeGroupBox1;
        private PPDEditor.Controls.MergeGroupBox mergeGroupBox2;
        private PPDEditor.Controls.MergeGroupBox mergeGroupBox3;
        private PPDEditor.Controls.MergeGroupBox mergeGroupBox4;
        private PPDEditor.Controls.MergeGroupBox mergeGroupBox5;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.Button button2;
        private System.Windows.Forms.OpenFileDialog openFileDialog1;
        private System.Windows.Forms.TextBox textBox1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Button button3;
        private System.Windows.Forms.SaveFileDialog saveFileDialog1;
        private System.Windows.Forms.ComboBox comboBox1;
        private System.Windows.Forms.Label label3;
    }
}