using System;
using System.Windows.Forms;

namespace PPDFramework
{
    /// <summary>
    /// 描画ループです（SharpDX.Desktop の RenderLoop.Run(control, callback, true) と同じ動作）。
    /// </summary>
    static class RenderLoop
    {
        /// <summary>
        /// コントロールを表示し、破棄されるまでメッセージの処理と描画を繰り返します。
        /// </summary>
        /// <param name="control">描画先のコントロール。</param>
        /// <param name="render">毎フレーム呼ぶ処理。</param>
        public static void Run(Control control, Action render)
        {
            if (control == null)
            {
                throw new ArgumentNullException(nameof(control));
            }
            if (render == null)
            {
                throw new ArgumentNullException(nameof(render));
            }

            var alive = true;
            void disposed(object sender, EventArgs e) => alive = false;
            control.Disposed += disposed;
            try
            {
                control.Show();
                while (true)
                {
                    Application.DoEvents();
                    if (!alive)
                    {
                        break;
                    }
                    render();
                }
            }
            finally
            {
                control.Disposed -= disposed;
            }
        }
    }
}
