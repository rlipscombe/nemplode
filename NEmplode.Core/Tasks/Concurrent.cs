using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace NEmplode.Tasks
{
    public static class Concurrent
    {
        public static void ForEach<T>(IEnumerable<T> source, int maxDegreeOfParallelism, CancellationToken cancellationToken, Func<T, Task> factory)
        {
            using (var pending = new SemaphoreSlim(maxDegreeOfParallelism))
            {
                var running = new SortedSet<Task>(new TaskComparer());
                try
                {
                    foreach (var item in source)
                    {
                        // Wait until there's a core available.
                        pending.Wait(cancellationToken);

                        // There's a core available: start the next job.
                        var task = factory(item);
                        running.Add(task);
                        task.ContinueWith(t =>
                            {
                                // ReSharper disable AccessToDisposedClosure -- We'll block on the remaining tasks before we dispose of 'pending'.
                                // TODO: If we convert this to Async, we'll need a ContinueWith to dispose of it.
                                running.Remove(t);
                                pending.Release();
                                // ReSharper restore AccessToDisposedClosure
                            });
                    }
                }
                catch (OperationCanceledException)
                {
                    // Ignore it.
                }
                finally
                {
                    Task.WaitAll(running.ToArray());
                }
            }
        }
    }
}