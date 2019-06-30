using PPDFrameworkCore;
using PPDMultiCommon.Model;
using PPDMultiCommon.Web;

namespace PPDMultiServer.Tcp
{
    public interface IContextManager
    {
        WebManager WebManager
        {
            get;
        }

        ITimerManager TimerManager
        {
            get;
        }

        RoomInfo RoomInfo
        {
            get;
        }

        Logger Logger
        {
            get;
        }

        void OnFailedToCreateRoom();
        void Enqueue(NetworkData networkData);
        void PushContext(ServerContextBase context);
        void PopContext();
    }
}
