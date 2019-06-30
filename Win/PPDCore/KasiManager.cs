using PPDCoreModel;
using PPDFramework;
using System.Collections.Generic;
using System.Globalization;
using System.IO;

namespace PPDCore
{
    class KasiManager : UpdatableGameComponent, ILylics
    {
        public delegate void KasiChangeEventHandler(string kasi);
        public event KasiChangeEventHandler KasiChanged;
        float nexttime = float.MaxValue;
        float starttime = float.MaxValue;
        SortedList<float, string> pool = new SortedList<float, string>();
        int index;
        bool initialized;
        string kasi;
        public KasiManager(PPDGameUtility ppdgameutility)
        {
            var path = Path.Combine(ppdgameutility.SongInformation.DirectoryPath, "kasi.txt");
            if (File.Exists(path))
            {
                using (StreamReader sr = new StreamReader(path))
                {
                    var data = sr.ReadToEnd();
                    sr.Close();
                    data = data.Replace("\r\n", "\r").Replace("\r", "\n");
                    if (string.IsNullOrEmpty(data)) return;
                    var datas = data.Split('\n');
                    foreach (string eachdata in datas)
                    {
                        var temp = eachdata.Split(':');
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

        public string Lylics
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
                Lylics = pool.Values[index];
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
            Lylics = "";
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
