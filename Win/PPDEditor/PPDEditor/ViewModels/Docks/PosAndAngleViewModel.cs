using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PPDEditor.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Numerics;

namespace PPDEditor.ViewModels.Docks
{
    /// <summary>
    /// ViewModel から使う、エディターと画面の機能です。
    /// </summary>
    public interface IPosAndAngleHost
    {
        void ShowMessage(string text);

        /// <summary>
        /// 保存する名前と、角度も保存するかを聞きます。
        /// </summary>
        /// <returns>キャンセルしたときは null。</returns>
        (string FileName, bool WithAngle)? AskSaveName(string caption);

        /// <summary>
        /// 選んでいるマーク（<paramref name="inAreaSelection"/> のときは選んでいる範囲のマーク）を取ります。
        /// </summary>
        bool GetSortedMarks(bool reverse, bool inAreaSelection, out Mark[] marks);

        /// <summary>
        /// 位置と角度をマークに当てはめます。
        /// </summary>
        /// <param name="mode">0: 選んでいる点から、1: 選んでいる範囲の中。</param>
        bool ApplyToMarks(Vector2[] positions, float[] angles, bool reverse, int mode, bool withAngle);

        /// <summary>
        /// 当てはめる位置と角度をゲームの画面に表示します。
        /// </summary>
        void ShowPreview(Vector2[] positions, float[] angles);
    }

    /// <summary>
    /// 線対称の線の決め方です。
    /// </summary>
    public enum LineSymmetryMode
    {
        /// <summary>
        /// 自由に決める
        /// </summary>
        Free,

        /// <summary>
        /// 水平な線（Y だけ決める）
        /// </summary>
        Horizontal,

        /// <summary>
        /// 垂直な線（X だけ決める）
        /// </summary>
        Vertical
    }

    /// <summary>
    /// マークの位置と角度を保存し（posdat フォルダー）、そのまま・点対称・線対称でマークに当てはめます。
    /// </summary>
    public partial class PosAndAngleViewModel : DockViewModel
    {
        const string DataDir = "posdat";

        readonly IPosAndAngleHost host;
        string[] filePaths = [];
        PosAndAngleInfo[] infos;
        Vector2[] displayPositions;
        float[] displayAngles;
        bool hasAngle;

        [ObservableProperty]
        public partial int SelectedFileIndex { get; set; } = -1;

        /// <summary>
        /// ゲームの画面に角度を表示するかです。
        /// </summary>
        [ObservableProperty]
        public partial bool DisplayAngle { get; set; }

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(IsPointEnabled), nameof(IsLineEnabled), nameof(IsLineXEnabled), nameof(IsLineYEnabled), nameof(IsSymmetricAngleEnabled))]
        public partial PosAndAngleApplyMode ApplyMode { get; set; }

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(IsLineXEnabled), nameof(IsLineYEnabled))]
        public partial LineSymmetryMode LineMode { get; set; }

        [ObservableProperty]
        public partial bool SymmetricAngle { get; set; }

        [ObservableProperty]
        public partial string PointX { get; set; } = "400";

        [ObservableProperty]
        public partial string PointY { get; set; } = "225";

        [ObservableProperty]
        public partial string LineX1 { get; set; } = "0";

        [ObservableProperty]
        public partial string LineY1 { get; set; } = "225";

        [ObservableProperty]
        public partial string LineX2 { get; set; } = "800";

        [ObservableProperty]
        public partial string LineY2 { get; set; } = "225";

        public PosAndAngleViewModel(IPosAndAngleHost host)
        {
            this.host = host;
            Reload();
            // 言語を変えたら、線の決め方の名前も変える
            Texts.PropertyChanged += (sender, e) => OnPropertyChanged(nameof(LineModeNames));
        }

        public ObservableCollection<string> FileNames { get; } = [];

        public string[] LineModeNames
        {
            get { return [Texts["PAALSComboBox1"], Texts["PAALSComboBox2"], Texts["PAALSComboBox3"]]; }
        }

        /// <summary>
        /// ゲームの画面に角度を描くかです（角度を表示するときで、角度を保存したデータのとき）。
        /// </summary>
        public bool DrawAngle
        {
            get { return DisplayAngle && hasAngle; }
        }

        public bool IsPointEnabled
        {
            get { return ApplyMode == PosAndAngleApplyMode.Point; }
        }

        public bool IsLineEnabled
        {
            get { return ApplyMode == PosAndAngleApplyMode.Line; }
        }

        public bool IsLineXEnabled
        {
            get { return IsLineEnabled && LineMode != LineSymmetryMode.Horizontal; }
        }

        public bool IsLineYEnabled
        {
            get { return IsLineEnabled && LineMode != LineSymmetryMode.Vertical; }
        }

        public bool IsSymmetricAngleEnabled
        {
            get { return ApplyMode != PosAndAngleApplyMode.None; }
        }

        [RelayCommand]
        private void Reload()
        {
            Directory.CreateDirectory(DataDir);
            filePaths = Directory.GetFiles(DataDir);
            FileNames.Clear();
            foreach (var path in filePaths)
            {
                FileNames.Add(Path.GetFileNameWithoutExtension(path));
            }
        }

        [RelayCommand]
        private void DeleteSelected()
        {
            if (SelectedFileIndex >= 0 && File.Exists(filePaths[SelectedFileIndex]))
            {
                File.Delete(filePaths[SelectedFileIndex]);
                Reload();
            }
        }

        partial void OnSelectedFileIndexChanged(int value)
        {
            if (value < 0 || value >= filePaths.Length)
            {
                return;
            }
            try
            {
                infos = PosAndAngleInfo.Load(filePaths[value]);
                hasAngle = infos.Any(i => i.Rotation.HasValue);
                UpdatePreview();
            }
            catch (Exception exception)
            {
                host.ShowMessage(exception.Message);
            }
        }

        partial void OnApplyModeChanged(PosAndAngleApplyMode value)
        {
            UpdatePreview();
        }

        partial void OnSymmetricAngleChanged(bool value)
        {
            UpdatePreview();
        }

        // 水平・垂直の線にしたら、画面の中央の線にする
        partial void OnLineModeChanged(LineSymmetryMode value)
        {
            switch (value)
            {
                case LineSymmetryMode.Horizontal:
                    (LineX1, LineY1, LineX2, LineY2) = ("0", "225", "800", "225");
                    break;
                case LineSymmetryMode.Vertical:
                    (LineX1, LineY1, LineX2, LineY2) = ("400", "0", "400", "450");
                    break;
            }
        }

        partial void OnPointXChanged(string value)
        {
            ValidateCoordinate(value, v => PointX = v);
        }

        partial void OnPointYChanged(string value)
        {
            ValidateCoordinate(value, v => PointY = v);
        }

        partial void OnLineX1Changed(string value)
        {
            ValidateCoordinate(value, v => LineX1 = v);
        }

        partial void OnLineY1Changed(string value)
        {
            ValidateCoordinate(value, v => LineY1 = v);
        }

        partial void OnLineX2Changed(string value)
        {
            ValidateCoordinate(value, v => LineX2 = v);
        }

        partial void OnLineY2Changed(string value)
        {
            ValidateCoordinate(value, v => LineY2 = v);
        }

        // 整数でないときは 0 にして知らせる（以前と同じ）
        private void ValidateCoordinate(string value, Action<string> reset)
        {
            if (int.TryParse(value, out _))
            {
                UpdatePreview();
            }
            else
            {
                reset("0");
                host.ShowMessage(Texts["PAALSInvalidNumber"]);
            }
        }

        /// <summary>
        /// 座標の名前をドラッグして、値を増やしたり減らしたりします。水平・垂直の線のときは、もう一方の点も合わせます。
        /// </summary>
        /// <param name="field">座標の名前（X、Y、X1、Y1、X2、Y2）。</param>
        public void Nudge(string field, int delta)
        {
            static string Add(string text, int delta)
            {
                return (int.Parse(text) + delta).ToString();
            }

            switch (field)
            {
                case "X" when IsPointEnabled:
                    PointX = Add(PointX, delta);
                    break;
                case "Y" when IsPointEnabled:
                    PointY = Add(PointY, delta);
                    break;
                case "X1" when IsLineXEnabled:
                    LineX1 = Add(LineX1, delta);
                    if (LineMode == LineSymmetryMode.Vertical) LineX2 = LineX1;
                    break;
                case "Y1" when IsLineYEnabled:
                    LineY1 = Add(LineY1, delta);
                    if (LineMode == LineSymmetryMode.Horizontal) LineY2 = LineY1;
                    break;
                case "X2" when IsLineXEnabled:
                    LineX2 = Add(LineX2, delta);
                    if (LineMode == LineSymmetryMode.Vertical) LineX1 = LineX2;
                    break;
                case "Y2" when IsLineYEnabled:
                    LineY2 = Add(LineY2, delta);
                    if (LineMode == LineSymmetryMode.Horizontal) LineY1 = LineY2;
                    break;
            }
        }

        private void UpdatePreview()
        {
            if (infos == null)
            {
                return;
            }
            (displayPositions, displayAngles) = PosAndAngleTransform.Apply(infos, ApplyMode,
                new Vector2(float.Parse(PointX), float.Parse(PointY)),
                new Vector2(float.Parse(LineX1), float.Parse(LineY1)),
                new Vector2(float.Parse(LineX2), float.Parse(LineY2)),
                SymmetricAngle, hasAngle);
            host.ShowPreview(displayPositions, displayAngles);
        }

        private void Apply(bool reverse, int mode, string errorKey)
        {
            if (SelectedFileIndex >= 0 && !host.ApplyToMarks(displayPositions, displayAngles, reverse, mode, hasAngle))
            {
                host.ShowMessage(Texts[errorKey]);
            }
        }

        [RelayCommand]
        private void ApplyAfterSelection()
        {
            Apply(false, 0, "PAALSNoSelectedPoint");
        }

        [RelayCommand]
        private void ApplyBeforeSelection()
        {
            Apply(true, 0, "PAALSNoSelectedPoint");
        }

        [RelayCommand]
        private void ApplyInAreaForward()
        {
            Apply(false, 1, "PAALSNoSelectedArea");
        }

        [RelayCommand]
        private void ApplyInAreaBackward()
        {
            Apply(true, 1, "PAALSNoSelectedArea");
        }

        private void Save(string captionKey, bool reverse, bool inAreaSelection)
        {
            var answer = host.AskSaveName(Texts[captionKey]);
            if (answer == null)
            {
                return;
            }
            if (host.GetSortedMarks(reverse, inAreaSelection, out Mark[] marks))
            {
                var (fileName, withAngle) = answer.Value;
                var saveInfos = marks.Select(mk => withAngle ? new PosAndAngleInfo(mk.Position, mk.Rotation) : new PosAndAngleInfo(mk.Position)).ToArray();
                PosAndAngleInfo.Save(saveInfos, Path.Combine(DataDir, Path.ChangeExtension(fileName, ".txt")));
            }
            else
            {
                host.ShowMessage(Texts["PAALSNoSelectedPoint"]);
            }
            Reload();
        }

        [RelayCommand]
        private void SaveAfterSelection()
        {
            Save("PAALSButton5", false, false);
        }

        [RelayCommand]
        private void SaveBeforeSelection()
        {
            Save("PAALSButton6", true, false);
        }

        [RelayCommand]
        private void SaveInAreaForward()
        {
            Save("PAALSButton7", false, true);
        }

        [RelayCommand]
        private void SaveInAreaBackward()
        {
            Save("PAALSButton8", true, true);
        }
    }
}
