using BMSTOPPD.Models;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace BMSTOPPD.ViewModels
{
    /// <summary>
    /// 画面に頼む操作（ファイルの選択、メッセージ、設定のウィンドウ）です。ウィンドウが実装します。
    /// </summary>
    public interface IDialogService
    {
        /// <summary>
        /// ファイルを選ばせます。選ばなかったときは null。
        /// </summary>
        string OpenFile(string filter);

        void ShowMessage(string text);

        void ShowSettingWindow();
    }

    public partial class MainWindowViewModel(IDialogService dialogService, IReadOnlyList<MarkSetting> settings) : ObservableObject
    {
        [ObservableProperty]
        public partial string FileName { get; set; } = "";

        [ObservableProperty]
        public partial string TimeOffset { get; set; } = "";

        [ObservableProperty]
        public partial bool IsDtx { get; set; }

        [RelayCommand]
        private void Open()
        {
            var path = dialogService.OpenFile("BMSファイル(*.bms):DTXファイル(*.dtx)|*.bms;*.dtx|すべてのファイル(*.*)|*.*");
            if (path != null)
            {
                FileName = path;
            }
        }

        [RelayCommand]
        private void Convert()
        {
            if (!File.Exists(FileName))
            {
                dialogService.ShowMessage("file not exist");
                return;
            }

            string data;
            using (var sr = new StreamReader(FileName, Encoding.GetEncoding(932)))
            {
                data = sr.ReadToEnd().Replace("\r", "");
            }
            _ = double.TryParse(TimeOffset, out double offset);
            string path;
            try
            {
                path = BmsConverter.Convert(data, FileName, offset, IsDtx, settings);
            }
            catch (Exception)
            {
                dialogService.ShowMessage("error");
                return;
            }
            dialogService.ShowMessage(path + "\nを生成しました");
        }

        [RelayCommand]
        private void ShowSetting()
        {
            dialogService.ShowSettingWindow();
        }
    }
}
