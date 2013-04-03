using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace NEmplode
{
    internal class DelayedOutputStream : Stream
    {
        private readonly Task<string> _ready;

        public DelayedOutputStream(Task<string> ready)
        {
            _ready = ready;
        }

        public override Task CopyToAsync(Stream destination, int bufferSize, CancellationToken cancellationToken)
        {
            var complete = new TaskCompletionSource<string>();

            // Note that we don't use the CancellationToken except for the CopyToAsync -- we need to do a bunch of cleanup, which we don't want canceled.
            _ready.ContinueWith(t =>
                {
                    var path = t.Result;
                    var stream = File.OpenRead(path);

                    stream.CopyToAsync(destination, bufferSize, cancellationToken)
                          .ContinueWith(t2 =>
                              {
                                  stream.Dispose();
                                  File.Delete(t.Result);
                                  complete.SetFromTask(t2);
                              });
                });

            return complete.Task;
        }

        public override void Flush()
        {
            throw new NotSupportedException();
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            throw new NotSupportedException();
        }

        public override void SetLength(long value)
        {
            throw new NotSupportedException();
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            throw new NotSupportedException();
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            throw new NotSupportedException();
        }

        public override bool CanRead
        {
            get { throw new NotSupportedException(); }
        }

        public override bool CanSeek
        {
            get { throw new NotSupportedException(); }
        }

        public override bool CanWrite
        {
            get { throw new NotSupportedException(); }
        }

        public override long Length
        {
            get { throw new NotSupportedException(); }
        }

        public override long Position
        {
            get { throw new NotSupportedException(); }
            set { throw new NotSupportedException(); }
        }
    }
}