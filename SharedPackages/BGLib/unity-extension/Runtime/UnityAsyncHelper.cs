using System;
using System.Threading.Tasks;
using System.Collections;
using UnityEngine;

/// <summary>
/// Helps with invoking async tasks safely caught in try catch logging any exceptions
/// Should be used if we do not need to await a task but still want to ensure it's exceptions are properly handled
/// </summary>
public static class UnityAsyncHelper {

    /// <summary>
    /// Safely finishes a task wrapped in try catch logging any possible exceptions
    /// </summary>
    public static async void InvokeSafe(Func<Task> asyncTask) {

        try {
            await asyncTask();
        }
        catch (Exception e) {
            Debug.LogException(e);
        }
    }

    /// <summary>
    /// Safely finishes a task wrapped in try catch logging any possible exceptions
    /// </summary>
    public static async void InvokeSafe<A>(Func<A, Task> asyncTask, A firstParameter) {

        try {
            await asyncTask(firstParameter);
        }
        catch (Exception e) {
            Debug.LogException(e);
        }
    }

    /// <summary>
    /// Safely finishes a task wrapped in try catch logging any possible exceptions
    /// </summary>
    public static async void InvokeSafe<A, B>(Func<A, B, Task> asyncTask, A firstParameter, B secondParameter) {

        try {
            await asyncTask(firstParameter, secondParameter);
        }
        catch (Exception e) {
            Debug.LogException(e);
        }
    }

    /// <summary>
    /// Safely finishes a task wrapped in try catch logging any possible exceptions
    /// </summary>
    public static async void InvokeSafe<A, B, C>(Func<A, B, C, Task> asyncTask, A firstParameter, B secondParameter, C thirdParameter) {

        try {
            await asyncTask(firstParameter, secondParameter, thirdParameter);
        }
        catch (Exception e) {
            Debug.LogException(e);
        }
    }

    /// <summary>
    /// Safely finishes a task wrapped in try catch logging any possible exceptions
    /// </summary>
    public static async void InvokeSafe<A, B, C, D>(Func<A, B, C, D, Task> asyncTask, A firstParameter, B secondParameter, C thirdParameter, D fourthParameter) {

        try {
            await asyncTask(firstParameter, secondParameter, thirdParameter, fourthParameter);
        }
        catch (Exception e) {
            Debug.LogException(e);
        }
    }

    public static Task WaitUntilAsync(MonoBehaviour coroutineStarter, Func<bool> predicate) {

        TaskCompletionSource<int> tcs = new();

        coroutineStarter.StartCoroutine(WaitUntilPredicateTrue());

        return tcs.Task;

        IEnumerator WaitUntilPredicateTrue() {

            yield return new WaitUntil(predicate);
            tcs.SetResult(0);
        }
    }

    public static Task WaitUntilAsync(ICoroutineStarter coroutineStarter, Func<bool> predicate) {

        TaskCompletionSource<int> tcs = new();

        coroutineStarter.StartCoroutine(WaitUntilPredicateTrue());

        return tcs.Task;

        IEnumerator WaitUntilPredicateTrue() {

            yield return new WaitUntil(predicate);
            tcs.SetResult(0);
        }
    }
}
