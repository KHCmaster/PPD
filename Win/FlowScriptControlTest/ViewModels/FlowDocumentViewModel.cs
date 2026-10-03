using CommunityToolkit.Mvvm.ComponentModel;
using FlowScriptControl.Controls;
using System;
using System.IO;

namespace FlowScriptControlTest.ViewModels
{
    /// <summary>
    /// 編集しているスクリプト 1 つ（タブ 1 つ）です。
    /// </summary>
    /// <remarks>
    /// 編集の領域（<see cref="FlowDrawPanel"/>）は状態を持ったコントロールで、タブを切り替えても作り直せないので、ここで持っておく。
    /// </remarks>
    public partial class FlowDocumentViewModel(FlowDrawPanel editor) : ObservableObject
    {
        /// <summary>
        /// ファイルのパスです。保存していないときは、仮の名前（GUID）です。
        /// </summary>
        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(Header))]
        private string filePath = Guid.NewGuid().ToString();

        public FlowDrawPanel Editor { get; } = editor;

        public string Header
        {
            get { return Path.GetFileName(FilePath); }
        }
    }
}
