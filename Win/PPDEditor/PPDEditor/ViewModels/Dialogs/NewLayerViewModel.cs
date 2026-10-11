using CommunityToolkit.Mvvm.ComponentModel;

namespace PPDEditor.ViewModels.Dialogs
{
    /// <summary>
    /// 新しいレイヤーの名前を入力するダイアログです。
    /// </summary>
    public partial class NewLayerViewModel(string layerName) : DialogViewModel
    {
        [ObservableProperty]
        public partial string LayerName { get; set; } = layerName;
    }
}
