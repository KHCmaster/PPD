using PPDEditor.Forms;
using PPDEditor.ViewModels.Dialogs;
using PPDEditor.ViewModels.Docks;
using System.Windows;
using System.Windows.Documents;

namespace PPDEditor
{
    /// <summary>
    /// 歌詞を編集します。処理は <see cref="KasiEditorViewModel"/> にあります。
    /// </summary>
    public sealed partial class KasiEditor : DockView, IKasiEditorView
    {
        public KasiEditor()
        {
            InitializeComponent();
            AttachViewModel(new KasiEditorViewModel(this, () => (float)WindowUtility.Seekmain.Currenttime));
        }

        public KasiEditorViewModel ViewModel
        {
            get { return (KasiEditorViewModel)DataContext; }
        }

        public void SetSkin()
        {
            TextElement.SetForeground(inputPanel, WpfSkin.TransparentTextBrush);
        }

        public float[] GetKasiChangeWithinTime(float starttime, float endtime)
        {
            return ViewModel.GetKasiChangeWithinTime(starttime, endtime);
        }

        public string GetFormattedContent(float time)
        {
            return ViewModel.GetFormattedContent(time);
        }

        public void FocusWithTimeData(float time)
        {
            ViewModel.FocusWithTimeData(time);
        }

        public void ChangeTime(float previoustime, float newtime)
        {
            ViewModel.ChangeTime(previoustime, newtime);
        }

        public void Setdata(string s)
        {
            ViewModel.SetData(s);
        }

        public string Askcurrentkasi()
        {
            return ViewModel.GetCurrentKasi();
        }

        public string Savedata()
        {
            return ViewModel.SaveData();
        }

        public void Clear()
        {
            ViewModel.Clear();
        }

        void IKasiEditorView.ShowMessage(string text)
        {
            MessageBox.Show(Window.GetWindow(this), text);
        }

        string IKasiEditorView.AskNewText(string text)
        {
            var viewModel = new ChangeKasiViewModel(text);
            return WpfDialog.ShowDialog(new ChangeKasiWindow(viewModel)) ? viewModel.NewText : null;
        }

        void IKasiEditorView.ShowSelectedRow()
        {
            kasiGrid.ScrollIntoView(kasiGrid.SelectedItem);
            ShowDock();
        }

        void IKasiEditorView.FocusInput()
        {
            inputBox.Focus();
        }

        void IKasiEditorView.RefreshTimeline()
        {
            WindowUtility.Seekmain.DrawAndRefresh();
        }
    }
}
