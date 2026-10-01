using BezierCaliculator;
using PPDConfiguration;
using System;
using System.Windows.Forms;

namespace Effect2DEditor
{
    public partial class RatioChangeForm : Form
    {
        public RatioChangeForm()
        {
            InitializeComponent();
        }
        public void SetLang(SettingReader setting)
        {
            if (setting != null)
            {
                button3.Text = setting["RCFButton3"];
                button4.Text = setting["RCFButton4"];
                button5.Text = setting["RCFButton5"];
            }
        }
        public BezierControlPoint[] Bcps
        {
            get
            {
                return bezierRatioDrawer1.BCPS;
            }
            set
            {
                bezierRatioDrawer1.BCPS = value;
                bezierRatioDrawer1.DrawandRefresh();
            }
        }
        public MainForm MainForm
        {
            get;
            set;
        }
        public bool IsLinear
        {
            get { return bezierRatioDrawer1.IsLinear; }
        }
        private void Button1_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        private void Button2_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.No;
            this.Close();
        }

        private void Button3_Click(object sender, EventArgs e)
        {
            bezierRatioDrawer1.SetLinearRatio();
        }

        private void Button3_VisibleChanged(object sender, EventArgs e)
        {
            if (Visible)
            {
                this.Location = Cursor.Position;
            }
        }

        private void Button4_Click(object sender, EventArgs e)
        {
            BezierControlPoint[] bcps = new BezierControlPoint[2];
            for (int i = 0; i < bcps.Length; i++)
            {
                bcps[i] = bezierRatioDrawer1.BCPS[i].Clone();
            }
            MainForm.CopiedBezier = bcps;
        }

        private void Button5_Click(object sender, EventArgs e)
        {
            if (MainForm.CopiedBezier != null)
            {
                BezierControlPoint[] bcps = new BezierControlPoint[2];
                for (int i = 0; i < bcps.Length; i++)
                {
                    bcps[i] = MainForm.CopiedBezier[i].Clone();
                }
                bezierRatioDrawer1.BCPS = bcps;
                bezierRatioDrawer1.DrawandRefresh();
            }
        }
    }
}
