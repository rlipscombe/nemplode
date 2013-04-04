using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using NEmplode;
using NEmplode.IO;
using NEmplode.Tagging;
using NEmplode.Tasks;

namespace ConvertMusic
{
    internal static class Program
    {
        private static void Main(string[] args)
        {
            if (args.Length != 2)
            {
                Console.WriteLine("ConvertMusic source-root destination-root");
            }

            string sourceRoot = Path.GetFullPath(args[0]);
            string destinationRoot = Path.GetFullPath(args[1]);

            var cancellationTokenSource = new CancellationTokenSource();
            Console.CancelKeyPress += (sender, e) =>
                {
                    Console.WriteLine("^C");
                    cancellationTokenSource.Cancel();
                    e.Cancel = true;
                };

            int maxDegreeOfParallelism = Environment.ProcessorCount;

            const string sourcePattern = "*.flac";
            var sourceFiles = Directory.EnumerateFiles(sourceRoot, sourcePattern, SearchOption.AllDirectories);
            var actions = sourceFiles
                .Select(sourceFileName =>
                    {
                        var relativePath =
                            PathExtensions.GetRelativePath(sourceRoot, sourceFileName);
                        var destinationFileName =
                            Path.ChangeExtension(Path.Combine(destinationRoot, relativePath), ".mp3");

                        var sourceLastWriteTimeUtc =
                            File.GetLastWriteTimeUtc(sourceFileName);
                        var destinationLastWriteTimeUtc =
                            File.GetLastWriteTimeUtc(destinationFileName);

                        return
                            new
                                {
                                    SourceFileName = sourceFileName,
                                    SourceLastWriteTimeUtc = sourceLastWriteTimeUtc,
                                    DestinationFileName = destinationFileName,
                                    DestinationLastWriteTimeUtc = destinationLastWriteTimeUtc
                                };
                    })
                .Where(x => x.SourceLastWriteTimeUtc > x.DestinationLastWriteTimeUtc);

            Concurrent.ForEach(actions, maxDegreeOfParallelism, cancellationTokenSource.Token, action =>
                {
                    var sourceFileName = action.SourceFileName;
                    var destinationFileName = action.DestinationFileName;

                    Console.WriteLine("Converting '{0}'", sourceFileName);
                    Console.WriteLine("        to '{0}'", destinationFileName);

                    Directory.CreateDirectory(Path.GetDirectoryName(destinationFileName));

                    return
                        Transcoder.ConvertAsync(sourceFileName, destinationFileName, cancellationTokenSource.Token)
                                  .ContinueWith(t =>
                                      {
                                          if (!t.IsFaulted && !t.IsCanceled)
                                              TagCopier.CopyTags(sourceFileName, destinationFileName);
                                      });
                });
        }
    }
}
