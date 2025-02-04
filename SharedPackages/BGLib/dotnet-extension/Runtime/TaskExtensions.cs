using System;
using System.Threading;
using System.Threading.Tasks;

public static class TaskExtensions {

    [Obsolete("Please use WaitAsync instead, as in .Net 6.0")]
    public static Task<T> WithCancellation<T>(this Task<T> task, CancellationToken cancellationToken) {

        return task.WaitAsync(cancellationToken);
    }

    /// <summary>
    /// Wait for the non-cancelable task to finish and abort the waiting process on cancellation
    /// Note that task execution itself won't be cancelled, only the waiting process is interrupted upon cancellation
    /// </summary>
    public static Task WaitAsync(this Task task, CancellationToken cancellationToken) {

        if (!cancellationToken.CanBeCanceled) {
            return task;
        }

        if (cancellationToken.IsCancellationRequested) {
            return Task.FromCanceled(cancellationToken);
        }

        return WaitAsyncInternal(task, cancellationToken);
    }

    /// <summary>
    /// Wait for the non-cancelable typed task to finish and abort the waiting process on cancellation
    /// Note that task execution itself won't be cancelled, only the waiting process is interrupted upon cancellation
    /// </summary>
    public static Task<T> WaitAsync<T>(this Task<T> task, CancellationToken cancellationToken) {

        if (!cancellationToken.CanBeCanceled) {
            return task;
        }

        if (cancellationToken.IsCancellationRequested) {
            return Task.FromCanceled<T>(cancellationToken);
        }

        return WaitAsyncInternal(task, cancellationToken);
    }

    /// <summary>
    /// Wait for either cancellable task source or original task, whichever finishes first
    /// Inspired by https://github.com/StephenCleary/AsyncEx/blob/5ede2ebad24bb3696fd730de2d7e11cda92bf8dc/src/Nito.AsyncEx.Tasks/TaskExtensions.cs#L20
    /// </summary>
    private static async Task WaitAsyncInternal(Task task, CancellationToken cancellationToken) {

        var tcs = new TaskCompletionSource<int>();

        await using (cancellationToken.Register(() => tcs.TrySetCanceled(cancellationToken))) {
            await await Task.WhenAny(task, tcs.Task);
        }
    }

    private static async Task<T> WaitAsyncInternal<T>(Task<T> task, CancellationToken cancellationToken) {

        var tcs = new TaskCompletionSource<T>();

        await using (cancellationToken.Register(() => tcs.TrySetCanceled(cancellationToken))) {
            return await await Task.WhenAny(task, tcs.Task);
        }
    }
}
