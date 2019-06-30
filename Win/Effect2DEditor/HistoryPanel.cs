using Effect2DEditor.Command;
using System;
using System.Drawing;
using System.Windows.Forms;

namespace Effect2DEditor
{
    public partial class HistoryPanel : CustomUserControl
    {
        const int ItemHeight = 20;
        const int CheckBoxOffset = 3;
        bool ignorescroll;

        public CommandManager CommandManager
        {
            get;
            private set;
        }

        public HistoryPanel()
        {
            CommandManager = new CommandManager();
            InitializeComponent();
            InitializeBuffer();
            vScrollBar1.Maximum = 0;
            vScrollBar1.Minimum = 0;
            vScrollBar1.SmallChange = 1;
            vScrollBar1.LargeChange = 1;
            vScrollBar1.Scroll += vScrollBar1_Scroll;
            this.MouseWheel += HistoryPanel_MouseWheel;
            this.SizeChanged += HistoryPanel_SizeChanged;
            DrawAndRefresh();
        }

        void HistoryPanel_SizeChanged(object sender, EventArgs e)
        {
            if (CommandManager == null) return;
            ignorescroll = true;
            AdjustVScrollBar();
            ignorescroll = false;
            DrawAndRefresh();
        }

        void HistoryPanel_MouseWheel(object sender, MouseEventArgs e)
        {
            int num = vScrollBar1.Value;
            if (e.Delta >= 0) num--;
            else num++;
            if (num >= vScrollBar1.Minimum && num <= vScrollBar1.Maximum)
            {
                vScrollBar1.Value = num;
                DrawAndRefresh();
            }
        }

        void vScrollBar1_Scroll(object sender, ScrollEventArgs e)
        {
            DrawAndRefresh();
        }

        public void SetEvent()
        {
            CommandManager.CommandChanged += CommandManager_CommandChanged;
        }

        void CommandManager_CommandChanged(object sender, EventArgs e)
        {
            AdjustVScrollBar();
            DrawAndRefresh();
        }
        private void AdjustVScrollBar()
        {
            int maxvalue = MaxItemNumber - ShownItemNumber;
            if (maxvalue < 0) maxvalue = 0;
            vScrollBar1.Maximum = maxvalue;
            if (!ignorescroll) vScrollBar1.Value = maxvalue;
        }
        private int MaxItemNumber
        {
            get
            {
                return CommandManager.DoneCommandNumber + CommandManager.UndoneCommandNumber;
            }
        }
        private int ShownItemNumber
        {
            get
            {
                return this.Height / ItemHeight;
            }
        }
        private int ContentWidth
        {
            get
            {
                return this.Width - vScrollBar1.Width;
            }
        }
        protected override void DrawToBuffer(Graphics g)
        {
            g.Clear(SystemColors.Control);
            if (CommandManager == null) return;
            int StartIndex = vScrollBar1.Value, EndIndex = vScrollBar1.Value + ShownItemNumber + 1;
            int iter = MaxItemNumber - 1;
            foreach (CommandBase com in CommandManager.UndoneCommand)
            {
                int inneriter = CommandManager.DoneCommandNumber + (MaxItemNumber - 1 - iter);
                if (StartIndex <= inneriter && inneriter <= EndIndex)
                {
                    DrawItem(ItemHeight * (inneriter - StartIndex), com, g, false);
                }
                iter--;
            }
            foreach (CommandBase com in CommandManager.DoneCommand)
            {
                if (StartIndex <= iter && iter <= EndIndex)
                {
                    DrawItem(ItemHeight * (iter - StartIndex), com, g, true);
                }
                iter--;
            }
        }
        private void DrawItem(int y, CommandBase command, Graphics g, bool IsChecked)
        {
            var rec = new Rectangle(0, y, ContentWidth - 1, ItemHeight);
            if (IsChecked) g.FillRectangle(Brushes.White, rec);
            g.DrawRectangle(Pens.Black, rec);
            g.DrawRectangle(Pens.Black, new Rectangle(CheckBoxOffset, y + CheckBoxOffset, ItemHeight - CheckBoxOffset * 2, ItemHeight - CheckBoxOffset * 2));
            var sf = new StringFormat
            {
                Trimming = StringTrimming.EllipsisCharacter
            };
            g.DrawString(command.Explaination, Font, IsChecked ? Brushes.Black : Brushes.Gray, new RectangleF(ItemHeight, y + (ItemHeight - Font.Height) / 2 + 1, ContentWidth - ItemHeight - 1, Font.Height), sf);
            if (IsChecked)
            {
                g.DrawImage(Effect2DEditor.Properties.Resources.check, 5, y);
            }
        }
        private int ItemIndex(int y)
        {
            return vScrollBar1.Value + y / ItemHeight;
        }
        private void HistoryPanel_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.X >= CheckBoxOffset && e.X <= ItemHeight - CheckBoxOffset)
            {
                var itemindex = ItemIndex(e.Y);
                int shownyindex = itemindex - vScrollBar1.Value;
                if (itemindex < MaxItemNumber && e.Y >= shownyindex * ItemHeight + CheckBoxOffset && e.Y <= (shownyindex + 1) * ItemHeight - CheckBoxOffset)
                {
                    if (itemindex < CommandManager.DoneCommandNumber)
                    {
                        int undonum = CommandManager.DoneCommandNumber - itemindex;
                        ignorescroll = true;
                        for (int i = 0; i < undonum; i++)
                        {
                            CommandManager.Undo();
                        }
                        ignorescroll = false;
                    }
                    else
                    {
                        int redonum = itemindex - CommandManager.DoneCommandNumber + 1;
                        ignorescroll = true;
                        for (int i = 0; i < redonum; i++)
                        {
                            CommandManager.Redo();
                        }
                        ignorescroll = false;
                    }
                }
            }
        }
    }
}
