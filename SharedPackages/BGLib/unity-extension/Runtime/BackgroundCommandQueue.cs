using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public interface IBackgroundCommand {

    Task Execute();
}

/// <summary>
/// BackgroundCommandQueue runs commands sequentially in a single background thread.
/// Thread is spawned on demand and is finished when the whole queue is processed.
///
/// Commands should implement IBackgroundCommand interface. Their respective Execute()
/// implementations are guaranteed to be processed in the same order they were enqueued.
/// </summary>
public class BackgroundCommandQueue {

    private readonly object _sync = new();
    private readonly Queue<IBackgroundCommand> _commandsQueue = new Queue<IBackgroundCommand>();
    private bool _isRunning;

    public void Enqueue(IBackgroundCommand command) {

        lock (_sync) {
            _commandsQueue.Enqueue(command);
            if (!_isRunning) {
                _isRunning = true;
                Task.Run(RunInternal);
            }
        }
    }

    private async Task RunInternal() {

        while (true) {
            IBackgroundCommand command;
            lock (_sync) {
                if (!_commandsQueue.TryDequeue(out command)) {
                    _isRunning = false;
                    break;
                }
            }

            try {
                await command.Execute();
            }
            catch (Exception e) {
                Debug.LogException(e);
            }
        }
    }
}

/// <summary>
/// Abstract implementation for a synchronous command without a result
/// Implementation exposes resultTask Task which gets completed when action is executed in the background
/// </summary>
public abstract class SyncBackgroundCommand : IBackgroundCommand {

    private readonly TaskCompletionSource<int> _taskCompletionSource = new TaskCompletionSource<int>();

    public Task resultTask => _taskCompletionSource.Task;

    public Task Execute() {

        try {
            ExecuteInternal();
            _taskCompletionSource.SetResult(0);
        }
        catch(Exception e) {
            _taskCompletionSource.SetException(e);
        }

        return Task.CompletedTask;
    }

    protected abstract void ExecuteInternal();
}

/// <summary>
/// Abstract implementation for a synchronous command with a typed result
/// Implementation exposes resultTask Task which gets completed when action is executed in the background
/// </summary>
public abstract class SyncBackgroundCommand<T> : IBackgroundCommand {

    private readonly TaskCompletionSource<T> _taskCompletionSource = new TaskCompletionSource<T>();

    public Task<T> resultTask => _taskCompletionSource.Task;

    public Task Execute() {
        
        try {
            var result = ExecuteInternal();
            _taskCompletionSource.SetResult(result);
        }
        catch(Exception e) {
            _taskCompletionSource.SetException(e);
        }

        return Task.CompletedTask;
    }

    protected abstract T ExecuteInternal();
}
