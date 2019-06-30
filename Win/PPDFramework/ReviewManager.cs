using System;
using System.Collections.Generic;
using System.IO;

namespace PPDFramework
{
    /// <summary>
    /// レビューデータのクラスです。
    /// </summary>
    public class ReviewManager
    {
        const string folderName = "Review_Preset";
        const string fileName = "Preset_{0}.txt";


        private static ReviewManager reviewManager = new ReviewManager();

        private string[] presets;

        /// <summary>
        /// レビューマネージャーのインスタンスです。
        /// </summary>
        public static ReviewManager Instance
        {
            get
            {
                return reviewManager;
            }
        }

        /// <summary>
        /// プリセットの文字列リストを取得します。
        /// </summary>
        public string[] Presets
        {
            get
            {
                return presets;
            }
        }


        private ReviewManager()
        {
            var list = new List<string>();
            var filePath = Path.Combine(folderName, String.Format(fileName, PPDSetting.Setting.LangISO));
            if (File.Exists(filePath))
            {
                try
                {
                    foreach (string str in File.ReadAllLines(filePath))
                    {
                        if (str.StartsWith("#") || str.StartsWith(";"))
                        {
                            continue;
                        }

                        list.Add(str);
                    }
                }
                catch
                {

                }
            }

            presets = list.ToArray();
        }
    }
}
