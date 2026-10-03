using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.IO;

namespace PPDEditor.ViewModels.Dialogs
{
    /// <summary>
    /// マークの位置（と角度）を保存する名前を決めるダイアログです。
    /// </summary>
    public partial class SavePosAndAngleViewModel(string caption) : DialogViewModel
    {
        /// <summary>
        /// 保存するもの（押したボタンの文字）です。
        /// </summary>
        public string Caption { get; } = caption;

        [ObservableProperty]
        private bool withAngle;

        [ObservableProperty]
        private string fileName = "";

        /// <summary>
        /// 名前が使えなかったときに起こります（入力し直してもらう）。
        /// </summary>
        public event EventHandler FileNameRejected;

        protected override bool Validate()
        {
            if (FileName == "")
            {
                ShowMessage(Texts["PAALSNameError1"]);
                return false;
            }
            if (File.Exists("posdat\\" + FileName + ".txt"))
            {
                ShowMessage(Texts["PAALSNameError2"]);
                FileNameRejected?.Invoke(this, EventArgs.Empty);
                return false;
            }
            return true;
        }
    }
}
