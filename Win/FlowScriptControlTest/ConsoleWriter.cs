using System;
using System.IO;
using System.Text;
using System.Threading;

namespace FlowScriptControlTest
{
    class ConsoleWriter : StringWriter
    {
        public event Action<string> Output;

        private static TextWriter original;
        private Thread thread;
        private ManualResetEvent resetEvent;

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

        public void RestoreOriginal()
        {
            Console.SetOut(original);
        }

        public void Stop()
        {
            if (thread != null && thread.IsAlive)
            {
                resetEvent.Set();
                thread = null;
            }
            RestoreOriginal();
        }

        private void Work()
        {
            var lastLength = 0;
            while (true)
            {
                if (resetEvent.WaitOne(1))
                {
                    break;
                }
                try
                {
                    var builder = GetStringBuilder();
                    if (builder.Length > lastLength)
                    {
                        var str = builder.ToString();
                        var subStr = str.Substring(lastLength, str.Length - lastLength);
                        original.Write(subStr);
                        OnOutput(subStr);
                        lastLength = str.Length;
                    }
                }
                catch
                {
                }
            }
        }

        private void OnOutput(string str)
        {
            Output?.Invoke(str);
        }
    }
}
