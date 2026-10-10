using Effect2DEditor.Command;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using CommandManager = Effect2DEditor.Command.CommandManager;

namespace Effect2DEditor
{
    /// <summary>
    /// 編集の履歴です。チェックボックスを押すと、そこまで元に戻す・やり直します。
    /// </summary>
    public class HistoryPanel : UserControl
    {
        const int ItemHeight = 20;
        const int CheckBoxOffset = 3;
        static readonly ImageSource checkImage = Utility.LoadImage("check.png");

        readonly DrawingSurface surface;
        readonly ScrollBar vScrollBar1;
        bool ignorescroll;

        CommandManager commandManager;

        /// <summary>
        /// 表示する編集の履歴です。
        /// </summary>
        public CommandManager CommandManager
        {
            get { return commandManager; }
            set
            {
                if (commandManager != null)
                {
                    commandManager.CommandChanged -= CommandManager_CommandChanged;
                }
                commandManager = value;
                if (commandManager != null)
                {
                    commandManager.CommandChanged += CommandManager_CommandChanged;
                }
                CommandManager_CommandChanged(this, EventArgs.Empty);
            }
        }

        public HistoryPanel()
        {
            surface = new DrawingSurface { Render = Draw };
            vScrollBar1 = new ScrollBar { Orientation = Orientation.Vertical, Minimum = 0, Maximum = 0, SmallChange = 1, LargeChange = 1 };
            var grid = new Grid();
            grid.ColumnDefinitions.Add(new ColumnDefinition());
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            grid.Children.Add(surface);
            Grid.SetColumn(vScrollBar1, 1);
            grid.Children.Add(vScrollBar1);
            Content = grid;

            vScrollBar1.Scroll += VScrollBar1_Scroll;
            surface.MouseWheel += HistoryPanel_MouseWheel;
            surface.SizeChanged += HistoryPanel_SizeChanged;
            surface.MouseLeftButtonDown += HistoryPanel_MouseDown;
        }

        private int VValue
        {
            get { return (int)vScrollBar1.Value; }
        }

        void HistoryPanel_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (CommandManager == null) return;
            ignorescroll = true;
            AdjustVScrollBar();
            ignorescroll = false;
            surface.DrawAndRefresh();
        }

        void HistoryPanel_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            int num = VValue;
            if (e.Delta >= 0) num--;
            else num++;
            if (num >= vScrollBar1.Minimum && num <= vScrollBar1.Maximum)
            {
                vScrollBar1.Value = num;
                surface.DrawAndRefresh();
            }
        }

        void VScrollBar1_Scroll(object sender, ScrollEventArgs e)
        {
            vScrollBar1.Value = Math.Round(vScrollBar1.Value);
            surface.DrawAndRefresh();
        }

        void CommandManager_CommandChanged(object sender, EventArgs e)
        {
            if (CommandManager == null) return;
            AdjustVScrollBar();
            surface.DrawAndRefresh();
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
                return (int)surface.ActualHeight / ItemHeight;
            }
        }
        private double ContentWidth
        {
            get
            {
                return surface.ActualWidth;
            }
        }
        private void Draw(DrawingContext g)
        {
            if (CommandManager == null) return;
            int StartIndex = VValue, EndIndex = VValue + ShownItemNumber + 1;
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
        private void DrawItem(int y, CommandBase command, DrawingContext g, bool IsChecked)
        {
            if (IsChecked) g.DrawRectangle(Brushes.White, null, new Rect(0, y, ContentWidth - 1, ItemHeight));
            DrawingSurface.DrawFrame(g, DrawingSurface.BlackPen, 0, y, ContentWidth - 1, ItemHeight);
            DrawingSurface.DrawFrame(g, DrawingSurface.BlackPen, CheckBoxOffset, y + CheckBoxOffset, ItemHeight - CheckBoxOffset * 2, ItemHeight - CheckBoxOffset * 2);
            surface.DrawText(g, command.Explaination, IsChecked ? Brushes.Black : Brushes.Gray, ItemHeight, y + (ItemHeight - DrawingSurface.TextHeight) / 2 + 1, ContentWidth - ItemHeight - 1);
            if (IsChecked)
            {
                g.DrawImage(checkImage, new Rect(5, y, 16, 16));
            }
        }
        private int ItemIndex(double y)
        {
            return VValue + (int)(y / ItemHeight);
        }
        private void HistoryPanel_MouseDown(object sender, MouseButtonEventArgs e)
        {
            var pos = e.GetPosition(surface);
            if (pos.X >= CheckBoxOffset && pos.X <= ItemHeight - CheckBoxOffset)
            {
                var itemindex = ItemIndex(pos.Y);
                int shownyindex = itemindex - VValue;
                if (itemindex < MaxItemNumber && pos.Y >= shownyindex * ItemHeight + CheckBoxOffset && pos.Y <= (shownyindex + 1) * ItemHeight - CheckBoxOffset)
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
