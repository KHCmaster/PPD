using System;
using System.Threading;
using System.Windows.Forms;

namespace FlowScriptControl
{
    public partial class LoadingForm : Form
    {
        private static LoadingForm _form;
        private static Form _mainForm;
        private static Thread _thread;
        private static readonly Lock syncObject = new();

        private static bool closeRequired;
        readonly System.Windows.Forms.Timer timer = new();

        public static LoadingForm Form
        {
            get { return _form; }
        }

        public static void ShowLoading(Form mainForm)
        {
            if (_form != null || _thread != null)
            {
                return;
            }

            closeRequired = false;

            _mainForm = mainForm;
            _mainForm?.Activated += MainForm_Activated;

            _thread = new System.Threading.Thread(new System.Threading.ThreadStart(StartThread))
            {
                Name = "LoadingForm"
            };
            _thread.Start();
        }

        public static void ShowLoading()
        {
            closeRequired = false;
            ShowLoading(null);
        }

        public static void CloseLoading()
        {
            closeRequired = true;
            lock (syncObject)
            {
                if (_mainForm != null)
                {
                    _mainForm.Activated -= MainForm_Activated;
                    _mainForm.Activate();
                }

                _form = null;
                _thread = null;
                _mainForm = null;
            }
        }

        public static void ChangeProgress(float progress)
        {
            lock (syncObject)
            {
                if (_form != null && _form.IsDisposed == false)
                {
                    _form.ChangeProgressInner(progress);
                }
            }
        }

        private void ChangeProgressInner(float progress)
        {
            if (_form.InvokeRequired)
            {
                _form.Invoke((Action<float>)ChangeProgressInner, progress);
                return;
            }
            else
            {
                _form.progressBar1.Value = (int)(100 * progress);
            }
        }

        private static void StartThread()
        {
            _form = new LoadingForm();
            try
            {
                Application.Run(_form);
            }
            catch
            {
            }
        }

        private static void Form_Click(object sender, EventArgs e)
        {
            CloseLoading();
        }

        private static void MainForm_Activated(object sender, EventArgs e)
        {
            CloseLoading();
        }

        private LoadingForm()
        {
            InitializeComponent();
            timer = new System.Windows.Forms.Timer
            {
                Interval = 1000
            };
            timer.Tick += Timer_Tick;
            timer.Start();
#if DEBUG
#else
            this.TopMost = true;
#endif
        }

        void Timer_Tick(object sender, EventArgs e)
        {
            if (closeRequired)
            {
                this.Close();
                timer.Stop();
                timer.Dispose();
            }
        }
    }
}
