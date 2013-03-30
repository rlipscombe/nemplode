using System;
using System.Diagnostics;
using System.IO;

namespace ConvertMusic
{
    internal sealed class CodecProcess
    {
        private readonly Process _process;

        public CodecProcess(string fileName, string arguments)
        {
            _process = new Process
                {
                    StartInfo = GetProcessStartInfo(fileName, arguments),
                    EnableRaisingEvents = true
                };

            // Hook up the events. We don't hook up OutputDataReceived, because that wants to treat the data as strings; we need binary.
            _process.ErrorDataReceived += (sender, e) => OnErrorDataReceived(e);
            _process.Exited += (sender, e) =>
                {
                    _process.WaitForExit();
                    _process.StandardOutput.Close();

                    OnExited();
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

        public void Start()
        {
            _process.Start();
            _process.PriorityClass = ProcessPriorityClass.BelowNormal;
            _process.BeginErrorReadLine();
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

        public event EventHandler Exited;

        private void OnExited()
        {
            EventHandler handler = Exited;
            if (handler != null)
                handler(this, EventArgs.Empty);
        }

        public event DataReceivedEventHandler ErrorDataReceived;

        private void OnErrorDataReceived(DataReceivedEventArgs e)
        {
            DataReceivedEventHandler handler = ErrorDataReceived;
            if (handler != null)
                handler(this, e);
        }
    }
}