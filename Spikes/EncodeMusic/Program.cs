using System;
using System.IO;
using System.Threading;
using NEmplode;
using NEmplode.Tagging;
using NEmplode.Tasks;

namespace EncodeMusic
{
    internal static class Program
    {
        private static void Main(string[] args)
        {
            if (args.Length != 2)
            {
                Console.WriteLine("EncodeMusic source-root destination-root");
            }

            string sourceRoot = args[0];
            string destinationRoot = args[1];

            var cancellationTokenSource = new CancellationTokenSource();
            Console.CancelKeyPress += (sender, e) =>
            {
                Console.WriteLine("^C");
                cancellationTokenSource.Cancel();
                e.Cancel = true;
            };

            int maxDegreeOfParallelism = Environment.ProcessorCount;

            var sourceFiles = Directory.EnumerateFiles(sourceRoot, "*.wav", SearchOption.AllDirectories);
            Concurrent.ForEach(sourceFiles, maxDegreeOfParallelism, cancellationTokenSource.Token, sourceFileName =>
            {
                // TODO: This is not how to figure out the destination.
                var destinationFileName = Path.ChangeExtension(sourceFileName, ".flac");
                return
                    Transcoder.EncodeAsync(sourceFileName, destinationFileName, cancellationTokenSource.Token)
                              .ContinueWith(t =>
                              {
                                  if (!t.IsFaulted && !t.IsCanceled)
                                      TagCopier.CopyTags(sourceFileName, destinationFileName);
                              });
            });
        }
    }
}
