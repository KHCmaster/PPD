using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PPDEditor.Models;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;

namespace PPDEditor.ViewModels.Dialogs
{
    /// <summary>
    /// 難易度ごとのプロジェクトを 1 つのプロジェクトにマージ（結合）するダイアログです。
    /// </summary>
    public partial class MergeViewModel : DialogViewModel
    {
        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(OkCommand))]
        public partial string DestProjectFilePath { get; set; } = "";

        [ObservableProperty]
        public partial AvailableDifficulty? BaseInfoProjectDifficulty { get; set; }

        public MergeViewModel()
        {
            Slots =
            [
                new MergeSlot(AvailableDifficulty.Easy),
                new MergeSlot(AvailableDifficulty.Normal),
                new MergeSlot(AvailableDifficulty.Hard),
                new MergeSlot(AvailableDifficulty.Extreme),
                new MergeSlot(AvailableDifficulty.Base)
            ];
        }

        /// <summary>
        /// マージ先の難易度ごとの、マージするプロジェクトです（Easy、Normal、Hard、Extreme、Base の順）。
        /// </summary>
        public MergeSlot[] Slots { get; }

        /// <summary>
        /// プロジェクトの基本情報を取得できる難易度（プロジェクトを選んだ難易度）です。
        /// </summary>
        public ObservableCollection<AvailableDifficulty> BaseDifficulties { get; } = [];

        /// <summary>
        /// プロジェクトを選んだ難易度です。
        /// </summary>
        public AvailableDifficulty MergeDifficulty
        {
            get
            {
                var ret = AvailableDifficulty.None;
                foreach (var slot in Slots)
                {
                    // 難易度のないプロジェクトは数えない（以前は数えていたので、マージで存在しない「None」の譜面を探して失敗した）
                    ret |= File.Exists(slot.ProjectPath) && slot.SelectedDifficulty.HasValue ? slot.Difficulty : AvailableDifficulty.None;
                }
                return ret;
            }
        }

        public MergeInfo GetMergeInfo(AvailableDifficulty difficulty)
        {
            var slot = Slots.FirstOrDefault(s => s.Difficulty == difficulty);
            return slot == null ? null : new MergeInfo(slot.ProjectPath, slot.ProjectVersion, slot.SelectedDifficulty ?? AvailableDifficulty.None);
        }

        protected override bool CanOk()
        {
            return MergeDifficulty != AvailableDifficulty.None && DestProjectFilePath != "";
        }

        // マージするプロジェクトを選び、その中の難易度を選べるようにする
        [RelayCommand]
        private void BrowseProject(MergeSlot slot)
        {
            var projectPath = View?.OpenFile(Texts["ProjectFilter"]);
            if (projectPath == null)
            {
                slot.Clear();
            }
            else
            {
                slot.Load(projectPath);
                if (slot.Difficulties.Count == 0)
                {
                    ShowMessage(Texts["NoAvailableDifficulty"]);
                }
            }
            UpdateBaseDifficulties();
        }

        [RelayCommand]
        private void BrowseDest()
        {
            DestProjectFilePath = View?.SaveFile(Texts["ProjectFilter"]) ?? "";
            UpdateBaseDifficulties();
        }

        private void UpdateBaseDifficulties()
        {
            var mergeDifficulty = MergeDifficulty;
            var selected = BaseInfoProjectDifficulty;
            BaseDifficulties.Clear();
            foreach (var difficulty in ProjectFiles.DifficultyArray)
            {
                if (difficulty != AvailableDifficulty.None && (mergeDifficulty & difficulty) == difficulty)
                {
                    BaseDifficulties.Add(difficulty);
                }
            }
            BaseInfoProjectDifficulty = selected.HasValue && BaseDifficulties.Contains(selected.Value) ? selected : BaseDifficulties.FirstOrDefault();
            if (BaseDifficulties.Count == 0)
            {
                BaseInfoProjectDifficulty = null;
            }
            OkCommand.NotifyCanExecuteChanged();
        }
    }

    /// <summary>
    /// マージ先の 1 つの難易度に入れるプロジェクトと、その中の難易度です。
    /// </summary>
    public partial class MergeSlot(AvailableDifficulty difficulty) : ObservableObject
    {
        public AvailableDifficulty Difficulty { get; } = difficulty;

        [ObservableProperty]
        public partial string ProjectPath { get; set; } = "";

        [ObservableProperty]
        public partial AvailableDifficulty? SelectedDifficulty { get; set; }
        public int ProjectVersion
        {
            get;
            private set;
        }

        /// <summary>
        /// 選んだプロジェクトにある難易度です。
        /// </summary>
        public ObservableCollection<AvailableDifficulty> Difficulties { get; } = [];

        /// <summary>
        /// プロジェクトを読み込み、その中の難易度を調べます（バージョン 1 のプロジェクトは Base だけ）。
        /// </summary>
        public void Load(string path)
        {
            ProjectPath = path;
            ProjectVersion = ProjectFiles.GetVersion(path);
            Difficulties.Clear();
            if (ProjectVersion == 1)
            {
                Difficulties.Add(AvailableDifficulty.Base);
            }
            else
            {
                var available = ProjectFiles.GetAvailableDifficulty(Path.Combine(Path.GetDirectoryName(path), Path.GetFileNameWithoutExtension(path)));
                foreach (var d in ProjectFiles.DifficultyArray)
                {
                    if (d != AvailableDifficulty.None && (available & d) == d)
                    {
                        Difficulties.Add(d);
                    }
                }
            }
            SelectedDifficulty = Difficulties.Count > 0 ? Difficulties[0] : null;
        }

        public void Clear()
        {
            ProjectPath = "";
            Difficulties.Clear();
            SelectedDifficulty = null;
        }
    }
}
