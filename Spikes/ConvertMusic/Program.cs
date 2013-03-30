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

            const int sourceBufferSize = 163840;
            const int encoderOutputBufferSize = 163840;
            const int decoderOutputBufferSize = 163840;

            var decoderExited = new ManualResetEvent(false);
            var encoderExited = new ManualResetEvent(false);

            // Wire a graph together.
            var source = File.OpenRead(sourceFileName);

            var decoder = new CodecProcess(decoderPath, decoderArguments);
            decoder.ErrorDataReceived += (sender, e) => { Console.WriteLine(e.Data); };
            decoder.Exited += (sender, e) => { decoderExited.Set(); };
            decoder.Start();

            // Start reading from the source file. When this completes, we'll write to the decoder's input.
            // Note that the decoder has to be started before you can get the stream.
            var sourceToDecoder = new ProcessPipe("sourceToDecoder", source, decoder.InputStream, sourceBufferSize);
            sourceToDecoder.Start();

            var encoder = new CodecProcess(encoderPath, encoderArguments);
            encoder.ErrorDataReceived += (sender, e) => { Console.WriteLine(e.Data); };
            encoder.Exited += (sender, e) => { encoderExited.Set(); };
            encoder.Start();

            // Start reading from decoder output. When this completes, we'll write to the encoder's input.
            var decoderToEncoder = new ProcessPipe("decoderToEncoder", decoder.OutputStream, encoder.InputStream, decoderOutputBufferSize);
            decoderToEncoder.Start();

            var destination = File.Create(destinationFileName);

            // Start reading from encoder output. When this completes, we'll write to the destination file.
            var encoderToDestination = new ProcessPipe("encoderToDestination", encoder.OutputStream, destination, encoderOutputBufferSize);
            encoderToDestination.Start();

            // And now we wait until everything's stopped...
            var waitHandles = new WaitHandle[]
                {
                    decoderExited,
                    encoderExited
                };

            int done = 0;
            while (done < 2)
            {
                var signal = WaitHandle.WaitAny(waitHandles);
                switch (signal)
                {
                    case 0: // decoder exited.
                        {
                            decoderToEncoder.Stop();
                            decoderExited.Reset();
                            ++done;
                        }
                        break;

                    case 1: // encoder exited.
                        {
                            encoderToDestination.Stop();
                            encoderExited.Reset();
                            ++done;
                        }
                        break;
                }
            }

            // TODO: What if the codec failed?
            // TODO: Cancel / Timeout / Flush error output.
        }
    }
}
