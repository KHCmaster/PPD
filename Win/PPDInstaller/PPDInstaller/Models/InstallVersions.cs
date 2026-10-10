using PPDConfiguration;
using System.Diagnostics;
using System.IO;
using System.Reflection;

namespace PPDInstaller.Models
{
    /// <summary>
    /// インストールするソフトのバージョンです（なければ空の文字列）。
    /// </summary>
    sealed class InstallVersions
    {
        const string PPDexepath = "Data\\PPD\\PPD.exe";
        const string BMSTOPPDexepath = "Data\\BMSTOPPD\\BMSTOPPD.exe";
        const string Effect2DEditorexepath = "Data\\Effect2DEditor\\Effect2DEditor.exe";

        public string PPD { get; private init; } = "";

        public string BMSTOPPD { get; private init; } = "";

        public string Effect2DEditor { get; private init; } = "";

        public string Installer { get; private init; } = "";

        /// <summary>
        /// インストールするファイル（Data フォルダー）とインストーラー自身のバージョンを調べます。
        /// </summary>
        public static InstallVersions Read()
        {
            var location = Assembly.GetExecutingAssembly().Location;
            return new InstallVersions
            {
                PPD = GetFileVersion(PPDexepath),
                BMSTOPPD = GetFileVersion(BMSTOPPDexepath),
                Effect2DEditor = GetFileVersion(Effect2DEditorexepath),
                Installer = GetFileVersion(location)
            };
        }

        /// <summary>
        /// インストール先の install.info から、インストール済みのバージョンを読み込みます。
        /// </summary>
        /// <returns>ファイルがないときは null。</returns>
        public static InstallVersions ReadInstalled(string installDirectory)
        {
            var path = Path.Combine(installDirectory, "PPD", "install.info");
            if (!File.Exists(path))
            {
                return null;
            }
            var setting = new SettingReader(File.ReadAllText(path));
            return new InstallVersions
            {
                PPD = setting["PPDVersion"].Trim(),
                BMSTOPPD = setting["BMSTOPPDVersion"].Trim()
            };
        }

        private static string GetFileVersion(string path)
        {
            return File.Exists(path) ? FileVersionInfo.GetVersionInfo(path).FileVersion : "";
        }
    }
}
