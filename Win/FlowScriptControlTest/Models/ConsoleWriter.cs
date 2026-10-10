using System;
using System.IO;
using System.Text;
using System.Threading;

namespace FlowScriptControlTest.Models
{
    class ConsoleWriter : StringWriter
    {
        public event Action<string> Output;

        private static readonly TextWriter original;
        private Thread thread;
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Usage", "CA2213:Disposable fields should be disposed", Justification = "転送のスレッドが使い終わったときに、そのスレッドで破棄する（Dispose で破棄すると、転送中のスレッドが破棄済みのイベントを使ってしまう）")]
        private readonly ManualResetEvent resetEvent;

        public override Encoding Encoding
        {
            get
            {
                return Encoding.UTF8;
            }
        }

        static ConsoleWriter()
        {
            original = Console.Out;
        }

        public ConsoleWriter()
        {
            Console.SetOut(this);
            resetEvent = new ManualResetEvent(false);
            thread = new Thread(Work)
            {
                IsBackground = true
            };
            thread.Start();
        }

        public static void RestoreOriginal()
        {
            Console.SetOut(original);
        }

        /// <summary>
        /// 出力の転送を止め、コンソールの出力先を元に戻します。転送のスレッドは、残りを出力してから終わります。
        /// </summary>
        public void Stop()
        {
            // 出力先を先に戻す（この後のコンソールへの出力が、閉じたライターに書かれないように）
            RestoreOriginal();
            if (thread != null)
            {
                thread = null;
                // 転送のスレッドは UI のスレッドに出力を渡すので、ここ（UI のスレッド）では終わるのを待たない。
                // イベントは転送のスレッドが終わるときに破棄する
                resetEvent.Set();
            }
        }

        private void Work()
        {
            var lastLength = 0;
            void flush()
            {
                try
                {
                    var builder = GetStringBuilder();
                    if (builder.Length > lastLength)
                    {
                        var str = builder.ToString();
                        var subStr = str[lastLength..];
                        original.Write(subStr);
                        OnOutput(subStr);
                        lastLength = str.Length;
                    }
                }
                catch
                {
                }
            }
            while (!resetEvent.WaitOne(1))
            {
                flush();
            }
            // 止める直前に書かれた分も出力する
            flush();
            resetEvent.Dispose();
        }

        private void OnOutput(string str)
        {
            Output?.Invoke(str);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                // イベントは転送のスレッドが破棄する
                Stop();
            }
            base.Dispose(disposing);
        }
    }
}
