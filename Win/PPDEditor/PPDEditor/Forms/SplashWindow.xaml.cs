using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;

namespace PPDEditor.Forms
{
    /// <summary>
    /// 起動中に表示するウィンドウです。メインのウィンドウの初期化を止めないように、別のスレッドで表示します。
    /// </summary>
    public partial class SplashWindow : Window
    {
        //Splashウィンドウ
        private static SplashWindow _form;
        //メインフォーム
        private static System.Windows.Forms.Form _mainForm;
        //Splashを表示するスレッド
        private static Thread _thread;
        //lock用のオブジェクト
        private static readonly Lock syncObject = new();
        //表示する前に閉じるように言われたか
        private static bool closeRequested;

        /// <summary>
        /// Splashウィンドウ
        /// </summary>
        public static SplashWindow Form
        {
            get { return _form; }
        }

        /// <summary>
        /// Splashウィンドウを表示する
        /// </summary>
        /// <param name="mainForm">メインフォーム</param>
        public static void ShowSplash(System.Windows.Forms.Form mainForm)
        {
            if (_form != null || _thread != null)
                return;

            _mainForm = mainForm;
            closeRequested = false;
            //メインフォームのActivatedイベントでSplashウィンドウを消す
            _mainForm?.Activated += MainForm_Activated;

            //スレッドの作成（WPF のウィンドウなので STA にする）
            _thread = new Thread(StartThread)
            {
                Name = "SplashForm"
            };
            _thread.SetApartmentState(ApartmentState.STA);
            _thread.Start();
        }

        /// <summary>
        /// Splashウィンドウを表示する
        /// </summary>
        public static void ShowSplash()
        {
            ShowSplash(null);
        }

        /// <summary>
        /// Splashウィンドウを消す
        /// </summary>
        public static void CloseSplash()
        {
            lock (syncObject)
            {
                closeRequested = true;
                if (_form != null)
                {
                    var form = _form;
                    // ウィンドウを閉じて、表示のスレッドのメッセージループを終わらせる
                    form.Dispatcher.BeginInvoke(() =>
                    {
                        form.Close();
                        form.Dispatcher.InvokeShutdown();
                    });
                }

                if (_mainForm != null)
                {
                    _mainForm.Activated -= MainForm_Activated;
                    //メインフォームをアクティブにする
                    _mainForm.Activate();
                }

                _form = null;
                _thread = null;
                _mainForm = null;
            }
        }

        private static readonly Queue<string> queue = new();
        public static void ChangeStatus(string status)
        {
            lock (syncObject)
            {
                if (_form != null)
                {
                    var form = _form;
                    form.Dispatcher.BeginInvoke(() => form.statusLabel.Text = status);
                }
                else
                {
                    queue.Enqueue(status);
                }
            }
        }

        //スレッドで開始するメソッド
        private static void StartThread()
        {
            var form = new SplashWindow();
            lock (syncObject)
            {
                if (closeRequested)
                {
                    // 表示する前に閉じるように言われた
                    queue.Clear();
                    return;
                }
                while (queue.Count > 0)
                {
                    form.statusLabel.Text = queue.Dequeue();
                }
                _form = form;
            }
            form.Show();
            Dispatcher.Run();
        }

        //Splashウィンドウがクリックされた時
        private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            //Splashウィンドウを閉じる
            CloseSplash();
        }

        //メインフォームがアクティブになった時
        private static void MainForm_Activated(object sender, EventArgs e)
        {
            //Splashウィンドウを閉じる
            CloseSplash();
        }

        private SplashWindow()
        {
            InitializeComponent();
            versionLabel.Text = String.Format("{0} {1}", Utility.Language["Version"], FileVersionInfo.GetVersionInfo(Environment.ProcessPath).ProductVersion);

#if DEBUG
#else
            this.Topmost = true;
#endif
        }
    }
}
