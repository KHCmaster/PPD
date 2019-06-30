using System;
using System.Windows.Forms;

namespace PPDFramework
{
    /// <summary>
    /// フォーム拡張です。
    /// </summary>
    public static class FormExtention
    {
        /// <summary>
        /// インボークします。
        /// </summary>
        /// <param name="control"></param>
        /// <param name="action"></param>
        public static void Invoke(this Control control, Action action)
        {
            if (control.InvokeRequired && !control.IsDisposed)
            {
                try
                {
                    control.Invoke(action);
                }
                catch
                {
                }
            }
            else
            {
                action();
            }
        }
    }
}
