using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine.Assertions;
using UnityEngine.Rendering;

public static class ExtensionMethods {

    public static bool ContainsLayer(this LayerMask layerMask, int layer) {

        return (layerMask.value & (1 << layer)) != 0;
    }

    // Coroutines
    public static Coroutine StartUniqueCoroutine(this MonoBehaviour m, System.Func<IEnumerator> func) {

        m.StopCoroutine(func.Method.Name);
        return m.StartCoroutine(func.Method.Name);
    }

    public static Coroutine StartUniqueCoroutine<T>(this MonoBehaviour m, System.Func<T, IEnumerator> func, T value) {

        m.StopCoroutine(func.Method.Name);
        return m.StartCoroutine(func.Method.Name, value);
    }

    public static void StopUniqueCoroutine(this MonoBehaviour m, System.Func<IEnumerator> func) {

        m.StopCoroutine(func.Method.Name);
    }

    public static void StopUniqueCoroutine<T>(this MonoBehaviour m, System.Func<T, IEnumerator> func) {

        m.StopCoroutine(func.Method.Name);
    }

    // Transform
    public static bool IsDescendantOf(this Transform transform, Transform parent) {

        while (transform != null && transform != parent) {
            transform = transform.parent;
        }

        return (transform == parent);
    }

    public static void SetLocalPositionAndRotation(this Transform tr, Vector3 pos, Quaternion rot) {

        tr.localPosition = pos;
        tr.localRotation = rot;
    }

    public static string GetPath(this Transform current) {

        if (current.parent == null) {
            return $"/ {current.name}";
        }
        return $"{current.parent.GetPath()} / {current.name}";
    }

    // Could be probably done in better way, taken from https://forum.unity.com/threads/mirror-reflections-in-vr.416728/#post-6067572
    public static Quaternion Reflect(this Quaternion source, Vector3 normal) {
        return Quaternion.LookRotation(Vector3.Reflect(source * Vector3.forward, normal), Vector3.Reflect(source * Vector3.up, normal));
    }

    // Textures
    public static Texture2D CreateTexture2D(this RenderTexture renderTexture, TextureFormat textureFormat = TextureFormat.RGB24) {

        Assert.IsTrue(renderTexture.dimension == TextureDimension.Tex2D);

        GL.Flush();

        Texture2D outputTexture = new Texture2D(renderTexture.width, renderTexture.height, textureFormat, mipChain:false);

        RenderTexture.active = renderTexture;
        outputTexture.ReadPixels(new Rect(0, 0, renderTexture.width, renderTexture.height), destX:0, destY:0);
        outputTexture.Apply();
        RenderTexture.active = null;

        GL.Flush();

        return outputTexture;
    }

    public static Vector2 Rotate(this Vector2 vector, float rads) {

        return new Vector2(
            vector.x * Mathf.Cos(rads) - vector.y * Mathf.Sin(rads),
            vector.x * Mathf.Sin(rads) + vector.y * Mathf.Cos(rads)
        );
    }

    // This method (extension) only exists for List, I needed it for IReadonlyList
    // Taken and modifier from https://github.com/microsoft/referencesource/blob/master/mscorlib/system/collections/generic/list.cs
    public static List<T> GetRange<T>(this IReadOnlyList<T> list, int index, int count) {

        if (index < 0) {
            throw new ArgumentOutOfRangeException();
        }

        if (count < 0) {
            throw new ArgumentOutOfRangeException();
        }

        if (list.Count - index < count) {
            throw new ArgumentException();
        }

        List<T> result = new List<T>(count);
        for (int i = 0; i < count; i++) {
            result.Add(list[index+i]);
        }
        return result;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T LastUnsafe<T>(this IReadOnlyList<T> list) {

        return list[list.Count - 1];
    }

    public static void SetSeed(this ParticleSystem particleSystem, uint seed) {

        var wasPlaying = particleSystem.isPlaying;
        particleSystem.Stop();
        particleSystem.useAutoRandomSeed = false;
        particleSystem.randomSeed = seed;
        if (wasPlaying) {
            particleSystem.Play();
        }
    }
}

public static class EssentialHelpers {

    public static double CurrentTimeStamp => DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)).TotalSeconds;

    public static void SafeDestroy(UnityEngine.Object obj) {

        if (obj == null) {
            return;
        }

        if (Application.isPlaying) {
            UnityEngine.Object.Destroy(obj);
        }
        else {
            UnityEngine.Object.DestroyImmediate(obj);
        }

        obj = null;
    }

    public static T GetOrAddComponent<T>(GameObject go) where T : Component {

        T comp = go.GetComponent<T>();
        if (!comp) {
            comp = go.AddComponent<T>();
        }

        return comp;
    }
}
