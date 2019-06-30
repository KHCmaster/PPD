using PPDFramework;
using PPDFramework.Web;
using System;
using System.IO;

namespace PPD
{
    class SendErrorHandler : ErrorHandlerPPD
    {
        protected override void WriteError(Exception e)
        {
            base.WriteError(e);
            try
            {
                var sw = new StringWriter();
                WriteError(e, sw);
                sw.Write("\n");
                sw.Write(PPDSetting.Setting.CurrentRaw);
                sw.Write("\n");
                sw.Write(System.Environment.OSVersion);
            }
            catch
            {
            }
        }
    }
}
