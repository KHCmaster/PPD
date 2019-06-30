using PPDPack;
using System;
using System.IO;
using System.Net.NetworkInformation;
using System.Text;

namespace PPDCore
{
    public abstract class TcpDebugControllerBase
    {
        protected const int Port = 54201;
        protected const string LocalHost = "localhost";
        protected const string MethodName = "MethodName";
        protected const string SourceChangedEvent = "SourceChangedEvent";
        protected const string OperationAcceptedEvent = "OperationAcceptedEvent";
        protected const string OperationWaitedEvent = "OperationWaitedEvent";
        protected const string ErrorOccurredEvent = "ErrorOccurredEvent";
        protected const string LogReceivedEvent = "LogReceivedEvent";
        protected const string SerializedText = "SerializedText";
        protected const string FileName = "FileName";
        protected const string GetBreakPoints = "GetBreakPoints";
        protected const string GetFileList = "GetFileList";
        protected const string Return = "Return";
        protected const string ContinueMethod = "ContinueMethod";
        protected const string StepInMethod = "StepInMethod";
        protected const string AddBreakPointMethod = "AddBreakPointMethod";
        protected const string RemoveBreakPointMethod = "RemoveBreakPointMethod";
        protected const string ChangeSourceMethod = "ChangeSourceMethod";
        protected const string SourceId = "SourceId";
        protected const string ErrorText = "ErrorText";
        protected const string ErrorId = "ErrorId";
        protected const string LogType = "LogType";
        protected const string LogText = "LogText";

        protected void WriteString(PPDPackStreamWriter writer, string value)
        {
            var writeData = Encoding.UTF8.GetBytes(value);
            writer.Write(writeData, 0, writeData.Length);
        }

        protected void WriteInt(PPDPackStreamWriter writer, int value)
        {
            var writeData = BitConverter.GetBytes(value);
            writer.Write(writeData, 0, writeData.Length);
        }

        protected void WriteLong(PPDPackStreamWriter writer, long value)
        {
            var writeData = BitConverter.GetBytes(value);
            writer.Write(writeData, 0, writeData.Length);
        }

        protected void WriteUShort(PPDPackStreamWriter writer, ushort value)
        {
            var writeData = BitConverter.GetBytes(value);
            writer.Write(writeData, 0, writeData.Length);
        }

        protected void WriteBoolean(PPDPackStreamWriter writer, bool value)
        {
            var writeData = BitConverter.GetBytes(value);
            writer.Write(writeData, 0, writeData.Length);
        }

        protected string ParseString(PackReader reader, string keyName)
        {
            if (reader.FileList.Contains(keyName))
            {
                using (PPDPackStreamReader packStreamReader = reader.Read(keyName))
                {
                    using (StreamReader streamReader = new StreamReader(packStreamReader, Encoding.UTF8))
                    {
                        var ret = streamReader.ReadToEnd();
                        return ret;
                    }
                }
            }

            return null;
        }

        protected int ParseInt(PackReader reader, string keyName)
        {
            if (reader.FileList.Contains(keyName))
            {
                using (PPDPackStreamReader packStreamReader = reader.Read(keyName))
                {
                    byte[] data = new byte[sizeof(int)];
                    packStreamReader.Read(data, 0, data.Length);
                    var ret = BitConverter.ToInt32(data, 0);
                    return ret;
                }
            }

            return 0;
        }

        protected byte[] Read(Stream stream)
        {
            using (MemoryStream ms = new MemoryStream())
            {
                byte[] buffer = new byte[1024 * 1024];
                try
                {
                    var readSize = stream.Read(buffer, 0, buffer.Length);
                    if (readSize == 0)
                    {
                        OnClosed();
                        return new byte[0];
                    }
                    ms.Write(buffer, 0, readSize);
                    return ms.ToArray();
                }
                catch
                {
                    OnClosed();
                    return new byte[0];
                }
            }
        }

        protected byte[] GetBytes(string[] keys, Action<PPDPackStreamWriter> writeCallback)
        {
            using (MemoryStream stream = new MemoryStream())
            {
                var packWriter = new PackWriter(stream);
                foreach (PPDPackStreamWriter writer in packWriter.Write(keys))
                {
                    writeCallback(writer);
                }
                return stream.ToArray();
            }
        }

        protected virtual void OnClosed()
        {

        }

        public static bool IsListening()
        {
            var ipprop = IPGlobalProperties.GetIPGlobalProperties();
            foreach (var listener in ipprop.GetActiveTcpListeners())
            {
                if (listener.Port == Port)
                {
                    return true;
                }
            }
            return false;
        }
    }
}
