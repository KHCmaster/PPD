using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace PPDPack.V2
{
    public class PackWriter : IDisposable
    {
        public bool disposed;
        Stream stream;

        List<FileInfo> files;

        public PackWriter(string filename)
        {
            InnerStruct(File.Open(filename, FileMode.Create));
        }

        public PackWriter(Stream stream)
        {
            InnerStruct(stream);
        }

        private void InnerStruct(Stream stream)
        {
            this.stream = stream;
            files = new List<FileInfo>();
            stream.Write(PackUtility.FileSignature, 0, PackUtility.FileSignature.Length);
            uint temp = 0;
            var bytes = BitConverter.GetBytes(temp);
            stream.Write(bytes, 0, bytes.Length);
            stream.Write(bytes, 0, bytes.Length);
        }

        public void Write(string fileName, Action<PPDPackStreamWriter> callback)
        {
            long startpos = stream.Position;
            if (callback != null)
            {
                var st = new PPDPackStreamWriter(stream, fileName);
                callback(st);
            }
            var size = (uint)(stream.Position - startpos);
            files.Add(new FileInfo(fileName, size));
        }

        private void WriteHeader()
        {
            var sum = (uint)files.Sum(f => f.FileSize);
            uint fileSize = sum + (uint)files.Sum(f => 2 + Encoding.UTF8.GetBytes(f.FileName).Length + 4);
            fileSize += (uint)PackUtility.FileSignature.Length + 4 + 4;
            stream.Seek(PackUtility.FileSignature.Length, SeekOrigin.Begin);
            var bytes = BitConverter.GetBytes(fileSize);
            stream.Write(bytes, 0, bytes.Length);
            bytes = BitConverter.GetBytes((uint)(PackUtility.FileSignature.Length + 4 + 4 + sum));
            stream.Write(bytes, 0, bytes.Length);
        }

        private void WriteFooter()
        {
            stream.Seek(0, SeekOrigin.End);
            foreach (var file in files)
            {
                var fileName = Encoding.UTF8.GetBytes(file.FileName);
                var size16 = (ushort)Math.Min(fileName.Length, ushort.MaxValue);
                var bytes = BitConverter.GetBytes(size16);
                stream.Write(bytes, 0, bytes.Length);
                stream.Write(fileName, 0, size16);
                bytes = BitConverter.GetBytes(file.FileSize);
                stream.Write(bytes, 0, bytes.Length);
            }
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
                        WriteFooter();
                        WriteHeader();
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
