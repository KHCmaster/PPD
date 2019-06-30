using FlowScriptDrawControl.Model;
using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace FlowScriptDrawControl.Control
{
    /// <summary>
    /// BoundCommentControl.xaml の相互作用ロジック
    /// </summary>
    public partial class BoundCommentControl : UserControl
    {
        private SourceControl source;
        private TextEditManager textEditManager;

        public Comment CurrentComment
        {
            get
            {
                return (Comment)DataContext;
            }
        }

        public SourceControl Source
        {
            get
            {
                return source;
            }
            set
            {
                if (source != value)
                {
                    if (source != null)
                    {
                        source.CurrentPositionable.PropertyChanged -= CurrentPositionable_PropertyChanged;
                        source.CurrentPositionable.SizeChanged -= CurrentPositionable_SizeChanged;
                    }
                    source = value;
                    if (source != null)
                    {
                        source.CurrentPositionable.PropertyChanged += CurrentPositionable_PropertyChanged;
                        source.CurrentPositionable.SizeChanged += CurrentPositionable_SizeChanged;
                    }
                }
            }
        }

        public BoundCommentControl()
        {
            InitializeComponent();

            textBox.TextChanged += textBox_TextChanged;
            Loaded += BoundCommentControl_Loaded;
        }

        private void UpdateFrame()
        {
            frame.Points.Clear();
            double maxWidth = textBox.MinWidth;
            double height = 0;
            foreach (string line in textBlock.Text.Split('\n'))
            {
                var size = Utility.MeasureSize(line + " ", FontFamily, FontSize);
                maxWidth = Math.Max(maxWidth, size.Width);
                height += size.Height;
            }
            frame.Points.Add(new Point(0, 0));
            frame.Points.Add(new Point(maxWidth + mainGrid.Margin.Left * 2, 0));
            frame.Points.Add(new Point(maxWidth + mainGrid.Margin.Left * 2, height + mainGrid.Margin.Left * 2));
            frame.Points.Add(new Point((maxWidth + mainGrid.Margin.Left * 2) / 2 + 2, height + mainGrid.Margin.Left * 2));
            frame.Points.Add(new Point((maxWidth + mainGrid.Margin.Left * 2) / 2, height + mainGrid.Margin.Left * 2 + 4));
            frame.Points.Add(new Point((maxWidth + mainGrid.Margin.Left * 2) / 2 - 2, height + mainGrid.Margin.Left * 2));
            frame.Points.Add(new Point(0, height + mainGrid.Margin.Left * 2));
        }

        void textBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            UpdateFrame();
            UpdateLayout();
            UpdatePosition();
        }

        public void UpdatePosition()
        {
            if (source == null)
            {
                return;
            }

            this.RenderTransform = new TranslateTransform(
                source.CurrentSource.Position.X + source.ActualWidth / 2 - this.ActualWidth / 2,
                source.CurrentSource.Position.Y - this.ActualHeight);
        }

        void CurrentPositionable_SizeChanged()
        {
            UpdatePosition();
        }

        void CurrentPositionable_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "Transform")
            {
                UpdatePosition();
            }
        }

        void BoundCommentControl_Loaded(object sender, RoutedEventArgs e)
        {
            textEditManager = new TextEditManager(this, textBlock, textBox);
        }
    }
}