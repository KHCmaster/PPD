using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.IO;
using System.Linq;

namespace PPDEditor.ViewModels.Dialogs
{
    /// <summary>
    /// ボタンごとに鳴らす音を変えるダイアログです。
    /// </summary>
    public partial class SoundChangeEditViewModel : DialogViewModel
    {
        readonly string[] sounds;

        [ObservableProperty]
        private int previewIndex;

        public SoundChangeEditViewModel(ushort[] change, string[] sounds)
        {
            this.sounds = sounds;
            SoundNames = [.. sounds.Select(Path.GetFileNameWithoutExtension)];
            Slots = [.. change.Select(index => new SoundSlot { SelectedIndex = index })];
        }

        /// <summary>
        /// 音を鳴らしてほしいときに起こります（音のファイルのパス）。
        /// </summary>
        public event Action<string> PlaySound;

        /// <summary>
        /// 音の名前です（ファイル名から拡張子を除いたもの）。
        /// </summary>
        public string[] SoundNames { get; }

        /// <summary>
        /// ボタンごとの音です（スクエアからエルの順）。
        /// </summary>
        public SoundSlot[] Slots { get; }

        public ushort[] Change
        {
            get { return [.. Slots.Select(s => (ushort)s.SelectedIndex)]; }
        }

        // 最初の音（なし）は鳴らさない
        [RelayCommand]
        private void Play()
        {
            if (PreviewIndex > 0)
            {
                PlaySound?.Invoke(sounds[PreviewIndex]);
            }
        }
    }

    /// <summary>
    /// 1 つのボタンで鳴らす音です。
    /// </summary>
    public partial class SoundSlot : ObservableObject
    {
        [ObservableProperty]
        private int selectedIndex;
    }
}
