using System;
using System.Configuration;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace ConvertMusic
{
    internal static class Transcoder
    {
        public static Task ConvertAsync(string sourceFileName, string destinationFileName, CancellationToken cancellationToken)
        {
            // TODO: Have a registry for these, and some content negotiation.
            string decoderPath = ConfigurationManager.AppSettings["DecoderPath"];
            string decoderArguments = ConfigurationManager.AppSettings["DecoderArguments"];

            string encoderPath = ConfigurationManager.AppSettings["EncoderPath"];
            string encoderArguments = ConfigurationManager.AppSettings["EncoderArguments"];

            // Wire a graph together.
            var source = File.OpenRead(sourceFileName);
            var decoder = new CodecProcess(decoderPath, decoderArguments) { ErrorDataFilter = CodecProcess.FlacErrorDataFilter };
            var encoder = new CodecProcess(encoderPath, encoderArguments) { ErrorDataFilter = CodecProcess.LameErrorDataFilter };
            var destination = File.Create(destinationFileName);

            const int bufferSize = 16384;

            decoder.ErrorDataReceived += (sender, e) => {Console.WriteLine("{0}: {1}", sourceFileName, e.Data);};
            var decoderTask = decoder.Start(cancellationToken);

            // Note that the process has to be started before you can get the stream,
            // otherwise you get InvalidOperationException containing "StandardIn has not been redirected.".
            var sourceToDecoderTask =
                source.CopyToAsync(decoder.InputStream, bufferSize, cancellationToken)
                      .ContinueWith(t => decoder.InputStream.Close());

            encoder.ErrorDataReceived += (sender, e) => { Console.WriteLine("{0}: {1}", destinationFileName, e.Data); };
            var encoderTask = encoder.Start(cancellationToken);

            var decoderToEncoderTask =
                decoder.OutputStream
                       .CopyToAsync(encoder.InputStream, bufferSize, cancellationToken)
                       .ContinueWith(t => encoder.InputStream.Close());

            var encoderToDestination =
                encoder.OutputStream
                       .CopyToAsync(destination, bufferSize, cancellationToken)
                       .ContinueWith(t => destination.Close());

            return Task.WhenAll(sourceToDecoderTask, decoderTask, decoderToEncoderTask, encoderTask, encoderToDestination)
                       .ContinueWith(t =>
                           {
                               if (t.IsCanceled || t.IsFaulted)
                               {
                                   Console.WriteLine("Deleting '{0}'.", destinationFileName);
                                   File.Delete(destinationFileName);
                               }
                           });
        }
    }
}