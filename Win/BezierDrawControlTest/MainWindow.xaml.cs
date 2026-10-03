using BezierDrawControlTest.ViewModels;
using System;
using System.Windows;

namespace BezierDrawControlTest
{
    /// <summary>
    /// ベジエ曲線の編集のコントロールを試すウィンドウです。処理は <see cref="MainWindowViewModel"/> にあります。
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            var viewModel = new MainWindowViewModel();
            DataContext = viewModel;
            bezierControl1.Initialize(viewModel.CanvasWidth, viewModel.CanvasHeight);
            viewModel.Attach(bezierControl1.Controller);
            bezierControl1.DrawAndRefresh();
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            bezierControl1.Dispose();
        }
    }
}
