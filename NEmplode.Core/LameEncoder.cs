using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace NEmplode
{
    internal class LameEncoder : IMediaCodec
    {
        private readonly CodecProcess _inner;
        private readonly DelayedOutputStream _outputStream;
        private readonly TaskCompletionSource<string> _ready = new TaskCompletionSource<string>();
        private readonly string _tempFileName;

        public LameEncoder(string encoderFileName)
        {
            const string encoderArgumentsFormat = @"--silent --preset standard --id3v2-only --pad-id3v2-size 256 - ""{0}""";

            var result = Path.GetTempFileName();
            _tempFileName = result;
            var encoderArguments = string.Format(encoderArgumentsFormat, _tempFileName);

            _inner = new CodecProcess(encoderFileName, encoderArguments);
            _outputStream = new DelayedOutputStream(_ready.Task);
        }

        public Stream InputStream
        {
            get { return _inner.InputStream; }
        }

        public Stream OutputStream
        {
            get { return _outputStream; }
        }

        public Task Start(CancellationToken cancellationToken)
        {
            return _inner.Start(cancellationToken)
                         .ContinueWith(t => _ready.SetResult(_tempFileName));
        }

        event ErrorDataReceivedEventHandler IMediaCodec.ErrorDataReceived
        {
            add { _inner.ErrorDataReceived += value; }
            remove { _inner.ErrorDataReceived -= value; }
        }
    }
}