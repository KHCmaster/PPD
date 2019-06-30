using PPDUpdater.Model;
using System;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Xml;

namespace PPDUpdater.Executor
{
    class CheckUpdateListExecutor : CExecutor
    {
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
                var webreq = (System.Net.HttpWebRequest)System.Net.WebRequest.Create(Url);
                var webres = (System.Net.HttpWebResponse)webreq.GetResponse();
                var st = webres.GetResponseStream();
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
                webres.Close();
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
