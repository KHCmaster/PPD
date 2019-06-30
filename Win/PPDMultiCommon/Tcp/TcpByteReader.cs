using PPDMultiCommon.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace PPDMultiCommon.Tcp
{
    public delegate void ByteReadedEventHandler(ReadInfo readInfo, int id);

    public class TcpByteReader
    {
        static byte[] PPDPACK = { (byte)'P', (byte)'P', (byte)'D', (byte)'P', (byte)'A', (byte)'C', (byte)'K' };

        public event ByteReadedEventHandler ByteReaded;
        private Dictionary<int, byte[]> lastArrayDictionary = new Dictionary<int, byte[]>();
        private object lockObject = new object();

        public void Read(byte[] data, int id)
        {
            lock (lockObject)
            {
                if (lastArrayDictionary.TryGetValue(id, out byte[] lastArray))
                {
                    lastArrayDictionary.Remove(id);
                    byte[] copyArray = new byte[data.Length + lastArray.Length];
                    Array.Copy(lastArray, copyArray, lastArray.Length);
                    Array.Copy(data, 0, copyArray, lastArray.Length, data.Length);
                    data = copyArray;
                    lastArray = null;
                }
                if (data.Length > PPDPACK.Length)
                {
                    if (data.Take(PPDPACK.Length).SequenceEqual(PPDPACK))
                    {
                        if (ByteReaded != null)
                        {
                            ByteReaded.Invoke(new ReadInfo(new OldProtocolNetworkData()), id);
                            return;
                        }
                    }
                }

                while (data.Length > 0)
                {
                    try
                    {
                        var networkData = Parser.Parse(data, out byte[] rest);
                        data = rest;
                        if (ByteReaded != null)
                        {
                            ByteReaded.Invoke(new ReadInfo(networkData), id);
                        }
                    }
                    catch (ThreadAbortException)
                    {
                        return;
                    }
                    catch (Exception)
                    {
                        break;
                    }
                }

                if (data != null && data.Length > 0)
                {
                    lastArrayDictionary.Add(id, data);
                }
            }
        }
    }
}
