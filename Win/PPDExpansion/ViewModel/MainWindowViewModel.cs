using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using PPDExpansion.Message;
using PPDExpansionCore;
using PPDExpansionCore.Tcp;
using System;
using System.Windows.Input;
using System.Windows.Threading;

namespace PPDExpansion.ViewModel
{
    sealed class MainWindowViewModel : ObservableObject, IDisposable
    {
        private readonly Dispatcher dispatcher;

        private string title;
        private bool isConnected;
        private bool showMyHighScore;
        private bool showWebHighScore;
        private bool showCurrentStatus;
        private bool showGridLines;
        private bool showIIDXStyle;
        private bool showScoreDiff;
        private int width;
        private int height;
        private bool isMulti;

        private Host host;

        private ICommand closeCommand;
        private ICommand closedCommand;

        public string Title
        {
            get { return title; }
            set
            {
                if (title != value)
                {
                    title = value;
                    OnPropertyChanged(nameof(Title));
                }
            }
        }

        public bool IsConnected
        {
            get { return isConnected; }
            set
            {
                if (isConnected != value)
                {
                    isConnected = value;
                    OnPropertyChanged(nameof(IsConnected));
                    Title = String.Format("PPDExpansion{0}", isConnected ?
                        String.Format(" - {0}", Utility.Language["Connecting"]) : "");
                }
            }
        }

        public SinglePlayViewModel SinglePlayViewModel
        {
            get;
            private set;
        }

        public MultiPlayViewModel MultiPlayViewModel
        {
            get;
            private set;
        }

        public bool IsMulti
        {
            get { return isMulti; }
            set
            {
                if (isMulti != value)
                {
                    isMulti = value;
                    OnPropertyChanged(nameof(IsMulti));
                }
            }
        }

        public bool ShowMyHighScore
        {
            get { return showMyHighScore; }
            set
            {
                if (showMyHighScore != value)
                {
                    PPDExpansionSetting.Setting.ShowMyHighScore = showMyHighScore = value;
                    OnPropertyChanged(nameof(ShowMyHighScore));
                }
            }
        }

        public bool ShowWebHighScore
        {
            get { return showWebHighScore; }
            set
            {
                if (showWebHighScore != value)
                {
                    PPDExpansionSetting.Setting.ShowWebHighScore = showWebHighScore = value;
                    OnPropertyChanged(nameof(ShowWebHighScore));
                }
            }
        }

        public bool ShowCurrentStatus
        {
            get { return showCurrentStatus; }
            set
            {
                if (showCurrentStatus != value)
                {
                    PPDExpansionSetting.Setting.ShowCurrentStatus = showCurrentStatus = value;
                    OnPropertyChanged(nameof(ShowCurrentStatus));
                }
            }
        }

        public bool ShowGridLines
        {
            get { return showGridLines; }
            set
            {
                if (showGridLines != value)
                {
                    PPDExpansionSetting.Setting.ShowGridLines = showGridLines = value;
                    OnPropertyChanged(nameof(ShowGridLines));
                }
            }
        }

        public bool ShowIIDXStyle
        {
            get { return showIIDXStyle; }
            set
            {
                if (showIIDXStyle != value)
                {
                    PPDExpansionSetting.Setting.ShowIIDXStyle = showIIDXStyle = value;
                    OnPropertyChanged(nameof(ShowIIDXStyle));
                }
            }
        }

        public bool ShowScoreDiff
        {
            get { return showScoreDiff; }
            set
            {
                if (showScoreDiff != value)
                {
                    PPDExpansionSetting.Setting.ShowScoreDiff = showScoreDiff = value;
                    OnPropertyChanged(nameof(ShowScoreDiff));
                }
            }
        }

        public int Width
        {
            get { return width; }
            set
            {
                if (width != value)
                {
                    width = value;
                    OnPropertyChanged(nameof(Width));
                }
            }
        }

        public int Height
        {
            get { return height; }
            set
            {
                if (height != value)
                {
                    height = value;
                    OnPropertyChanged(nameof(Height));
                }
            }
        }

        public ICommand CloseCommand
        {
            get
            {
                return closeCommand ??= new RelayCommand(
                    CloseCommand_Execute);
            }
        }

        public ICommand ClosedCommand
        {
            get
            {
                return closedCommand ??= new RelayCommand(
                    ClosedCommand_Execute);
            }
        }

        public MainWindowViewModel()
        {
            this.dispatcher = Dispatcher.CurrentDispatcher;
            Title = "PPDExpansion";
            Width = PPDExpansionSetting.Setting.Width;
            Height = PPDExpansionSetting.Setting.Height;
            ShowMyHighScore = PPDExpansionSetting.Setting.ShowMyHighScore;
            ShowWebHighScore = PPDExpansionSetting.Setting.ShowWebHighScore;
            ShowCurrentStatus = PPDExpansionSetting.Setting.ShowCurrentStatus;
            ShowGridLines = PPDExpansionSetting.Setting.ShowGridLines;
            ShowIIDXStyle = PPDExpansionSetting.Setting.ShowIIDXStyle;
            ShowScoreDiff = PPDExpansionSetting.Setting.ShowScoreDiff;
            SinglePlayViewModel = new ViewModel.SinglePlayViewModel(this);
            MultiPlayViewModel = new ViewModel.MultiPlayViewModel(this);
            host = new Host(App.Port);
            host.DataReceived += Host_DataReceived;
            host.Connected += Host_Connected;
            host.Disconnected += Host_Disconnected;
            host.Start();
        }

        void Host_Disconnected()
        {
            dispatcher.Invoke((Action)(() =>
            {
                IsConnected = false;
            }));
        }

        void Host_Connected()
        {
            dispatcher.Invoke((Action)(() =>
            {
                IsConnected = true;
            }));
        }

        void Host_DataReceived(PackableBase obj)
        {
            dispatcher.Invoke((Action)(() =>
            {
                if (obj is ScoreInfo scoreInfo)
                {
                    IsMulti = scoreInfo.PlayType == PlayType.MultiPlay;
                }

                if (IsMulti)
                {
                    MultiPlayViewModel.ProcessData(obj);
                }
                else
                {
                    SinglePlayViewModel.ProcessData(obj);
                }
            }));
        }

        private void CloseCommand_Execute()
        {
            WeakReferenceMessenger.Default.Send(new CloseWindowMessage(this));
        }

        /// <summary>
        /// PPD との接続を閉じます。
        /// </summary>
        public void Dispose()
        {
            host?.Dispose();
            host = null;
        }

        private void ClosedCommand_Execute()
        {
            Dispose();
            PPDExpansionSetting.Setting.Width = Width;
            PPDExpansionSetting.Setting.Height = Height;
        }
    }
}
