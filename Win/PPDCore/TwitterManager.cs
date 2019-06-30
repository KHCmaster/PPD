using PPDConfiguration;
using PPDFrameworkCore;
using PPDTwitter;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Threading;

namespace PPDCore
{
    class TwitterManager
    {
        const string ConsumerKey = "";
        const string ConsumerSecret = "";
        const int RetryCount = 5;

        private string Token
        {
            get;
            set;
        }

        private string TokenSecret
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

        private static TwitterManager manager = new TwitterManager();
        private TwitterManager()
        {
            if (File.Exists("PPD.ini"))
            {
                var sr = new StreamReader("PPD.ini");
                var setting = new SettingReader(sr.ReadToEnd());
                sr.Close();
                Token = setting.ReadString("token");
                TokenSecret = setting.ReadString("tokensecret");
                IsAvailable |= (Token != "" && TokenSecret != "");
            }
        }

        public static TwitterManager Manager
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
                if (threads[i].ThreadState == ThreadState.Stopped)
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
                var twitterManager = new PPDTwitterManager(Token, TokenSecret, ConsumerKey, ConsumerSecret);
                if (File.Exists(filePath))
                {
                    using (FileStream fs = File.Open(filePath, FileMode.Open))
                    {
                        result = twitterManager.TweetWithMedia(status, fs);
                    }
                }
                else
                {
                    result = twitterManager.Tweet(status);
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
