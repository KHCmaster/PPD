using Effect2DEditor.ViewModels.Dialogs;
using System.Windows;

namespace Effect2DEditor
{
    /// <summary>
    /// アニメーションのフレーム数を聞くウィンドウです。処理は <see cref="AskFrameCountViewModel"/> にあります。
    /// </summary>
    public partial class AskFrameCountWindow : Window
    {
        public AskFrameCountWindow()
        {
            InitializeComponent();
        }
    }
}
