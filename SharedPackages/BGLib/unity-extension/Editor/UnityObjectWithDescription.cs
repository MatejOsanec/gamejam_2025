using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEditor;
using UnityEngine;

public class UnityObjectWithDescription {

    public UnityEngine.Object obj { get; private set; }
    public string description { get; private set; }

    public UnityObjectWithDescription(UnityEngine.Object obj) {

        Init(obj, "");
    }

    public UnityObjectWithDescription(UnityEngine.Object obj, string description) {

        Init(obj, description);
    }

    private void Init(UnityEngine.Object obj, string description) {

        this.obj = obj;
        this.description = description;
    }

    public static string FormatListToString(IList<UnityObjectWithDescription> checkResult) {

        if (checkResult == null || checkResult.Count == 0) {
            return string.Empty;
        }

        var sb = new StringBuilder();
        foreach (var objectWithDescription in checkResult) {
            int instanceId;
            var obj = objectWithDescription.obj;
            if (obj is ScriptableObject scrObj) {
                instanceId = scrObj.GetInstanceID();
            }
            else {
                var prefab = PrefabUtility.GetCorrespondingObjectFromSource(obj);
                instanceId = prefab != null? prefab.GetInstanceID(): obj.GetInstanceID();
            }
            string path = AssetDatabase.GetAssetPath(instanceId);
            sb.Append(objectWithDescription.obj.name);
            sb.Append("; ");
            sb.Append(objectWithDescription.description);
            sb.Append("; Path:");
            sb.Append(path);
            sb.AppendLine();
        }

        return sb.ToString();
    }
}
