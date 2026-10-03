using CommunityToolkit.Mvvm.Input;
using PPDEditorCommon;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace PPDEditor.ViewModels.Docks
{
    /// <summary>
    /// タイムラインの右クリックメニューの操作を受けるものです（タイムラインの描画のコントロールと、パラメーターのドック）。
    /// </summary>
    public interface ITimeLineOperations
    {
        void AssignID();

        /// <summary>
        /// ID の割り当てを解除します。確かめてよければ true を返す関数を渡します。
        /// </summary>
        void UnassignID(Func<bool> confirm);

        void InterpolatePosition();

        void InterpolateAngle(bool clockwise);

        bool Confirm(string text, string caption);

        /// <summary>
        /// プリセットを読み込んだ時間です（変わったらメニューを作り直す）。
        /// </summary>
        DateTime PresetsLoadTime { get; }

        ParameterPresetFolder PresetRoot { get; }

        /// <summary>
        /// 選んでいるマークの共通のパラメーターです。マークを選んでいないときは null です。
        /// </summary>
        Dictionary<string, string> GetSelectedCommonParameters();

        void ApplyParameters(KeyValuePair<string, string>[] parameters);

        void UnapplyParameters(KeyValuePair<string, string>[] parameters);
    }

    /// <summary>
    /// タイムラインのドックです。描画と編集はタイムラインのコントロールが行い、ここでは右クリックメニューを扱います。
    /// </summary>
    public partial class TimeLineViewModel(ITimeLineOperations operations) : DockViewModel
    {
        DateTime presetsLoadTime;

        /// <summary>
        /// パラメーターのプリセットのメニューです。
        /// </summary>
        public ObservableCollection<PresetNode> Presets { get; } = [];

        [RelayCommand]
        private void AssignID()
        {
            operations.AssignID();
        }

        [RelayCommand]
        private void UnassignID()
        {
            operations.UnassignID(() => operations.Confirm(Texts["UnassignIDConfirm"], Texts["Confirm"]));
        }

        [RelayCommand]
        private void InterpolatePosition()
        {
            operations.InterpolatePosition();
        }

        [RelayCommand]
        private void InterpolateAngleClockwise()
        {
            operations.InterpolateAngle(true);
        }

        [RelayCommand]
        private void InterpolateAngleCounterclockwise()
        {
            operations.InterpolateAngle(false);
        }

        /// <summary>
        /// メニューを開いたときに、プリセットの一覧とチェックの状態を新しくします。
        /// </summary>
        public void RefreshPresets()
        {
            if (presetsLoadTime != operations.PresetsLoadTime)
            {
                Presets.Clear();
                foreach (var node in PresetNode.Build(operations.PresetRoot))
                {
                    Presets.Add(node);
                }
                presetsLoadTime = operations.PresetsLoadTime;
            }
            var common = operations.GetSelectedCommonParameters();
            if (common != null)
            {
                PresetNode.UpdateCheckState(Presets, common);
            }
        }

        /// <summary>
        /// プリセットのパラメーターを当てはめます。すべて当てはまっているときは外します。
        /// </summary>
        [RelayCommand]
        private void TogglePreset(PresetNode node)
        {
            if (node?.Preset == null)
            {
                return;
            }
            if (node.IsChecked)
            {
                operations.UnapplyParameters(node.Preset.Parameters);
            }
            else
            {
                operations.ApplyParameters(node.Preset.Parameters);
            }
        }
    }
}
