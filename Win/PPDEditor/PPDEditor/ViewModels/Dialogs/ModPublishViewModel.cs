using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.IO;

namespace PPDEditor.ViewModels.Dialogs
{
    /// <summary>
    /// MOD を発行するときの情報（ファイル名、表示名、作者名、バージョン、発行先）を入力するダイアログです。
    /// </summary>
    public partial class ModPublishViewModel : DialogViewModel
    {
        [ObservableProperty]
        private string modFileName = "";

        [ObservableProperty]
        private string modDisplayName = "";

        [ObservableProperty]
        private string modAuthorName = PPDStaticSetting.AuthorName;

        [ObservableProperty]
        private string modVersion = "";

        [ObservableProperty]
        private string publishFolder;

        public ModPublishViewModel()
        {
            publishFolder = Path.GetFullPath("publish_mod");
            Directory.CreateDirectory(publishFolder);
        }

        [RelayCommand]
        private void BrowseFolder()
        {
            PublishFolder = View?.SelectFolder(PublishFolder) ?? PublishFolder;
        }

        protected override bool Validate()
        {
            try
            {
                Directory.CreateDirectory(PublishFolder);
                if (String.IsNullOrEmpty(ModFileName))
                {
                    ShowMessage(Texts["NoFilename"]);
                    return false;
                }
                if (!Utility.CheckValidFileName(ModFileName))
                {
                    ShowMessage(String.Format(Texts["ContainsInvalidChars"], Texts["FileName"]));
                    return false;
                }
                return true;
            }
            catch (Exception e)
            {
                ShowMessage(e.Message);
                return false;
            }
        }
    }
}
