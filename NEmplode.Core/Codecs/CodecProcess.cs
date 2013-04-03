using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using NEmplode.Processes;

namespace NEmplode.Codecs
{
    internal sealed class CodecProcess : IMediaCodec
    {
        private readonly Process _process;

        public CodecProcess(string fileName, string arguments)
        {
            _process = new Process
                {
                    StartInfo = GetProcessStartInfo(fileName, arguments),
                    EnableRaisingEvents = true
                };
        }

        public Stream InputStream
        {
            get { return _process.StandardInput.BaseStream; }
        }

        public Stream OutputStream
        {
            get { return _process.StandardOutput.BaseStream; }
        }

        public Task Start(CancellationToken cancellationToken)
        {
            var completion = new TaskCompletionSource<bool>();
            cancellationToken.Register(() => ProcessExtensions.Terminate(_process));

            // Hook up the events. We don't hook up OutputDataReceived, because that wants to treat the data as strings; we need binary.
            _process.ErrorDataReceived += (sender, e) => OnErrorDataReceived(new ErrorDataReceivedEventArgs(e.Data));
            _process.Exited += (sender, e) =>
            {
                _process.WaitForExit(1000);
                _process.StandardOutput.Close();

                if (_process.ExitCode == 0)
                    completion.SetResult(true);
                else if (cancellationToken.IsCancellationRequested)
                    completion.SetCanceled();
                else
                    completion.SetException(new CodecProcessFailedException(_process.ExitCode));
            };

            _process.Start();
            _process.PriorityClass = ProcessPriorityClass.BelowNormal;
            _process.BeginErrorReadLine();

            return completion.Task;
        }

        private static ProcessStartInfo GetProcessStartInfo(string fileName, string arguments)
        {
            return new ProcessStartInfo
                {
                    FileName = fileName,
                    Arguments = arguments,

                    // We don't want a visible window.
                    CreateNoWindow = true,

                    // In order to do redirection, we need to not use ShellExecute.
                    UseShellExecute = false,

                    // Redirect standard input. We'll be writing the WAV data into this.
                    RedirectStandardInput = true,

                    // Redirect standard output. This is what we're reading from.
                    RedirectStandardOutput = true,

                    // Redirect standard error, so that we can display it.
                    RedirectStandardError = true,
                };
        }

        public Func<ErrorDataReceivedEventArgs, ErrorDataReceivedEventArgs> ErrorDataFilter { get; set; }

        public event ErrorDataReceivedEventHandler ErrorDataReceived;

        private void OnErrorDataReceived(ErrorDataReceivedEventArgs e)
        {
            if (ErrorDataFilter != null)
                e = ErrorDataFilter(e);
            
            if (e == null)
                return;

            ErrorDataReceivedEventHandler handler = ErrorDataReceived;
            if (handler != null)
                handler(this, e);
        }

        public static ErrorDataReceivedEventArgs FlacErrorDataFilter(ErrorDataReceivedEventArgs e)
        {
            if (e == null || string.IsNullOrWhiteSpace(e.Data))
                return null;

            if (e.Data.StartsWith("-: "))
                return new ErrorDataReceivedEventArgs(e.Data.Substring(3));

            return e;
        }

        public static ErrorDataReceivedEventArgs LameErrorDataFilter(ErrorDataReceivedEventArgs e)
        {
            if (e == null || string.IsNullOrWhiteSpace(e.Data))
                return null;

            return e;
        }
    }
}