using PPDEditor.Controls;
using PPDEditor.Forms;
using PPDEditorCommon;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using Keys = System.Windows.Forms.Keys;

namespace PPDEditor
{
    public partial class TimeLineForm : DockView
    {
        static readonly Brush translucentWhite = WpfSkin.ToBrush(System.Drawing.Color.FromArgb(220, 255, 255, 255));

        readonly TimeLineRowManager rowManager = new();
        DateTime presetsLoadTime;

        public TimeLineRowManager RowManager
        {
            get
            {
                return rowManager;
            }
        }

        /// <summary>
        /// タイムラインの背景です。メインのウィンドウに背景の画像があるときは、半透明にしてそれを透かす（以前と同じ）。
        /// </summary>
        public Brush TimeLineBackground
        {
            get
            {
                return WindowUtility.DockPanel?.BackgroundImage != null ? translucentWhite : Brushes.White;
            }
        }

        public TimeLineForm()
        {
            InitializeComponent();
            seekex1.Setseekmain(seekmain1);
        }

        public void SetLang()
        {
            this.選択中のマークにIDを割り当てMenuItem.Header = Utility.Language["AssignID"];
            this.選択中のマークのID割り当てを解除MenuItem.Header = Utility.Language["DeassignID"];
            this.角度を線形補完MenuItem.Header = Utility.Language["InterpolateAngle"];
            this.時計回りMenuItem.Header = Utility.Language["Clockwise"];
            this.反時計回りMenuItem.Header = Utility.Language["Unclockwise"];
            this.位置を線形補完MenuItem.Header = Utility.Language["InterpolatePosition"];
            this.パラメーターMenuItem.Header = Utility.Language["Parameter"];
        }

        private void TimeLineForm_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Space)
            {
                WindowUtility.MainForm.PlayOrPause();
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

        public Seekmain Seekmain
        {
            get
            {
                return this.seekmain1;
            }
        }
        public void SetSkin()
        {
            seekex1.SetSkin();
            Seekmain.SetSkin();
        }

        private void 選択中のマークにIDを割り当てMenuItem_Click(object sender, RoutedEventArgs e)
        {
            Seekmain.AssignID();
        }

        private void 選択中のマークのID割り当てを解除MenuItem_Click(object sender, RoutedEventArgs e)
        {
            Seekmain.UnassignID(() =>
            {
                return MessageBox.Show(Window.GetWindow(this), Utility.Language["UnassignIDConfirm"], Utility.Language["Confirm"], MessageBoxButton.OKCancel) == MessageBoxResult.OK;
            });
        }

        private void 時計回りMenuItem_Click(object sender, RoutedEventArgs e)
        {
            Seekmain.AngleInterpolation(true);
        }

        private void 反時計回りMenuItem_Click(object sender, RoutedEventArgs e)
        {
            Seekmain.AngleInterpolation(false);
        }

        private void 位置を線形補完MenuItem_Click(object sender, RoutedEventArgs e)
        {
            Seekmain.PositionInterpolation();
        }

        private void ContextMenuStrip1_Opened(object sender, RoutedEventArgs e)
        {
            if (presetsLoadTime != WindowUtility.InfoForm.PresetsLoadTime)
            {
                パラメーターMenuItem.Items.Clear();
                AddPresetMenuItems(パラメーターMenuItem, WindowUtility.InfoForm.PresetRoot);
                presetsLoadTime = WindowUtility.InfoForm.PresetsLoadTime;
            }
            UpdatePresetCheckState();
        }

        private void AddPresetMenuItems(MenuItem parent, ParameterPresetFolder folder)
        {
            foreach (var childFolder in folder.Folders)
            {
                var child = new MenuItem
                {
                    Header = childFolder.Name.Replace("_", "__"),
                    ToolTip = childFolder.Name
                };
                parent.Items.Add(child);
                AddPresetMenuItems(child, childFolder);
            }
            foreach (var preset in folder.Presets)
            {
                var name = Path.GetFileNameWithoutExtension(preset.FilePath);
                var child = new MenuItem
                {
                    Header = name.Replace("_", "__"),
                    ToolTip = name,
                    Tag = preset
                };
                child.Click += Child_Click;
                parent.Items.Add(child);
            }
        }

        private static IEnumerable<MenuItem> GetPresetMenuItems(MenuItem parent)
        {
            foreach (var child in parent.Items.OfType<MenuItem>())
            {
                if (child.Tag is ParameterPreset)
                {
                    yield return child;
                }
                foreach (var descendant in GetPresetMenuItems(child))
                {
                    yield return descendant;
                }
            }
        }

        /// <summary>
        /// 選んでいるマークに、プリセットのパラメーターがすべて当てはまるものにチェックを、一部だけのものに印を付ける。
        /// </summary>
        private void UpdatePresetCheckState()
        {
            var marks = Utility.GetSelectedMarks();
            if (marks.Length == 0)
            {
                return;
            }

            var common = Utility.GetCommonParameters(marks);
            foreach (var menuItem in GetPresetMenuItems(パラメーターMenuItem))
            {
                var state = Utility.GetPresetCheckState((ParameterPreset)menuItem.Tag, common);
                menuItem.IsChecked = state == true;
                menuItem.Icon = state == null ? new TextBlock { Text = "－", HorizontalAlignment = HorizontalAlignment.Center } : null;
            }
        }

        void Child_Click(object sender, RoutedEventArgs e)
        {
            var menuItem = (MenuItem)sender;
            var preset = (ParameterPreset)menuItem.Tag;
            if (menuItem.IsChecked)
            {
                WindowUtility.InfoForm.UnapplyParameters(preset.Parameters);
            }
            else
            {
                WindowUtility.InfoForm.ApplyParameters(preset.Parameters);
            }
        }
    }
}
