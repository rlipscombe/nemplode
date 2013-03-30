using System;
using System.Diagnostics;
using System.IO;
using NEmplode.IO;

namespace NEmplode.Codecs
{
    public static class FlacDecoder1
    {
        static FlacDecoder1()
        {
            DecoderPath = @"C:\Program Files (x86)\Flac\bin\flac.exe";
            BufferSize = 16384;
        }

        public static string DecoderPath { get; set; }
        public static int BufferSize { get; set; }

        public static Stream OpenRead(string path)
        {
            Stream stream = File.OpenRead(path);

            var process = new Process
                {
                    StartInfo = GetProcessStartInfo(),
                    EnableRaisingEvents = true
                };

            // Hook up the events. We don't hook up OutputDataReceived, because that wants to treat the data as strings; we need binary.
            process.ErrorDataReceived += (sender, e) => { Console.Error.WriteLine(e.Data); };
            process.Exited += (sender, e) => { };

            // Start the process.
            process.Start();

            // Start an asynchronous read from stderr.
            process.BeginErrorReadLine();

            // Our input stream is pull -- we have to read from it.
            // The decoder is push -- we push data into it, and data arrives at the other end. But stdout is pull -- because we can't use the event (text rather than binary), it's a stream we need to read from.
            // The encoder is also push -- we push data into it, and data arrives at the other end.
            // The output stream is push.
            // So we need some pull-to-push shunts. BeginRead/EndRead -> BeginWrite/EndWrite -> BeginRead should do.
            // Data arriving

            // So: sourceStream >- -> process.StandardInput.BaseStream | process.StandardOutput.BaseStream. >- -> outputStream

            // Now, if we're transcoding, that looks like this:
            // sourceStream >- -> stdin|stdout >- -> stdin | stdout >- -> destination.

            //process.StandardInput.BaseStream.BeginWrite();
            //process.StandardOutput.BaseStream.BeginRead();
            throw new NotImplementedException();
        }

        private static ProcessStartInfo GetProcessStartInfo()
        {
            var processStartInfo =
                new ProcessStartInfo
                    {
                        // We're using FLAC.EXE as our decoder.
                        FileName = DecoderPath,

                        // Decode from stdin to stdout.
                        Arguments = @"--silent --decode --stdout -",

                        // We don't want a visible window.
                        CreateNoWindow = true,

                        // In order to do redirection, we need to not use ShellExecute.
                        UseShellExecute = false,

                        // Redirect standard input. We'll be writing the FLAC data into this.
                        RedirectStandardInput = true,

                        // Redirect standard output. This is what we're reading from.
                        RedirectStandardOutput = true,

                        // Redirect standard error, so that we can display it.
                        RedirectStandardError = true,
                    };
            return processStartInfo;
        }
    }
}