using PPDInstaller.Controls;

namespace PPDInstaller
{
    public interface IPanelManager
    {
        void Next();
        void Previous();
        bool CanNext { get; }
        bool CanPrevious { get; }
        string DataDirectory { get; }

        T GetPanel<T>() where T : PanelBase;

        PanelBase CurrentPanel { get; }
    }
}
