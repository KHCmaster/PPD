using FlowScriptDrawControl.Model;
using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace FlowScriptDrawControl.Control
{
    /// <summary>
    /// ErrorCommentControl.xaml の相互作用ロジック
    /// </summary>
    public partial class ErrorCommentControl : UserControl
    {
        private SourceControl source;

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
                    UpdatePosition();
                }
            }
        }

        public ErrorCommentControl()
        {
            InitializeComponent();
            Loaded += ErrorCommentControl_Loaded;
        }

        void ErrorCommentControl_Loaded(object sender, RoutedEventArgs e)
        {
            DataContextChanged += ErrorCommentControl_DataContextChanged;
            UpdateFrame();
            UpdateLayout();
            UpdatePosition();
        }

        void ErrorCommentControl_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (e.OldValue != null)
            {
                var comment = (Comment)e.NewValue;
                comment.PropertyChanged -= comment_PropertyChanged;
            }
            if (e.NewValue != null)
            {
                var comment = (Comment)e.NewValue;
                comment.PropertyChanged += comment_PropertyChanged;
            }
            UpdateFrame();
            UpdateLayout();
            UpdatePosition();
        }

        void comment_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "Text")
            {
                UpdateFrame();
                UpdateLayout();
                UpdatePosition();
            }
        }

        private void UpdateFrame()
        {
            Console.WriteLine(CurrentComment.Text);
            frame.Points.Clear();
            double maxWidth = mainGrid.ActualWidth;
            double height = mainGrid.ActualHeight;
            frame.Points.Add(new Point(0, 0));
            frame.Points.Add(new Point(maxWidth + mainGrid.Margin.Left * 2, 0));
            frame.Points.Add(new Point(maxWidth + mainGrid.Margin.Left * 2, height + mainGrid.Margin.Left * 2));
            frame.Points.Add(new Point((maxWidth + mainGrid.Margin.Left * 2) / 2 + 2, height + mainGrid.Margin.Left * 2));
            frame.Points.Add(new Point((maxWidth + mainGrid.Margin.Left * 2) / 2, height + mainGrid.Margin.Left * 2 + 4));
            frame.Points.Add(new Point((maxWidth + mainGrid.Margin.Left * 2) / 2 - 2, height + mainGrid.Margin.Left * 2));
            frame.Points.Add(new Point(0, height + mainGrid.Margin.Left * 2));
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

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            var canvas = Utility.GetParent<Canvas>(this);
            if (canvas != null)
            {
                canvas.Children.Remove(this);
            }
        }

        private void CopyButton_Click(object sender, RoutedEventArgs e)
        {
            Clipboard.SetText(CurrentComment.Text);
        }
    }
}
