using CommunityToolkit.Mvvm.Input;
using System.Windows.Input;

namespace PPDEditorCommon.Dialog.ViewModel
{
    abstract class TextSettingViewModel(SettingBase setting) : SettingViewModelBase(setting)
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
                    OnPropertyChanged("Text");
                }
            }
        }

        public ICommand ChangedCommand
        {
            get
            {
                return changedCommand ??= new RelayCommand(
                    ChangedCommand_Execute);
            }
        }

        protected abstract void Validate();

        private void ChangedCommand_Execute()
        {
            Validate();
        }
    }
}
