using System;
using System.Diagnostics;
using System.Threading;
using NEmplode.Async;

namespace NEmplode.IO
{
    public sealed class CapturedProcess : IDisposable
    {
        private readonly ManualResetEvent _timeout;
        private readonly Process _process;
        private readonly ManualResetEvent _processExited;
        private readonly WaitableQueue<string> _errorData;
        private readonly WaitableQueue<string> _outputData;

        public CapturedProcess(string fileName, string arguments)
        {
            _timeout = new ManualResetEvent(false);

            _errorData = new WaitableQueue<string>();
            _outputData = new WaitableQueue<string>();

            _processExited = new ManualResetEvent(false);
            _process = new Process();

            _process.StartInfo = GetProcessStartInfo(fileName, arguments);
            _process.EnableRaisingEvents = true;
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

                // Redirect standard input. TODO: We'll close the fd, since we're non-interactive.
                RedirectStandardInput = true,

                // Redirect standard output, so that we can display it.
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
            // The error and output data events are called from a background thread; we'll marshal them back to the main thread.
            _process.ErrorDataReceived += (sender, e) => _errorData.Enqueue(e.Data);
            _process.OutputDataReceived += (sender, e) => _outputData.Enqueue(e.Data);
            _process.Exited += (sender, e) => _processExited.Set();

            OnLogMessage("{0} {1}", _process.StartInfo.FileName, _process.StartInfo.Arguments);
            _process.Start();
            _process.BeginErrorReadLine();
            _process.BeginOutputReadLine();

            return HandleEvents(_timeout, cancellationToken, _errorData, _outputData,
                                _processExited, _process);
        }

        private bool HandleEvents(
            ManualResetEvent timeout, CancellationToken cancel,
            WaitableQueue<string> errorData, WaitableQueue<string> outputData,
            ManualResetEvent processExited,
            Process process)
        {
            var waitHandles = new[]
                {
                    timeout,
                    cancel.WaitHandle,
                    errorData.Available,
                    outputData.Available,
                    processExited,
                };

            bool result = true;
            bool done = false;
            while (!done)
            {
                var signal = WaitHandle.WaitAny(waitHandles);
                switch (signal)
                {
                    case 0: // Timeout.
                    case 1: // Cancellation.
                        {
                            ProcessExtensions.TerminateProcess(process);
                            done = true;
                            result = false;
                        }
                        break;

                    case 2: // Error data received.
                        {
                            errorData.Dequeue(x => OnErrorDataReceived(new ProcessOutputEventArgs(x)));
                        }
                        break;

                    case 3: // Output data received.
                        {
                            outputData.Dequeue(x => OnOutputDataReceived(new ProcessOutputEventArgs(x)));
                        }
                        break;

                    case 4: // Process exited.
                        {
                            ProcessExtensions.WaitForExit(process);
                            processExited.Reset();
                            done = true;
                        }
                        break;
                }
            }

            ProcessExtensions.WaitForExit(process);

            errorData.Dequeue(x => OnErrorDataReceived(new ProcessOutputEventArgs(x)));
            outputData.Dequeue(x => OnOutputDataReceived(new ProcessOutputEventArgs(x)));

            return result && process.ExitCode == 0;
        }

        public void Dispose()
        {
            _timeout.Dispose();

            _errorData.Dispose();
            _outputData.Dispose();

            _processExited.Dispose();
            _process.Dispose();
        }
    }

    public class LogEventArgs : EventArgs
    {
        private readonly string _message;

        public LogEventArgs(string message)
        {
            _message = message;
        }

        public string Message
        {
            get { return _message; }
        }
    }
}
