using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace PPDEditor.Forms
{
    public partial class SplashForm : Form
    {
        private delegate void StatusChangeDelegate(string status);
        //Splashフォーム
        private static SplashForm _form;
        //メインフォーム
        private static Form _mainForm;
        //Splashを表示するスレッド
        private static System.Threading.Thread _thread;
        //lock用のオブジェクト
        private static readonly object syncObject = new object();

        /// <summary>
        /// Splashフォーム
        /// </summary>
        public static SplashForm Form
        {
            get { return _form; }
        }

        /// <summary>
        /// Splashフォームを表示する
        /// </summary>
        /// <param name="mainForm">メインフォーム</param>
        public static void ShowSplash(Form mainForm)
        {
            if (_form != null || _thread != null)
                return;

            _mainForm = mainForm;
            //メインフォームのActivatedイベントでSplashフォームを消す
            if (_mainForm != null)
            {
                _mainForm.Activated += _mainForm_Activated;
            }

            //スレッドの作成
            _thread = new System.Threading.Thread(
                new System.Threading.ThreadStart(StartThread))
            {
                Name = "SplashForm"
            };
            //_thread.IsBackground = true;
            //_thread.SetApartmentState(System.Threading.ApartmentState.STA);
            //スレッドの開始
            _thread.Start();
        }

        /// <summary>
        /// Splashフォームを表示する
        /// </summary>
        public static void ShowSplash()
        {
            ShowSplash(null);
        }

        /// <summary>
        /// Splashフォームを消す
        /// </summary>
        public static void CloseSplash()
        {
            lock (syncObject)
            {
                if (_form != null && _form.IsDisposed == false)
                {
                    //Splashフォームを閉じる
                    //Invokeが必要か調べる
                    if (_form.InvokeRequired)
                        _form.Invoke(new MethodInvoker(_form.Close));
                    else
                        _form.Close();
                }

                if (_mainForm != null)
                {
                    _mainForm.Activated -= _mainForm_Activated;
                    //メインフォームをアクティブにする
                    _mainForm.Activate();
                }

                _form = null;
                _thread = null;
                _mainForm = null;
            }
        }

        private static Queue<string> queue = new Queue<string>();
        public static void ChangeStatus(string status)
        {
            lock (syncObject)
            {
                if (_form != null && _form.IsDisposed == false)
                {
                    _form.InnerChangeStatus(status);
                }
                else
                {
                    queue.Enqueue(status);
                }
            }
        }

        private void InnerChangeStatus(string status)
        {
            if (_form.InvokeRequired)
            {
                _form.Invoke(new StatusChangeDelegate(InnerChangeStatus), status);
                return;
            }
            else
            {
                _form.statusLabel.Text = status;
            }
        }

        //スレッドで開始するメソッド
        private static void StartThread()
        {
            //Splashフォームを作成
            _form = new SplashForm();
            lock (syncObject)
            {
                while (queue.Count > 0)
                {
                    _form.InnerChangeStatus(queue.Dequeue());
                }
            }
            //Splashフォームを表示する
            Application.Run(_form);
        }

        //Splashフォームがクリックされた時
        private static void _form_Click(object sender, EventArgs e)
        {
            //Splashフォームを閉じる
            CloseSplash();
        }

        //メインフォームがアクティブになった時
        private static void _mainForm_Activated(object sender, EventArgs e)
        {
            //Splashフォームを閉じる
            CloseSplash();
        }

        private SplashForm()
        {
            InitializeComponent();
            versionLabel.Text = String.Format("{0} {1}", Utility.Language["Version"], Application.ProductVersion);
            versionLabel.Location = new Point(this.Width - versionLabel.Width - versionLabel.Margin.Right, versionLabel.Location.Y);

#if DEBUG
#else
            this.TopMost = true;            
#endif
        }
    }
}
