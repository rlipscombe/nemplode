using System;
using System.IO;

namespace ConvertMusic
{
    internal class ProcessPipe
    {
        private readonly byte[] _buffer;

        private readonly string _name;
        private readonly Stream _source;
        private readonly Stream _destination;

        public ProcessPipe(string name, Stream source, Stream destination, int bufferSize)
        {
            _name = name;
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
            int count = _source.EndRead(ar);
            if (count != 0)
            {
                _destination.BeginWrite(_buffer, 0, count, WriteCallback, null);
            }
            else
            {
                _destination.Close();
            }
        }

        private void WriteCallback(IAsyncResult ar)
        {
            _destination.EndWrite(ar);
            _source.BeginRead(_buffer, 0, _buffer.Length, ReadCallback, null);
        }

        public void Stop()
        {
            _source.Close();
            _destination.Close();
        }
    }
}