using System.Collections.Generic;
using UnityEngine;

[ExecuteAlways]
public sealed class CopyPosition : MonoBehaviour {

	public Transform source;

    public void Refresh() => transform.position = source.position;

    private void Awake() => CopyPositionUpdater.Add(this);

    // `CopyPositionUpdater` is an optimization replacing:
    //
    // `void CopyPosition.LateUpdate() => Refresh();`
    //
    // Since Unity Messages like `(Late)Update` are rather expensive,
    // we can combine those of all instance into a single one.
    // And without a `Start` method either, Unity doesn't track possible enabled/disabled.
    //
    // E.g. in the `Lattice` environment, it combines 32 messages into one.
    //
    // The downside is the complexity of this class.
    [ExecuteAlways]
    private sealed class CopyPositionUpdater : MonoBehaviour {

        public static void Add(CopyPosition copyPosition) {

            if (!_instance) {
                GameObject gameObject = new(nameof(CopyPositionUpdater)) {
                    hideFlags = HideFlags.HideAndDontSave
                };
                if (Application.isPlaying) {
                    DontDestroyOnLoad(gameObject);
                }
                _instance = gameObject.AddComponent<CopyPositionUpdater>();
            }
            _instance._copyPositions.Add(copyPosition);
        }

        
        private static CopyPositionUpdater _instance;

        private readonly List<CopyPosition> _copyPositions = new (32);

        private void LateUpdate() {

            int n = _copyPositions.Count;
            if (n == 0) {
                DestroyImmediate(gameObject);
                return;
            }

            for (int i = 0; i < n; i++) {
                CopyPosition copyPosition = _copyPositions[i];
                if (copyPosition) {
                    copyPosition.Refresh();
                    continue;
                }
                --n;
                _copyPositions[i] = _copyPositions[n];
                i--;
                _copyPositions.RemoveAt(n);
            }
        }
    }
}
