using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class CoroutineHelpers {

    public static IEnumerator ExecuteAfterDelayCoroutine(System.Action action, float timeSeconds) {

        yield return new WaitForSeconds(timeSeconds);
        action?.Invoke();
    }

    public static IEnumerator ExecuteAfterFrameEnd(System.Action action) {

        yield return new WaitForEndOfFrame();
        action?.Invoke();
    }

    public static void StartSingleCoroutine(this ICoroutineStarter coroutineStarter, ref Coroutine handle, IEnumerator routine) {

        coroutineStarter.StopSingleCoroutine(ref handle);
        handle = coroutineStarter.StartCoroutine(routine);
    }

    public static void StopSingleCoroutine(this ICoroutineStarter coroutineStarter, ref Coroutine handle) {

        if (handle != null) {
            coroutineStarter.StopCoroutine(handle);
            handle = null;
        }
    }
}
