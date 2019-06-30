using System;
using System.IO;

namespace PPDMultiServer.Tcp
{
    public class Logger
    {
        private StreamWriter writer;

        public Logger(string folderPath, string filePath)
        {
            filePath = Path.Combine(folderPath, filePath);
            writer = new StreamWriter(filePath, true);
        }

        public void AddLog(string text)
        {
            writer.Write(String.Format("[{0}]", DateTime.Now));
            writer.WriteLine(text);
            writer.Flush();
        }

        public void AddLog(string format, params object[] args)
        {
            AddLog(String.Format(format, args));
        }

        public void Close()
        {
            if (writer != null)
            {
                writer.Close();
                writer = null;
            }
        }
    }
}
