using System.Collections.Generic;
using UnityEngine;

namespace BGLib.HierarchyIcons {

    public class HierarchyIgnorePrefabOverrides : MonoBehaviour {

#if UNITY_EDITOR
        [SerializeField] List<Object>? _toIgnore;
        public List<Object>? toIgnore => _toIgnore;

        public void AddIgnore(Object toAdd) {

            if (_toIgnore == null) {
                _toIgnore = new List<Object>();
            }

            if (_toIgnore.Contains(toAdd)) {
                return;
            }

            _toIgnore.Add(toAdd);
        }
#endif
    }
}
