using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;

namespace HMUI {


    public class StackLayoutGroup : LayoutGroup {

        [SerializeField] protected bool m_ChildForceExpandWidth = true;
        public bool childForceExpandWidth { get { return m_ChildForceExpandWidth; } set { SetProperty(ref m_ChildForceExpandWidth, value); } }

        [SerializeField] protected bool m_ChildForceExpandHeight = true;
        public bool childForceExpandHeight { get { return m_ChildForceExpandHeight; } set { SetProperty(ref m_ChildForceExpandHeight, value); } }

        protected StackLayoutGroup() { }

        public override void CalculateLayoutInputHorizontal() {

            base.CalculateLayoutInputHorizontal();
            CalcAlongAxis(0);
        }

        public override void CalculateLayoutInputVertical() {
            
            CalcAlongAxis(1);
        }

        public override void SetLayoutHorizontal() {

            SetChildrenAlongAxis(0);
        }

        public override void SetLayoutVertical() {

            SetChildrenAlongAxis(1);
        }

        private void CalcAlongAxis(int axis) {

            float combinedPadding = (axis == 0 ? padding.horizontal : padding.vertical);

            float maxMin = 0.0f;
            float maxPreferred = 0.0f;
            float maxFlexible = 0.0f;

            for (int i = 0; i < rectChildren.Count; i++) {

                RectTransform child = rectChildren[i];
                float min = LayoutUtility.GetMinSize(child, axis);
                float preferred = LayoutUtility.GetPreferredSize(child, axis);
                float flexible = LayoutUtility.GetFlexibleSize(child, axis);
                if ((axis == 0 ? childForceExpandWidth : childForceExpandHeight)) {
                    flexible = Mathf.Max(flexible, 1);
                }

                maxMin = Mathf.Max(min + combinedPadding, maxMin);
                maxPreferred = Mathf.Max(preferred + combinedPadding, maxPreferred);
                maxFlexible = Mathf.Max(flexible, maxFlexible);
            }

            maxPreferred = Mathf.Max(maxMin, maxPreferred);
            SetLayoutInputForAxis(totalMin: maxMin, totalPreferred: maxPreferred, totalFlexible: maxFlexible, axis: axis);
        }

        private void SetChildrenAlongAxis(int axis) {

            float size = rectTransform.rect.size[axis];

            float innerSize = size - (axis == 0 ? padding.horizontal : padding.vertical);
            for (int i = 0; i < rectChildren.Count; i++) {
                RectTransform child = rectChildren[i];
                float min = LayoutUtility.GetMinSize(child, axis);
                float preferred = LayoutUtility.GetPreferredSize(child, axis);
                float flexible = LayoutUtility.GetFlexibleSize(child, axis);
                if ((axis == 0 ? childForceExpandWidth : childForceExpandHeight))
                    flexible = Mathf.Max(flexible, 1);

                float requiredSpace = Mathf.Clamp(innerSize, min, flexible > 0 ? size : preferred);
                float startOffset = GetStartOffset(axis, requiredSpace);
                SetChildAlongAxis(child, axis, startOffset, requiredSpace);
            }
        }
    }
}