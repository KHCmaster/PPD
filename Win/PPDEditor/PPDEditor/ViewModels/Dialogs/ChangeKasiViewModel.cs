using CommunityToolkit.Mvvm.ComponentModel;

namespace PPDEditor.ViewModels.Dialogs
{
    /// <summary>
    /// 歌詞を修正するダイアログです。
    /// </summary>
    public partial class ChangeKasiViewModel(string beforeText) : DialogViewModel
    {
        public string BeforeText { get; } = beforeText;

        [ObservableProperty]
        private string newText = "";
    }
}
