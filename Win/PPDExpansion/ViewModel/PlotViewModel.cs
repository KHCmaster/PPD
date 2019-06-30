using GalaSoft.MvvmLight.Command;
using PPDExpansionCore;
using System;
using System.Windows;
using System.Windows.Controls.DataVisualization.Charting.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;

namespace PPDExpansion.ViewModel
{
    abstract class PlotViewModel : PlayViewModel
    {
        protected readonly Dispatcher dispatcher;
        protected readonly DateTime baseTime = new DateTime(2000, 1, 1);

        protected ScoreInfo currentScoreInfo;

        private DateTime xMinimum;
        private DateTime xMaximum;
        private int? yMinimum;
        private int? yMaximum;
        private bool isPanning;
        private Point panStartPoint;
        private long panStartX;
        private int panStartY;
        private long panStartWidth;
        private int panStartHeight;

        private ICommand zoomCommand;
        private ICommand resetZoomCommand;
        private ICommand panStartCommand;
        private ICommand panCommand;

        public DateTime XMinimum
        {
            get { return xMinimum; }
            set
            {
                if (xMinimum != value)
                {
                    xMinimum = value;
                    RaisePropertyChanged("XMinimum");
                }
            }
        }

        public DateTime XMaximum
        {
            get { return xMaximum; }
            set
            {
                if (xMaximum != value)
                {
                    xMaximum = value;
                    RaisePropertyChanged("XMaximum");
                }
            }
        }

        public int? YMinimum
        {
            get { return yMinimum; }
            set
            {
                if (yMinimum != value)
                {
                    yMinimum = value;
                    RaisePropertyChanged("YMinimum");
                }
            }
        }

        public int? YMaximum
        {
            get { return yMaximum; }
            set
            {
                if (yMaximum != value)
                {
                    yMaximum = value;
                    RaisePropertyChanged("YMaximum");
                }
            }
        }

        public abstract int ActualYMaximum { get; }

        public ICommand ZoomCommand
        {
            get
            {
                return zoomCommand ?? (zoomCommand = new RelayCommand<MouseWheelEventArgs>(
                    ZoomCommand_Execute));
            }
        }

        public ICommand ResetZoomCommand
        {
            get
            {
                return resetZoomCommand ?? (resetZoomCommand = new RelayCommand<MouseButtonEventArgs>(
                    ResetZoomCommand_Execute));
            }
        }

        public ICommand PanStartCommand
        {
            get
            {
                return panStartCommand ?? (panStartCommand = new RelayCommand<MouseButtonEventArgs>(
                    PanStartCommand_Execute));
            }
        }

        public ICommand PanCommand
        {
            get
            {
                return panCommand ?? (panCommand = new RelayCommand<MouseEventArgs>(
                    PanCommand_Execute));
            }
        }

        protected PlotViewModel(MainWindowViewModel mainWindowViewModel)
            : base(mainWindowViewModel)
        {
            this.dispatcher = Dispatcher.CurrentDispatcher;
        }


        private void ZoomCommand_Execute(MouseWheelEventArgs e)
        {
            if (!IsPlotArea((DependencyObject)e.OriginalSource, out EdgePanel edgePanel))
            {
                return;
            }
            if (YMaximum == null)
            {
                YMaximum = ActualYMaximum;
            }
            var pos = e.GetPosition(edgePanel);
            var ratio = new Point(pos.X / edgePanel.ActualWidth, 1 - pos.Y / edgePanel.ActualHeight);
            int h = YMaximum.Value - YMinimum.Value;
            long w = XMaximum.ToBinary() - XMinimum.ToBinary();
            int centerY = YMinimum.Value + (int)(h * ratio.Y);
            long centerX = XMinimum.ToBinary() + (long)(w * ratio.X);
            if (Math.Sign(e.Delta) > 0)
            {
                XMinimum = ClipDate(DateTime.FromBinary(centerX - w / 4));
                XMaximum = ClipDate(DateTime.FromBinary(centerX + w / 4));
                YMinimum = ClipScore(centerY - h / 4);
                YMaximum = ClipScore(centerY + h / 4);
            }
            else
            {
                if (w == 0)
                {
                    w = 1;
                }
                XMinimum = ClipDate(DateTime.FromBinary(centerX - w));
                XMaximum = ClipDate(DateTime.FromBinary(centerX + w));
                if (h == 0)
                {
                    h = 1;
                }
                YMinimum = ClipScore(centerY - h);
                YMaximum = ClipScore(centerY + h);
            }
        }

        private DateTime ClipDate(DateTime date)
        {
            if (date < baseTime)
            {
                return baseTime;
            }
            if (date > baseTime.AddSeconds(currentScoreInfo.Length + 5))
            {
                return baseTime.AddSeconds(currentScoreInfo.Length + 5);
            }
            return date;
        }

        private int ClipScore(int score)
        {
            if (score < 0)
            {
                return 0;
            }
            if (score > ActualYMaximum * 1.1f)
            {
                return (int)(ActualYMaximum * 1.1f);
            }
            return score;
        }

        protected void ResetZoom()
        {
            XMinimum = baseTime;
            XMaximum = baseTime.AddSeconds(currentScoreInfo.Length + 5);
            YMinimum = 0;
            if (ActualYMaximum == 0)
            {
                YMaximum = 0;
            }
            else
            {
                YMaximum = (int)(ActualYMaximum * 1.1f);
            }
        }

        private void ResetZoomCommand_Execute(MouseButtonEventArgs e)
        {
            if (!IsPlotArea((DependencyObject)e.OriginalSource, out EdgePanel edgePanel))
            {
                return;
            }
            if (e.ChangedButton == MouseButton.Right && e.ButtonState == MouseButtonState.Released)
            {
                ResetZoom();
            }
            isPanning &= (e.ChangedButton != MouseButton.Left || e.ButtonState != MouseButtonState.Released);
        }

        private void PanStartCommand_Execute(MouseButtonEventArgs e)
        {
            if (!IsPlotArea((DependencyObject)e.OriginalSource, out EdgePanel edgePanel))
            {
                return;
            }
            if (e.ChangedButton == MouseButton.Left && e.ButtonState == MouseButtonState.Pressed)
            {
                panStartPoint = e.GetPosition(edgePanel);
                if (YMaximum == null)
                {
                    YMaximum = ActualYMaximum;
                }
                panStartX = XMinimum.ToBinary();
                panStartY = YMinimum.Value;
                panStartHeight = YMaximum.Value - YMinimum.Value;
                panStartWidth = XMaximum.ToBinary() - XMinimum.ToBinary();
                isPanning = true;
            }
        }

        private void PanCommand_Execute(MouseEventArgs e)
        {
            if (!IsPlotArea((DependencyObject)e.OriginalSource, out EdgePanel edgePanel))
            {
                return;
            }
            if (!isPanning)
            {
                return;
            }

            var pos = e.GetPosition(edgePanel);
            if (YMaximum == null)
            {
                YMaximum = ActualYMaximum;
            }
            XMinimum = DateTime.FromBinary(panStartX + (long)(panStartWidth * (-pos.X + panStartPoint.X) / edgePanel.ActualWidth));
            YMinimum = panStartY + (int)(panStartHeight * (pos.Y - panStartPoint.Y) / edgePanel.ActualHeight);
            XMaximum = DateTime.FromBinary(xMinimum.ToBinary() + panStartWidth);
            YMaximum = YMinimum + panStartHeight;
        }

        private bool IsPlotArea(DependencyObject dependencyObject, out EdgePanel edgePanel)
        {
            edgePanel = null;
            while (dependencyObject != null)
            {
                if (dependencyObject.GetType() == typeof(EdgePanel))
                {
                    edgePanel = (EdgePanel)dependencyObject;
                    return true;
                }
                dependencyObject = VisualTreeHelper.GetParent(dependencyObject);
            }
            return false;
        }
    }
}
