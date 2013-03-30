using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;

namespace ConvertMusic
{
    static class Program
    {
        static void Main(string[] args)
        {
            const string decoderPath = @"C:\Program Files (x86)\Flac\bin\flac.exe";
            const string decoderArguments = @"--silent --decode --stdout -";

            const string encoderPath = @"C:\Program Files (x86)\LAME\lame.exe";
            const string encoderArguments = @"--silent --preset standard --id3v2-only --pad-id3v2-size 256 - -";

            const string sourceFileName = @"D:\Rips\Temp\01 - Divided By Night.flac";
            const string destinationFileName = @"D:\Rips\Temp\01 - Divided By Night.mp3";

            const int sourceBufferSize = 163840;
            const int encoderOutputBufferSize = 163840;
            const int decoderOutputBufferSize = 163840;

            // Wire a graph together.
            var source = File.OpenRead(sourceFileName);

            var decoder = new Process
                {
                    StartInfo = GetProcessStartInfo(decoderPath, decoderArguments),
                    EnableRaisingEvents = true
                };

            // Hook up the events. We don't hook up OutputDataReceived, because that wants to treat the data as strings; we need binary.
            decoder.ErrorDataReceived += (sender, e) => Console.Error.WriteLine(e.Data);
            decoder.Exited += (sender, e) => Console.WriteLine("decoder exited!");

            // Start the process.
            decoder.Start();
            decoder.BeginErrorReadLine();

            // Start reading from the source file. When this completes, we'll write to the decoder's input.
            var sourceToDecoder = new OverlappedPipe("sourceToDecoder", source, decoder.StandardInput.BaseStream, sourceBufferSize);
            sourceToDecoder.Start();

            var encoder = new Process
                {
                    StartInfo = GetProcessStartInfo(encoderPath, encoderArguments),
                    EnableRaisingEvents = true
                };

            // Hook up the events. We don't hook up OutputDataReceived, because that wants to treat the data as strings; we need binary.
            encoder.ErrorDataReceived += (sender, e) => Console.Error.WriteLine(e.Data);
            encoder.Exited += (sender, e) => Console.WriteLine("encoder exited!");

            // Start the process.
            encoder.Start();
            encoder.BeginErrorReadLine();

            // Start reading from decoder output. When this completes, we'll write to the encoder's input.
            var decoderToEncoder = new OverlappedPipe("decoderToEncoder", decoder.StandardOutput.BaseStream,
                                                       encoder.StandardInput.BaseStream, decoderOutputBufferSize);
            decoderToEncoder.Start();

            var destination = File.Create(destinationFileName);

            // Start reading from encoder output. When this completes, we'll write to the destination file.
            var encoderToDestination = new OverlappedPipe("encoderToDestination", encoder.StandardOutput.BaseStream, destination,
                                                           encoderOutputBufferSize);
            encoderToDestination.Start();

            // And now we wait until everything's stopped...

            // There's a bunch of interesting events to wait for at this point now.
            Console.ReadLine();
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
    }

    internal class OverlappedPipe
    {
        private readonly byte[] _buffer;

        private readonly string _name;
        private readonly Stream _source;
        private readonly Stream _destination;

        public OverlappedPipe(string name, Stream source, Stream destination, int bufferSize)
        {
            _name = name;
            _source = source;
            _destination = destination;
            _buffer = new byte[bufferSize];
        }

        public void Start()
        {
            Console.WriteLine("{0}: Start: BeginRead", _name);
            _source.BeginRead(_buffer, 0, _buffer.Length, ReadCallback, null);
        }

        private void ReadCallback(IAsyncResult ar)
        {
            int count = _source.EndRead(ar);
            if (count != 0)
            {
                Console.WriteLine("{0}: ReadCallback: {1} byte(s): BeginWrite", _name, count);
                _destination.BeginWrite(_buffer, 0, count, WriteCallback, null);
            }
            else
            {
                Console.WriteLine("{0}: ReadCallback: 0 byte(s): Close", _name);
                _destination.Close();
            }
        }

        private void WriteCallback(IAsyncResult ar)
        {
            Console.WriteLine("{0}: WriteCallback: BeginRead", _name);
            _destination.EndWrite(ar);
            _source.BeginRead(_buffer, 0, _buffer.Length, ReadCallback, null);
        }
    }
}
