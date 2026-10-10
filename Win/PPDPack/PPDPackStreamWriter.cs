using System;
using System.IO;

namespace PPDPack
{
    public class PPDPackStreamWriter(Stream stream, string filename) : Stream
    {
        public Stream BaseStream
        {
            get;
            private set;
        } = stream;
        public string Filename
        {
            get;
            private set;
        } = filename;

        private readonly long startpos = stream.Position;

        public override bool CanRead
        {
            get { return false; }
        }

        public override bool CanSeek
        {
            get { return false; }
        }

        public override bool CanWrite
        {
            get { return false; }
        }

        public override void Flush()
        {
            return;
        }

        public override long Length
        {
            get { return 0; }
        }

        public override long Position
        {
            get
            {
                return BaseStream.Position - startpos;
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            return 0;
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            return 0;
        }

        public override void SetLength(long value)
        {
            return;
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            BaseStream.Write(buffer, offset, count);
        }
    }
}
