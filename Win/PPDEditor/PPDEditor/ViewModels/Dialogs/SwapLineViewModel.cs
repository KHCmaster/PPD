using CommunityToolkit.Mvvm.ComponentModel;
using System.Linq;

namespace PPDEditor.ViewModels.Dialogs
{
    /// <summary>
    /// 入れ替える 2 つのライン（マークの種類）を選ぶダイアログです。
    /// </summary>
    public partial class SwapLineViewModel : DialogViewModel
    {
        const int LineCount = 10;

        /// <summary>
        /// ラインの名前です（スクエア、クロス…）。
        /// </summary>
        public string[] Lines { get; } = [.. Enumerable.Range(1, LineCount).Select(i => LanguageTexts.Instance["SwapLineBox" + i])];

        [ObservableProperty]
        private int first;

        [ObservableProperty]
        private int second;
    }
}
