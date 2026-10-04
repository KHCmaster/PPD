using PPDEditor.Controls;
using PPDEditor.Forms;
using PPDEditor.ViewModels.Docks;
using PPDEditorCommon;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using Keys = System.Windows.Forms.Keys;

namespace PPDEditor
{
    /// <summary>
    /// タイムライン（マークの一覧と、マークを置く画面）のドックです。右クリックメニューの処理は <see cref="TimeLineViewModel"/> にあります。
    /// </summary>
    public sealed partial class TimeLineForm : DockView, ITimeLineOperations
    {
        static readonly Brush translucentWhite = WpfSkin.ToBrush(System.Drawing.Color.FromArgb(220, 255, 255, 255));

        public TimeLineForm()
        {
            InitializeComponent();
            seekex1.Setseekmain(seekmain1);
            AttachViewModel(new TimeLineViewModel(this));
        }

        public TimeLineViewModel ViewModel
        {
            get { return (TimeLineViewModel)DataContext; }
        }

        public TimeLineRowManager RowManager { get; } = new();

        /// <summary>
        /// タイムラインの背景です。メインのウィンドウに背景の画像があるときは、半透明にしてそれを透かす（以前と同じ）。
        /// </summary>
        public Brush TimeLineBackground
        {
            get
            {
                return PPDEditorSkin.Skin.BackGround != null ? translucentWhite : Brushes.White;
            }
        }

        public Seekmain Seekmain
        {
            get { return seekmain1; }
        }

        private void TimeLineForm_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Space)
            {
                WindowUtility.Editor.PlayOrPause();
                e.Handled = true;
            }
        }

        public void DxFormPreviewKeyDown(Keys key, bool Control, bool Shift, bool Alt)
        {
            seekmain1.CheckPreviewKey(key, Control, Shift, Alt);
        }

        public void InvalidateAll()
        {
            seekex1.DrawAndRefresh();
            seekmain1.DrawAndRefresh();
        }

        public void SetSkin()
        {
            seekex1.SetSkin();
            Seekmain.SetSkin();
        }

        private void ContextMenu_Opened(object sender, RoutedEventArgs e)
        {
            ViewModel.RefreshPresets();
        }

        void ITimeLineOperations.AssignID()
        {
            Seekmain.AssignID();
        }

        void ITimeLineOperations.UnassignID(Func<bool> confirm)
        {
            Seekmain.UnassignID(confirm);
        }

        void ITimeLineOperations.InterpolatePosition()
        {
            Seekmain.PositionInterpolation();
        }

        void ITimeLineOperations.InterpolateAngle(bool clockwise)
        {
            Seekmain.AngleInterpolation(clockwise);
        }

        bool ITimeLineOperations.Confirm(string text, string caption)
        {
            return MessageBox.Show(Window.GetWindow(this), text, caption, MessageBoxButton.OKCancel) == MessageBoxResult.OK;
        }

        DateTime ITimeLineOperations.PresetsLoadTime
        {
            get { return WindowUtility.InfoForm.PresetsLoadTime; }
        }

        ParameterPresetFolder ITimeLineOperations.PresetRoot
        {
            get { return WindowUtility.InfoForm.PresetRoot; }
        }

        Dictionary<string, string> ITimeLineOperations.GetSelectedCommonParameters()
        {
            var marks = Utility.GetSelectedMarks();
            return marks.Length == 0 ? null : Utility.GetCommonParameters(marks);
        }

        void ITimeLineOperations.ApplyParameters(KeyValuePair<string, string>[] parameters)
        {
            WindowUtility.InfoForm.ApplyParameters(parameters);
        }

        void ITimeLineOperations.UnapplyParameters(KeyValuePair<string, string>[] parameters)
        {
            WindowUtility.InfoForm.UnapplyParameters(parameters);
        }
    }
}
