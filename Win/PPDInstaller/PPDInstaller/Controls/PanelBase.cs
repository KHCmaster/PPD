using PPDConfiguration;
using System.Windows.Forms;

namespace PPDInstaller.Controls
{
    public partial class PanelBase : UserControl
    {
        public IPanelManager PanelManager
        {
            get;
            set;
        }

        public virtual bool CanPrevious
        {
            get { return true; }
        }

        public virtual bool CanNext
        {
            get { return true; }
        }

        public virtual bool CanCancel
        {
            get { return true; }
        }

        public virtual bool IsNextVisible
        {
            get { return true; }
        }

        public virtual bool IsPreviousVisible
        {
            get { return true; }
        }

        public virtual bool IsCancelVisible
        {
            get { return true; }
        }

        public virtual string PreviousText
        {
            get;
            protected set;
        }

        public virtual string NextText
        {
            get;
            protected set;
        }

        public virtual string CancelText
        {
            get;
            protected set;
        }

        public PanelBase()
        {
            InitializeComponent();
        }

        public virtual void SetLang(SettingReader setting)
        {
            PreviousText = setting["Button3"];
            NextText = setting["Button2"];
            CancelText = setting["Button1"];
        }

        public virtual void OnShown(bool skip)
        {
        }
    }
}
