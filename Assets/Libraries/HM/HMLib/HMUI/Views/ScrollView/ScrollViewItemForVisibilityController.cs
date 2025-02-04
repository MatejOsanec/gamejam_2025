using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HMUI {

    [RequireComponent(typeof(RectTransform))]
    public class ScrollViewItemForVisibilityController : MonoBehaviour {

        public void GetWorldCorners(Vector3[] fourCornersArray) {

            GetComponent<RectTransform>().GetWorldCorners(fourCornersArray);
        }
    }
}