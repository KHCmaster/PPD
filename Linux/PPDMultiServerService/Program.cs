using PPDFrameworkCore;
using PPDMultiCommon.Web;
using PPDMultiServer.Tcp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;

namespace PPDMultiServerService
{
    // Win の Service と同じ処理を、Windows サービスではなくコンソールアプリとして行う。
    // 使い方: PPDMultiServerService <部屋情報の XML> [ログのフォルダー]
    // SIGTERM（systemctl stop）や Ctrl+C で部屋を閉じて終了する。
    static class Program
    {
        static PPDServer[] servers;
        static TimerManager timerManager;
        static DateTime lastGCDateTime = DateTime.Now;
        static volatile bool stopping;

        static int Main(string[] args)
        {
            if (args.Length == 0)
            {
                Console.Error.WriteLine("Usage: PPDMultiServerService <MultiRoomInfo.xml> [logDir]");
                return 2;
            }
            var roomInfoPath = Path.GetFullPath(args[0]);
            var logDir = args.Length > 1 ? args[1] : "";
            if (!File.Exists(roomInfoPath))
            {
                Console.Error.WriteLine($"roomInfoPath {roomInfoPath} does not exist.");
                return 1;
            }

            Logger logger = null;
            if (!String.IsNullOrEmpty(logDir))
            {
                Directory.CreateDirectory(logDir);
                logger = new Logger(logDir, "service.log");
            }

            using var stop = new ManualResetEventSlim();
            using var sigterm = PosixSignalRegistration.Create(PosixSignal.SIGTERM, Stop);
            using var sigint = PosixSignalRegistration.Create(PosixSignal.SIGINT, Stop);
            void Stop(PosixSignalContext context)
            {
                context.Cancel = true;
                stop.Set();
            }

            timerManager = new TimerManager(new GameTimer());
            var started = new List<PPDServer>();
            foreach (var room in ServiceRoomInfo.Parse(roomInfoPath))
            {
                var server = new PPDServer(room.Port, room.RegisterToPPD ? new WebManager() : null,
                    new RoomInfo(room.UserName, room.RoomName, room.Password, room.Port), timerManager, logDir, room.AllowedModIds);
                server.FailedToCreateRoom += (sender, e) => Console.WriteLine("Failed to create room {0}", ((PPDServer)sender).RoomInfo.RoomName);
                started.Add(server);
                server.Start();
                Thread.Sleep(1000);
                Console.WriteLine($"Room {room.Port} Started");
                logger?.AddLog($"Room {room.Port} Started");
            }
            servers = [.. started];

            var updateThread = new Thread(UpdateImpl) { IsBackground = true };
            updateThread.Start();
            stop.Wait();

            Console.WriteLine("Stopping");
            stopping = true;
            updateThread.Join();
            foreach (var server in servers)
            {
                server.Close();
            }
            ThreadManager.Instance.Dispose();
            logger?.AddLog("Stopped");
            logger?.Dispose();
            return 0;
        }

        static void UpdateImpl()
        {
            while (!stopping)
            {
                timerManager.Update();
                foreach (var server in servers)
                {
                    server.Update();
                }
                ThreadManager.Instance.Update();
                CheckGC();
                Thread.Sleep(1);
            }
        }

        static void CheckGC()
        {
            if (DateTime.Now - lastGCDateTime > TimeSpan.FromHours(1) && servers.Sum(s => s.ConnectionCount) == 0)
            {
                GC.Collect();
                lastGCDateTime = DateTime.Now;
            }
        }
    }
}
