using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace HMUI {

    public class CurvedCanvasSettingsHelper {

        private Canvas _cachedCanvas = null;
        private bool _cachedCanvasIsRootCanvas;
        private CurvedCanvasSettings _curvedCanvasSettings = null;
        private bool _hasCachedData;

        private static Dictionary<Canvas, CurvedCanvasSettings> _curvedCanvasCache = new Dictionary<Canvas, CurvedCanvasSettings>();

#if UNITY_EDITOR
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void NoDomainReloadInit() {

            _curvedCanvasCache = new Dictionary<Canvas, CurvedCanvasSettings>();
        }
#endif

        public void Reset() {

            _hasCachedData = false;
            _cachedCanvas = null;
            _cachedCanvasIsRootCanvas = false;
            _curvedCanvasSettings = null;
        }

        public CurvedCanvasSettings GetCurvedCanvasSettings(Canvas canvas) {

            if (canvas == null) {
                return null;
            }

            Canvas rootCanvas = null;

            if (_hasCachedData) {

                if (canvas.transform.hasChanged == false) {
                    return _curvedCanvasSettings;
                }

                if (!_cachedCanvasIsRootCanvas && _cachedCanvas == canvas) {
                    return _curvedCanvasSettings;
                }

                rootCanvas = canvas.rootCanvas;
                if (_cachedCanvasIsRootCanvas && _cachedCanvas == rootCanvas) {
                    return _curvedCanvasSettings;
                }
            }

            _curvedCanvasSettings = GetCurvedCanvasSettingsForCanvas(canvas);

            if (_curvedCanvasSettings != null) {

                _cachedCanvasIsRootCanvas = false;
                _cachedCanvasIsRootCanvas = canvas;
                _hasCachedData = true;

                return _curvedCanvasSettings;
            }
            else {

                rootCanvas = canvas.rootCanvas;
                _curvedCanvasSettings = GetCurvedCanvasSettingsForCanvas(rootCanvas);

                _cachedCanvasIsRootCanvas = true;
                _cachedCanvas = rootCanvas;
                _hasCachedData = true;

                return _curvedCanvasSettings;
            }
        }

        // We expect that CurvedCanvasSettings components are not added or removed during the runtime.
        private static CurvedCanvasSettings GetCurvedCanvasSettingsForCanvas(Canvas canvas) {

            if (Application.isPlaying && _curvedCanvasCache.TryGetValue(canvas, out CurvedCanvasSettings curvedCanvasSettings)) {
                return curvedCanvasSettings;
            }
            else {
                curvedCanvasSettings = canvas.GetComponent<CurvedCanvasSettings>();
                _curvedCanvasCache[canvas] = curvedCanvasSettings;
                return curvedCanvasSettings;
            }
        }
    }

}
