using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using System.Drawing.Imaging;

namespace WeifenLuo.WinFormsUI.Docking
{
    public partial class AutoHideFloatWindow : FloatWindow
    {
        bool hidden = false;
        bool ismouseover = false;
        Size lastcs;
        Timer timer;

        public Size TrueClientSize
        {
            get
            {
                return lastcs;
            }
            set
            {
                lastcs = value;
            }
        }

        public bool AutoHidden
        {
            get
            {
                return hidden;
            }
            set
            {
                hidden = value;
            }
        }

        internal protected AutoHideFloatWindow(DockPanel dockPanel, DockPane pane)
            : base(dockPanel, pane)
        {
            InternalStruct();
        }

        internal protected AutoHideFloatWindow(DockPanel dockPanel, DockPane pane, Rectangle bounds)
            : base(dockPanel, pane, bounds)
        {
            InternalStruct();
        }

        private void InternalStruct()
        {
            MinimumSize = new Size(1, 1);
            _buttPosition.Size = Resources.DockPane_Dock.Size;
            timer = new Timer();
            timer.Interval = 100;
            timer.Tick += new EventHandler(timer_Tick);
            this.Activated += new EventHandler(AutoHideFloatWindow_Activated);
            this.Deactivate += new EventHandler(AutoHideFloatWindow_Deactivate);
            lastcs = ClientSize;
        }

        void AutoHideFloatWindow_Deactivate(object sender, EventArgs e)
        {
            if (hidden)
            {
                timer.Start();
            }
        }

        void AutoHideFloatWindow_Activated(object sender, EventArgs e)
        {
            if (hidden)
            {
                timer.Stop();
                ClientSize = lastcs;
            }
        }

        void timer_Tick(object sender, EventArgs e)
        {
            SuspendLayout();
            double newheight = 0.2 * this.ClientSize.Height;
            if (newheight < 1)
            {
                newheight = 1;
                timer.Stop();
            }
            this.ClientSize = new Size(this.ClientSize.Width, (int)newheight);
            ResumeLayout();
        }

        Rectangle _buttPosition = new Rectangle();

        [DllImport("user32.dll")]
        private static extern IntPtr GetWindowDC(IntPtr hWnd);
        [DllImport("user32.dll")]
        private static extern int GetWindowRect(IntPtr hWnd,
                                                ref Rectangle lpRect);
        [DllImport("user32.dll")]
        private static extern int ReleaseDC(IntPtr hWnd, IntPtr hDC);
        protected override void WndProc(ref Message m)
        {
            int x, y;
            Rectangle windowRect = new Rectangle();
            GetWindowRect(m.HWnd, ref windowRect);

            switch (m.Msg)
            {
                // WM_NCPAINT
                case 0x85:
                // WM_PAINT
                case 0x0A:
                    base.WndProc(ref m);

                    DrawButton(m.HWnd);

                    m.Result = IntPtr.Zero;

                    break;

                // WM_ACTIVATE
                case 0x86:
                    base.WndProc(ref m);
                    DrawButton(m.HWnd);

                    break;

                // WM_NCMOUSEMOVE
                case 0xA0:
                    // Extract the least significant 16 bits
                    x = ((int)m.LParam << 16) >> 16;
                    // Extract the most significant 16 bits
                    y = (int)m.LParam >> 16;

                    x -= windowRect.Left;
                    y -= windowRect.Top;

                    base.WndProc(ref m);

                    if (hidden)
                    {
                        if (ClientSize != lastcs)
                        {
                            ClientSize = lastcs;
                            timer.Stop();
                        }
                    }

                    bool before = ismouseover;
                    ismouseover = _buttPosition.Contains(new Point(x, y));

                    if (ismouseover != before)
                    {
                        RefreshNC(ref m);
                        DrawButton(m.HWnd);
                    }

                    break;

                // WM_NCLBUTTONDOWN
                case 0xA1:
                    // Extract the least significant 16 bits
                    x = ((int)m.LParam << 16) >> 16;
                    // Extract the most significant 16 bits
                    y = (int)m.LParam >> 16;

                    x -= windowRect.Left;
                    y -= windowRect.Top;

                    if (_buttPosition.Contains(new Point(x, y)) && DockPanel != null && DockPanel.AllowToggleAutoHide)
                    {
                        if (!hidden)
                        {
                            lastcs = ClientSize;
                        }
                        hidden = !hidden;
                        RefreshNC(ref m);
                        DrawButton(m.HWnd);
                    }
                    else
                        base.WndProc(ref m);

                    break;

                // WM_NCLBUTTONUP
                /*case 0xA2:
                    // Extract the least significant 16 bits
                    x = ((int)m.LParam << 16) >> 16;
                    // Extract the most significant 16 bits
                    y = (int)m.LParam >> 16;

                    x -= windowRect.Left;
                    y -= windowRect.Top;

                    if (_buttPosition.Contains(new Point(x, y)) &&
                        _buttState == ButtonState.Pushed)
                    {
                        _buttState = ButtonState.Normal;
                        // [[TODO]]: Fire a click event for your button 
                        //           however you want to do it.
                        DrawButton(m.HWnd);
                    }
                    else
                        base.WndProc(ref m);

                    break;
                    */
                // WM_NCHITTEST
                case 0x84:
                    // Extract the least significant 16 bits
                    x = ((int)m.LParam << 16) >> 16;
                    // Extract the most significant 16 bits
                    y = (int)m.LParam >> 16;

                    x -= windowRect.Left;
                    y -= windowRect.Top;

                    if (_buttPosition.Contains(new Point(x, y)))
                        m.Result = (IntPtr)18; // HTBORDER
                    else
                        base.WndProc(ref m);

                    break;
                case 0x02A0://WM_NCMOUSEHOVER
                    RefreshNC(ref m);
                    break;
                case 0x02A2://WM_NCMOUSELEAVE
                    RefreshNC(ref m);
                    break;
                case 0x0005://WM_SIZE
                    RefreshNC(ref m);
                    break;
                default:
                    base.WndProc(ref m);
                    break;
            }
        }

        private void RefreshNC(ref Message m)
        {
            Message tempm = new Message();
            tempm.HWnd = m.HWnd;
            tempm.Msg = 0x85;
            WndProc(ref tempm);
        }

        private void DrawButton(IntPtr hwnd)
        {
            IntPtr hDC = GetWindowDC(hwnd);
            int x, y;

            using (Graphics g = Graphics.FromHdc(hDC))
            {
                // Work out size and positioning
                int CaptionHeight = SystemInformation.CaptionHeight;
                Size ButtonSize = SystemInformation.CaptionButtonSize;
                x = Bounds.Width - ButtonSize.Width - SystemInformation.FrameBorderSize.Width * 2 - 5;
                y = (CaptionHeight - _buttPosition.Height) / 2;
                _buttPosition.Location = new Point(x, y);


                // Draw our "button"
                Bitmap Image = hidden ? Resources.DockPane_AutoHide : Resources.DockPane_Dock;

                if (ismouseover)
                {
                    using (Pen pen = new Pen(ForeColor))
                    {
                        g.DrawRectangle(pen, Rectangle.Inflate(new Rectangle(x, y, Image.Size.Width, Image.Size.Height), -1, -1));
                    }
                }

                using (ImageAttributes imageAttributes = new ImageAttributes())
                {
                    ColorMap[] colorMap = new ColorMap[2];
                    colorMap[0] = new ColorMap();
                    colorMap[0].OldColor = Color.FromArgb(0, 0, 0);
                    colorMap[0].NewColor = ForeColor;
                    colorMap[1] = new ColorMap();
                    colorMap[1].OldColor = Image.GetPixel(0, 0);
                    colorMap[1].NewColor = Color.Transparent;

                    imageAttributes.SetRemapTable(colorMap);

                    g.DrawImage(
                       Image,
                       new Rectangle(x, y, Image.Width, Image.Height),
                       0, 0,
                       Image.Width,
                       Image.Height,
                       GraphicsUnit.Pixel,
                       imageAttributes);
                }
            }

            ReleaseDC(hwnd, hDC);
        }
    }
}
