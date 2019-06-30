using PPDFrameworkCore;
using PPDMultiCommon.Model;
using PPDMultiCommon.Tcp;
using PPDMultiCommon.Web;
using System;
using System.Collections.Generic;

namespace PPDMultiServer.Tcp
{
    public class PPDServer : IContextManager
    {
        static byte[] PPDPackV2DisconnectBytes = { 80, 80, 68, 80, 65, 67, 75, 86, 50, 51, 0, 0, 0, 23, 0, 0, 0, 7, 0, 0, 0, 0, 0, 10, 0, 77, 101, 116, 104, 111, 100, 84, 121, 112, 101, 2, 0, 0, 0, 6, 0, 82, 101, 97, 115, 111, 110, 4, 0, 0, 0 };

        public event EventHandler FailedToCreateRoom;

        List<ServerContextBase> contexts;
        List<ServerContextBase> afterPushContexts;
        List<ServerContextBase> afterPopContexts;
        Dictionary<ServerContextBase, ServerContextBase> contextChains;
        ServerContextBase currentContext;

        Queue<NetworkData> hostHandledData;
        Dictionary<int, DateTime> userLastUpdatedAt;
        Dictionary<int, DateTime> delayedDisconnects;
        TcpByteReader byteReader;
        Host host;
        static readonly Object updateLock = new object();

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

        public PPDServer(int port, WebManager webManager, RoomInfo roomInfo, ITimerManager timerManager)
            : this(port, webManager, roomInfo, timerManager, "", new string[0])
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
            contexts = new List<ServerContextBase>();
            afterPushContexts = new List<ServerContextBase>();
            afterPopContexts = new List<ServerContextBase>();
            hostHandledData = new Queue<NetworkData>();
            contextChains = new Dictionary<ServerContextBase, ServerContextBase>();
            userLastUpdatedAt = new Dictionary<int, DateTime>();
            delayedDisconnects = new Dictionary<int, DateTime>();
            byteReader = new TcpByteReader();
            byteReader.ByteReaded += byteReader_ByteReaded;
            host = new Host
            {
                Port = port
            };
            host.ReadClient += host_ReadClient;
            host.Closed += host_Closed;
        }

        public void Start()
        {
            if (Logger != null)
            {
                Logger.AddLog("Start");
            }
            host.Start();
            var menuContext = new MenuContext(null, this);
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
                host.Stop();
                host.ReadClient -= host_ReadClient;
                host.Closed -= host_Closed;
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
                lock (this)
                {
                    networkData = hostHandledData.Dequeue();
                }
                foreach (ServerContextBase context in contexts)
                {
                    currentContext = context;
                    context.Process(networkData);
                }
                if (networkData is AddUserNetworkData)
                {
                    lock (userLastUpdatedAt)
                    {
                        userLastUpdatedAt[((AddUserNetworkData)networkData).Id] = DateTime.Now;
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
                if (contextChains.ContainsKey(context))
                {
                    ServerContextBase parent = contextChains[context];
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

        void host_Closed(int id)
        {
            var networkData = new DeleteUserNetworkData
            {
                Id = id
            };

            lock (this)
            {
                hostHandledData.Enqueue(networkData);
            }
            lock (userLastUpdatedAt)
            {
                userLastUpdatedAt.Remove(id);
            }
        }

        void host_ReadClient(byte[] data, int id)
        {
            byteReader.Read(data, id);
        }

        void byteReader_ByteReaded(ReadInfo readInfo, int id)
        {
            var userInput = readInfo.NetworkData is PingNetworkData;
            readInfo.NetworkData.Id = id;
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
            lock (this)
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
    }
}
