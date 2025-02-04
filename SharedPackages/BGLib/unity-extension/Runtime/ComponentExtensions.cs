using UnityEngine;

public static class ComponentExtensions {

    public static T GetComponentInParentOnly<T>(this Component c) where T : Component {

        if (c.transform.parent == null) {
            return null;
        }

        return c.transform.parent.GetComponentInParent<T>();
    }
}
