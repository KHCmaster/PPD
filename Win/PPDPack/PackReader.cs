using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace PPDPack
{
    /// <summary>
    /// 
    /// </summary>
    /// <remarks>
    /// filetype...PPDPACK
    /// filenamelist...filename_length(MAX_BYTE)
    /// 0
    /// fileinfolist...filename,datasize(MAX_UINT32)
    /// data...
    /// </remarks>
    public class PackReader : IDisposable
    {
        public bool disposed;
        Stream stream;
        List<string> filenames;
        List<int> sizes;
        int headeroffset;
        bool fileOpened;

        public List<string> FileList
        {
            get
            {
                return filenames;
            }
        }

        public int LastIndex
        {
            get;
            private set;
        }

        public PackReader(string filepath)
        {
            if (!File.Exists(filepath))
            {
                throw new ArgumentException("ファイルが存在しません");
            }
            fileOpened = true;
            InnerStruct(File.Open(filepath, FileMode.Open));
        }

        public PackReader(Stream stream)
        {
            InnerStruct(stream);
        }

        private void InnerStruct(Stream stream)
        {
            this.stream = stream;
            filenames = new List<string>();
            sizes = new List<int>();
            AnalyzeHeader();
        }

        private void AnalyzeHeader()
        {
            if (stream.Length < PackUtility.FileSignature.Length + 1)
            {
                if (fileOpened)
                {
                    stream.Close();
                    stream.Dispose();
                    stream = null;
                }
                throw new Exception("PPDPACKファイルではありません");
            }

            byte[] header = new byte[PackUtility.FileSignature.Length];
            stream.Read(header, 0, header.Length);
            for (int i = 0; i < header.Length; i++)
            {
                if (header[i] != PackUtility.FileSignature[i])
                {
                    if (fileOpened)
                    {
                        stream.Close();
                        stream.Dispose();
                        stream = null;
                    }
                    throw new Exception("PPDPACKファイルではありません");
                }
            }

            var filenamelength = new List<byte>();
            var length = (byte)stream.ReadByte();
            while (length > 0)
            {
                if (stream.Position >= stream.Length)
                {
                    throw new Exception("不完全なPACKファイルです");
                }
                filenamelength.Add(length);
                length = (byte)stream.ReadByte();
            }

            int sizeSum = 0;
            foreach (byte namelength in filenamelength)
            {
                byte[] name = new byte[namelength];
                stream.Read(name, 0, name.Length);
                filenames.Add(Encoding.UTF8.GetString(name));
                byte[] size = new byte[4];
                stream.Read(size, 0, size.Length);
                var intSize = (int)BitConverter.ToUInt32(size, 0);
                sizes.Add(intSize);
                sizeSum += intSize;
            }
            headeroffset = (int)stream.Position;
            LastIndex = headeroffset + sizeSum;
        }

        public PPDPackStreamReader Read(string filename)
        {
            var index = filenames.FindIndex((st) => (st == filename));
            if (index < 0) return null;

            int offset = headeroffset;

            for (int i = 0; i < index; i++)
            {
                offset += sizes[i];
            }

            stream.Seek(offset, SeekOrigin.Begin);

            var ret = new PPDPackStreamReader(stream, sizes[index], offset);
            return ret;
        }

        public void Close()
        {
            Dispose();
        }

        #region IDisposable メンバ

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private void Dispose(bool disposing)
        {
            if (!this.disposed)
            {
                if (disposing)
                {
                    if (stream != null)
                    {
                        stream.Close();
                        stream = null;
                    }
                }
            }
            disposed = true;
        }

        #endregion
    }
}
