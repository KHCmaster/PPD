using System;
using System.Collections.Generic;
using System.Text;
using PPDFramework;
using System.IO;
using System.Globalization;

namespace PPD
{
    class KasiManager : UpdatableGameComponent
    {
        public delegate void KasiChangeEventHandler(string kasi);
        public event KasiChangeEventHandler KasiChanged;
        float nexttime = float.MaxValue;
        float starttime = float.MaxValue;
        SortedList<float, string> pool = new SortedList<float, string>();
        int index = 0;
        bool initialized = false;
        string kasi;
        public KasiManager(PPDGameUtility ppdgameutility)
        {
            string path = Path.Combine(ppdgameutility.SongInformation.DirectoryPath, "kasi.txt");
            if (File.Exists(path))
            {
                using (StreamReader sr = new StreamReader(path))
                {
                    string data = sr.ReadToEnd();
                    sr.Close();
                    data = data.Replace("\r\n", "\r").Replace("\r", "\n");
                    if (data == null || data == "") return;
                    string[] datas = data.Split('\n');
                    foreach (string eachdata in datas)
                    {
                        string[] temp = eachdata.Split(':');
                        try
                        {
                            if (temp.Length >= 2) pool.Add(float.Parse(temp[0], CultureInfo.InvariantCulture), temp[1]);
                            else pool.Add(float.Parse(temp[0], CultureInfo.InvariantCulture), "");
                        }
                        catch
                        {
                        }
                    }
                    if (pool.Count > 0)
                    {
                        nexttime = pool.Keys[0];
                        starttime = nexttime;
                    }
                    initialized = true;
                }
            }
        }

        public string Kasi
        {
            get
            {
                return kasi;
            }
            set
            {
                kasi = value;
                if (KasiChanged != null) KasiChanged.Invoke(kasi);
            }
        }

        private void ChangeKasi(float time)
        {
            while (time >= nexttime && index < pool.Count)
            {
                Kasi = pool.Values[index];
                index++;
                if (index < pool.Count) nexttime = pool.Keys[index];
            }
        }

        public void Seek(float time)
        {
            Retry();
            ChangeKasi(time);
        }

        public void Retry()
        {
            index = 0;
            nexttime = starttime;
            Kasi = "";
        }

        public override void Update()
        {
        }

        public void Update(float time)
        {
            if (initialized)
            {
                ChangeKasi(time);
            }
        }
    }
}
