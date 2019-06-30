namespace Effect2DEditor
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
            WeifenLuo.WinFormsUI.Docking.DockPanelSkin dockPanelSkin1 = new WeifenLuo.WinFormsUI.Docking.DockPanelSkin();
            WeifenLuo.WinFormsUI.Docking.AutoHideStripSkin autoHideStripSkin1 = new WeifenLuo.WinFormsUI.Docking.AutoHideStripSkin();
            WeifenLuo.WinFormsUI.Docking.DockPanelGradient dockPanelGradient1 = new WeifenLuo.WinFormsUI.Docking.DockPanelGradient();
            WeifenLuo.WinFormsUI.Docking.TabGradient tabGradient1 = new WeifenLuo.WinFormsUI.Docking.TabGradient();
            WeifenLuo.WinFormsUI.Docking.DockPaneStripSkin dockPaneStripSkin1 = new WeifenLuo.WinFormsUI.Docking.DockPaneStripSkin();
            WeifenLuo.WinFormsUI.Docking.DockPaneStripGradient dockPaneStripGradient1 = new WeifenLuo.WinFormsUI.Docking.DockPaneStripGradient();
            WeifenLuo.WinFormsUI.Docking.TabGradient tabGradient2 = new WeifenLuo.WinFormsUI.Docking.TabGradient();
            WeifenLuo.WinFormsUI.Docking.DockPanelGradient dockPanelGradient2 = new WeifenLuo.WinFormsUI.Docking.DockPanelGradient();
            WeifenLuo.WinFormsUI.Docking.TabGradient tabGradient3 = new WeifenLuo.WinFormsUI.Docking.TabGradient();
            WeifenLuo.WinFormsUI.Docking.DockPaneStripToolWindowGradient dockPaneStripToolWindowGradient1 = new WeifenLuo.WinFormsUI.Docking.DockPaneStripToolWindowGradient();
            WeifenLuo.WinFormsUI.Docking.TabGradient tabGradient4 = new WeifenLuo.WinFormsUI.Docking.TabGradient();
            WeifenLuo.WinFormsUI.Docking.TabGradient tabGradient5 = new WeifenLuo.WinFormsUI.Docking.TabGradient();
            WeifenLuo.WinFormsUI.Docking.DockPanelGradient dockPanelGradient3 = new WeifenLuo.WinFormsUI.Docking.DockPanelGradient();
            WeifenLuo.WinFormsUI.Docking.TabGradient tabGradient6 = new WeifenLuo.WinFormsUI.Docking.TabGradient();
            WeifenLuo.WinFormsUI.Docking.TabGradient tabGradient7 = new WeifenLuo.WinFormsUI.Docking.TabGradient();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainForm));
            this.openFileDialog1 = new System.Windows.Forms.OpenFileDialog();
            this.menuStrip1 = new System.Windows.Forms.MenuStrip();
            this.ファイルToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.新規作成ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.開くToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.保存するToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.上書き保存するToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.最近使用したファイルToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.終了するToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.編集ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.元に戻すToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.やり直すToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.アニメーションToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.カンバスサイズToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.カンバス背景変更ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.表示ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.キャンバスウィンドウToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.プロパティウィンドウToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.ヒストリーウィンドウToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.タイムラインウィンドウToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.言語ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.saveFileDialog1 = new System.Windows.Forms.SaveFileDialog();
            this.dockPanel1 = new WeifenLuo.WinFormsUI.Docking.DockPanel();
            this.menuStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // openFileDialog1
            // 
            this.openFileDialog1.Filter = "ETD|*.etd";
            this.openFileDialog1.InitialDirectory = ".";
            this.openFileDialog1.RestoreDirectory = true;
            // 
            // menuStrip1
            // 
            this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.ファイルToolStripMenuItem,
            this.編集ToolStripMenuItem,
            this.表示ToolStripMenuItem,
            this.言語ToolStripMenuItem});
            this.menuStrip1.Location = new System.Drawing.Point(0, 0);
            this.menuStrip1.Name = "menuStrip1";
            this.menuStrip1.Size = new System.Drawing.Size(1005, 26);
            this.menuStrip1.TabIndex = 1;
            this.menuStrip1.Text = "menuStrip1";
            // 
            // ファイルToolStripMenuItem
            // 
            this.ファイルToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.新規作成ToolStripMenuItem,
            this.開くToolStripMenuItem,
            this.保存するToolStripMenuItem,
            this.上書き保存するToolStripMenuItem,
            this.最近使用したファイルToolStripMenuItem,
            this.終了するToolStripMenuItem});
            this.ファイルToolStripMenuItem.Name = "ファイルToolStripMenuItem";
            this.ファイルToolStripMenuItem.Size = new System.Drawing.Size(68, 22);
            this.ファイルToolStripMenuItem.Text = "ファイル";
            // 
            // 新規作成ToolStripMenuItem
            // 
            this.新規作成ToolStripMenuItem.Name = "新規作成ToolStripMenuItem";
            this.新規作成ToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.N)));
            this.新規作成ToolStripMenuItem.Size = new System.Drawing.Size(208, 22);
            this.新規作成ToolStripMenuItem.Text = "新規作成";
            this.新規作成ToolStripMenuItem.Click += new System.EventHandler(this.新規作成ToolStripMenuItem_Click);
            // 
            // 開くToolStripMenuItem
            // 
            this.開くToolStripMenuItem.Name = "開くToolStripMenuItem";
            this.開くToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.O)));
            this.開くToolStripMenuItem.Size = new System.Drawing.Size(208, 22);
            this.開くToolStripMenuItem.Text = "開く";
            this.開くToolStripMenuItem.Click += new System.EventHandler(this.開くToolStripMenuItem_Click);
            // 
            // 保存するToolStripMenuItem
            // 
            this.保存するToolStripMenuItem.Name = "保存するToolStripMenuItem";
            this.保存するToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)(((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.Shift)
                        | System.Windows.Forms.Keys.S)));
            this.保存するToolStripMenuItem.Size = new System.Drawing.Size(208, 22);
            this.保存するToolStripMenuItem.Text = "保存する";
            this.保存するToolStripMenuItem.Click += new System.EventHandler(this.保存するToolStripMenuItem_Click);
            // 
            // 上書き保存するToolStripMenuItem
            // 
            this.上書き保存するToolStripMenuItem.Name = "上書き保存するToolStripMenuItem";
            this.上書き保存するToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.S)));
            this.上書き保存するToolStripMenuItem.Size = new System.Drawing.Size(208, 22);
            this.上書き保存するToolStripMenuItem.Text = "上書き保存する";
            this.上書き保存するToolStripMenuItem.Click += new System.EventHandler(this.上書き保存するToolStripMenuItem_Click);
            // 
            // 最近使用したファイルToolStripMenuItem
            // 
            this.最近使用したファイルToolStripMenuItem.Name = "最近使用したファイルToolStripMenuItem";
            this.最近使用したファイルToolStripMenuItem.Size = new System.Drawing.Size(208, 22);
            this.最近使用したファイルToolStripMenuItem.Text = "最近使用したファイル";
            // 
            // 終了するToolStripMenuItem
            // 
            this.終了するToolStripMenuItem.Name = "終了するToolStripMenuItem";
            this.終了するToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Alt | System.Windows.Forms.Keys.F4)));
            this.終了するToolStripMenuItem.Size = new System.Drawing.Size(208, 22);
            this.終了するToolStripMenuItem.Text = "終了する";
            this.終了するToolStripMenuItem.Click += new System.EventHandler(this.終了するToolStripMenuItem_Click);
            // 
            // 編集ToolStripMenuItem
            // 
            this.編集ToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.元に戻すToolStripMenuItem,
            this.やり直すToolStripMenuItem,
            this.アニメーションToolStripMenuItem,
            this.カンバスサイズToolStripMenuItem,
            this.カンバス背景変更ToolStripMenuItem});
            this.編集ToolStripMenuItem.Name = "編集ToolStripMenuItem";
            this.編集ToolStripMenuItem.Size = new System.Drawing.Size(44, 22);
            this.編集ToolStripMenuItem.Text = "編集";
            // 
            // 元に戻すToolStripMenuItem
            // 
            this.元に戻すToolStripMenuItem.Name = "元に戻すToolStripMenuItem";
            this.元に戻すToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.Z)));
            this.元に戻すToolStripMenuItem.Size = new System.Drawing.Size(172, 22);
            this.元に戻すToolStripMenuItem.Text = "元に戻す";
            this.元に戻すToolStripMenuItem.Click += new System.EventHandler(this.元に戻すToolStripMenuItem_Click);
            // 
            // やり直すToolStripMenuItem
            // 
            this.やり直すToolStripMenuItem.Name = "やり直すToolStripMenuItem";
            this.やり直すToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.Y)));
            this.やり直すToolStripMenuItem.Size = new System.Drawing.Size(172, 22);
            this.やり直すToolStripMenuItem.Text = "やり直す";
            this.やり直すToolStripMenuItem.Click += new System.EventHandler(this.やり直すToolStripMenuItem_Click);
            // 
            // アニメーションToolStripMenuItem
            // 
            this.アニメーションToolStripMenuItem.Name = "アニメーションToolStripMenuItem";
            this.アニメーションToolStripMenuItem.Size = new System.Drawing.Size(172, 22);
            this.アニメーションToolStripMenuItem.Text = "アニメーション";
            this.アニメーションToolStripMenuItem.Click += new System.EventHandler(this.アニメーションToolStripMenuItem_Click);
            // 
            // カンバスサイズToolStripMenuItem
            // 
            this.カンバスサイズToolStripMenuItem.Name = "カンバスサイズToolStripMenuItem";
            this.カンバスサイズToolStripMenuItem.Size = new System.Drawing.Size(172, 22);
            this.カンバスサイズToolStripMenuItem.Text = "カンバスサイズ";
            this.カンバスサイズToolStripMenuItem.Click += new System.EventHandler(this.カンバスサイズToolStripMenuItem_Click);
            // 
            // カンバス背景変更ToolStripMenuItem
            // 
            this.カンバス背景変更ToolStripMenuItem.Name = "カンバス背景変更ToolStripMenuItem";
            this.カンバス背景変更ToolStripMenuItem.Size = new System.Drawing.Size(172, 22);
            this.カンバス背景変更ToolStripMenuItem.Text = "カンバス背景変更";
            this.カンバス背景変更ToolStripMenuItem.Click += new System.EventHandler(this.カンバス背景変更ToolStripMenuItem_Click);
            // 
            // 表示ToolStripMenuItem
            // 
            this.表示ToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.キャンバスウィンドウToolStripMenuItem,
            this.プロパティウィンドウToolStripMenuItem,
            this.ヒストリーウィンドウToolStripMenuItem,
            this.タイムラインウィンドウToolStripMenuItem});
            this.表示ToolStripMenuItem.Name = "表示ToolStripMenuItem";
            this.表示ToolStripMenuItem.Size = new System.Drawing.Size(44, 22);
            this.表示ToolStripMenuItem.Text = "表示";
            // 
            // キャンバスウィンドウToolStripMenuItem
            // 
            this.キャンバスウィンドウToolStripMenuItem.Name = "キャンバスウィンドウToolStripMenuItem";
            this.キャンバスウィンドウToolStripMenuItem.Size = new System.Drawing.Size(208, 22);
            this.キャンバスウィンドウToolStripMenuItem.Text = "キャンバスウィンドウ";
            this.キャンバスウィンドウToolStripMenuItem.Click += new System.EventHandler(this.キャンバスウィンドウToolStripMenuItem_Click);
            // 
            // プロパティウィンドウToolStripMenuItem
            // 
            this.プロパティウィンドウToolStripMenuItem.Name = "プロパティウィンドウToolStripMenuItem";
            this.プロパティウィンドウToolStripMenuItem.Size = new System.Drawing.Size(208, 22);
            this.プロパティウィンドウToolStripMenuItem.Text = "プロパティウィンドウ";
            this.プロパティウィンドウToolStripMenuItem.Click += new System.EventHandler(this.プロパティウィンドウToolStripMenuItem_Click);
            // 
            // ヒストリーウィンドウToolStripMenuItem
            // 
            this.ヒストリーウィンドウToolStripMenuItem.Name = "ヒストリーウィンドウToolStripMenuItem";
            this.ヒストリーウィンドウToolStripMenuItem.Size = new System.Drawing.Size(208, 22);
            this.ヒストリーウィンドウToolStripMenuItem.Text = "ヒストリーウィンドウ";
            this.ヒストリーウィンドウToolStripMenuItem.Click += new System.EventHandler(this.ヒストリーウィンドウToolStripMenuItem_Click);
            // 
            // タイムラインウィンドウToolStripMenuItem
            // 
            this.タイムラインウィンドウToolStripMenuItem.Name = "タイムラインウィンドウToolStripMenuItem";
            this.タイムラインウィンドウToolStripMenuItem.Size = new System.Drawing.Size(208, 22);
            this.タイムラインウィンドウToolStripMenuItem.Text = "タイムラインウィンドウ";
            this.タイムラインウィンドウToolStripMenuItem.Click += new System.EventHandler(this.タイムラインウィンドウToolStripMenuItem_Click);
            // 
            // 言語ToolStripMenuItem
            // 
            this.言語ToolStripMenuItem.Name = "言語ToolStripMenuItem";
            this.言語ToolStripMenuItem.Size = new System.Drawing.Size(44, 22);
            this.言語ToolStripMenuItem.Text = "言語";
            // 
            // saveFileDialog1
            // 
            this.saveFileDialog1.DefaultExt = "etd";
            this.saveFileDialog1.FileName = ".";
            this.saveFileDialog1.Filter = "ETD|*.etd";
            this.saveFileDialog1.RestoreDirectory = true;
            // 
            // dockPanel1
            // 
            this.dockPanel1.ActiveAutoHideContent = null;
            this.dockPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dockPanel1.DockBackColor = System.Drawing.SystemColors.Control;
            this.dockPanel1.DocumentStyle = WeifenLuo.WinFormsUI.Docking.DocumentStyle.DockingWindow;
            this.dockPanel1.Location = new System.Drawing.Point(0, 26);
            this.dockPanel1.Name = "dockPanel1";
            this.dockPanel1.Size = new System.Drawing.Size(1005, 625);
            dockPanelGradient1.EndColor = System.Drawing.SystemColors.ControlLight;
            dockPanelGradient1.StartColor = System.Drawing.SystemColors.ControlLight;
            autoHideStripSkin1.DockStripGradient = dockPanelGradient1;
            tabGradient1.EndColor = System.Drawing.SystemColors.Control;
            tabGradient1.StartColor = System.Drawing.SystemColors.Control;
            tabGradient1.TextColor = System.Drawing.SystemColors.ControlDarkDark;
            autoHideStripSkin1.TabGradient = tabGradient1;
            autoHideStripSkin1.TextFont = new System.Drawing.Font("メイリオ", 9F);
            dockPanelSkin1.AutoHideStripSkin = autoHideStripSkin1;
            tabGradient2.EndColor = System.Drawing.SystemColors.ControlLightLight;
            tabGradient2.StartColor = System.Drawing.SystemColors.ControlLightLight;
            tabGradient2.TextColor = System.Drawing.SystemColors.ControlText;
            dockPaneStripGradient1.ActiveTabGradient = tabGradient2;
            dockPanelGradient2.EndColor = System.Drawing.SystemColors.Control;
            dockPanelGradient2.StartColor = System.Drawing.SystemColors.Control;
            dockPaneStripGradient1.DockStripGradient = dockPanelGradient2;
            tabGradient3.EndColor = System.Drawing.SystemColors.ControlLight;
            tabGradient3.StartColor = System.Drawing.SystemColors.ControlLight;
            tabGradient3.TextColor = System.Drawing.SystemColors.ControlText;
            dockPaneStripGradient1.InactiveTabGradient = tabGradient3;
            dockPaneStripSkin1.DocumentGradient = dockPaneStripGradient1;
            dockPaneStripSkin1.TextFont = new System.Drawing.Font("メイリオ", 9F);
            tabGradient4.EndColor = System.Drawing.SystemColors.ActiveCaption;
            tabGradient4.LinearGradientMode = System.Drawing.Drawing2D.LinearGradientMode.Vertical;
            tabGradient4.StartColor = System.Drawing.SystemColors.GradientActiveCaption;
            tabGradient4.TextColor = System.Drawing.SystemColors.ActiveCaptionText;
            dockPaneStripToolWindowGradient1.ActiveCaptionGradient = tabGradient4;
            tabGradient5.EndColor = System.Drawing.SystemColors.Control;
            tabGradient5.StartColor = System.Drawing.SystemColors.Control;
            tabGradient5.TextColor = System.Drawing.SystemColors.ControlText;
            dockPaneStripToolWindowGradient1.ActiveTabGradient = tabGradient5;
            dockPanelGradient3.EndColor = System.Drawing.SystemColors.ControlLight;
            dockPanelGradient3.StartColor = System.Drawing.SystemColors.ControlLight;
            dockPaneStripToolWindowGradient1.DockStripGradient = dockPanelGradient3;
            tabGradient6.EndColor = System.Drawing.SystemColors.InactiveCaption;
            tabGradient6.LinearGradientMode = System.Drawing.Drawing2D.LinearGradientMode.Vertical;
            tabGradient6.StartColor = System.Drawing.SystemColors.GradientInactiveCaption;
            tabGradient6.TextColor = System.Drawing.SystemColors.InactiveCaptionText;
            dockPaneStripToolWindowGradient1.InactiveCaptionGradient = tabGradient6;
            tabGradient7.EndColor = System.Drawing.Color.Transparent;
            tabGradient7.StartColor = System.Drawing.Color.Transparent;
            tabGradient7.TextColor = System.Drawing.SystemColors.ControlDarkDark;
            dockPaneStripToolWindowGradient1.InactiveTabGradient = tabGradient7;
            dockPaneStripSkin1.ToolWindowGradient = dockPaneStripToolWindowGradient1;
            dockPanelSkin1.DockPaneStripSkin = dockPaneStripSkin1;
            this.dockPanel1.Skin = dockPanelSkin1;
            this.dockPanel1.TabIndex = 2;
            // 
            // MainForm
            // 
            this.AllowDrop = true;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1005, 651);
            this.Controls.Add(this.dockPanel1);
            this.Controls.Add(this.menuStrip1);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "MainForm";
            this.Text = "Effect2DEditor";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.MainForm_FormClosing);
            this.DragDrop += new System.Windows.Forms.DragEventHandler(this.MainForm_DragDrop);
            this.DragEnter += new System.Windows.Forms.DragEventHandler(this.MainForm_DragEnter);
            this.menuStrip1.ResumeLayout(false);
            this.menuStrip1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.OpenFileDialog openFileDialog1;
        private System.Windows.Forms.MenuStrip menuStrip1;
        private System.Windows.Forms.ToolStripMenuItem ファイルToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem 開くToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem 保存するToolStripMenuItem;
        private System.Windows.Forms.SaveFileDialog saveFileDialog1;
        private System.Windows.Forms.ToolStripMenuItem 編集ToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem 元に戻すToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem やり直すToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem アニメーションToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem カンバスサイズToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem カンバス背景変更ToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem 上書き保存するToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem 終了するToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem 最近使用したファイルToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem 言語ToolStripMenuItem;
        private WeifenLuo.WinFormsUI.Docking.DockPanel dockPanel1;
        private System.Windows.Forms.ToolStripMenuItem 新規作成ToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem 表示ToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem キャンバスウィンドウToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem プロパティウィンドウToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem ヒストリーウィンドウToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem タイムラインウィンドウToolStripMenuItem;



    }
}

