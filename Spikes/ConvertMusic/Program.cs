using System;
using System.IO;
using System.Threading;

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

            // TODO: Progress.

            var cancellationTokenSource = new CancellationTokenSource();
            Console.CancelKeyPress += (sender, e) =>
                {
                    Console.WriteLine("^C");
                    cancellationTokenSource.Cancel();
                };
            cancellationTokenSource.CancelAfter(TimeSpan.FromMinutes(5));

            var cancel = cancellationTokenSource.Token;

            // Wire a graph together.
            var source = File.OpenRead(sourceFileName);
            var decoder = new CodecProcess(decoderPath, decoderArguments);
            var encoder = new CodecProcess(encoderPath, encoderArguments);
            var destination = File.Create(destinationFileName);

            var decoderExited = new ManualResetEvent(false);
            decoder.Exited += (sender, e) => decoderExited.Set();
            decoder.ErrorDataReceived += (sender, e) => { Console.WriteLine(e.Data); };
            decoder.Start();

            // Note that the process has to be started before you can get the stream,
            // otherwise you get InvalidOperationException containing "StandardIn has not been redirected.".
            var sourceToDecoder = new ProcessPipe(source, decoder.InputStream);
            sourceToDecoder.Start();

            var encoderExited = new ManualResetEvent(false);
            encoder.Exited += (sender, e) => encoderExited.Set();
            encoder.ErrorDataReceived += (sender, e) => { Console.WriteLine(e.Data); };
            encoder.Start();

            var decoderToEncoder = new ProcessPipe(decoder.OutputStream, encoder.InputStream);
            decoderToEncoder.Start();

            var encoderToDestination = new ProcessPipe(encoder.OutputStream, destination);
            encoderToDestination.Start();

            // And now we wait until everything's stopped...
            var waitHandles = new[]
                {
                    cancel.WaitHandle,
                    decoderExited,
                    encoderExited
                };

            int pending = 2;
            while (pending != 0)
            {
                var signal = WaitHandle.WaitAny(waitHandles);
                switch (signal)
                {
                    case 0: // timeout/cancel
                        {
                            sourceToDecoder.Abort();
                            decoder.Abort();
                            decoderToEncoder.Abort();
                            encoder.Abort();
                            encoderToDestination.Abort();
                        }
                        break;

                    case 1: // decoder exited.
                        {
                            // TODO: If the decoder has quit -- it should stop the connection?
                            decoderToEncoder.Stop();
                            decoderExited.Reset();
                            --pending;
                        }
                        break;

                    case 2: // encoder exited.
                        {
                            encoderToDestination.Stop();
                            encoderExited.Reset();
                            --pending;
                        }
                        break;
                }
            }

            // TODO: What if the codec failed?
            // TODO: Flush error output.
        }
    }
}
