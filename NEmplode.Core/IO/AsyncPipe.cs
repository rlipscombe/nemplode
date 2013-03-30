using System;
using System.IO;

namespace NEmplode.IO
{
    internal sealed class AsyncPipe
    {
        private readonly Stream _source;
        private readonly Stream _destination;
        private readonly byte[] _buffer;
        private volatile bool _stop;

        public AsyncPipe(Stream source, Stream destination, int bufferSize)
        {
            _source = source;
            _destination = destination;
            _buffer = new byte[bufferSize];
        }

        public void Start()
        {
            _source.BeginRead(_buffer, 0, _buffer.Length, ReadCallback, null);
        }

        private void ReadCallback(IAsyncResult ar)
        {
            // End the read.
            int bytesRead = _source.EndRead(ar);
            if (bytesRead != 0 && !_stop)
            {
                // Issue an asynchronous write. Once the write's complete, we can issue another read.
                _destination.BeginWrite(_buffer, 0, bytesRead, WriteCallback, null);
            }
        }

        private void WriteCallback(IAsyncResult ar)
        {
            // End the write.
            _destination.EndWrite(ar);

            // Issue another read, if the source is still sending.
            if (!_stop)
                _source.BeginRead(_buffer, 0, _buffer.Length, ReadCallback, null);
        }

        public void Stop()
        {
            _stop = true;
        }
    }
}