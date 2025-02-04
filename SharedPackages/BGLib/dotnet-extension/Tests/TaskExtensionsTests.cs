using System;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;

public class TaskExtensionsTests {

    private TaskCompletionSource<int> _taskCompletionSource;
    private CancellationTokenSource _cancellationSource;

    [SetUp]
    public void Setup() {

        _taskCompletionSource = new TaskCompletionSource<int>();
        _cancellationSource = new CancellationTokenSource();
    }

    private Task<int> GetCancellableTaskT() => _taskCompletionSource.Task.WaitAsync(_cancellationSource.Token);
    private Task GetCancellableTask() => (_taskCompletionSource.Task as Task).WaitAsync(_cancellationSource.Token);

    [Test]
    public async Task WaitAsync_WhenNotCancelled_ReturnsTaskResult() {

        var task = GetCancellableTask();

        _taskCompletionSource.SetResult(12);

        await task;
        Assert.That(task.IsCompletedSuccessfully, Is.True);
    }

    [Test]
    public async Task WaitAsyncT_WhenNotCancelled_ReturnsTaskResult() {

        var task = GetCancellableTaskT();

        _taskCompletionSource.SetResult(12);

        Assert.That(await task, Is.EqualTo(12));
    }

    [Test]
    public async Task WaitAsync_WhenSourceTaskThrowsException_ExceptionIsPropagated() {

        var task = GetCancellableTask();

        _taskCompletionSource.SetException(new Exception("failed"));

        await Task.Yield();

        Assert.That(task.IsFaulted, Is.True);
        Assert.That(task.Exception?.InnerException?.Message, Is.EqualTo("failed"));
    }

    [Test]
    public async Task WaitAsyncT_WhenSourceTaskThrowsException_ExceptionIsPropagated() {

        var task = GetCancellableTaskT();

        _taskCompletionSource.SetException(new Exception("failed"));

        await Task.Yield();

        Assert.That(task.IsFaulted, Is.True);
        Assert.That(task.Exception?.InnerException?.Message, Is.EqualTo("failed"));
    }

    [Test]
    public async Task WaitAsync_WhenCancelledExternally_ResultIsCancelled() {

        var task = GetCancellableTask();

        _cancellationSource.Cancel();

        await Task.Yield();

        Assert.That(task.IsCanceled, Is.True);
    }

    [Test]
    public async Task WaitAsyncT_WhenCancelledExternally_ResultIsCancelled() {

        var task = GetCancellableTaskT();

        _cancellationSource.Cancel();

        await Task.Yield();

        Assert.That(task.IsCanceled, Is.True);
    }

    [Test]
    public async Task WaitAsync_WhenSourceTaskIsCancelled_ResultIsCancelled() {

        var task = GetCancellableTask();

        _taskCompletionSource.SetCanceled();

        await Task.Yield();

        Assert.That(task.IsCanceled, Is.True);
    }

    [Test]
    public async Task WaitAsyncT_WhenSourceTaskIsCancelled_ResultIsCancelled() {

        var task = GetCancellableTaskT();

        _taskCompletionSource.SetCanceled();

        await Task.Yield();

        Assert.That(task.IsCanceled, Is.True);
    }
}
