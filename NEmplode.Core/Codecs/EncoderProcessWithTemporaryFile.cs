using System.IO;
using System.Threading;
using System.Threading.Tasks;
using NEmplode.IO;

namespace NEmplode.Codecs
{
    internal abstract class EncoderProcessWithTemporaryFile : IMediaCodec
    {
        private readonly string _encoderFileName;

        private readonly TaskCompletionSource<string> _ready;
        private readonly DelayedOutputStream _outputStream;

        private CodecProcess _inner;

        protected EncoderProcessWithTemporaryFile(string encoderFileName)
        {
            _encoderFileName = encoderFileName;
            // TODO: Merge these.
            _ready = new TaskCompletionSource<string>();
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
            var tempFileName = Path.GetTempFileName();
            var encoderArguments = GetEncoderArguments(tempFileName);

            _inner = new CodecProcess(_encoderFileName, encoderArguments);
            _inner.ErrorDataReceived += (sender, e) => OnErrorDataReceived(e);
            return _inner.Start(cancellationToken)
                         .ContinueWith(t => _ready.SetResult(tempFileName));
        }

        private void OnErrorDataReceived(ErrorDataReceivedEventArgs e)
        {
            var handler = ErrorDataReceived;
            if (handler != null)
                handler(this, e);
        }

        protected abstract string GetEncoderArguments(string tempFileName);

        public event ErrorDataReceivedEventHandler ErrorDataReceived;
    }
}