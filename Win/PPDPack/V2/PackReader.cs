using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace PPDPack.V2
{
    public class PackReader : IDisposable
    {
        public bool disposed;
        Stream stream;
        Dictionary<string, FileInfo> files;
        int headerOffset;
        bool fileOpened;

        public string[] Files
        {
            get
            {
                return files.Values.Select(f => f.FileName).ToArray();
            }
        }

        public uint FileSize
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
            files = new Dictionary<string, FileInfo>();
            AnalyzeHeader();
        }

        private void AnalyzeHeader()
        {
            if (stream.Length < PackUtility.FileSignature.Length + 8)
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

            byte[] bytes = new byte[4];
            stream.Read(bytes, 0, bytes.Length);
            FileSize = BitConverter.ToUInt32(bytes, 0);
            stream.Read(bytes, 0, bytes.Length);
            headerOffset = BitConverter.ToInt32(bytes, 0);
            if (stream.Length < FileSize)
            {
                throw new Exception("不完全なPACKファイルです");
            }
            stream.Seek(headerOffset, SeekOrigin.Begin);
            uint fileHeaderPos = (uint)PackUtility.FileSignature.Length + 4 + 4;
            while (stream.Position < stream.Length && stream.Position < FileSize)
            {
                byte[] bytes2 = new byte[2];
                stream.Read(bytes2, 0, bytes2.Length);
                var fileNameLength = BitConverter.ToUInt16(bytes2, 0);
                byte[] fileNameBytes = new byte[fileNameLength];
                stream.Read(fileNameBytes, 0, fileNameBytes.Length);
                var fileName = Encoding.UTF8.GetString(fileNameBytes);
                stream.Read(bytes, 0, bytes.Length);
                var fileSize = BitConverter.ToUInt32(bytes, 0);
                files.Add(fileName, new FileInfo(fileName, fileSize, fileHeaderPos));
                fileHeaderPos += fileSize;
            }
        }

        public PPDPackStreamReader Read(string fileName)
        {
            var info = files[fileName];
            if (info == null)
            {
                return null;
            }

            stream.Seek(info.Offset, SeekOrigin.Begin);

            var ret = new PPDPackStreamReader(stream, (int)info.FileSize, (int)info.Offset);
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
