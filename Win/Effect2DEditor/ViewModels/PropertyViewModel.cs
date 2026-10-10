using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Effect2D;
using System;

namespace Effect2DEditor.ViewModels
{
    /// <summary>
    /// プロパティウィンドウの ViewModel から使う、エディターの機能です。
    /// </summary>
    public interface IPropertyHost
    {
        /// <summary>
        /// 選んでいるステートの値を変えます。
        /// </summary>
        void ChangeStateValue(RatioType type, float value);

        void ChangeBlendMode(BlendMode blendMode);

        /// <summary>
        /// 値の変化の割合を編集します。
        /// </summary>
        void EditRatio(RatioType type);
    }

    /// <summary>
    /// プロパティウィンドウの 1 つの値（X、Y、透明度など）です。
    /// </summary>
    public partial class StateValueRow(PropertyViewModel owner, RatioType type, string label, float divisor, float? min, float? max) : ObservableObject
    {
        public RatioType Type { get; } = type;

        public string Label { get; } = label;

        /// <summary>
        /// 名前をドラッグしたときに、動かした量を割る数です（1 ピクセルあたりの変化が 1 / divisor になる）。
        /// </summary>
        public float Divisor { get; } = divisor;

        [ObservableProperty]
        private string text = "";

        [ObservableProperty]
        private bool isTextEnabled = true;

        /// <summary>
        /// 変化の割合のボタンを使えるかどうか。
        /// </summary>
        [ObservableProperty]
        private bool isRatioEnabled;

        partial void OnTextChanged(string value)
        {
            if (owner.IgnoreInput || !float.TryParse(value, out float val))
            {
                return;
            }
            // 範囲の外の値は直して、入力欄にも反映する
            var clamped = val;
            if (min.HasValue && clamped < min.Value) clamped = min.Value;
            if (max.HasValue && clamped > max.Value) clamped = max.Value;
            if (clamped != val)
            {
                owner.IgnoreInput = true;
                Text = clamped.ToString();
                owner.IgnoreInput = false;
            }
            owner.Host.ChangeStateValue(Type, clamped);
        }

        /// <summary>
        /// 名前のドラッグで、値を増やしたり減らしたりします。
        /// </summary>
        public void AddValue(int pixels)
        {
            if (float.TryParse(Text, out float val))
            {
                Text = (val + pixels / Divisor).ToString();
            }
        }
    }

    /// <summary>
    /// 選んでいるステートの値（ブレンド、位置、透明度、回転、大きさ）と、変化の割合の編集です。
    /// </summary>
    public partial class PropertyViewModel : ObservableObject
    {
        public PropertyViewModel(IPropertyHost host)
        {
            Host = host;
            Rows =
            [
                new(this, RatioType.X, "X", 1, null, null),
                new(this, RatioType.Y, "Y", 1, null, null),
                new(this, RatioType.Alpha, "Alpha", 100, 0, 1),
                new(this, RatioType.Rotation, "Rotation", 1, null, null),
                new(this, RatioType.ScaleX, "ScaleX", 100, 0, null),
                new(this, RatioType.ScaleY, "ScaleY", 100, 0, null),
            ];
        }

        internal IPropertyHost Host { get; }

        /// <summary>
        /// ステートを表示している間は、値の変更をエディターに伝えない。
        /// </summary>
        internal bool IgnoreInput { get; set; }

        public BlendMode[] BlendModes { get; } = Enum.GetValues<BlendMode>();

        public StateValueRow[] Rows { get; }

        [ObservableProperty]
        private BlendMode? selectedBlendMode;

        /// <summary>
        /// ベジエの位置の変化の割合のボタンを使えるかどうか。
        /// </summary>
        [ObservableProperty]
        private bool isBezierPositionEnabled;

        /// <summary>
        /// 選んでいるステートで、変化の割合を変えられるかどうか（ステートの組の終わりのステートで、キーでないとき）。
        /// </summary>
        public bool IsRatioMakerAvailable { get; set; }

        partial void OnSelectedBlendModeChanged(BlendMode? value)
        {
            if (!IgnoreInput && value.HasValue)
            {
                Host.ChangeBlendMode(value.Value);
            }
        }

        /// <summary>
        /// ステートの値を表示します（null のときは空にする）。
        /// </summary>
        public void SetState(EffectStateStructure state)
        {
            IgnoreInput = true;
            SelectedBlendMode = state?.BlendMode ?? BlendMode.None;
            Rows[0].Text = state?.X.ToString() ?? "";
            Rows[1].Text = state?.Y.ToString() ?? "";
            Rows[2].Text = state?.Alpha.ToString() ?? "";
            Rows[3].Text = state?.Rotation.ToString() ?? "";
            Rows[4].Text = state?.ScaleX.ToString() ?? "";
            Rows[5].Text = state?.ScaleY.ToString() ?? "";
            IgnoreInput = false;
            foreach (var row in Rows)
            {
                row.IsRatioEnabled = IsRatioMakerAvailable;
            }
        }

        /// <summary>
        /// 位置をベジエで動かすかどうかで、位置の入力とボタンを切り替えます。
        /// </summary>
        public void SetBezierPosition(bool isBezierPosition)
        {
            Rows[0].IsRatioEnabled = Rows[1].IsRatioEnabled = !isBezierPosition;
            Rows[0].IsTextEnabled = Rows[1].IsTextEnabled = !isBezierPosition;
            IsBezierPositionEnabled = isBezierPosition;
        }

        [RelayCommand]
        private void EditRatio(RatioType type)
        {
            Host.EditRatio(type);
        }
    }
}
