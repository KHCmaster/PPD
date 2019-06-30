using System;
using System.IO;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace PPDInstaller.Executor
{
    class BMSTOPPDInstallExecutor : CExecutor
    {
        Regex[] BMSTOPPDIgnoreList = {
          new Regex("setting\\.ini",RegexOptions.IgnoreCase)
        };
        string dataDirectory;
        string installDirectory;
        public BMSTOPPDInstallExecutor(string dataDirectory, string installDirectory, Control control)
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
    }
}
