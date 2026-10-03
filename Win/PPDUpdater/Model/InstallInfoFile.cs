using System;
using System.IO;

namespace PPDUpdater.Model
{
    /// <summary>
    /// インストールした内容を覚えておくファイル（install.info）を読み書きします。
    /// </summary>
    static class InstallInfoFile
    {
        const string fileName = "install.info";

        /// <summary>
        /// 読み込みます。ファイルがないときは何もインストールしていないことにします。
        /// </summary>
        public static InstallInfo Load()
        {
            var installInfo = new InstallInfo();
            if (!File.Exists(fileName))
            {
                return installInfo;
            }
            var setting = new SettingReader(File.ReadAllText(fileName));
            installInfo.PPD = setting["PPD"] == "1";
            installInfo.SharpDX = setting["SharpDX"] == "1";
            installInfo.DirectShowLib = setting["DirectShowLib"] == "1";
            installInfo.IPAFont = setting["IPAFont"] == "1";
            installInfo.PPDeditor = setting["PPDeditor"] == "1";
            installInfo.BMSTOPPD = setting["BMSTOPPD"] == "1";
            installInfo.Ffdshow = setting["ffdshow"] == "1";
            installInfo.MP4Splitter = setting["MP4Splitter"] == "1";
            installInfo.FLVSplitter = setting["FLVSplitter"] == "1";
            installInfo.Effect2DEditor = setting["Effect2DEditor"] == "1";
            installInfo.PPDVersion = new VersionInfo(setting["PPDVersion"]);
            installInfo.PPDeditorVersion = new VersionInfo(setting["PPDeditorVersion"]);
            installInfo.BMSTOPPDVersion = new VersionInfo(setting["BMSTOPPDVersion"]);
            installInfo.Effect2DEditorVersion = new VersionInfo(setting["Effect2DEditorVersion"]);
            installInfo.InstallVersion = new VersionInfo(setting["InstallVersion"]);
            return installInfo;
        }

        public static void Save(InstallInfo installInfo)
        {
            using StreamWriter sw = new(fileName);
            WriteData(sw, "PPD", installInfo.PPD);
            WriteData(sw, "SharpDX", installInfo.SharpDX);
            WriteData(sw, "DirectShowLib", installInfo.DirectShowLib);
            WriteData(sw, "IPAFont", installInfo.IPAFont);
            WriteData(sw, "PPDeditor", installInfo.PPDeditor);
            WriteData(sw, "ffdshow", installInfo.Ffdshow);
            WriteData(sw, "MP4Splitter", installInfo.MP4Splitter);
            WriteData(sw, "FLVSplitter", installInfo.FLVSplitter);
            WriteData(sw, "Effect2DEditor", installInfo.Effect2DEditor);
            WriteData(sw, "PPDVersion", installInfo.PPDVersion.ToString());
            WriteData(sw, "PPDeditorVersion", installInfo.PPDeditorVersion.ToString());
            WriteData(sw, "BMSTOPPDVersion", installInfo.BMSTOPPDVersion.ToString());
            WriteData(sw, "Effect2DEditorVersion", installInfo.Effect2DEditorVersion.ToString());
            WriteData(sw, "InstallVersion", installInfo.InstallVersion.ToString());
        }

        private static void WriteData(StreamWriter sw, string key, bool value)
        {
            WriteData(sw, key, value ? "1" : "0");
        }

        private static void WriteData(StreamWriter sw, string key, string value)
        {
            sw.Write(String.Format("[{0}]{1}", key, value));
            sw.WriteLine();
        }
    }
}
