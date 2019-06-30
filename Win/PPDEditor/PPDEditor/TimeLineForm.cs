using System;
using DigitalRune.Windows.Docking;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace testgame
{
    public partial class TimeLineForm : DockableForm
    {
        bool changed = false;
        public TimeLineForm()
        {
            InitializeComponent();
            seekex1.setseekmain(seekmain1);
            seekex1.readfile();
            seekmain1.settlf(this);
            seekex1.MouseDown += new MouseEventHandler(seekex1_MouseDown);
            seekmain1.MouseDown += new MouseEventHandler(seekmain1_MouseDown);
            seekmain1.KeyDown += new KeyEventHandler(seekmain1_KeyDown);
            seekex1.KeyDown += new KeyEventHandler(seekex1_KeyDown);
        }

        void seekex1_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Space && this.DockPanel.ActiveContent == this)
            {
                Form1 fm1 = this.DockPanel.FindForm() as Form1;
                if (fm1 != null)
                {
                    fm1.playorpause();
                }
            }
        }

        void seekmain1_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Space && this.DockPanel.ActiveContent == this)
            {
                Form1 fm1 = this.DockPanel.FindForm() as Form1;
                if (fm1 != null)
                {
                    fm1.playorpause();
                }
            }
        }
        public void dxformkeydown(Keys key,bool playorpause)
        {
            if (playorpause)
            {
                Form1 fm1 = this.DockPanel.FindForm() as Form1;
                if (fm1 != null)
                {
                    fm1.playorpause();
                }
            }
            seekmain1.checkkey(key);
        }
        public void dxformpreviewkeydown(Keys key)
        {
            seekmain1.checkpreviewkey(key);
        }
        void seekmain1_MouseDown(object sender, MouseEventArgs e)
        {
            this.Pane.Activate();
        }

        void seekex1_MouseDown(object sender, MouseEventArgs e)
        {
            this.Pane.Activate();
        }


        public Seekmain seekmain
        {
            get
            {
                return this.seekmain1;
            }
        }
        public void setlang(settinganalyzer sanal)
        {
            this.toolStripMenuItem1.Text = sanal.getData("Deletedata0");
            this.deToolStripMenuItem.Text = sanal.getData("Deletedata1");
            this.toolStripMenuItem2.Text = sanal.getData("Deletedata2");
            this.toolStripMenuItem3.Text = sanal.getData("Deletedata3");
            this.toolStripMenuItem4.Text = sanal.getData("Deletedata4");
            this.toolStripMenuItem5.Text = sanal.getData("Deletedata5");
            this.toolStripMenuItem6.Text = sanal.getData("Deletedata6");
            this.toolStripMenuItem7.Text = sanal.getData("Deletedata7");
            this.toolStripMenuItem8.Text = sanal.getData("Deletedata8");
            this.toolStripMenuItem9.Text = sanal.getData("Deletedata9");
            this.toolStripMenuItem10.Text = sanal.getData("Deletedata10");
        }
        public void updatescroll()
        {
            if (seekmain1.Currenttime * seekmain1.defaultinterval - seekmain1.Width * splitContainer3.Panel2.HorizontalScroll.Value / splitContainer3.Panel2.HorizontalScroll.Maximum > splitContainer3.Panel2.Width)
            {
                try
                {
                    splitContainer3.Panel2.HorizontalScroll.Value = (int)((seekmain1.Currenttime * seekmain1.defaultinterval / seekmain1.Width) * splitContainer3.Panel2.HorizontalScroll.Maximum);
                }
                catch (Exception e)
                {
                }
            }
        }
        public void ContentSaved()
        {
            changed = false;
            if (this.TabText.StartsWith("*") && this.TabText.Length > 1)
            {
                this.TabText = this.TabText.Substring(1);
            }
        }
        public void ContentChanged()
        {
            changed = true;
            if (!this.TabText.StartsWith("*"))
            {
                this.TabText = "*" + this.TabText;
            }

        }
        public bool IsContentChanged
        {
            get
            {
                return changed;
            }
        }


        private void toolStripMenuItem1_Click(object sender, EventArgs e)
        {
            seekmain.deletedata(-1);
        }
        private void deToolStripMenuItem_Click(object sender, EventArgs e)
        {
            seekmain.deletedata(0);
        }
        private void toolStripMenuItem2_Click(object sender, EventArgs e)
        {
            seekmain.deletedata(1);
        }

        private void toolStripMenuItem3_Click(object sender, EventArgs e)
        {
            seekmain.deletedata(2);
        }

        private void toolStripMenuItem4_Click(object sender, EventArgs e)
        {
            seekmain.deletedata(3);
        }

        private void toolStripMenuItem5_Click(object sender, EventArgs e)
        {
            seekmain.deletedata(4);
        }

        private void toolStripMenuItem6_Click(object sender, EventArgs e)
        {
            seekmain.deletedata(5);
        }

        private void toolStripMenuItem7_Click(object sender, EventArgs e)
        {
            seekmain.deletedata(6);
        }

        private void toolStripMenuItem8_Click(object sender, EventArgs e)
        {
            seekmain.deletedata(7);
        }

        private void toolStripMenuItem9_Click(object sender, EventArgs e)
        {
            seekmain.deletedata(8);
        }

        private void toolStripMenuItem10_Click(object sender, EventArgs e)
        {
            seekmain.deletedata(9);
        }
    }
}
