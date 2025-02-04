using System.Collections.Generic;
using System.Text;
using UnityEngine;

public static class ConstructorStringExtensions {

    public static string ToConstructorString(this Vector3 vector) {

        return $"new Vector3(x:{vector.x}f, y:{vector.y}f, z:{vector.z}f)";
    }

    public static string ToConstructorString(this Quaternion quaternion) {

        return $"new Quaternion(x:{quaternion.x}f, y:{quaternion.y}f, z:{quaternion.z}f, w:{quaternion.w}f)";
    }

    public static string ToConstructorString(this Pose pose) {

        return $"new Pose({pose.position.ToConstructorString()}, {pose.rotation.ToConstructorString()})";
    }

    public static string ToConstructorString<T>(List<T> list) where T : IConstructorStringGenerator {

        var sb = new StringBuilder();
        sb.Append("new List<");
        sb.Append(list.GetType().GetGenericArguments()[0].Name);
        sb.Append(">() {");
        sb.AppendLine();
        foreach (var element in list) {
            sb.Append("    ");
            sb.Append(element.ToConstructorString());
            sb.Append(",");
            sb.AppendLine();
        }
        sb.Append("}");
        return sb.ToString();
    }
}

public interface IConstructorStringGenerator {

    string ToConstructorString();
}
