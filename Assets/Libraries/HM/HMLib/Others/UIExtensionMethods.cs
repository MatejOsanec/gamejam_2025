using UnityEngine;

public static class UIExtensionMethods {

    public static void CopySizeAndPositionFrom(this RectTransform target, RectTransform source) {
        
        target.pivot = source.pivot;
        target.anchorMin = source.anchorMin;
        target.anchorMax = source.anchorMax;
        target.offsetMin = source.offsetMin;
        target.offsetMax = source.offsetMax;
        target.sizeDelta = source.sizeDelta;
        target.anchoredPosition = source.anchoredPosition;
    }

    public static Rect GetWorldRect(this RectTransform target) {

        Vector3[] corners = new Vector3[4];
        target.GetWorldCorners(corners);
        Vector3 position = corners[0];
        Vector2 size = new Vector2(
            target.lossyScale.x * target.rect.size.x,
            target.lossyScale.y * target.rect.size.y
        );
        return new Rect(position, size);
    }
}
