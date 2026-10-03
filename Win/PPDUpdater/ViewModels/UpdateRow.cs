using CommunityToolkit.Mvvm.ComponentModel;
using PPDUpdater.Model;

namespace PPDUpdater.ViewModels
{
    /// <summary>
    /// アップデートの進み具合です。名前は言語ファイルのキーと同じにしてあります。
    /// </summary>
    public enum UpdateState
    {
        Waiting,
        Downloading,
        DownloadFinished,
        Installing,
        InstallFinished
    }

    /// <summary>
    /// アップデートの一覧の 1 行です。
    /// </summary>
    public partial class UpdateRow : ObservableObject
    {
        readonly LanguageTexts texts;

        internal UpdateRow(UpdateInfo info, UpdateState state, LanguageTexts texts)
        {
            Info = info;
            this.state = state;
            this.texts = texts;
            // 言語を変えたら状態の表示も変える
            texts.PropertyChanged += (sender, e) => OnPropertyChanged(nameof(StateText));
        }

        internal UpdateInfo Info { get; }

        public string Version
        {
            get { return Info.ToString(); }
        }

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(StateText))]
        private UpdateState state;

        public string StateText
        {
            get { return texts[State.ToString()]; }
        }

        [ObservableProperty]
        private int downloadProgress;

        [ObservableProperty]
        private int installProgress;
    }
}
