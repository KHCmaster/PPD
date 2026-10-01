using System;
using System.IO;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace PPDInstaller.Executor
{
    partial class BMSTOPPDInstallExecutor(string dataDirectory, string installDirectory, Control control) : CExecutor(control)
    {
        readonly Regex[] BMSTOPPDIgnoreList = [
          SettingIniRegex()
        ];
        readonly string dataDirectory = dataDirectory;
        readonly string installDirectory = installDirectory;

        public override void Execute()
        {
            base.Execute();
            try
            {
                var dir = Path.Combine(installDirectory, "BMSTOPPD");
                if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);
                Utility.CopyDirectory(Path.Combine(dataDirectory, "BMSTOPPD"), dir, BMSTOPPDIgnoreList);
                Success = true;
            }
            catch (Exception e)
            {
                ErrorLog = string.Format("BMSTOPPD Install Error:\n{0}\n{1}", e.Message, e.StackTrace);
            }
            OnFinish();
        }

        [GeneratedRegex("setting\\.ini", RegexOptions.IgnoreCase)]
        private static partial Regex SettingIniRegex();
    }
}
