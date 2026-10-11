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
        public partial string ModFileName { get; set; } = "";

        [ObservableProperty]
        public partial string ModDisplayName { get; set; } = "";

        [ObservableProperty]
        public partial string ModAuthorName { get; set; } = PPDStaticSetting.AuthorName;

        [ObservableProperty]
        public partial string ModVersion { get; set; } = "";

        [ObservableProperty]
        public partial string PublishFolder { get; set; }

        public ModPublishViewModel()
        {
            PublishFolder = Path.GetFullPath("publish_mod");
            Directory.CreateDirectory(PublishFolder);
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
