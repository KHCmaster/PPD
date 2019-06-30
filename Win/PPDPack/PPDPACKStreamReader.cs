using System;
using System.IO;

namespace PPDPack
{
    public class PPDPackStreamReader : Stream
    {
        Stream _baseStream;
        int _length;
        int _offset;
        public PPDPackStreamReader(Stream baseStream, int length, int offset)
        {
            this._length = length;
            this._baseStream = baseStream;
            this._offset = offset;
        }
        public override bool CanRead
        {
            get { return true; }
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
            throw new NotImplementedException();
        }

        public override long Length
        {
            get { return _length; }
        }

        public override long Position
        {
            get
            {
                return _baseStream.Position - _offset;
            }
            set
            {
                if (value < 0)
                {
                    throw new ArgumentException("Positionに負の値が設定されました");
                }
                if (_baseStream.Length < _offset + value)
                {
                    throw new ArgumentException("Positionに長さを超える値が設定されました");
                }
                _baseStream.Position = _offset + value;
            }
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            if (offset < 0)
            {
                throw new ArgumentException("offsetが負です");
            }
            if (buffer == null)
            {
                throw new NullReferenceException("bufferがnullです");
            }
            if (count < 0)
            {
                throw new ArgumentException("countが負です");
            }
            count = (int)Math.Min(count, _offset + _length - _baseStream.Position);
            return _baseStream.Read(buffer, offset, count);
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            switch (origin)
            {
                case SeekOrigin.Begin:
                    if (offset < 0)
                    {
                        throw new ArgumentException("origin:beginに対してoffsetが負です");
                    }
                    if (offset > _length)
                    {
                        throw new ArgumentException("offsetがlengthよりも大きいです");
                    }
                    return _baseStream.Seek(_offset + offset, origin);
                case SeekOrigin.Current:
                    if (_baseStream.Position + offset < _offset)
                    {
                        throw new ArgumentException("offsetにより負の範囲にでます");
                    }
                    if (_baseStream.Position + offset > _offset + _length)
                    {
                        throw new ArgumentException("offsetによりlengthを超えます");
                    }
                    return _baseStream.Seek(offset, origin);
                case SeekOrigin.End:
                    if (offset > 0)
                    {
                        throw new ArgumentException("offsetによりlengthを超えます");
                    }
                    if (offset + _length < 0)
                    {
                        throw new ArgumentException("offsetにより負の範囲にでます");
                    }
                    return _baseStream.Seek(_offset + offset + _length, SeekOrigin.Begin);
            }
            return 0;
        }

        public override void SetLength(long value)
        {
            throw new NotImplementedException();
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            throw new NotImplementedException();
        }
    }
}
