using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace ConvertMusic
{
    internal static class Program
    {
        private static void Main(string[] args)
        {
            var rand = new Random();

            if (args.Length != 2)
            {
                Console.WriteLine("ConvertMusic source-root destination-root");
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

            var sourceFiles = Directory.EnumerateFiles(sourceRoot, "*.flac", SearchOption.AllDirectories);
            int maxDegreeOfParallelism = Environment.ProcessorCount;
            using (var pending = new SemaphoreSlim(maxDegreeOfParallelism))
            {
                var tasks = new SortedSet<Task>(new TaskComparer());
                foreach (var sourceFile in sourceFiles)
                {
                    if (cancellationTokenSource.IsCancellationRequested)
                        break;

                    // Wait until there's a core available.
                    pending.Wait();

                    // There's a core available: start a transcode job.
                    var sourceFileName = sourceFile;
                    var destinationFileName = Path.ChangeExtension(sourceFileName, ".mp3");
                    var task = Transcoder.ConvertAsync(sourceFileName, destinationFileName, cancellationTokenSource.Token);
                    tasks.Add(task);
                    task.ContinueWith(t =>
                        {
                            if (!t.IsFaulted && !t.IsCanceled)
                                Console.WriteLine("Converted '{0}' to '{1}'.", sourceFileName, destinationFileName);

                            // ReSharper disable AccessToDisposedClosure -- We'll block on the remaining tasks before we dispose of 'pending'.
                            tasks.Remove(t);
                            pending.Release();
                            // ReSharper restore AccessToDisposedClosure
                        });
                }

                Task.WaitAll(tasks.ToArray());
            }
        }
    }
}
