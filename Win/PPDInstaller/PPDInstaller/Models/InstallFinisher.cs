using PPDConfiguration;
using System;
using System.Collections.Generic;
using System.IO;

namespace PPDInstaller.Models
{
    /// <summary>
    /// インストールした後の処理です（設定ファイルの言語とフォント、install.info、スタートメニュー）。
    /// </summary>
    static class InstallFinisher
    {
        static readonly string[] languageSettingFiles = ["PPD.ini", "PPDUpdater.ini", "PPDConfig.ini", "KeyConfiger.ini", "PPDEditor.ini", "Effect2DEditor.ini"];

        /// <summary>
        /// PPD をインストールしたときは、各ソフトの設定ファイルに言語を、PPD.ini にフォントを書き込みます。
        /// </summary>
        public static void WriteLanguage(string installDirectory, InstallInfo installInfo, string language, string fontName)
        {
            if (!installInfo.PPD)
            {
                return;
            }
            ReplaceSetting(Path.Combine(installDirectory, "PPD", "PPD.ini"), "fontname", fontName);
            foreach (var fileName in languageSettingFiles)
            {
                ReplaceSetting(Path.Combine(installDirectory, "PPD", fileName), "Language", language);
            }
        }

        private static void ReplaceSetting(string path, string key, string value)
        {
            if (!File.Exists(path))
            {
                return;
            }
            var setting = new SettingReader(File.ReadAllText(path));
            setting.ReplaceOrAdd(key, value);
            using SettingWriter sw = new(path, false);
            foreach (KeyValuePair<string, string> kvp in setting.Dictionary)
            {
                sw.Write(kvp.Key, kvp.Value);
            }
        }

        /// <summary>
        /// install.info に、インストールしたもの（前回までの分も含む）とバージョンを書き込みます。
        /// </summary>
        /// <param name="showError">エラーを知らせます。</param>
        public static void WriteInstallData(string installDirectory, InstallInfo installInfo, InstallVersions versions, Action<string> showError)
        {
            var path = Path.Combine(installDirectory, "PPD", "install.info");
            var installedInfo = new Dictionary<string, bool>();
            foreach (string key in InstallInfo.InstallationInfoData)
            {
                installedInfo.Add(key, false);
            }
            try
            {
                if (File.Exists(path))
                {
                    var setting = new SettingReader(File.ReadAllText(path));
                    foreach (string key in InstallInfo.InstallationInfoData)
                    {
                        installedInfo[key] |= setting[key] == "1";
                    }
                }

                foreach (string key in InstallInfo.InstallationInfoData)
                {
                    installedInfo[key] |= installInfo.GetInstalled(key);
                }
            }
            catch (Exception e)
            {
                showError(e.Message);
            }

            try
            {
                using var sw = new SettingWriter(path, false);
                foreach (KeyValuePair<string, bool> kvp in installedInfo)
                {
                    sw.Write(kvp.Key, kvp.Value);
                }
                if (versions.PPD != "")
                {
                    sw.Write("PPDVersion", versions.PPD);
                }
                if (versions.BMSTOPPD != "")
                {
                    sw.Write("BMSTOPPDVersion", versions.BMSTOPPD);
                }
                if (versions.Effect2DEditor != "")
                {
                    sw.Write("Effect2DEditorVersion", versions.Effect2DEditor);
                }
                sw.Write("InstallVersion", versions.Installer);
            }
            catch (Exception e)
            {
                showError(e.Message);
            }
        }

        /// <summary>
        /// スタートメニューにショートカットを作ります。失敗しても続けます。
        /// </summary>
        /// <param name="names">ショートカットの名前（設定、キー設定、譜面フォルダ）。</param>
        public static void RegisterStartMenu(string installDirectory, InstallInfo installInfo, (string Config, string KeyConfig, string SongsFolder) names)
        {
            if (!installInfo.RegisterToMenu)
            {
                return;
            }
            try
            {
                var main = Path.Combine(NativeMethods.GetCommonStartmenuPath(), "PPD");
                Directory.CreateDirectory(main);
                if (installInfo.PPD)
                {
                    var ppddir = Path.Combine(main, "PPD");
                    var ppdInstallDir = Path.Combine(installDirectory, "PPD");
                    Directory.CreateDirectory(ppddir);
                    CreateShortCut(Path.Combine(ppddir, "PPD.lnk"), Path.Combine(ppdInstallDir, "PPD.exe"), "", ppdInstallDir);
                    CreateShortCut(Path.Combine(ppddir, names.Config + ".lnk"), Path.Combine(ppdInstallDir, "PPDConfig.exe"), "", ppdInstallDir);
                    CreateShortCut(Path.Combine(ppddir, names.KeyConfig + ".lnk"), Path.Combine(ppdInstallDir, "KeyConfiger.exe"), "", ppdInstallDir);
                    CreateShortCut(Path.Combine(ppddir, "PPDeditor.lnk"), Path.Combine(ppdInstallDir, "PPDeditor.exe"), "", Path.Combine(installDirectory, "PPDeditor"));
                    CreateShortCut(Path.Combine(ppddir, names.SongsFolder + ".lnk"), "explorer.exe", @"/n," + Path.Combine(ppdInstallDir, "songs"), "");
                }
                if (installInfo.BMSTOPPD)
                {
                    var bmstoppddir = Path.Combine(main, "BMSTOPPD");
                    var bmstoppdInstallDir = Path.Combine(installDirectory, "BMSTOPPD");
                    Directory.CreateDirectory(bmstoppddir);
                    CreateShortCut(Path.Combine(bmstoppddir, "BMSTOPPD.lnk"), Path.Combine(bmstoppdInstallDir, "BMSTOPPD.exe"), "", bmstoppdInstallDir);
                }
            }
            catch
            {
            }
        }

        private static void CreateShortCut(string path, string targetPath, string arguments, string workingDirectory)
        {
            var wsc = new IWshRuntimeLibrary.WshShellClass();
            var ws = (IWshRuntimeLibrary.WshShortcut)wsc.CreateShortcut(path);
            ws.TargetPath = targetPath;
            ws.IconLocation = ws.TargetPath + ",0";
            ws.Arguments = arguments;
            ws.WorkingDirectory = workingDirectory;
            ws.Save();
        }
    }
}
