using System.Collections.Concurrent;

namespace DevPulse.Scheduler;

public class PriorityScheduler
{
    private readonly ConcurrentQueue<Action> _highPriority = new();
    private readonly ConcurrentQueue<Action> _lowPriority = new();

    public void QueueTask(Action task, bool highPriority = false)
    {
        if (highPriority) _highPriority.Enqueue(task);
        else _lowPriority.Enqueue(task);
    }

    public bool TryExecuteNext()
    {
        if (_highPriority.TryDequeue(out var highTask))
        {
            highTask();
            return true;
        }
        if (_lowPriority.TryDequeue(out var lowTask))
        {
            lowTask();
            return true;
        }
        return false;
    }
}
