using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;

namespace PPDFramework
{
    /// <summary>
    /// プロファイルマネージャー
    /// </summary>
    public class ProfileManager
    {
        List<Profile> profiles;
        int iter;
        static ProfileManager manager = new ProfileManager();
        private ProfileManager()
        {
            iter = 0;
            profiles = new List<Profile>();

            string directory = "profiles";

            var path = Path.Combine(directory, String.Format("profile_{0}.xml", PPDSetting.Setting.LangISO));
            if (File.Exists(path))
            {
                ReadProfile(path);
            }

            if (profiles.Count == 0)
            {
                profiles.Add(new Profile());
            }

            profiles[0].DisplayText = "";
            profiles[0].GodMode = false;
            profiles[0].CoolPoint = 5;
            profiles[0].GoodPoint = 2;
            profiles[0].SafePoint = 0;
            profiles[0].SadPoint = -20;
            profiles[0].WorstPoint = -51;

            iter = 0;
        }

        private void ReadProfile(string path)
        {
            using (XmlReader reader = XmlReader.Create(path))
            {
                while (reader.Read())
                {
                    if (reader.IsStartElement("Profile"))
                    {
                        try
                        {
                            var text = reader.GetAttribute("DisplayText");
                            int coolPoint = int.Parse(reader.GetAttribute("CoolPoint")),
                                goodPoint = int.Parse(reader.GetAttribute("GoodPoint")),
                                safePoint = int.Parse(reader.GetAttribute("SafePoint")),
                                sadPoint = int.Parse(reader.GetAttribute("SadPoint")),
                                worstPoint = int.Parse(reader.GetAttribute("WorstPoint"));
                            bool godMode = reader.GetAttribute("GodMode") == "1",
                                muteSE = reader.GetAttribute("MuteSE") == "1",
                                connect = reader.GetAttribute("Connect") == "1";

                            var p = new Profile
                            {
                                Connect = connect,
                                CoolPoint = coolPoint,
                                DisplayText = text,
                                GodMode = godMode,
                                GoodPoint = goodPoint,
                                MuteSE = muteSE,
                                SadPoint = sadPoint,
                                SafePoint = safePoint,
                                WorstPoint = worstPoint,
                                Index = iter
                            };

                            profiles.Add(p);
                            iter++;
                        }
                        catch
                        {
                        }
                    }
                }
            }
        }

        /// <summary>
        /// プロファイルマネージャー
        /// </summary>
        public static ProfileManager Instance
        {
            get
            {
                return manager;
            }
        }

        /// <summary>
        /// 標準プロファイル
        /// </summary>
        public Profile Default
        {
            get
            {
                return profiles[0];
            }
        }

        /// <summary>
        /// 次のプロファイル
        /// </summary>
        /// <returns></returns>
        public Profile Next()
        {
            iter++;
            if (iter >= profiles.Count) iter = 0;
            return profiles[iter];
        }

        /// <summary>
        /// 前のプロファイル
        /// </summary>
        /// <returns></returns>
        public Profile Previous()
        {
            iter--;
            if (iter < 0) iter = profiles.Count - 1;
            return profiles[iter];
        }

        /// <summary>
        /// 現在のプロファイル
        /// </summary>
        public Profile Current
        {
            get { return profiles[iter]; }
        }

        /// <summary>
        /// プロファイル配列
        /// </summary>
        public Profile[] Profiles
        {
            get
            {
                return profiles.ToArray();
            }
        }
    }
}
