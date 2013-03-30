using System;
using System.Collections.Generic;
using System.Threading;

namespace NEmplode.Async
{
    public class WaitableQueue<T> : IDisposable
    {
        private readonly object _sync = new object();
        private readonly Queue<T> _queue = new Queue<T>();
        private readonly ManualResetEvent _available = new ManualResetEvent(false);

        public WaitHandle Available
        {
            get { return _available; }
        }

        public void Enqueue(T value)
        {
            lock (_sync)
            {
                _queue.Enqueue(value);
                _available.Set();
            }
        }

        public void Dispose()
        {
            _available.Dispose();
        }

        public void Dequeue(Action<T> action)
        {
            lock (_sync)
            {
                while (_queue.Count != 0)
                {
                    T value = _queue.Dequeue();
                    action(value);
                }

                _available.Reset();
            }
        }
    }
}