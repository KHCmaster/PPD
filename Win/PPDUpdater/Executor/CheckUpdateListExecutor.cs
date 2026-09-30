using PPDUpdater.Model;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Windows.Forms;
using System.Xml;

namespace PPDUpdater.Executor
{
    class CheckUpdateListExecutor : CExecutor
    {
        // HttpWebRequest と同じく Cookie は保持しない
        private static readonly HttpClient client = new HttpClient(new HttpClientHandler { UseCookies = false });

        public string Url
        {
            get;
            private set;
        }

        public List<UpdateInfo> UpdateInfos
        {
            get;
            private set;
        }

        public CheckUpdateListExecutor(string url, Control control)
            : base(control)
        {
            this.Url = url;
            UpdateInfos = new List<UpdateInfo>();
        }

        public override void Execute()
        {
            base.Execute();
            try
            {
                var webres = client.Send(new HttpRequestMessage(HttpMethod.Get, Url), HttpCompletionOption.ResponseHeadersRead);
                webres.EnsureSuccessStatusCode();
                var st = webres.Content.ReadAsStream();
                var temp = new Uri(Url);
                string host = temp.Scheme + "://" + temp.Host;
                for (int i = 0; i < temp.Segments.Length - 1; i++)
                {
                    host += temp.Segments[i];
                }
                var reader = XmlReader.Create(st);
                while (reader.Read())
                {
                    if (reader.IsStartElement("Update"))
                    {
                        var version = reader.GetAttribute("Version");
                        string path = host + reader.GetAttribute("Path");
                        var info = new UpdateInfo(version, path);
                        UpdateInfos.Add(info);
                    }
                }
                reader.Close();
                webres.Dispose();
                UpdateInfos.Sort(UpdateInfoComparer.Comparer);
                Success = true;
            }
            catch (Exception e)
            {
                ErrorLog = e.Message + "\r\n" + e.StackTrace;
                Success = false;
            }
            OnFinish();
        }
    }
}
