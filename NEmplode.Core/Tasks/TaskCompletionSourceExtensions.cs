using System.Threading.Tasks;

namespace NEmplode.Tasks
{
    internal static class TaskCompletionSourceExtensions
    {
        public static void SetFromTask<T>(this TaskCompletionSource<T> taskCompletionSource, Task<T> task)
        {
            if (task.IsFaulted)
                taskCompletionSource.SetException(task.Exception);
            else if (task.IsCanceled)
                taskCompletionSource.SetCanceled();

            taskCompletionSource.SetResult(task.Result);
        }

        public static void SetFromTask<T>(this TaskCompletionSource<T> taskCompletionSource, Task task)
        {
            if (task.IsFaulted)
                taskCompletionSource.SetException(task.Exception);
            else if (task.IsCanceled)
                taskCompletionSource.SetCanceled();

            taskCompletionSource.SetResult(default(T));
        }
    }
}