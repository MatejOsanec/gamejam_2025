using System.Collections.Generic;
using UnityEngine.UIElements;

public static class VisualElementExtensions {

    /// <summary> Removes all children of this visual element </summary>
    public static void RemoveAllChildren(this VisualElement ve) {

        for (int i = ve.childCount - 1; i >= 0; i--) {
            ve.RemoveAt(i);
        }
    }

    public static List<VisualElement> GetChildren(this VisualElement ve) {

        var result = new List<VisualElement>(ve.childCount);
        for (int i = 0; i < ve.childCount; i++) {
            result.Add(ve.ElementAt(i));
        }
        return result;
    }

    public static bool IsHidden(this VisualElement ve) {

        return ve.style.display == DisplayStyle.None;
    }

    /// <summary> Sets display style to none - it will not take up space in the layout once hidden. Note that this does not touch the Visibility property which also hides an object, but still lets it take up space. </summary>
    public static void Hide(this VisualElement ve) {

        ve.style.display = DisplayStyle.None;
    }

    /// <summary> Reverses the Hide action, affects Display Style (not Visibility) </summary>
    public static void Show(this VisualElement ve) {

        ve.style.display = DisplayStyle.Flex;
    }

    public static void SetVisible(this VisualElement ve, bool isVisible) {

        ve.style.display = isVisible ? DisplayStyle.Flex : DisplayStyle.None;
    }

    public static void ToggleDisplay(this VisualElement ve) {

        if (ve.style.display == DisplayStyle.None) {
            ve.Show();
        }
        else {
            ve.Hide();
        }
    }
}
