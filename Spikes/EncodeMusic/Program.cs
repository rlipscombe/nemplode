using System;
using System.IO;
using System.Linq;
using System.Threading;
using NEmplode;
using NEmplode.IO;
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
                return;
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

            const string sourcePattern = "*.wav";
            var sourceFiles = Directory.EnumerateFiles(sourceRoot, sourcePattern, SearchOption.AllDirectories);
            var actions = sourceFiles
                .Select(sourceFileName =>
                {
                    var relativePath =
                        PathExtensions.GetRelativePath(sourceRoot, sourceFileName);
                    var destinationFileName =
                        Path.ChangeExtension(Path.Combine(destinationRoot, relativePath), ".flac");

                    return
                        new
                        {
                            SourceFileName = sourceFileName,
                            DestinationFileName = destinationFileName,
                        };
                });

            Concurrent.ForEach(actions, maxDegreeOfParallelism, cancellationTokenSource.Token, action =>
            {
                var sourceFileName = action.SourceFileName;
                var destinationFileName = action.DestinationFileName;

                Console.WriteLine("Converting '{0}'", sourceFileName);
                Console.WriteLine("        to '{0}'", destinationFileName);

                Directory.CreateDirectory(Path.GetDirectoryName(destinationFileName));

                return
                    Transcoder.EncodeAsync(sourceFileName, destinationFileName, cancellationTokenSource.Token);
            });
        }
    }
}
