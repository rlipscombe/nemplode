using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using NEmplode.Async;

namespace NEmplode.IO
{
    public sealed class Pipeline : IDisposable
    {
        private readonly ManualResetEvent _timeout;
        private readonly Process _sourceProcess;
        private readonly ManualResetEvent _sourceProcessExited;
        private readonly Process _destinationProcess;
        private readonly ManualResetEvent _destinationProcessExited;
        private readonly WaitableQueue<string> _errorData;
        private readonly WaitableQueue<string> _outputData;

        public Pipeline(string sourceFileName, string sourceArguments,
                        string destinationFileName, string destinationArguments)
        {
            _timeout = new ManualResetEvent(false);

            _errorData = new WaitableQueue<string>();
            _outputData = new WaitableQueue<string>();

            _sourceProcessExited = new ManualResetEvent(false);
            _destinationProcessExited = new ManualResetEvent(false);

            _sourceProcess = new Process();
            _destinationProcess = new Process();

            _sourceProcess.StartInfo = GetProcessStartInfo(sourceFileName, sourceArguments);
            _sourceProcess.EnableRaisingEvents = true;

            _destinationProcess.StartInfo = GetProcessStartInfo(destinationFileName, destinationArguments);
            _destinationProcess.EnableRaisingEvents = true;
        }

        private ProcessStartInfo GetProcessStartInfo(string fileName, string arguments)
        {
            return new ProcessStartInfo
            {
                FileName = fileName,
                Arguments = arguments,

                // We don't want a visible window.
                CreateNoWindow = true,

                // In order to do redirection, we need to not use ShellExecute.
                UseShellExecute = false,

                // Redirect standard input. TODO: For the source process, we'll close the fd, since we're non-interactive.
                RedirectStandardInput = true,

                // Redirect standard output. For the source process, we'll pipe this into the destination process.
                RedirectStandardOutput = true,

                // Redirect standard error, so that we can display it.
                RedirectStandardError = true,
            };
        }

        public event EventHandler<LogEventArgs> LogMessage;

        private void OnLogMessage(string format, params object[] args)
        {
            OnLogMessage(new LogEventArgs(string.Format(format, args)));
        }

        private void OnLogMessage(LogEventArgs e)
        {
            EventHandler<LogEventArgs> handler = LogMessage;
            if (handler != null)
                handler(this, e);
        }

        public event EventHandler<ProcessOutputEventArgs> OutputDataReceived;

        private void OnOutputDataReceived(ProcessOutputEventArgs e)
        {
            EventHandler<ProcessOutputEventArgs> handler = OutputDataReceived;
            if (handler != null)
                handler(this, e);
        }

        public event EventHandler<ProcessOutputEventArgs> ErrorDataReceived;

        private void OnErrorDataReceived(ProcessOutputEventArgs e)
        {
            EventHandler<ProcessOutputEventArgs> handler = ErrorDataReceived;
            if (handler != null)
                handler(this, e);
        }

        public bool Run(CancellationToken cancellationToken)
        {
            // The error event is called from a background thread; we'll marshal it back to the main thread.
            _sourceProcess.ErrorDataReceived += (sender, e) => _errorData.Enqueue(e.Data);
            _sourceProcess.Exited += (sender, e) => _sourceProcessExited.Set();

            _sourceProcess.Start();
            _sourceProcess.BeginErrorReadLine();

            // The error and output data events are called from a background thread; we'll marshal them back to the main thread.
            _destinationProcess.ErrorDataReceived += (sender, e) => _errorData.Enqueue(e.Data);
            _destinationProcess.OutputDataReceived += (sender, e) => _outputData.Enqueue(e.Data);
            _destinationProcess.Exited += (sender, e) => _destinationProcessExited.Set();

            _destinationProcess.Start();
            _destinationProcess.BeginErrorReadLine();
            _destinationProcess.BeginOutputReadLine();

            var sourceOutput = _sourceProcess.StandardOutput.BaseStream;
            var destinationInput = _destinationProcess.StandardInput.BaseStream;

            const int bufferSize = 16384;
            var pipe = new AsyncPipe(sourceOutput, destinationInput, bufferSize);
            pipe.Start();

            return HandleEvents(_timeout, cancellationToken, _errorData, _outputData,
                                _sourceProcessExited, _destinationProcessExited, _sourceProcess, _destinationProcess, pipe);
        }

        private bool HandleEvents(
            ManualResetEvent timeout, CancellationToken cancel,
            WaitableQueue<string> errorData, WaitableQueue<string> outputData,
            ManualResetEvent sourceProcessExited, ManualResetEvent destinationProcessExited,
            Process sourceProcess, Process destinationProcess,
            AsyncPipe pipe)
        {
            var waitHandles = new[]
                {
                    timeout,
                    cancel.WaitHandle,
                    errorData.Available,
                    outputData.Available,
                    sourceProcessExited,
                    destinationProcessExited
                };

            bool result = true;
            int done = 0;
            while (done < 2)
            {
                var signal = WaitHandle.WaitAny(waitHandles);
                switch (signal)
                {
                    case 0: // Timeout.
                    case 1: // Cancellation.
                        {
                            ProcessExtensions.TerminateProcess(sourceProcess);
                            ProcessExtensions.TerminateProcess(destinationProcess);
                            done = 2;
                            result = false;
                        }
                        break;

                    case 2: // Error data received from either process.
                        {
                            errorData.Dequeue(x => OnErrorDataReceived(new ProcessOutputEventArgs(x)));
                        }
                        break;

                    case 3: // Output data received -- this is from the destination process.
                        {
                            outputData.Dequeue(x => OnOutputDataReceived(new ProcessOutputEventArgs(x)));
                        }
                        break;

                    case 4: // First process exited.
                        {
                            ProcessExtensions.WaitForExit(sourceProcess);
                            sourceProcessExited.Reset();

                            // When the source process quits, we need to close the pipe.
                            pipe.Stop();
                            sourceProcess.StandardOutput.Close();
                            destinationProcess.StandardInput.Close();
                            ++done;
                        }
                        break;

                    case 5: // Second process exited.
                        {
                            ProcessExtensions.WaitForExit(destinationProcess);
                            destinationProcessExited.Reset();
                            ++done;
                        }
                        break;
                }
            }

            ProcessExtensions.WaitForExit(sourceProcess);
            ProcessExtensions.WaitForExit(destinationProcess);

            errorData.Dequeue(x => OnErrorDataReceived(new ProcessOutputEventArgs(x)));
            outputData.Dequeue(x => OnOutputDataReceived(new ProcessOutputEventArgs(x)));

            return (result && sourceProcess.ExitCode == 0 && destinationProcess.ExitCode == 0);
        }

        private sealed class AsyncPipe
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

        public void Dispose()
        {
            _timeout.Dispose();

            _errorData.Dispose();
            _outputData.Dispose();

            _sourceProcessExited.Dispose();
            _destinationProcessExited.Dispose();

            _sourceProcess.Dispose();
            _destinationProcess.Dispose();
        }
    }
}