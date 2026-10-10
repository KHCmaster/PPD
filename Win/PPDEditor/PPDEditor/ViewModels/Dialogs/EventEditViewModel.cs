using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PPDEditorCommon;
using PPDFramework;
using PPDFramework.PPDStructure.EVDData;
using System;
using System.Collections.ObjectModel;
using System.Linq;

namespace PPDEditor.ViewModels.Dialogs
{
    /// <summary>
    /// 同時押しの描画順序の決め方です。
    /// </summary>
    public enum InitializeOrderMode
    {
        Normal,
        AC,
        Custom
    }

    /// <summary>
    /// イベント（音量、BPM 変更、マークの表示や動き、描画順序など）を編集するダイアログです。
    /// </summary>
    public partial class EventEditViewModel : DialogViewModel
    {
        static readonly ButtonType[] normalInitializeOrder = [
            ButtonType.Square,
            ButtonType.Cross,
            ButtonType.Circle,
            ButtonType.Triangle,
            ButtonType.Left,
            ButtonType.Down,
            ButtonType.Right,
            ButtonType.Up,
            ButtonType.R,
            ButtonType.L
        ];
        static readonly ButtonType[] acInitializeOrder = [
            ButtonType.Triangle,
            ButtonType.Square,
            ButtonType.Cross,
            ButtonType.Circle,
            ButtonType.Up,
            ButtonType.Left,
            ButtonType.Down,
            ButtonType.Right,
            ButtonType.R,
            ButtonType.L
        ];

        [ObservableProperty]
        private int movieVolumePercent;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(IsBpmInvalid))]
        private string bpm;

        [ObservableProperty]
        private bool bpmRapidChange;

        [ObservableProperty]
        private DisplayState displayState;

        [ObservableProperty]
        private MoveState moveState;

        [ObservableProperty]
        private NoteType noteType;

        [ObservableProperty]
        private string slideScale;

        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(MoveUpCommand), nameof(MoveDownCommand))]
        private InitializeOrderMode orderMode;

        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(MoveUpCommand), nameof(MoveDownCommand))]
        private int selectedOrderIndex = -1;

        /// <param name="eventData">編集するイベント。</param>
        /// <param name="isReadOnly">見るだけで、編集できないようにするか。</param>
        public EventEditViewModel(EventData eventData, bool isReadOnly = false)
        {
            IsEditable = !isReadOnly;
            movieVolumePercent = eventData.MovieVolumePercent;
            BeforeBpm = eventData.BPM.ToString();
            bpm = BeforeBpm;
            bpmRapidChange = eventData.BPMRapidChange;
            Sounds =
            [
                new SoundSetting("EEFLabel2", eventData.SquareVolumePercent, eventData.SquareKeepPlaying, eventData.SquareReleaseSound),
                new SoundSetting("EEFLabel3", eventData.CrossVolumePercent, eventData.CrossKeepPlaying, eventData.CrossReleaseSound),
                new SoundSetting("EEFLabel4", eventData.CircleVolumePercent, eventData.CircleKeepPlaying, eventData.CircleReleaseSound),
                new SoundSetting("EEFLabel5", eventData.TriangleVolumePercent, eventData.TriangleKeepPlaying, eventData.TriangleReleaseSound),
                new SoundSetting("EEFLabel6", eventData.LeftVolumePercent, eventData.LeftKeepPlaying, eventData.LeftReleaseSound),
                new SoundSetting("EEFLabel7", eventData.DownVolumePercent, eventData.DownKeepPlaying, eventData.DownReleaseSound),
                new SoundSetting("EEFLabel8", eventData.RightVolumePercent, eventData.RightKeepPlaying, eventData.RightReleaseSound),
                new SoundSetting("EEFLabel9", eventData.UpVolumePercent, eventData.UpKeepPlaying, eventData.UpReleaseSound),
                new SoundSetting("EEFLabel10", eventData.RVolumePercent, eventData.RKeepPlaying, eventData.RReleaseSound),
                new SoundSetting("EEFLabel11", eventData.LVolumePercent, eventData.LKeepPlaying, eventData.LReleaseSound)
            ];
            displayState = eventData.DisplayState;
            moveState = eventData.MoveState;
            noteType = eventData.NoteType;
            slideScale = eventData.SlideScale.ToString();

            SetOrder(eventData.InitializeOrder);
            if (Utility.IsSameArray(normalInitializeOrder, eventData.InitializeOrder))
            {
                orderMode = InitializeOrderMode.Normal;
            }
            else if (Utility.IsSameArray(acInitializeOrder, eventData.InitializeOrder))
            {
                orderMode = InitializeOrderMode.AC;
            }
            else
            {
                orderMode = InitializeOrderMode.Custom;
            }
        }

        public bool IsEditable { get; }

        /// <summary>
        /// 変更前の BPM です。
        /// </summary>
        public string BeforeBpm { get; }

        public bool IsBpmInvalid
        {
            get { return !float.TryParse(Bpm, out _); }
        }

        /// <summary>
        /// ボタンごとの音の設定です（スクエアからエルの順）。
        /// </summary>
        public SoundSetting[] Sounds { get; }

        /// <summary>
        /// 同時押しの描画順序です。
        /// </summary>
        public ObservableCollection<InitializeOrderItem> InitializeOrder { get; } = [];

        /// <summary>
        /// 編集したイベントです。数字でない BPM は 100、スケールは 1 にします。
        /// </summary>
        public EventData EventData
        {
            get
            {
                if (!float.TryParse(Bpm, out float bpmValue))
                {
                    bpmValue = 100;
                }
                if (!float.TryParse(SlideScale, out float slideScaleValue))
                {
                    slideScaleValue = 1;
                }
                return new EventData
                {
                    MovieVolumePercent = MovieVolumePercent,
                    SquareVolumePercent = Sounds[0].VolumePercent,
                    CrossVolumePercent = Sounds[1].VolumePercent,
                    CircleVolumePercent = Sounds[2].VolumePercent,
                    TriangleVolumePercent = Sounds[3].VolumePercent,
                    LeftVolumePercent = Sounds[4].VolumePercent,
                    DownVolumePercent = Sounds[5].VolumePercent,
                    RightVolumePercent = Sounds[6].VolumePercent,
                    UpVolumePercent = Sounds[7].VolumePercent,
                    RVolumePercent = Sounds[8].VolumePercent,
                    LVolumePercent = Sounds[9].VolumePercent,
                    BPM = bpmValue,
                    BPMRapidChange = BpmRapidChange,
                    SquareKeepPlaying = Sounds[0].KeepPlaying,
                    CrossKeepPlaying = Sounds[1].KeepPlaying,
                    CircleKeepPlaying = Sounds[2].KeepPlaying,
                    TriangleKeepPlaying = Sounds[3].KeepPlaying,
                    LeftKeepPlaying = Sounds[4].KeepPlaying,
                    DownKeepPlaying = Sounds[5].KeepPlaying,
                    RightKeepPlaying = Sounds[6].KeepPlaying,
                    UpKeepPlaying = Sounds[7].KeepPlaying,
                    RKeepPlaying = Sounds[8].KeepPlaying,
                    LKeepPlaying = Sounds[9].KeepPlaying,
                    SquareReleaseSound = Sounds[0].ReleaseSound,
                    CrossReleaseSound = Sounds[1].ReleaseSound,
                    CircleReleaseSound = Sounds[2].ReleaseSound,
                    TriangleReleaseSound = Sounds[3].ReleaseSound,
                    LeftReleaseSound = Sounds[4].ReleaseSound,
                    DownReleaseSound = Sounds[5].ReleaseSound,
                    RightReleaseSound = Sounds[6].ReleaseSound,
                    UpReleaseSound = Sounds[7].ReleaseSound,
                    RReleaseSound = Sounds[8].ReleaseSound,
                    LReleaseSound = Sounds[9].ReleaseSound,
                    DisplayState = DisplayState,
                    MoveState = MoveState,
                    NoteType = NoteType,
                    InitializeOrder = [.. InitializeOrder.Select(item => item.ButtonType)],
                    SlideScale = slideScaleValue
                };
            }
        }

        // 普通・AC を選んだら、その順序にする（カスタムのときは今の順序を並べ替える）
        partial void OnOrderModeChanged(InitializeOrderMode value)
        {
            switch (value)
            {
                case InitializeOrderMode.Normal:
                    SetOrder(normalInitializeOrder);
                    break;
                case InitializeOrderMode.AC:
                    SetOrder(acInitializeOrder);
                    break;
            }
        }

        private void SetOrder(ButtonType[] order)
        {
            InitializeOrder.Clear();
            foreach (var buttonType in order)
            {
                InitializeOrder.Add(new InitializeOrderItem(buttonType));
            }
        }

        private bool CanMoveUp()
        {
            return OrderMode == InitializeOrderMode.Custom && SelectedOrderIndex > 0;
        }

        [RelayCommand(CanExecute = nameof(CanMoveUp))]
        private void MoveUp()
        {
            Move(-1);
        }

        private bool CanMoveDown()
        {
            return OrderMode == InitializeOrderMode.Custom && SelectedOrderIndex >= 0 && SelectedOrderIndex < InitializeOrder.Count - 1;
        }

        [RelayCommand(CanExecute = nameof(CanMoveDown))]
        private void MoveDown()
        {
            Move(1);
        }

        private void Move(int direction)
        {
            var index = SelectedOrderIndex;
            InitializeOrder.Move(index, index + direction);
            SelectedOrderIndex = index + direction;
        }
    }

    /// <summary>
    /// 1 つのボタンの音の設定（音量、押している間だけ鳴らす、長押しを離したときに鳴らす）です。
    /// </summary>
    public partial class SoundSetting(string labelKey, int volumePercent, bool keepPlaying, bool releaseSound) : ObservableObject
    {
        /// <summary>
        /// 名前の言語のキーです。
        /// </summary>
        public string LabelKey { get; } = labelKey;

        public string Label
        {
            get { return LanguageTexts.Instance[LabelKey]; }
        }

        [ObservableProperty]
        private int volumePercent = volumePercent;

        [ObservableProperty]
        private bool keepPlaying = keepPlaying;

        [ObservableProperty]
        private bool releaseSound = releaseSound;
    }

    /// <summary>
    /// 描画順序の 1 つのボタンです。
    /// </summary>
    public class InitializeOrderItem(ButtonType buttonType)
    {
        public ButtonType ButtonType { get; } = buttonType;

        public string Name
        {
            get { return LanguageTexts.Instance[String.Format("SMLabel{0}", (int)ButtonType + 1)]; }
        }
    }
}
