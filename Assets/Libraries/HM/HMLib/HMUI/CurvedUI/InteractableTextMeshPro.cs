using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace HMUI {

    public class InteractableTextMeshPro : UIBehaviour {

        [SerializeField] float _interactionAlpha = 1.0f;
        [SerializeField] float _noInteractionAlpha = 0.25f;

        [Space]
        [SerializeField] TextMeshProUGUI _text = default;

        private readonly List<CanvasGroup> _canvasGroupCache = new List<CanvasGroup>();
        
        protected override void OnCanvasGroupChanged() {
            
            // Figure out if parent groups allow interaction
            // If no interaction is allowed... then we need
            // to not do that :)
            var groupAllowInteraction = true;
            Transform t = transform;
            while (t != null) {
                t.GetComponents(_canvasGroupCache);
                bool shouldBreak = false;
                for (var i = 0; i < _canvasGroupCache.Count; i++) {
                    // if the parent group does not allow interaction
                    // we need to break
                    if (!_canvasGroupCache[i].interactable) {
                        groupAllowInteraction = false;
                        shouldBreak = true;
                    }
                    // if this is a 'fresh' group, then break
                    // as we should not consider parents
                    if (_canvasGroupCache[i].ignoreParentGroups) {
                        shouldBreak = true;
                    }
                }
                if (shouldBreak) {
                    break;
                }

                t = t.parent;
            }

            _text.alpha = groupAllowInteraction ? _interactionAlpha : _noInteractionAlpha;
        }
    }
}
