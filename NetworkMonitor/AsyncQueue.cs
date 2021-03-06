﻿using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace NetworkMonitor
{
    /// <summary>
    /// A simple queue that supports async.
    /// </summary>
    /// <remarks>
    /// This version is from. https://stackoverflow.com/questions/21225361/is-there-anything-like-asynchronous-blockingcollectiont
    /// Too good and straighforward to pass up.
    /// </remarks>
    /// <typeparam name="T"></typeparam>
    public class AsyncQueue<T>
    {
        private readonly SemaphoreSlim _sem;
        private readonly ConcurrentQueue<T> _que;

        public AsyncQueue()
        {
            _sem = new SemaphoreSlim(0);
            _que = new ConcurrentQueue<T>();
        }

        public void Enqueue(T item)
        {
            _que.Enqueue(item);
            _sem.Release();
        }

        public void EnqueueRange(IEnumerable<T> source)
        {
            var n = 0;
            foreach (var item in source)
            {
                _que.Enqueue(item);
                n++;
            }
            _sem.Release(n);
        }

        public async Task<T> DequeueAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            for (; ; )
            {
                await _sem.WaitAsync(cancellationToken);

                T item;
                if (_que.TryDequeue(out item))
                {
                    return item;
                }
            }
        }
    }
}
