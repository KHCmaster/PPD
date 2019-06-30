namespace Effect2DEditor
{
    partial class RatioChangeForm
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
            BezierCaliculator.BezierControlPoint bezierControlPoint1 = new BezierCaliculator.BezierControlPoint();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(RatioChangeForm));
            BezierCaliculator.BezierControlPoint bezierControlPoint2 = new BezierCaliculator.BezierControlPoint();
            this.button1 = new System.Windows.Forms.Button();
            this.button2 = new System.Windows.Forms.Button();
            this.button3 = new System.Windows.Forms.Button();
            this.button4 = new System.Windows.Forms.Button();
            this.button5 = new System.Windows.Forms.Button();
            this.bezierRatioDrawer1 = new Effect2DEditor.BezierRatioDrawer();
            this.SuspendLayout();
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(12, 146);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(75, 23);
            this.button1.TabIndex = 1;
            this.button1.Text = "OK";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // button2
            // 
            this.button2.Location = new System.Drawing.Point(93, 146);
            this.button2.Name = "button2";
            this.button2.Size = new System.Drawing.Size(75, 23);
            this.button2.TabIndex = 2;
            this.button2.Text = "Cancel";
            this.button2.UseVisualStyleBackColor = true;
            this.button2.Click += new System.EventHandler(this.button2_Click);
            // 
            // button3
            // 
            this.button3.Location = new System.Drawing.Point(160, 12);
            this.button3.Name = "button3";
            this.button3.Size = new System.Drawing.Size(75, 23);
            this.button3.TabIndex = 3;
            this.button3.Text = "線形変化";
            this.button3.UseVisualStyleBackColor = true;
            this.button3.VisibleChanged += new System.EventHandler(this.button3_VisibleChanged);
            this.button3.Click += new System.EventHandler(this.button3_Click);
            // 
            // button4
            // 
            this.button4.Location = new System.Drawing.Point(160, 41);
            this.button4.Name = "button4";
            this.button4.Size = new System.Drawing.Size(75, 23);
            this.button4.TabIndex = 4;
            this.button4.Text = "コピー";
            this.button4.UseVisualStyleBackColor = true;
            this.button4.Click += new System.EventHandler(this.button4_Click);
            // 
            // button5
            // 
            this.button5.Location = new System.Drawing.Point(160, 70);
            this.button5.Name = "button5";
            this.button5.Size = new System.Drawing.Size(75, 23);
            this.button5.TabIndex = 5;
            this.button5.Text = "ペースト";
            this.button5.UseVisualStyleBackColor = true;
            this.button5.Click += new System.EventHandler(this.button5_Click);
            // 
            // bezierRatioDrawer1
            // 
            bezierControlPoint1.First = ((System.Drawing.PointF)(resources.GetObject("bezierControlPoint1.First")));
            bezierControlPoint1.FirstDirection = ((System.Drawing.PointF)(resources.GetObject("bezierControlPoint1.FirstDirection")));
            bezierControlPoint1.FirstLength = 0F;
            bezierControlPoint1.Second = ((System.Drawing.PointF)(resources.GetObject("bezierControlPoint1.Second")));
            bezierControlPoint1.Third = ((System.Drawing.PointF)(resources.GetObject("bezierControlPoint1.Third")));
            bezierControlPoint1.ThirdDirection = ((System.Drawing.PointF)(resources.GetObject("bezierControlPoint1.ThirdDirection")));
            bezierControlPoint1.ThirdLength = 60F;
            bezierControlPoint1.ValidFirst = false;
            bezierControlPoint1.ValidSecond = true;
            bezierControlPoint1.ValidThird = true;
            bezierControlPoint2.First = ((System.Drawing.PointF)(resources.GetObject("bezierControlPoint2.First")));
            bezierControlPoint2.FirstDirection = ((System.Drawing.PointF)(resources.GetObject("bezierControlPoint2.FirstDirection")));
            bezierControlPoint2.FirstLength = 60F;
            bezierControlPoint2.Second = ((System.Drawing.PointF)(resources.GetObject("bezierControlPoint2.Second")));
            bezierControlPoint2.Third = ((System.Drawing.PointF)(resources.GetObject("bezierControlPoint2.Third")));
            bezierControlPoint2.ThirdDirection = ((System.Drawing.PointF)(resources.GetObject("bezierControlPoint2.ThirdDirection")));
            bezierControlPoint2.ThirdLength = 0F;
            bezierControlPoint2.ValidFirst = true;
            bezierControlPoint2.ValidSecond = true;
            bezierControlPoint2.ValidThird = false;
            this.bezierRatioDrawer1.BCPS = new BezierCaliculator.BezierControlPoint[] {
        bezierControlPoint1,
        bezierControlPoint2};
            this.bezierRatioDrawer1.Location = new System.Drawing.Point(26, 12);
            this.bezierRatioDrawer1.Name = "bezierRatioDrawer1";
            this.bezierRatioDrawer1.Size = new System.Drawing.Size(128, 128);
            this.bezierRatioDrawer1.TabIndex = 0;
            // 
            // RatioChangeForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(250, 205);
            this.ControlBox = false;
            this.Controls.Add(this.button5);
            this.Controls.Add(this.button4);
            this.Controls.Add(this.button3);
            this.Controls.Add(this.button2);
            this.Controls.Add(this.button1);
            this.Controls.Add(this.bezierRatioDrawer1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "RatioChangeForm";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.Text = "RatioChangeForm";
            this.ResumeLayout(false);

        }

        #endregion

        private BezierRatioDrawer bezierRatioDrawer1;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.Button button2;
        private System.Windows.Forms.Button button3;
        private System.Windows.Forms.Button button4;
        private System.Windows.Forms.Button button5;
    }
}