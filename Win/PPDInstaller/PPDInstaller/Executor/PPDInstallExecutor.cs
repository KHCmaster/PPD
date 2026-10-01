using System;
using System.IO;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace PPDInstaller.Executor
{
    partial class PPDInstallExecutor(string dataDirectory, string installDirectory, Control control) : CExecutor(control)
    {

        readonly Regex[] PPDIgnoreList = [
            PPDIniRegex(),
            KeyConfigIniRegex()
            /*"0.profile",
            "1.profile",
            "2.profile",
            "3.profile",
            "4.profile"*/
        ];

        readonly string dataDirectory = dataDirectory;
        readonly string installDirectory = installDirectory;

        public override void Execute()
        {
            base.Execute();
            try
            {
                var dir = Path.Combine(installDirectory, "PPD");
                if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);
                Utility.CopyDirectory(Path.Combine(dataDirectory, "PPD"), dir, PPDIgnoreList);
                Success = true;
            }
            catch (Exception e)
            {
                ErrorLog = string.Format("PPD Install Error:\n{0}\n{1}", e.Message, e.StackTrace);
            }
            OnFinish();
        }

        [GeneratedRegex("PPD\\.ini", RegexOptions.IgnoreCase)]
        private static partial Regex PPDIniRegex();

        [GeneratedRegex("keyconfig\\.ini", RegexOptions.IgnoreCase)]
        private static partial Regex KeyConfigIniRegex();
    }
}
