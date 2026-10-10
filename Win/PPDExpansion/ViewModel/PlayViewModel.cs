using CommunityToolkit.Mvvm.ComponentModel;
using PPDExpansionCore;

namespace PPDExpansion.ViewModel
{
    abstract class PlayViewModel(MainWindowViewModel mainWindowViewModel) : ObservableObject
    {
        public abstract PlayType PlayType { get; }
        public abstract void ProcessData(PackableBase packable);

        public MainWindowViewModel MainWindowViewModel
        {
            get;
            private set;
        } = mainWindowViewModel;
    }
}
