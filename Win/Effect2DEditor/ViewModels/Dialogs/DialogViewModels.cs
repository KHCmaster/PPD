using BezierCaliculator;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Effect2DEditor.Models;
using System;
using Color = System.Drawing.Color;

namespace Effect2DEditor.ViewModels.Dialogs
{
    /// <summary>
    /// OK とキャンセルで閉じるダイアログの ViewModel の基底クラスです。キャンセルはボタンの IsCancel で閉じる。
    /// </summary>
    public abstract partial class DialogViewModel : ObservableObject
    {
        /// <summary>
        /// OK で閉じるときに起きます。
        /// </summary>
        public event Action CloseRequested;

        public LanguageTexts Texts
        {
            get { return LanguageTexts.Instance; }
        }

        [RelayCommand]
        private void Ok()
        {
            OnOk();
            CloseRequested?.Invoke();
        }

        /// <summary>
        /// OK で閉じる前に、結果を決めます。
        /// </summary>
        protected virtual void OnOk()
        {
        }
    }

    /// <summary>
    /// アニメーションを作るときの、1 枚の画像を表示するフレーム数を聞きます。
    /// </summary>
    public partial class AskFrameCountViewModel : DialogViewModel
    {
        [ObservableProperty]
        private string frameCountText = "";

        [ObservableProperty]
        private bool withReverse;

        /// <summary>
        /// フレーム数（数でないときや 1 より小さいときは 1）。
        /// </summary>
        public int FrameCount
        {
            get { return int.TryParse(FrameCountText, out int val) && val >= 1 ? val : 1; }
        }
    }

    /// <summary>
    /// カンバスの大きさを変えます。
    /// </summary>
    public partial class CanvasSizeViewModel(int width, int height) : DialogViewModel
    {
        [ObservableProperty]
        private string widthText = width.ToString();

        [ObservableProperty]
        private string heightText = height.ToString();

        /// <summary>
        /// 幅（数でないときは 256、1 より小さいときは 1）。
        /// </summary>
        public int CanvasWidth
        {
            get { return Math.Max(1, int.TryParse(WidthText, out int val) ? val : 256); }
        }

        public int CanvasHeight
        {
            get { return Math.Max(1, int.TryParse(HeightText, out int val) ? val : 256); }
        }
    }

    /// <summary>
    /// ダイアログの ViewModel から使う、色やファイルを選ぶ画面です。
    /// </summary>
    public interface IPickerView
    {
        /// <summary>
        /// 色を選ばせます。
        /// </summary>
        /// <returns>キャンセルしたときは null。</returns>
        Color? PickColor(Color current);

        /// <summary>
        /// 開くファイルを選ばせます。
        /// </summary>
        /// <returns>キャンセルしたときは null。</returns>
        string OpenFile();
    }

    /// <summary>
    /// カンバスの背景（市松模様、色、画像）を変えます。
    /// </summary>
    public partial class ChangeBackGroundViewModel(BackGroundMode mode, Color color, string fileName) : DialogViewModel
    {
        [ObservableProperty]
        private BackGroundMode mode = mode;

        [ObservableProperty]
        private Color color = color;

        [ObservableProperty]
        private string fileName = fileName;

        public IPickerView View { get; set; }

        [RelayCommand]
        private void PickColor()
        {
            var picked = View?.PickColor(Color);
            if (picked.HasValue)
            {
                Color = picked.Value;
                Mode = BackGroundMode.Color;
            }
        }

        [RelayCommand]
        private void PickImage()
        {
            var path = View?.OpenFile();
            if (path != null)
            {
                FileName = path;
                Mode = BackGroundMode.Image;
            }
        }
    }

    /// <summary>
    /// ダイアログの ViewModel から使う、変化の割合を描くところです。
    /// </summary>
    public interface IRatioEditor
    {
        /// <summary>
        /// 今の変化の割合（ベジエの 2 つの点）です。
        /// </summary>
        BezierControlPoint[] Bcps { get; set; }

        bool IsLinear { get; }

        void SetLinear();
    }

    /// <summary>
    /// ステートの値の変化の割合（線形かベジエ）を変えます。
    /// </summary>
    public partial class RatioChangeViewModel(BezierControlPoint[] bcps) : DialogViewModel
    {
        /// <summary>
        /// コピーした変化の割合です（エディターを閉じるまで、ダイアログをまたいで使う）。
        /// </summary>
        static BezierControlPoint[] copiedBezier;

        /// <summary>
        /// 最初の変化の割合（ないときは null）。OK で閉じたときは結果。
        /// </summary>
        public BezierControlPoint[] Bcps { get; private set; } = bcps;

        public bool IsLinear { get; private set; }

        public IRatioEditor Editor { get; set; }

        [RelayCommand]
        private void SetLinear()
        {
            Editor.SetLinear();
        }

        [RelayCommand]
        private void Copy()
        {
            copiedBezier = Clone(Editor.Bcps);
        }

        [RelayCommand]
        private void Paste()
        {
            if (copiedBezier != null)
            {
                Editor.Bcps = Clone(copiedBezier);
            }
        }

        protected override void OnOk()
        {
            Bcps = Editor.Bcps;
            IsLinear = Editor.IsLinear;
        }

        private static BezierControlPoint[] Clone(BezierControlPoint[] source)
        {
            var bcps = new BezierControlPoint[2];
            for (int i = 0; i < bcps.Length; i++)
            {
                bcps[i] = source[i].Clone();
            }
            return bcps;
        }
    }
}
