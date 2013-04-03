using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using NEmplode.Codecs;
using NEmplode.Tasks;

namespace NEmplode
{
    public static class Transcoder
    {
        public static Task EncodeAsync(string sourceFileName,
                                       string destinationFileName,
                                       CancellationToken cancellationToken)
        {
            // Wire a graph together.
            var source = File.OpenRead(sourceFileName);
            var encoder = CodecFactory.CreateEncoder(destinationFileName);
            var destination = File.Create(destinationFileName);

            const int bufferSize = 16384;

            encoder.ErrorDataReceived += (sender, e) => { Console.WriteLine("{0}: {1}", destinationFileName, e.Data); };
            var encoderTask = encoder.Start(cancellationToken);

            var sourceToEncoderTask =
                source.CopyToAsync(encoder.InputStream, bufferSize, cancellationToken)
                      .ContinueWith(t => encoder.InputStream.Close());

            var encoderToDestination =
                encoder.OutputStream
                       .CopyToAsync(destination, bufferSize, cancellationToken)
                       .ContinueWith(t => destination.Close());

            // We need to propagate the cancelation/exception; use a TCS.
            var completion = new TaskCompletionSource<bool>();
            Task.WhenAll(sourceToEncoderTask, encoderTask, encoderToDestination)
                .ContinueWith(t =>
                {
                    if (t.IsCanceled || t.IsFaulted)
                    {
                        Console.WriteLine("Deleting '{0}'.", destinationFileName);
                        File.Delete(destinationFileName);
                    }

                    completion.SetFromTask(t);
                });

            return completion.Task;
        }

        public static Task ConvertAsync(string sourceFileName,
                                        string destinationFileName,
                                        CancellationToken cancellationToken)
        {
            // Wire a graph together.
            var source = File.OpenRead(sourceFileName);
            var decoder = CodecFactory.CreateDecoder(sourceFileName);
            var encoder = CodecFactory.CreateEncoder(destinationFileName);
            var destination = File.Create(destinationFileName);

            const int bufferSize = 16384;

            decoder.ErrorDataReceived += (sender, e) => { Console.WriteLine("{0}: {1}", sourceFileName, e.Data); };
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

            // We need to propagate the cancelation/exception; use a TCS.
            var completion = new TaskCompletionSource<bool>();
            Task.WhenAll(sourceToDecoderTask, decoderTask, decoderToEncoderTask, encoderTask, encoderToDestination)
                .ContinueWith(t =>
                    {
                        if (t.IsCanceled || t.IsFaulted)
                        {
                            Console.WriteLine("Deleting '{0}'.", destinationFileName);
                            File.Delete(destinationFileName);
                        }

                        completion.SetFromTask(t);
                    });

            return completion.Task;
        }
    }
}