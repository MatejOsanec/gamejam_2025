using UnityEngine;

public class SetChildrenLightId : MonoBehaviour {

#if UNITY_EDITOR
    [SerializeField] int _ID = -1;

    protected void OnValidate() {

        var childLightIDs = GetComponentsInChildren<LightWithIdMonoBehaviour>();

        foreach (var childLightID in childLightIDs) {

            if (childLightID.lightId != _ID) {
                childLightID.__EditorSetLightId(_ID);
                UnityEditor.EditorUtility.SetDirty(childLightID);
            }
        }
    }
#endif
}
