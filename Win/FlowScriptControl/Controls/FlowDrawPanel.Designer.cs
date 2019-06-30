namespace FlowScriptControl.Controls
{
    partial class FlowDrawPanel
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
            this.contextMenuStrip1 = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.addNodeToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.スコープを追加ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.スコープを削除ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.コメントを追加ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.バインドコメントを追加ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.バインドコメントを削除ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.スコープ内の要素を選択ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.スコープ内の全要素を選択ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.ブレイクポイントを設定ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.ブレイクポイントを解除ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.リンクしているノードを検索ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.切り取りToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.コピーToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.貼り付けToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.リンクも貼り付けToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.削除ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
            this.表示領域を合わせるToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.elementHost1 = new System.Windows.Forms.Integration.ElementHost();
            this.toolStripSeparator3 = new System.Windows.Forms.ToolStripSeparator();
            this.ノード名をコピーToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.プロパティ名をコピーToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.contextMenuStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // contextMenuStrip1
            // 
            this.contextMenuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.addNodeToolStripMenuItem,
            this.スコープを追加ToolStripMenuItem,
            this.スコープを削除ToolStripMenuItem,
            this.コメントを追加ToolStripMenuItem,
            this.バインドコメントを追加ToolStripMenuItem,
            this.バインドコメントを削除ToolStripMenuItem,
            this.スコープ内の要素を選択ToolStripMenuItem,
            this.スコープ内の全要素を選択ToolStripMenuItem,
            this.ブレイクポイントを設定ToolStripMenuItem,
            this.ブレイクポイントを解除ToolStripMenuItem,
            this.リンクしているノードを検索ToolStripMenuItem,
            this.toolStripSeparator1,
            this.切り取りToolStripMenuItem,
            this.コピーToolStripMenuItem,
            this.貼り付けToolStripMenuItem,
            this.リンクも貼り付けToolStripMenuItem,
            this.削除ToolStripMenuItem,
            this.toolStripSeparator2,
            this.ノード名をコピーToolStripMenuItem,
            this.プロパティ名をコピーToolStripMenuItem,
            this.toolStripSeparator3,
            this.表示領域を合わせるToolStripMenuItem});
            this.contextMenuStrip1.Name = "contextMenuStrip1";
            this.contextMenuStrip1.Size = new System.Drawing.Size(233, 462);
            this.contextMenuStrip1.Opening += new System.ComponentModel.CancelEventHandler(this.contextMenuStrip1_Opening);
            // 
            // addNodeToolStripMenuItem
            // 
            this.addNodeToolStripMenuItem.ForeColor = System.Drawing.SystemColors.ControlText;
            this.addNodeToolStripMenuItem.Name = "addNodeToolStripMenuItem";
            this.addNodeToolStripMenuItem.Size = new System.Drawing.Size(232, 22);
            this.addNodeToolStripMenuItem.Text = "ノードを追加";
            // 
            // スコープを追加ToolStripMenuItem
            // 
            this.スコープを追加ToolStripMenuItem.Name = "スコープを追加ToolStripMenuItem";
            this.スコープを追加ToolStripMenuItem.Size = new System.Drawing.Size(232, 22);
            this.スコープを追加ToolStripMenuItem.Text = "スコープを追加";
            this.スコープを追加ToolStripMenuItem.Click += new System.EventHandler(this.スコープを追加ToolStripMenuItem_Click);
            // 
            // スコープを削除ToolStripMenuItem
            // 
            this.スコープを削除ToolStripMenuItem.Name = "スコープを削除ToolStripMenuItem";
            this.スコープを削除ToolStripMenuItem.Size = new System.Drawing.Size(232, 22);
            this.スコープを削除ToolStripMenuItem.Text = "スコープを削除";
            this.スコープを削除ToolStripMenuItem.Click += new System.EventHandler(this.スコープを削除ToolStripMenuItem_Click);
            // 
            // コメントを追加ToolStripMenuItem
            // 
            this.コメントを追加ToolStripMenuItem.Name = "コメントを追加ToolStripMenuItem";
            this.コメントを追加ToolStripMenuItem.Size = new System.Drawing.Size(232, 22);
            this.コメントを追加ToolStripMenuItem.Text = "コメントを追加";
            this.コメントを追加ToolStripMenuItem.Click += new System.EventHandler(this.コメントを追加ToolStripMenuItem_Click);
            // 
            // バインドコメントを追加ToolStripMenuItem
            // 
            this.バインドコメントを追加ToolStripMenuItem.Name = "バインドコメントを追加ToolStripMenuItem";
            this.バインドコメントを追加ToolStripMenuItem.Size = new System.Drawing.Size(232, 22);
            this.バインドコメントを追加ToolStripMenuItem.Text = "バインドコメントを追加";
            this.バインドコメントを追加ToolStripMenuItem.Click += new System.EventHandler(this.バインドコメントを追加ToolStripMenuItem_Click);
            // 
            // バインドコメントを削除ToolStripMenuItem
            // 
            this.バインドコメントを削除ToolStripMenuItem.Name = "バインドコメントを削除ToolStripMenuItem";
            this.バインドコメントを削除ToolStripMenuItem.Size = new System.Drawing.Size(232, 22);
            this.バインドコメントを削除ToolStripMenuItem.Text = "バインドコメントを削除";
            this.バインドコメントを削除ToolStripMenuItem.Click += new System.EventHandler(this.バインドコメントを削除ToolStripMenuItem_Click);
            // 
            // スコープ内の要素を選択ToolStripMenuItem
            // 
            this.スコープ内の要素を選択ToolStripMenuItem.Name = "スコープ内の要素を選択ToolStripMenuItem";
            this.スコープ内の要素を選択ToolStripMenuItem.Size = new System.Drawing.Size(232, 22);
            this.スコープ内の要素を選択ToolStripMenuItem.Text = "スコープ内の要素を選択";
            this.スコープ内の要素を選択ToolStripMenuItem.Click += new System.EventHandler(this.スコープ内の要素を選択ToolStripMenuItem_Click);
            // 
            // スコープ内の全要素を選択ToolStripMenuItem
            // 
            this.スコープ内の全要素を選択ToolStripMenuItem.Name = "スコープ内の全要素を選択ToolStripMenuItem";
            this.スコープ内の全要素を選択ToolStripMenuItem.Size = new System.Drawing.Size(232, 22);
            this.スコープ内の全要素を選択ToolStripMenuItem.Text = "スコープ内の全要素を選択";
            this.スコープ内の全要素を選択ToolStripMenuItem.Click += new System.EventHandler(this.スコープ内の全要素を選択ToolStripMenuItem_Click);
            // 
            // ブレイクポイントを設定ToolStripMenuItem
            // 
            this.ブレイクポイントを設定ToolStripMenuItem.Name = "ブレイクポイントを設定ToolStripMenuItem";
            this.ブレイクポイントを設定ToolStripMenuItem.Size = new System.Drawing.Size(232, 22);
            this.ブレイクポイントを設定ToolStripMenuItem.Text = "ブレイクポイントを設定";
            this.ブレイクポイントを設定ToolStripMenuItem.Click += new System.EventHandler(this.ブレイクポイントを設定ToolStripMenuItem_Click);
            // 
            // ブレイクポイントを解除ToolStripMenuItem
            // 
            this.ブレイクポイントを解除ToolStripMenuItem.Name = "ブレイクポイントを解除ToolStripMenuItem";
            this.ブレイクポイントを解除ToolStripMenuItem.Size = new System.Drawing.Size(232, 22);
            this.ブレイクポイントを解除ToolStripMenuItem.Text = "ブレイクポイントを解除";
            this.ブレイクポイントを解除ToolStripMenuItem.Click += new System.EventHandler(this.ブレイクポイントを解除ToolStripMenuItem_Click);
            // 
            // リンクしているノードを検索ToolStripMenuItem
            // 
            this.リンクしているノードを検索ToolStripMenuItem.Name = "リンクしているノードを検索ToolStripMenuItem";
            this.リンクしているノードを検索ToolStripMenuItem.Size = new System.Drawing.Size(232, 22);
            this.リンクしているノードを検索ToolStripMenuItem.Text = "リンクしているノードを検索";
            this.リンクしているノードを検索ToolStripMenuItem.Click += new System.EventHandler(this.リンクしているノードを検索ToolStripMenuItem_Click);
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            this.toolStripSeparator1.Size = new System.Drawing.Size(229, 6);
            // 
            // 切り取りToolStripMenuItem
            // 
            this.切り取りToolStripMenuItem.Name = "切り取りToolStripMenuItem";
            this.切り取りToolStripMenuItem.Size = new System.Drawing.Size(232, 22);
            this.切り取りToolStripMenuItem.Text = "切り取り";
            this.切り取りToolStripMenuItem.Click += new System.EventHandler(this.切り取りToolStripMenuItem_Click);
            // 
            // コピーToolStripMenuItem
            // 
            this.コピーToolStripMenuItem.Name = "コピーToolStripMenuItem";
            this.コピーToolStripMenuItem.Size = new System.Drawing.Size(232, 22);
            this.コピーToolStripMenuItem.Text = "コピー";
            this.コピーToolStripMenuItem.Click += new System.EventHandler(this.コピーToolStripMenuItem_Click);
            // 
            // 貼り付けToolStripMenuItem
            // 
            this.貼り付けToolStripMenuItem.Name = "貼り付けToolStripMenuItem";
            this.貼り付けToolStripMenuItem.Size = new System.Drawing.Size(232, 22);
            this.貼り付けToolStripMenuItem.Text = "貼り付け";
            this.貼り付けToolStripMenuItem.Click += new System.EventHandler(this.貼り付けToolStripMenuItem_Click);
            // 
            // リンクも貼り付けToolStripMenuItem
            // 
            this.リンクも貼り付けToolStripMenuItem.Name = "リンクも貼り付けToolStripMenuItem";
            this.リンクも貼り付けToolStripMenuItem.Size = new System.Drawing.Size(232, 22);
            this.リンクも貼り付けToolStripMenuItem.Text = "リンクも貼り付け";
            this.リンクも貼り付けToolStripMenuItem.Click += new System.EventHandler(this.リンクも貼り付けToolStripMenuItem_Click);
            // 
            // 削除ToolStripMenuItem
            // 
            this.削除ToolStripMenuItem.Name = "削除ToolStripMenuItem";
            this.削除ToolStripMenuItem.Size = new System.Drawing.Size(232, 22);
            this.削除ToolStripMenuItem.Text = "削除";
            this.削除ToolStripMenuItem.Click += new System.EventHandler(this.削除ToolStripMenuItem_Click);
            // 
            // toolStripSeparator2
            // 
            this.toolStripSeparator2.Name = "toolStripSeparator2";
            this.toolStripSeparator2.Size = new System.Drawing.Size(229, 6);
            // 
            // 表示領域を合わせるToolStripMenuItem
            // 
            this.表示領域を合わせるToolStripMenuItem.Name = "表示領域を合わせるToolStripMenuItem";
            this.表示領域を合わせるToolStripMenuItem.Size = new System.Drawing.Size(232, 22);
            this.表示領域を合わせるToolStripMenuItem.Text = "表示領域を合わせる";
            this.表示領域を合わせるToolStripMenuItem.Click += new System.EventHandler(this.表示領域を合わせるToolStripMenuItem_Click);
            // 
            // elementHost1
            // 
            this.elementHost1.AllowDrop = true;
            this.elementHost1.ContextMenuStrip = this.contextMenuStrip1;
            this.elementHost1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.elementHost1.Font = new System.Drawing.Font("IPAゴシック", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(128)));
            this.elementHost1.Location = new System.Drawing.Point(0, 0);
            this.elementHost1.Name = "elementHost1";
            this.elementHost1.Size = new System.Drawing.Size(150, 150);
            this.elementHost1.TabIndex = 1;
            this.elementHost1.Text = "elementHost1";
            this.elementHost1.Child = null;
            // 
            // toolStripSeparator3
            // 
            this.toolStripSeparator3.Name = "toolStripSeparator3";
            this.toolStripSeparator3.Size = new System.Drawing.Size(229, 6);
            // 
            // ノード名をコピーToolStripMenuItem
            // 
            this.ノード名をコピーToolStripMenuItem.Name = "ノード名をコピーToolStripMenuItem";
            this.ノード名をコピーToolStripMenuItem.Size = new System.Drawing.Size(232, 22);
            this.ノード名をコピーToolStripMenuItem.Text = "ノード名をコピー";
            this.ノード名をコピーToolStripMenuItem.Click += new System.EventHandler(this.ノード名をコピーToolStripMenuItem_Click);
            // 
            // プロパティ名をコピーToolStripMenuItem
            // 
            this.プロパティ名をコピーToolStripMenuItem.Name = "プロパティ名をコピーToolStripMenuItem";
            this.プロパティ名をコピーToolStripMenuItem.Size = new System.Drawing.Size(232, 22);
            this.プロパティ名をコピーToolStripMenuItem.Text = "プロパティ名をコピー";
            this.プロパティ名をコピーToolStripMenuItem.Click += new System.EventHandler(this.プロパティ名をコピーToolStripMenuItem_Click);
            // 
            // FlowDrawPanel
            // 
            this.AllowDrop = true;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.elementHost1);
            this.Name = "FlowDrawPanel";
            this.Load += new System.EventHandler(this.FlowDrawPanel_Load);
            this.contextMenuStrip1.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.ContextMenuStrip contextMenuStrip1;
        private System.Windows.Forms.ToolStripMenuItem addNodeToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem コメントを追加ToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private System.Windows.Forms.ToolStripMenuItem 切り取りToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem コピーToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem 貼り付けToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem リンクも貼り付けToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator2;
        private System.Windows.Forms.ToolStripMenuItem 表示領域を合わせるToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem 削除ToolStripMenuItem;
        private System.Windows.Forms.Integration.ElementHost elementHost1;
        private System.Windows.Forms.ToolStripMenuItem バインドコメントを追加ToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem バインドコメントを削除ToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem スコープを追加ToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem スコープを削除ToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem スコープ内の要素を選択ToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem スコープ内の全要素を選択ToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem ブレイクポイントを設定ToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem ブレイクポイントを解除ToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem リンクしているノードを検索ToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator3;
        private System.Windows.Forms.ToolStripMenuItem ノード名をコピーToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem プロパティ名をコピーToolStripMenuItem;
    }
}
