using System;
using System.Configuration;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace ConvertMusic
{
    internal static class Program
    {
        private static void Main(string[] args)
        {
            if (args.Length != 2)
            {
                Console.WriteLine("ConvertMusic source-file destination-file");
            }

            // This ain't that simple, since MSBuild will insist on calling us with OEM code page filenames.
            string sourceFileName = args[0];
            string destinationFileName = args[1];

            var cancellationTokenSource = new CancellationTokenSource();
            Console.CancelKeyPress += (sender, e) =>
                {
                    Console.WriteLine("^C");
                    cancellationTokenSource.Cancel();
                    e.Cancel = true;
                };

            Transcoder.Convert(sourceFileName, destinationFileName, cancellationTokenSource.Token);
        }
    }

    internal static class Transcoder
    {
        public static void Convert(string sourceFileName, string destinationFileName, CancellationToken cancellationToken)
        {
            // TODO: Have a registry for these, and some content negotiation.
            string decoderPath = ConfigurationManager.AppSettings["DecoderPath"];
            string decoderArguments = ConfigurationManager.AppSettings["DecoderArguments"];

            string encoderPath = ConfigurationManager.AppSettings["EncoderPath"];
            string encoderArguments = ConfigurationManager.AppSettings["EncoderArguments"];

            // Wire a graph together.
            var source = File.OpenRead(sourceFileName);
            var decoder = new CodecProcess(decoderPath, decoderArguments);
            var encoder = new CodecProcess(encoderPath, encoderArguments);
            var destination = File.Create(destinationFileName);

            const int bufferSize = 16384;

            decoder.ErrorDataReceived += (sender, e) => { Console.WriteLine(e.Data); };
            var decoderTask = decoder.Start(cancellationToken);

            // Note that the process has to be started before you can get the stream,
            // otherwise you get InvalidOperationException containing "StandardIn has not been redirected.".
            var sourceToDecoderTask =
                source.CopyToAsync(decoder.InputStream, bufferSize, cancellationToken)
                      .ContinueWith(t => decoder.InputStream.Close());

            encoder.ErrorDataReceived += (sender, e) => { Console.WriteLine(e.Data); };
            var encoderTask = encoder.Start(cancellationToken);

            var decoderToEncoderTask =
                decoder.OutputStream
                       .CopyToAsync(encoder.InputStream, bufferSize, cancellationToken)
                       .ContinueWith(t => encoder.InputStream.Close());

            var encoderToDestination =
                encoder.OutputStream
                       .CopyToAsync(destination, bufferSize, cancellationToken)
                       .ContinueWith(t => destination.Close());

            Task.WhenAll(sourceToDecoderTask, decoderTask, decoderToEncoderTask, encoderTask, encoderToDestination)
                .ContinueWith(t =>
                {
                    if (t.IsCanceled || t.IsFaulted)
                    {
                        Console.WriteLine("Deleting '{0}'.", destinationFileName);
                        File.Delete(destinationFileName);
                    }
                })
                .Wait();
        }
    }
}
