using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PPDEditor.Command.PPDSheet;
using PPDEditor.Models;
using PPDEditorCommon;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;

namespace PPDEditor.ViewModels.Docks
{
    /// <summary>
    /// ViewModel から使う、エディターと画面の機能です。
    /// </summary>
    public interface IInfoEditor
    {
        /// <summary>
        /// 選んでいるマークです。
        /// </summary>
        Mark[] GetSelectedMarks();

        /// <summary>
        /// 選んでいるレイヤーの譜面です。
        /// </summary>
        PPDSheet SelectedSheet { get; }

        void ShowMessage(string text);

        /// <summary>
        /// パラメーターを編集させます。
        /// </summary>
        /// <returns>キャンセルしたときは null。</returns>
        Dictionary<string, string> EditParameters(Dictionary<string, string> parameters, string[] existKeys);

        /// <summary>
        /// プリセットを保存するファイルを選ばせます。
        /// </summary>
        /// <returns>キャンセルしたときは null。</returns>
        string AskPresetFilePath(string initialDirectory, string fileName);
    }

    /// <summary>
    /// 動画と選んでいるマークの情報、マークのパラメーターとそのプリセットを扱います。
    /// </summary>
    public partial class InfoViewModel : DockViewModel
    {
        readonly IInfoEditor editor;
        readonly string[] infoValues = ["", "", "", "", "", ""];
        Dictionary<string, string> copiedParameters;

        [ObservableProperty]
        public partial List<KeyValuePair<string, string>> Parameters { get; set; } = [];

        [ObservableProperty]
        public partial bool CanCopy { get; set; }

        [ObservableProperty]
        public partial bool CanPaste { get; set; }

        public InfoViewModel(IInfoEditor editor)
        {
            this.editor = editor;
            // 言語を変えたら、情報の名前も変える
            Texts.PropertyChanged += (sender, e) => OnPropertyChanged(nameof(InfoRows));
            LoadPresets();
        }

        /// <summary>
        /// 動画の幅、高さ、長さと、マークの位置、角度、ID です。
        /// </summary>
        public List<KeyValuePair<string, string>> InfoRows
        {
            get
            {
                string[] keys = [Texts["Width"], Texts["Height"], Texts["Length"], Texts["Position"], Texts["Angle"], "ID"];
                return [.. keys.Zip(infoValues, (key, value) => new KeyValuePair<string, string>(key, value))];
            }
        }

        /// <summary>
        /// プリセットを読み込んだ時間です（変わったらプリセットのメニューを作り直す）。
        /// </summary>
        public DateTime PresetsLoadTime
        {
            get;
            private set;
        }

        /// <summary>
        /// 読み込んだプリセットの一番上のフォルダーです。
        /// </summary>
        public ParameterPresetFolder PresetRoot
        {
            get;
            private set;
        }

        public ParameterPreset[] Presets
        {
            get { return [.. PresetRoot.GetAllPresets()]; }
        }

        /// <summary>
        /// プリセットのメニューです。
        /// </summary>
        public ObservableCollection<PresetNode> PresetNodes { get; } = [];

        public void SetMovieInfo(string width, string height, string length)
        {
            infoValues[0] = width;
            infoValues[1] = height;
            infoValues[2] = length;
            OnPropertyChanged(nameof(InfoRows));
        }

        public void SetMarkInfo(string pos, string angle, string id)
        {
            infoValues[3] = pos;
            infoValues[4] = angle;
            infoValues[5] = id;
            OnPropertyChanged(nameof(InfoRows));
        }

        /// <summary>
        /// マークに共通のパラメーターを表示します。
        /// </summary>
        public void UpdateParameters(Mark[] marks)
        {
            Parameters = marks.Length == 0 ? [] : [.. Utility.GetCommonParameters(marks)];
        }

        private void UpdateParameters()
        {
            UpdateParameters(editor.GetSelectedMarks());
        }

        private void LoadPresets()
        {
            PresetRoot = ParameterPresetStore.Load(() => editor.ShowMessage(Texts["ErrorInLoadingParameterPreset"]));
            PresetsLoadTime = DateTime.Now;
            PresetNodes.Clear();
            foreach (var node in PresetNode.Build(PresetRoot))
            {
                PresetNodes.Add(node);
            }
        }

        /// <summary>
        /// 編集のメニューを開いたときに、コピーと貼り付けができるかを確かめます。
        /// </summary>
        public void RefreshEditMenu()
        {
            var hasMarks = editor.GetSelectedMarks().Length > 0;
            CanCopy = hasMarks;
            CanPaste = copiedParameters != null && hasMarks;
        }

        /// <summary>
        /// プリセットのメニューを開いたときに、選んでいるマークに当てはまるプリセットに印を付けます。
        /// </summary>
        public void RefreshPresetMenu()
        {
            var marks = editor.GetSelectedMarks();
            if (marks.Length > 0)
            {
                PresetNode.UpdateCheckState(PresetNodes, Utility.GetCommonParameters(marks));
            }
        }

        // 選んでいるマークに共通のパラメーターを編集する（消したキーは消す）
        [RelayCommand]
        private void EditParameters()
        {
            var sheet = editor.SelectedSheet;
            if (sheet == null)
            {
                return;
            }
            var marks = editor.GetSelectedMarks();
            if (marks.Length == 0)
            {
                editor.ShowMessage(Texts["NoSelectedMarks"]);
                return;
            }

            var common = Utility.GetCommonParameters(marks, out string[] existKeys);
            var newParameters = editor.EditParameters(common, existKeys);
            if (newParameters == null)
            {
                return;
            }
            sheet.StartGroupCommand();
            foreach (var key in common.Keys.Where(key => !newParameters.ContainsKey(key)).ToArray())
            {
                sheet.RemoveParameter(key);
            }
            foreach (var pair in newParameters)
            {
                sheet.ChangeParameter(pair.Key, pair.Value);
            }
            sheet.EndGroupCommand();
            UpdateParameters();
        }

        [RelayCommand]
        private void Copy()
        {
            var marks = editor.GetSelectedMarks();
            if (marks.Length == 0)
            {
                return;
            }
            copiedParameters = Utility.GetCommonParameters(marks);
            if (copiedParameters.Count == 0)
            {
                copiedParameters = null;
            }
        }

        [RelayCommand]
        private void Paste()
        {
            if (copiedParameters == null || editor.GetSelectedMarks().Length == 0)
            {
                return;
            }
            ChangeParameters(copiedParameters);
        }

        [RelayCommand]
        private void SavePreset()
        {
            if (Parameters.Count == 0)
            {
                editor.ShowMessage(Texts["NoParameters"]);
                return;
            }
            var filePath = editor.AskPresetFilePath(Path.GetFullPath(ParameterPresetStore.PresetDir), "NewPresetParameter.xml");
            if (filePath != null)
            {
                ParameterPresetStore.Save(filePath, Parameters);
                LoadPresets();
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
                UnapplyParameters(node.Preset.Parameters);
            }
            else
            {
                ApplyParameters(node.Preset.Parameters);
            }
        }

        /// <summary>
        /// 選んでいるマークにパラメーターを当てはめます。
        /// </summary>
        public void ApplyParameters(KeyValuePair<string, string>[] parameters)
        {
            if (editor.GetSelectedMarks().Length == 0)
            {
                editor.ShowMessage(Texts["NoSelectedMarks"]);
                return;
            }
            ChangeParameters(parameters);
        }

        /// <summary>
        /// 選んでいるマークからパラメーターを外します。
        /// </summary>
        public void UnapplyParameters(KeyValuePair<string, string>[] parameters)
        {
            var marks = editor.GetSelectedMarks();
            if (marks.Length == 0)
            {
                editor.ShowMessage(Texts["NoSelectedMarks"]);
                return;
            }

            var sheet = editor.SelectedSheet;
            sheet.StartGroupCommand();
            foreach (var mark in marks)
            {
                foreach (var parameter in parameters)
                {
                    sheet.RemoveParameter(mark, parameter.Key);
                }
            }
            sheet.EndGroupCommand();
            UpdateParameters();
        }

        private void ChangeParameters(IEnumerable<KeyValuePair<string, string>> parameters)
        {
            var sheet = editor.SelectedSheet;
            sheet.StartGroupCommand();
            foreach (var pair in parameters)
            {
                sheet.ChangeParameter(pair.Key, pair.Value);
            }
            sheet.EndGroupCommand();
            UpdateParameters();
        }
    }
}
