using PPDConfiguration;
using PPDFrameworkCore;
using BlueSky;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Diagnostics;

namespace PPDCore
{
    class BlueSkyManager
    {
        const int RetryCount = 5;

        private string ID
        {
            get;
            set;
        }

        private string Password
        {
            get;
            set;
        }

        public bool IsAvailable
        {
            get;
            private set;
        }

        private List<Thread> threads = new List<Thread>();

        private static BlueSkyManager manager = new BlueSkyManager();
        private BlueSkyManager()
        {
            if (File.Exists("PPD.ini"))
            {
                var sr = new StreamReader("PPD.ini");
                var setting = new SettingReader(sr.ReadToEnd());
                sr.Close();
                ID = setting.ReadString("blueskyid");
                Password = setting.ReadString("blueskypassword");
                IsAvailable |= (ID != "" && Password != "");
            }
        }

        public static BlueSkyManager Manager
        {
            get
            {
                return manager;
            }
        }

        public void PostStatus(string status, string hashTag, string filePath, Action<bool> endCallBack)
        {
            for (int i = threads.Count - 1; i >= 0; i--)
            {
                if (threads[i].ThreadState == System.Threading.ThreadState.Stopped)
                {
                    threads[i].Interrupt();
                    threads[i].Join();
                    threads.RemoveAt(i);
                }
            }
            if (!hashTag.StartsWith(" "))
            {
                hashTag = " " + hashTag;
            }
            if (status.Length >= 112 - hashTag.Length)
            {
                status = status.Substring(0, 112 - hashTag.Length) + hashTag;
            }
            else
            {
                status += hashTag;
            }
            var thread = ThreadManager.Instance.GetThread(Post);
            threads.Add(thread);
            thread.Start(new ArrayList { status, endCallBack, filePath });
        }

        private void Post(object obj)
        {
            var list = obj as ArrayList;
            var status = list[0] as string;
            var action = list[1] as Action<bool>;
            var filePath = list[2] as string;
            int iter = 0;
            bool result;
            while (!(result = PostImpl(status, filePath)) && iter < RetryCount)
            {
                iter++;
            }
            action(result);
        }

        private bool PostImpl(string status, string filePath)
        {
            try
            {
                var result = false;
                var client = new Client($"{ID}.bsky.social", Password);
                if (File.Exists(filePath))
                {
                    try
                    {
                        var bytes = File.ReadAllBytes(filePath);
                        client.PostImage(status, bytes, "image/png").Wait();
                        result = true;
                    }
                    catch (Exception e)
                    {
                        Debug.WriteLine(e.Message);
                    }
                }
                else
                {
                    try
                    {
                        client.Post(status).Wait();
                        result = true;
                    }
                    catch (Exception e)
                    {
                        Debug.WriteLine(e.Message);
                    }
                }
                return result;
            }
            catch
            {
                return false;
            }
        }
    }
}
