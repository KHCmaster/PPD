using ErrorHandle;
using System;
using System.Windows.Forms;

namespace PPDFramework
{
    /// <summary>
    /// エラーハンドラーです。
    /// </summary>
    public class ErrorHandlerPPD : ErrorHandler
    {
        /// <summary>
        /// エラーを処理します。
        /// </summary>
        /// <param name="e"></param>
        protected override void OnProcessError(Exception e)
        {
            if (e is PPDFramework.PPDException ppde)
            {
                ProcessPPDError(ppde);
            }
            else if (e != null)
            {
                ProcessFatalError(e);
            }
        }

        void ProcessPPDError(PPDFramework.PPDException ppde)
        {
            var message = PPDFramework.PPDExceptionContentProvider.Provider.GetContent(ppde.ExceptionType);
            string content = ppde.Detail;
            MessageBox.Show(message + "\n" + content);
        }

        void ProcessFatalError(Exception e)
        {
            var message = PPDFramework.PPDExceptionContentProvider.Provider.GetContent(PPDFramework.PPDExceptionType.FatalError);
            MessageBox.Show(message);
#if DEBUG
            MessageBox.Show(e.Message + "\n" + e.StackTrace);
#endif
        }
    }
}
