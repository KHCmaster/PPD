using System;
using System.IO;
using System.Reflection;

namespace PPDFrameworkCore.Web
{
#pragma warning disable RECS0014 // If all fields, properties and methods members are static, the class can be made static.
    /// <summary>
    /// PPDWebへのマネージャークラスです。
    /// </summary>
    public class WebManager
#pragma warning restore RECS0014 // If all fields, properties and methods members are static, the class can be made static.
    {
        private static string baseUrl;
        private static object baseUrlLock = new object();

        /// <summary>
        /// 既定のURLです。
        /// </summary>
        public static string BaseUrl
        {
            get
            {
                lock (baseUrlLock)
                {
                    if (String.IsNullOrEmpty(baseUrl))
                    {
                        var dllDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
                        var path = Path.Combine(dllDir, "endpoint");
                        if (File.Exists(path))
                        {
                            baseUrl = File.ReadAllText(path).Trim();
                        }
                    }
                    if (String.IsNullOrEmpty(baseUrl))
                    {
                        baseUrl = "https://projectdxxx.me";
                    }
                }
                return baseUrl;
            }
        }
    }
}
