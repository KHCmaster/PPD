using System;
using System.IO;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace PPDInstaller.Executor
{
    class PPDInstallExecutor : CExecutor
    {

        Regex[] PPDIgnoreList = {
        new Regex("PPD\\.ini",RegexOptions.IgnoreCase),        new Regex("keyconfig\\.ini",RegexOptions.IgnoreCase)            /*"0.profile",
            "1.profile",
            "2.profile",
            "3.profile",
            "4.profile"*/
        };

        string dataDirectory;
        string installDirectory;
        public PPDInstallExecutor(string dataDirectory, string installDirectory, Control control)
            : base(control)
        {
            this.dataDirectory = dataDirectory;
            this.installDirectory = installDirectory;
        }

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
    }
}
