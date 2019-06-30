using GalaSoft.MvvmLight.Command;
using System.Windows.Input;

namespace PPDEditorCommon.Dialog.ViewModel
{
    abstract class TextSettingViewModel : SettingViewModelBase
    {
        private string text;
        private ICommand changedCommand;

        public string Text
        {
            get { return text; }
            set
            {
                if (this.text != value)
                {
                    this.text = value;
                    RaisePropertyChanged("Text");
                }
            }
        }

        public ICommand ChangedCommand
        {
            get
            {
                return changedCommand ?? (changedCommand = new RelayCommand(
                    ChangedCommand_Execute));
            }
        }

        protected TextSettingViewModel(SettingBase setting)
            : base(setting)
        {
        }

        protected abstract void Validate();

        private void ChangedCommand_Execute()
        {
            Validate();
        }
    }
}
