using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace PPDPack
{
    public class PackWriter : IDisposable
    {
        public bool disposed;
        Stream stream;
        List<int> headerpos;
        List<int> sizes;

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
            headerpos = new List<int>();
            sizes = new List<int>();
        }

        private void WriteHeader(string[] filenames)
        {
            stream.Write(PackUtility.FileSignature, 0, PackUtility.FileSignature.Length);
            var bytestrings = new List<byte[]>();
            for (int i = 0; i < filenames.Length; i++)
            {
                string filename = filenames[i];
                while (Encoding.UTF8.GetByteCount(filename.ToCharArray()) > byte.MaxValue)
                {
                    filename = filename.Substring(0, filename.Length - 1);
                }
                var bytestring = Encoding.UTF8.GetBytes(filename.ToCharArray());
                bytestrings.Add(bytestring);
                stream.WriteByte((byte)bytestring.Length);
            }
            stream.WriteByte(0);
            byte[] intbyte = new byte[4];
            foreach (byte[] bytestring in bytestrings)
            {
                stream.Write(bytestring, 0, bytestring.Length);
                headerpos.Add((int)stream.Position);
                stream.Write(intbyte, 0, intbyte.Length);
            }
        }

        public IEnumerable<PPDPackStreamWriter> Write(string[] filenames)
        {
            WriteHeader(filenames);
            foreach (string filename in filenames)
            {
                long startpos = stream.Position;
                var st = new PPDPackStreamWriter(stream, filename);
                yield return st;
                var size = (int)(stream.Position - startpos);
                sizes.Add(size);
            }
            for (int i = 0; i < headerpos.Count; i++)
            {
                stream.Seek(headerpos[i], SeekOrigin.Begin);
                var bytes = BitConverter.GetBytes(sizes[i]);
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
