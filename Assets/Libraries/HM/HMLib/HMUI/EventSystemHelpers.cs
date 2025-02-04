using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace HMUI {

    public static class EventSystemHelpers {

        public static bool IsInputFieldSelected() {

            var go = EventSystem.current.currentSelectedGameObject;
            if (go == null) {
                return false;
            }

            if (go.GetComponent<InputField>() == null) {
                return false;
            }

            return true;
        }
    }
}