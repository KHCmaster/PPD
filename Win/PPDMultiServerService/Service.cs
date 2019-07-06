using PPDFrameworkCore;
using PPDMultiCommon.Web;
using PPDMultiServer.Tcp;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Reflection;
using System.ServiceProcess;
using System.Threading;

namespace PPDMultiServerService
{
    public partial class Service : ServiceBase
    {
        PPDServer[] servers;
        Thread updateThread;
        GameTimer gameTimer;
        TimerManager timerManager;
        DateTime lastGCDateTime = DateTime.Now;

        public Service()
        {
            InitializeComponent();
        }

        public Service(bool startService)
        {
            if (startService)
            {
                OnStart(null);
            }
        }

        protected override void OnStart(string[] args)
        {
            gameTimer = new GameTimer();
            timerManager = new TimerManager(gameTimer);
            updateThread = ThreadManager.Instance.GetThread(UpdateImpl);
            Logger logger = null;
            try
            {
                var roomInfoPath = ConfigurationManager.AppSettings["roomInfoPath"];
                var assemblyDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
                if (roomInfoPath.StartsWith("./") || roomInfoPath.StartsWith(@".\"))
                {
                    roomInfoPath = Path.Combine(assemblyDir, roomInfoPath);
                }
                if (!File.Exists(roomInfoPath))
                {
                    throw new Exception($"roomInfoPath ${roomInfoPath} does not exist.");
                }
                var rooms = ServiceRoomInfo.Parse(roomInfoPath);
                string logDir = ConfigurationManager.AppSettings["logDir"];
                if (!String.IsNullOrEmpty(logDir) && !Directory.Exists(logDir))
                {
                    Directory.CreateDirectory(logDir);
                    logger = new Logger(logDir, "service.log");
                }
                var servers = new List<PPDServer>();
                foreach (var room in rooms)
                {
                    var server = new PPDServer(room.Port, room.RegisterToPPD ? new WebManager() : null,
                        new RoomInfo(room.UserName, room.RoomName, room.Password, room.Port), timerManager, logDir, room.AllowedModIds);
                    server.FailedToCreateRoom += server_FailedToCreateRoom;
                    servers.Add(server);
                    server.Start();
                    Thread.Sleep(1000);
                    if (logger != null)
                    {
                        logger.AddLog($"Room {room.Port} Started");
                    }
                }
                this.servers = servers.ToArray();
                updateThread.Start();
            }
            catch (Exception e)
            {
                if (logger != null)
                {
                    logger.AddLog(e.Message);
                    logger.AddLog(e.StackTrace);
                }

                Console.WriteLine(e.Message);
                Console.WriteLine(e.StackTrace);
            }
        }

        void server_FailedToCreateRoom(object sender, EventArgs e)
        {
            Console.WriteLine("Failed to create room {0}", ((PPDServer)sender).RoomInfo.RoomName);
        }

        protected override void OnStop()
        {
            if (servers != null)
            {
                lock (servers)
                {
                    foreach (PPDServer server in servers)
                    {
                        server.Close();
                    }
                }
                servers = null;
            }
            ThreadManager.Instance.Dispose();
        }

        private void UpdateImpl()
        {
            try
            {
                while (true)
                {
                    timerManager.Update();
                    foreach (PPDServer server in servers)
                    {
                        server.Update();
                    }
                    ThreadManager.Instance.Update();
                    CheckGC();
                    Thread.Sleep(1);
                }
            }
            catch (ThreadAbortException)
            {

            }
        }

        private void CheckGC()
        {
            lock (servers)
            {
                if (DateTime.Now - lastGCDateTime > TimeSpan.FromHours(1) && servers.Sum(s => s.ConnectionCount) == 0)
                {
                    GC.Collect();
                    lastGCDateTime = DateTime.Now;
                }
            }
        }
    }
}
