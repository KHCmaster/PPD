using CommunityToolkit.Mvvm.ComponentModel;

namespace PPDEditor.ViewModels.Dialogs
{
    /// <summary>
    /// 作者名を入力するダイアログです。
    /// </summary>
    public partial class AuthorNameViewModel : DialogViewModel
    {
        [ObservableProperty]
        public partial string AuthorName { get; set; } = "";
    }
}
