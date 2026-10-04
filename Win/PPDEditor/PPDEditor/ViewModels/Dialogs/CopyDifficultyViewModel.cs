using CommunityToolkit.Mvvm.ComponentModel;
using PPDEditor.Forms;
using System.Linq;

namespace PPDEditor.ViewModels.Dialogs
{
    /// <summary>
    /// 今の難易度の譜面を、ほかの難易度にコピーするダイアログです。
    /// </summary>
    public partial class CopyDifficultyViewModel : DialogViewModel
    {
        [ObservableProperty]
        private AvailableDifficulty destDifficulty;

        public CopyDifficultyViewModel(AvailableDifficulty currentDifficulty)
        {
            CurrentDifficulty = currentDifficulty;
            Difficulties = [.. Models.ProjectFiles.DifficultyArray.Where(d => d != currentDifficulty && d != AvailableDifficulty.None)];
            destDifficulty = Difficulties[0];
        }

        public AvailableDifficulty CurrentDifficulty { get; }

        /// <summary>
        /// コピー先にできる難易度です。
        /// </summary>
        public AvailableDifficulty[] Difficulties { get; }
    }
}
