using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using Color = System.Drawing.Color;

namespace PPDEditor.ViewModels.Dialogs
{
    /// <summary>
    /// グリッド（幅、高さ、オフセット、色）を設定するダイアログです。
    /// </summary>
    public partial class GridSettingViewModel(SquareGrid grid) : DialogViewModel
    {
        [ObservableProperty]
        private string width = grid.Width.ToString();

        [ObservableProperty]
        private string height = grid.Height.ToString();

        [ObservableProperty]
        private string offsetX = grid.OffsetX.ToString();

        [ObservableProperty]
        private string offsetY = grid.OffsetY.ToString();

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(ColorText), nameof(IsDarkColor))]
        private Color gridColor = grid.GridColor;

        /// <summary>
        /// 色を選ばせます（今の色を渡し、キャンセルしたときは null を返す）。
        /// </summary>
        public Func<Color, Color?> PickColor
        {
            get;
            set;
        }

        /// <summary>
        /// 色の表示（#RRGGBB）です。
        /// </summary>
        public string ColorText
        {
            get { return "#" + GridColor.R.ToString("X2") + GridColor.G.ToString("X2") + GridColor.B.ToString("X2"); }
        }

        /// <summary>
        /// 暗い色かです（色の上の文字を白にする）。
        /// </summary>
        public bool IsDarkColor
        {
            get { return GridColor.GetBrightness() <= 0.5f; }
        }

        /// <summary>
        /// 入力したグリッドです。範囲外や数字でない値は既定の値にします。
        /// </summary>
        public SquareGrid Grid
        {
            get
            {
                return new SquareGrid
                {
                    Width = Parse(Width, 5, x => x < 1 || x > 800 ? 5 : x),
                    Height = Parse(Height, 5, x => x < 1 || x > 400 ? 5 : x),
                    OffsetX = Parse(OffsetX, 0, x => x),
                    OffsetY = Parse(OffsetY, 0, x => x),
                    GridColor = GridColor
                };
            }
        }

        [RelayCommand]
        private void ChooseColor()
        {
            var color = PickColor?.Invoke(GridColor);
            if (color.HasValue)
            {
                GridColor = color.Value;
            }
        }

        private static int Parse(string text, int errorValue, Func<int, int> func)
        {
            return func(int.TryParse(text, out int value) ? value : errorValue);
        }
    }
}
