using PPDPack;
using System;
using System.IO;
using System.Linq;
using System.Net.NetworkInformation;
using System.Net.Sockets;

namespace PPDExpansionCore.Tcp
{
    public class Base
    {
        protected int ParseInt(PPDPack.V2.PackReader reader, string keyName)
        {
            if (reader.Files.Any(f => f == keyName))
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

        protected byte[] Read(NetworkStream stream)
        {
            using (MemoryStream ms = new MemoryStream())
            {
                byte[] buffer = new byte[1024 * 1024];
                try
                {
                    do
                    {
                        var readSize = stream.Read(buffer, 0, buffer.Length);
                        if (readSize == 0)
                        {
                            OnClosed();
                            return new byte[0];
                        }
                        ms.Write(buffer, 0, readSize);
                    } while (stream.DataAvailable);
                    return ms.ToArray();
                }
                catch
                {
                    OnClosed();
                    return new byte[0];
                }
            }
        }

        protected virtual void OnClosed()
        {

        }

        public static bool IsListening(int port)
        {
            var ipprop = IPGlobalProperties.GetIPGlobalProperties();
            foreach (var listener in ipprop.GetActiveTcpListeners())
            {
                if (listener.Port == port)
                {
                    return true;
                }
            }
            return false;
        }
    }
}
