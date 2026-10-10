using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.Generic;

namespace PPDEditor.ViewModels.Dialogs
{
    /// <summary>
    /// 値の候補から 1 つを選ぶダイアログです。
    /// </summary>
    public partial class SuggestViewModel(IEnumerable<KeyValuePair<string, string>> suggests) : DialogViewModel
    {
        public List<KeyValuePair<string, string>> Suggests { get; } = [.. suggests];

        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(OkCommand))]
        private KeyValuePair<string, string>? selectedSuggest;

        protected override bool CanOk()
        {
            return SelectedSuggest.HasValue;
        }
    }
}
