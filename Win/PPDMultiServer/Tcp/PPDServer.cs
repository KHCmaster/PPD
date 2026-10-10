using PPDFrameworkCore;
using PPDMultiCommon.Model;
using PPDMultiCommon.Tcp;
using PPDMultiCommon.Web;
using System;
using System.Collections.Generic;
using System.Threading;

namespace PPDMultiServer.Tcp
{
    public class PPDServer : IContextManager, IDisposable
    {
        private readonly Lock syncRoot = new();
        static readonly byte[] PPDPackV2DisconnectBytes = [80, 80, 68, 80, 65, 67, 75, 86, 50, 51, 0, 0, 0, 23, 0, 0, 0, 7, 0, 0, 0, 0, 0, 10, 0, 77, 101, 116, 104, 111, 100, 84, 121, 112, 101, 2, 0, 0, 0, 6, 0, 82, 101, 97, 115, 111, 110, 4, 0, 0, 0];

        public event EventHandler FailedToCreateRoom;

        readonly List<ServerContextBase> contexts;
        readonly List<ServerContextBase> afterPushContexts;
        readonly List<ServerContextBase> afterPopContexts;
        readonly Dictionary<ServerContextBase, ServerContextBase> contextChains;
        ServerContextBase currentContext;

        readonly Queue<NetworkData> hostHandledData;
        readonly Dictionary<int, DateTime> userLastUpdatedAt;
        readonly Dictionary<int, DateTime> delayedDisconnects;
        readonly TcpByteReader byteReader;
        Host host;
        static readonly Lock updateLock = new();

        public WebManager WebManager
        {
            get;
            private set;
        }

        public RoomInfo RoomInfo
        {
            get;
            private set;
        }

        public ITimerManager TimerManager
        {
            get;
            private set;
        }

        public Logger Logger
        {
            get;
            private set;
        }

        public int ConnectionCount
        {
            get
            {
                return host.ConnectionCount;
            }
        }

        public string[] AllowedModIds
        {
            get;
            private set;
        }

        /// <summary>
        /// 待ち受けるアドレスです。既定はすべてのアドレス（ほかのコンピューターからつなげる）。Start の前に設定します。
        /// </summary>
        public System.Net.IPAddress ListenAddress
        {
            get { return host.ListenAddress; }
            set { host.ListenAddress = value; }
        }

        public PPDServer(int port, WebManager webManager, RoomInfo roomInfo, ITimerManager timerManager)
            : this(port, webManager, roomInfo, timerManager, "", [])
        {

        }

        public PPDServer(int port, WebManager webManager, RoomInfo roomInfo, ITimerManager timerManager, string logDir, string[] allowedModIds)
        {
            RoomInfo = roomInfo;
            WebManager = webManager;
            TimerManager = timerManager;
            AllowedModIds = allowedModIds;
            if (!String.IsNullOrEmpty(logDir))
            {
                Logger = new Logger(logDir, String.Format("{0}.log", port));
            }
            contexts = [];
            afterPushContexts = [];
            afterPopContexts = [];
            hostHandledData = new Queue<NetworkData>();
            contextChains = [];
            userLastUpdatedAt = [];
            delayedDisconnects = [];
            byteReader = new TcpByteReader();
            byteReader.ByteReaded += ByteReader_ByteReaded;
            host = new Host
            {
                Port = port
            };
            host.ReadClient += Host_ReadClient;
            host.Closed += Host_Closed;
        }

        public void Start()
        {
            Logger?.AddLog("Start");
            host.Start();
            var menuContext = new MenuContext(this);
            InitializeServerContext(menuContext);
            contexts.Add(menuContext);
            menuContext.Start();
        }

        private void InitializeServerContext(ServerContextBase context)
        {
            context.ContextManager = this;
            context.Host = host;
        }

        public void Close()
        {
            if (host != null)
            {
                host.Dispose();
                host.ReadClient -= Host_ReadClient;
                host.Closed -= Host_Closed;
                host = null;
            }

            lock (updateLock)
            {
                for (int i = contexts.Count - 1; i >= 0; i--)
                {
                    contexts[i].Dispose();
                }
                contexts.Clear();
            }
            if (Logger != null)
            {
                Logger.AddLog("End");
                Logger.Close();
                // 閉じた後にもう一度 Close（Dispose）しても書かない
                Logger = null;
            }
        }

        public void Update()
        {
            lock (updateLock)
            {
                UpdateImpl();
            }
        }

        private void UpdateImpl()
        {
            while (hostHandledData.Count > 0)
            {
                NetworkData networkData = null;
                lock (syncRoot)
                {
                    networkData = hostHandledData.Dequeue();
                }
                foreach (ServerContextBase context in contexts)
                {
                    currentContext = context;
                    context.Process(networkData);
                }
                if (networkData is AddUserNetworkData data)
                {
                    lock (userLastUpdatedAt)
                    {
                        userLastUpdatedAt[data.Id] = DateTime.Now;
                    }
                }
                else if (networkData is OldProtocolNetworkData)
                {
                    host.Write(PPDPackV2DisconnectBytes, networkData.Id);
                    if (!delayedDisconnects.ContainsKey(networkData.Id))
                    {
                        delayedDisconnects.Add(networkData.Id, DateTime.Now);
                    }
                }
            }
            foreach (ServerContextBase context in contexts)
            {
                currentContext = context;
                context.Update();
            }
            foreach (ServerContextBase context in afterPushContexts)
            {
                contexts.Add(context);
            }
            foreach (ServerContextBase context in afterPopContexts)
            {
                contexts.Remove(context);
                if (contextChains.TryGetValue(context, out ServerContextBase parent))
                {
                    parent.OnChildPoped();
                    contextChains.Remove(context);
                }
            }
            afterPopContexts.Clear();
            afterPushContexts.Clear();

            lock (userLastUpdatedAt)
            {
                var removeConnections = new List<ConnectionBase>();
                var notExistUserIds = new List<int>();
                foreach (var kvp in userLastUpdatedAt)
                {
                    if (DateTime.Now - kvp.Value >= TimeSpan.FromMinutes(15))
                    {
                        var connection = host.GetConnection(kvp.Key);
                        if (connection != null)
                        {
                            hostHandledData.Enqueue(new DeleteUserNetworkData
                            {
                                Id = kvp.Key
                            });
                            removeConnections.Add(connection);
                        }
                        else
                        {
                            notExistUserIds.Add(kvp.Key);
                        }
                    }
                }
                foreach (var userId in notExistUserIds)
                {
                    userLastUpdatedAt.Remove(userId);
                }

                foreach (var connection in removeConnections)
                {
                    host.Close(connection);
                }
            }
            lock (delayedDisconnects)
            {
                var removeConnections = new List<ConnectionBase>();
                var notExistUserIds = new List<int>();
                foreach (var kvp in delayedDisconnects)
                {
                    if (DateTime.Now - kvp.Value >= TimeSpan.FromSeconds(1))
                    {
                        var connection = host.GetConnection(kvp.Key);
                        if (connection != null)
                        {
                            hostHandledData.Enqueue(new DeleteUserNetworkData
                            {
                                Id = kvp.Key
                            });
                            removeConnections.Add(connection);
                        }
                        else
                        {
                            notExistUserIds.Add(kvp.Key);
                        }
                    }
                }
                foreach (var userId in notExistUserIds)
                {
                    delayedDisconnects.Remove(userId);
                }
                foreach (var connection in removeConnections)
                {
                    host.Close(connection);
                }
            }
        }

        void Host_Closed(int id)
        {
            var networkData = new DeleteUserNetworkData
            {
                Id = id
            };

            lock (syncRoot)
            {
                hostHandledData.Enqueue(networkData);
            }
            lock (userLastUpdatedAt)
            {
                userLastUpdatedAt.Remove(id);
            }
        }

        void Host_ReadClient(byte[] data, int id)
        {
            byteReader.Read(data, id);
        }

        void ByteReader_ByteReaded(ReadInfo readInfo, int id)
        {
            var userInput = readInfo.NetworkData is PingNetworkData;
            readInfo.NetworkData.Id = id;
            if (readInfo.NetworkData is AddUserNetworkData addUser)
            {
                // キックした人を覚えるのに使う。送ってきた値ではなく、接続のアドレスにする
                addUser.Ip = host?.GetIp(id);
            }
            Enqueue(readInfo.NetworkData);
            if (userInput)
            {
                lock (userLastUpdatedAt)
                {
                    userLastUpdatedAt[id] = DateTime.Now;
                }
            }
        }

        public void Enqueue(NetworkData networkData)
        {
            lock (syncRoot)
            {
                hostHandledData.Enqueue(networkData);
            }
        }

        public void PushContext(ServerContextBase context)
        {
            lock (updateLock)
            {
                if (context != null && !afterPushContexts.Contains(context))
                {
                    InitializeServerContext(context);
                    afterPushContexts.Add(context);
                    contextChains.Add(context, currentContext);
                }
            }
        }

        public void PopContext()
        {
            lock (updateLock)
            {
                if (currentContext != null && !afterPopContexts.Contains(currentContext))
                {
                    afterPopContexts.Add(currentContext);
                }
            }
        }

        public void OnFailedToCreateRoom()
        {
            FailedToCreateRoom?.Invoke(this, EventArgs.Empty);
        }

        /// <summary>
        /// サーバーを終了します（Close と同じ）。
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// 破棄します。
        /// </summary>
        /// <param name="disposing">Dispose から呼ばれたときは true。</param>
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                Close();
            }
        }
    }
}
