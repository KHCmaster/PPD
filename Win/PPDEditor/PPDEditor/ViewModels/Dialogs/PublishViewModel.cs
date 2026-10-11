using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PPDEditor.Models;
using System.IO;
using System.Linq;

namespace PPDEditor.ViewModels.Dialogs
{
    /// <summary>
    /// 譜面フォルダーを発行するダイアログです（発行先、難易度、フォルダー名）。
    /// </summary>
    public partial class PublishViewModel : DialogViewModel
    {
        readonly string projectDir;

        [ObservableProperty]
        public partial string PublishFolderPath { get; set; } = Path.GetFullPath("publish");

        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(OkCommand))]
        public partial string PublishFolderName { get; set; }

        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(OkCommand))]
        public partial bool Easy { get; set; }

        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(OkCommand))]
        public partial bool Normal { get; set; }

        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(OkCommand))]
        public partial bool Hard { get; set; }

        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(OkCommand))]
        public partial bool Extreme { get; set; }

        /// <param name="availableDifficulty">プロジェクトにある難易度（選べるのはこれだけ）。</param>
        /// <param name="publishFolderName">発行するフォルダーの名前の初期値。</param>
        /// <param name="projectDir">プロジェクトのフォルダー（譜面を確かめるのに使う）。</param>
        public PublishViewModel(AvailableDifficulty availableDifficulty, string publishFolderName, string projectDir)
        {
            this.projectDir = projectDir;
            PublishFolderName = publishFolderName;
            AvailableDifficulty = availableDifficulty;
            Easy = CanEasy;
            Normal = CanNormal;
            Hard = CanHard;
            Extreme = CanExtreme;
        }

        public AvailableDifficulty AvailableDifficulty { get; }

        public bool CanEasy
        {
            get { return AvailableDifficulty.HasFlag(AvailableDifficulty.Easy); }
        }

        public bool CanNormal
        {
            get { return AvailableDifficulty.HasFlag(AvailableDifficulty.Normal); }
        }

        public bool CanHard
        {
            get { return AvailableDifficulty.HasFlag(AvailableDifficulty.Hard); }
        }

        public bool CanExtreme
        {
            get { return AvailableDifficulty.HasFlag(AvailableDifficulty.Extreme); }
        }

        /// <summary>
        /// 発行する難易度です。
        /// </summary>
        public AvailableDifficulty PublishDifficulty
        {
            get
            {
                var ret = AvailableDifficulty.None;
                ret |= Easy ? AvailableDifficulty.Easy : AvailableDifficulty.None;
                ret |= Normal ? AvailableDifficulty.Normal : AvailableDifficulty.None;
                ret |= Hard ? AvailableDifficulty.Hard : AvailableDifficulty.None;
                ret |= Extreme ? AvailableDifficulty.Extreme : AvailableDifficulty.None;
                return ret;
            }
        }

        [RelayCommand]
        private void BrowseFolder()
        {
            PublishFolderPath = View?.SelectFolder(PublishFolderPath) ?? PublishFolderPath;
        }

        protected override bool CanOk()
        {
            return PublishDifficulty != AvailableDifficulty.None && PublishFolderName != "";
        }

        // 同じ内容の譜面と、近すぎる時間のマークがないかを確かめる（すべて知らせる）
        protected override bool Validate()
        {
            var errors = PublishChecker.CheckSameScore(projectDir, PublishDifficulty);
            if (errors.Count == 0)
            {
                errors = PublishChecker.CheckNearTime(projectDir, PublishDifficulty);
            }
            foreach (var error in errors)
            {
                ShowMessage(error);
            }
            return errors.Count == 0;
        }
    }
}
