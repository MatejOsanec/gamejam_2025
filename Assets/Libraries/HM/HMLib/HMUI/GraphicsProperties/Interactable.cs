using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HMUI {

    public class Interactable : MonoBehaviour {

        [SerializeField] bool _interactable = true;


        public event System.Action<Interactable, bool> interactableChangeEvent;

        public bool interactable {
            get => _interactable;
            set {
                if (_interactable == value) {
                    return;
                }
                _interactable = value;
                interactableChangeEvent?.Invoke(this, _interactable);
            }
        }
    }
}
