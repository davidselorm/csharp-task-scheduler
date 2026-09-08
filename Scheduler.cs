using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace TaskSchedulerEngine
{
    public enum TaskPriority { High, Normal, Low }

    public class WorkStealingScheduler : TaskScheduler, IDisposable
    {
        private readonly BlockingCollection<Task>[] _workerQueues;
        private readonly Thread[] _threads;
        private readonly CancellationTokenSource _cts = new();
        private readonly int _concurrency;

        public WorkStealingScheduler(int concurrency = 0)
        {
            _concurrency = concurrency > 0 ? concurrency : Environment.ProcessorCount;
            _workerQueues = new BlockingCollection<Task>[_concurrency];
            _threads = new Thread[_concurrency];

            for (int i = 0; i < _concurrency; i++)
            {
                _workerQueues[i] = new BlockingCollection<Task>();
                int threadIndex = i;
                _threads[i] = new Thread(() => WorkerLoop(threadIndex))
                {
                    IsBackground = true,
                    Name = $"WorkStealingWorker-{threadIndex}"
                };
                _threads[i].Start();
            }
        }

        private void WorkerLoop(int index)
        {
            var myQueue = _workerQueues[index];
            while (!_cts.IsCancellationRequested)
            {
                Task? task = null;
                // 1. Try local queue
                if (myQueue.TryTake(out task, 10))
                {
                    TryExecuteTask(task);
                    continue;
                }

                // 2. Work-stealing from neighboring queues
                for (int i = 0; i < _concurrency; i++)
                {
                    if (i != index && _workerQueues[i].TryTake(out task))
                    {
                        TryExecuteTask(task);
                        break;
                    }
                }
            }
        }

        protected override void QueueTask(Task task)
        {
            int targetIndex = Thread.CurrentThread.ManagedThreadId % _concurrency;
            _workerQueues[targetIndex].Add(task);
        }

        protected override bool TryExecuteTaskInline(Task task, bool taskWasPreviouslyQueued)
        {
            return TryExecuteTask(task);
        }

        protected override IEnumerable<Task> GetScheduledTasks()
        {
            var tasks = new List<Task>();
            foreach (var q in _workerQueues)
            {
                tasks.AddRange(q.ToArray());
            }
            return tasks;
        }

        public void Dispose()
        {
            _cts.Cancel();
            foreach (var q in _workerQueues)
            {
                q.CompleteAdding();
            }
            _cts.Dispose();
        }
    }
}
