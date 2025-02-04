using System.Collections;
using UnityEngine;

public interface ICoroutineStarter {

    Coroutine StartCoroutine(IEnumerator routine);
    void StopCoroutine(Coroutine routine);
}

public class CoroutineStarter : MonoBehaviour, ICoroutineStarter {

    Coroutine ICoroutineStarter.StartCoroutine(IEnumerator routine) {

        if (this == null) {
            Debug.LogWarning("[CoroutineStarter] Cannot start a coroutine, MonoBehaviour is destroyed");
            return null;
        }

        return StartCoroutine(routine);
    }

    void ICoroutineStarter.StopCoroutine(Coroutine routine) {

        if (this == null) {
            Debug.LogWarning("[CoroutineStarter] Cannot stop a coroutine, MonoBehaviour is destroyed");
            return;
        }

        StopCoroutine(routine);
    }
}
