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
}
