using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;

namespace OpenFTTH.GDBIntegrator.Integrator.Queue
{
    public class SemaphoreQueue
    {
        private SemaphoreSlim semaphore;
        private ConcurrentQueue<TaskCompletionSource<bool>> queue = new ConcurrentQueue<TaskCompletionSource<bool>>();

        public SemaphoreQueue(int initialCount)
        {
            semaphore = new SemaphoreSlim(initialCount);
        }

        public SemaphoreQueue(int initialCount, int maxCount)
        {
            semaphore = new SemaphoreSlim(initialCount, maxCount);
        }

        public void Wait()
        {
            WaitAsync().Wait();
        }

        public Task WaitAsync()
        {
            var taskCompletionSource = new TaskCompletionSource<bool>();

            queue.Enqueue(taskCompletionSource);

            semaphore.WaitAsync().ContinueWith(t =>
            {
                TaskCompletionSource<bool> popped;
                if (queue.TryDequeue(out popped))
                    popped.SetResult(true);
            });

            return taskCompletionSource.Task;
        }

        public void Release()
        {
            semaphore.Release();
        }
    }
}
