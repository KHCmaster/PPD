namespace PPDEditor.DockForm.Script
{
    partial class ScriptListDockForm
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
            this.treeView1 = new System.Windows.Forms.TreeView();
            this.toolStripContainer1 = new System.Windows.Forms.ToolStripContainer();
            this.toolStrip1 = new System.Windows.Forms.ToolStrip();
            this.toolStripButton1 = new System.Windows.Forms.ToolStripButton();
            this.toolStripButton2 = new System.Windows.Forms.ToolStripButton();
            this.contextMenuStrip1 = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.無効化ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.新規フォルダ作成ToolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
            this.削除ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.名前の変更ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.contextMenuStrip2 = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.新規スクリプトの作成ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.新規フォルダ作成ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.削除ToolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
            this.名前の変更ToolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripContainer1.ContentPanel.SuspendLayout();
            this.toolStripContainer1.TopToolStripPanel.SuspendLayout();
            this.toolStripContainer1.SuspendLayout();
            this.toolStrip1.SuspendLayout();
            this.contextMenuStrip1.SuspendLayout();
            this.contextMenuStrip2.SuspendLayout();
            this.SuspendLayout();
            // 
            // treeView1
            // 
            this.treeView1.AllowDrop = true;
            this.treeView1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.treeView1.Location = new System.Drawing.Point(0, 0);
            this.treeView1.Name = "treeView1";
            this.treeView1.Size = new System.Drawing.Size(292, 242);
            this.treeView1.TabIndex = 0;
            this.treeView1.ItemDrag += new System.Windows.Forms.ItemDragEventHandler(this.treeView1_ItemDrag);
            this.treeView1.NodeMouseClick += new System.Windows.Forms.TreeNodeMouseClickEventHandler(this.treeView1_NodeMouseClick);
            this.treeView1.NodeMouseDoubleClick += new System.Windows.Forms.TreeNodeMouseClickEventHandler(this.treeView1_NodeMouseDoubleClick);
            this.treeView1.DragDrop += new System.Windows.Forms.DragEventHandler(this.treeView1_DragDrop);
            this.treeView1.DragOver += new System.Windows.Forms.DragEventHandler(this.treeView1_DragOver);
            this.treeView1.KeyDown += new System.Windows.Forms.KeyEventHandler(this.treeView1_KeyDown);
            // 
            // toolStripContainer1
            // 
            // 
            // toolStripContainer1.ContentPanel
            // 
            this.toolStripContainer1.ContentPanel.Controls.Add(this.treeView1);
            this.toolStripContainer1.ContentPanel.Size = new System.Drawing.Size(292, 242);
            this.toolStripContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.toolStripContainer1.Location = new System.Drawing.Point(0, 0);
            this.toolStripContainer1.Name = "toolStripContainer1";
            this.toolStripContainer1.Size = new System.Drawing.Size(292, 267);
            this.toolStripContainer1.TabIndex = 1;
            this.toolStripContainer1.Text = "toolStripContainer1";
            // 
            // toolStripContainer1.TopToolStripPanel
            // 
            this.toolStripContainer1.TopToolStripPanel.Controls.Add(this.toolStrip1);
            // 
            // toolStrip1
            // 
            this.toolStrip1.Dock = System.Windows.Forms.DockStyle.None;
            this.toolStrip1.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
            this.toolStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripButton1,
            this.toolStripButton2});
            this.toolStrip1.Location = new System.Drawing.Point(3, 0);
            this.toolStrip1.Name = "toolStrip1";
            this.toolStrip1.Size = new System.Drawing.Size(49, 25);
            this.toolStrip1.TabIndex = 0;
            // 
            // toolStripButton1
            // 
            this.toolStripButton1.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.toolStripButton1.Image = global::PPDEditor.Properties.Resources.update;
            this.toolStripButton1.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripButton1.Name = "toolStripButton1";
            this.toolStripButton1.Size = new System.Drawing.Size(23, 22);
            this.toolStripButton1.Text = "関数更新";
            this.toolStripButton1.Click += new System.EventHandler(this.toolStripButton1_Click);
            // 
            // toolStripButton2
            // 
            this.toolStripButton2.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.toolStripButton2.Image = global::PPDEditor.Properties.Resources.folder;
            this.toolStripButton2.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripButton2.Name = "toolStripButton2";
            this.toolStripButton2.Size = new System.Drawing.Size(23, 22);
            this.toolStripButton2.Text = "フォルダ作成";
            this.toolStripButton2.Click += new System.EventHandler(this.toolStripButton2_Click);
            // 
            // contextMenuStrip1
            // 
            this.contextMenuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.無効化ToolStripMenuItem,
            this.新規フォルダ作成ToolStripMenuItem1,
            this.削除ToolStripMenuItem,
            this.名前の変更ToolStripMenuItem});
            this.contextMenuStrip1.Name = "contextMenuStrip1";
            this.contextMenuStrip1.Size = new System.Drawing.Size(173, 92);
            this.contextMenuStrip1.Opening += new System.ComponentModel.CancelEventHandler(this.contextMenuStrip1_Opening);
            // 
            // 無効化ToolStripMenuItem
            // 
            this.無効化ToolStripMenuItem.Name = "無効化ToolStripMenuItem";
            this.無効化ToolStripMenuItem.Size = new System.Drawing.Size(172, 22);
            this.無効化ToolStripMenuItem.Text = "無効化";
            this.無効化ToolStripMenuItem.Click += new System.EventHandler(this.無効化ToolStripMenuItem_Click);
            // 
            // 新規フォルダ作成ToolStripMenuItem1
            // 
            this.新規フォルダ作成ToolStripMenuItem1.Name = "新規フォルダ作成ToolStripMenuItem1";
            this.新規フォルダ作成ToolStripMenuItem1.Size = new System.Drawing.Size(172, 22);
            this.新規フォルダ作成ToolStripMenuItem1.Text = "新規フォルダ作成";
            this.新規フォルダ作成ToolStripMenuItem1.Click += new System.EventHandler(this.新規フォルダ作成ToolStripMenuItem1_Click);
            // 
            // 削除ToolStripMenuItem
            // 
            this.削除ToolStripMenuItem.Name = "削除ToolStripMenuItem";
            this.削除ToolStripMenuItem.Size = new System.Drawing.Size(172, 22);
            this.削除ToolStripMenuItem.Text = "削除";
            this.削除ToolStripMenuItem.Click += new System.EventHandler(this.削除ToolStripMenuItem_Click);
            // 
            // 名前の変更ToolStripMenuItem
            // 
            this.名前の変更ToolStripMenuItem.Name = "名前の変更ToolStripMenuItem";
            this.名前の変更ToolStripMenuItem.Size = new System.Drawing.Size(172, 22);
            this.名前の変更ToolStripMenuItem.Text = "名前の変更";
            this.名前の変更ToolStripMenuItem.Click += new System.EventHandler(this.名前の変更ToolStripMenuItem_Click);
            // 
            // contextMenuStrip2
            // 
            this.contextMenuStrip2.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.新規スクリプトの作成ToolStripMenuItem,
            this.新規フォルダ作成ToolStripMenuItem,
            this.削除ToolStripMenuItem1,
            this.名前の変更ToolStripMenuItem1});
            this.contextMenuStrip2.Name = "contextMenuStrip2";
            this.contextMenuStrip2.Size = new System.Drawing.Size(197, 92);
            this.contextMenuStrip2.Opening += new System.ComponentModel.CancelEventHandler(this.contextMenuStrip2_Opening);
            // 
            // 新規スクリプトの作成ToolStripMenuItem
            // 
            this.新規スクリプトの作成ToolStripMenuItem.Name = "新規スクリプトの作成ToolStripMenuItem";
            this.新規スクリプトの作成ToolStripMenuItem.Size = new System.Drawing.Size(196, 22);
            this.新規スクリプトの作成ToolStripMenuItem.Text = "新規スクリプトの作成";
            this.新規スクリプトの作成ToolStripMenuItem.Click += new System.EventHandler(this.新規スクリプトの作成ToolStripMenuItem_Click);
            // 
            // 新規フォルダ作成ToolStripMenuItem
            // 
            this.新規フォルダ作成ToolStripMenuItem.Name = "新規フォルダ作成ToolStripMenuItem";
            this.新規フォルダ作成ToolStripMenuItem.Size = new System.Drawing.Size(196, 22);
            this.新規フォルダ作成ToolStripMenuItem.Text = "新規フォルダ作成";
            this.新規フォルダ作成ToolStripMenuItem.Click += new System.EventHandler(this.新規フォルダ作成ToolStripMenuItem_Click);
            // 
            // 削除ToolStripMenuItem1
            // 
            this.削除ToolStripMenuItem1.Name = "削除ToolStripMenuItem1";
            this.削除ToolStripMenuItem1.Size = new System.Drawing.Size(196, 22);
            this.削除ToolStripMenuItem1.Text = "削除";
            this.削除ToolStripMenuItem1.Click += new System.EventHandler(this.削除ToolStripMenuItem1_Click);
            // 
            // 名前の変更ToolStripMenuItem1
            // 
            this.名前の変更ToolStripMenuItem1.Name = "名前の変更ToolStripMenuItem1";
            this.名前の変更ToolStripMenuItem1.Size = new System.Drawing.Size(196, 22);
            this.名前の変更ToolStripMenuItem1.Text = "名前の変更";
            this.名前の変更ToolStripMenuItem1.Click += new System.EventHandler(this.名前の変更ToolStripMenuItem1_Click);
            // 
            // ScriptListDockForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(292, 267);
            this.Controls.Add(this.toolStripContainer1);
            this.Font = new System.Drawing.Font("MS UI Gothic", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(128)));
            this.HideOnClose = true;
            this.KeyPreview = true;
            this.Name = "ScriptListDockForm";
            this.Text = "ScriptListDockForm";
            this.toolStripContainer1.ContentPanel.ResumeLayout(false);
            this.toolStripContainer1.TopToolStripPanel.ResumeLayout(false);
            this.toolStripContainer1.TopToolStripPanel.PerformLayout();
            this.toolStripContainer1.ResumeLayout(false);
            this.toolStripContainer1.PerformLayout();
            this.toolStrip1.ResumeLayout(false);
            this.toolStrip1.PerformLayout();
            this.contextMenuStrip1.ResumeLayout(false);
            this.contextMenuStrip2.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TreeView treeView1;
        private System.Windows.Forms.ToolStripContainer toolStripContainer1;
        private System.Windows.Forms.ToolStrip toolStrip1;
        private System.Windows.Forms.ToolStripButton toolStripButton1;
        private System.Windows.Forms.ToolStripButton toolStripButton2;
        private System.Windows.Forms.ContextMenuStrip contextMenuStrip1;
        private System.Windows.Forms.ToolStripMenuItem 無効化ToolStripMenuItem;
        private System.Windows.Forms.ContextMenuStrip contextMenuStrip2;
        private System.Windows.Forms.ToolStripMenuItem 新規スクリプトの作成ToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem 新規フォルダ作成ToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem 新規フォルダ作成ToolStripMenuItem1;
        private System.Windows.Forms.ToolStripMenuItem 削除ToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem 削除ToolStripMenuItem1;
        private System.Windows.Forms.ToolStripMenuItem 名前の変更ToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem 名前の変更ToolStripMenuItem1;
    }
}