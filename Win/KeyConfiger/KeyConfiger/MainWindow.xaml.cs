using KeyConfiger.Models;
using KeyConfiger.ViewModels;
using PPDInput;
using System;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Threading;
using DIKey = Vortice.DirectInput.Key;

namespace KeyConfiger
{
    /// <summary>
    /// ボタンに割り当てるキーボードのキーとゲームパッドのボタンを設定するウィンドウです。処理は <see cref="MainWindowViewModel"/> にあります。
    /// </summary>
    /// <remarks>
    /// DirectInput はウィンドウのハンドルが要るので、ウィンドウを表示してから作り、タイマーで ViewModel に入力を調べさせる。
    /// </remarks>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1001:Types that own disposable fields should be disposable", Justification = "input はウィンドウを閉じたとき（Closed）に破棄する")]
    public partial class MainWindow : Window, IDialogService
    {
        readonly DispatcherTimer inputTimer;
        InputDevice input;

        public MainWindow()
        {
            InitializeComponent();
            inputTimer = new DispatcherTimer(TimeSpan.FromMilliseconds(16), DispatcherPriority.Input, InputTimer_Tick, Dispatcher);
            inputTimer.Stop();
        }

        private MainWindowViewModel ViewModel
        {
            get { return (MainWindowViewModel)DataContext; }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            input = new InputDevice(new Input(new WindowInteropHelper(this).Handle));
            ViewModel.AttachInput(input);
            inputTimer.Start();
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            inputTimer.Stop();
            input?.Dispose();
            input = null;
        }

        private void InputTimer_Tick(object sender, EventArgs e)
        {
            ViewModel.PollInput();
        }

        // 行をクリックしても入力待ちにする
        private void KeyGrid_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            for (var element = e.OriginalSource as DependencyObject; element != null; element = VisualTreeHelper.GetParent(element))
            {
                if (element is System.Windows.Controls.DataGridRow)
                {
                    ViewModel.WaitForInputCommand.Execute(null);
                    return;
                }
            }
        }

        // 入力待ちの間は、押したキーで選択行が動かないようにする
        private void KeyGrid_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            e.Handled |= ViewModel.IsWaitingInput;
        }

        public string AskNewConfigName()
        {
            var window = new NewConfigWindow { Owner = this };
            return window.ShowDialog() == true ? window.SettingName : null;
        }

        public void ShowMessage(string text)
        {
            MessageBox.Show(this, text);
        }

        public bool Confirm(string text, string caption)
        {
            return MessageBox.Show(this, text, caption, MessageBoxButton.OKCancel) == MessageBoxResult.OK;
        }

        void IDialogService.Close()
        {
            Close();
        }

        /// <summary>
        /// PPDInput の入力を <see cref="IInputDevice"/> として使います。
        /// </summary>
        sealed class InputDevice(Input input) : IInputDevice, IDisposable
        {
            public string[] JoyStickNames
            {
                get { return input.JoyStickNames; }
            }

            public int CurrentJoyStickIndex
            {
                get { return input.CurrentJoyStickIndex; }
                set { input.CurrentJoyStickIndex = value; }
            }

            public void Load()
            {
                input.Load();
            }

            public bool TryGetPressedKey(out int key)
            {
                var result = input.GetPressedKey(out DIKey k);
                key = (int)k;
                return result;
            }

            public bool TryGetPressedButton(out int button)
            {
                return input.GetPressedButton(out button);
            }

            public void Dispose()
            {
                input.Dispose();
            }
        }
    }
}
