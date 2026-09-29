using CommunityToolkit.Mvvm.ComponentModel;
using PPDExpansionCore;

namespace PPDExpansion.ViewModel
{
    abstract class PlayViewModel : ObservableObject
    {
        public abstract PlayType PlayType { get; }
        public abstract void ProcessData(PackableBase packable);

        public MainWindowViewModel MainWindowViewModel
        {
            get;
            private set;
        }

        protected PlayViewModel(MainWindowViewModel mainWindowViewModel)
        {
            MainWindowViewModel = mainWindowViewModel;
        }
    }
}
