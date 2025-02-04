using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

//This class was inspired by https://github.com/facebookexternal/beat-saber/blob/avatars/main/SharedPackages/BeatSaber/Avatars/Runtime/BinaryReadWriteHelper.cs
public static class BinaryReadWriteExtensions {

    public static void Write(this BinaryWriter binaryWriter, Color color) {

        binaryWriter.Write(color.r);
        binaryWriter.Write(color.g);
        binaryWriter.Write(color.b);
        binaryWriter.Write(color.a);
    }

    public static Color ReadColor(this BinaryReader binaryReader) {

        return new Color(r: binaryReader.ReadSingle(), g: binaryReader.ReadSingle(), b: binaryReader.ReadSingle(), a: binaryReader.ReadSingle());
    }

    public static void Write(this BinaryWriter binaryWriter, Vector3 vector) {

        binaryWriter.Write(vector.x);
        binaryWriter.Write(vector.y);
        binaryWriter.Write(vector.z);
    }

    public static Vector3 ReadVector3(this BinaryReader binaryReader) {

        return new Vector3(x: binaryReader.ReadSingle(), y: binaryReader.ReadSingle(), z: binaryReader.ReadSingle());
    }

    public static void Write(this BinaryWriter binaryWriter, Quaternion quaternion) {

        binaryWriter.Write(quaternion.x);
        binaryWriter.Write(quaternion.y);
        binaryWriter.Write(quaternion.z);
        binaryWriter.Write(quaternion.w);
    }

    public static Quaternion ReadQuaternion(this BinaryReader binaryReader) {

        return new Quaternion(x: binaryReader.ReadSingle(), y: binaryReader.ReadSingle(), z: binaryReader.ReadSingle(), w: binaryReader.ReadSingle());
    }

    public static void Write(this BinaryWriter binaryWriter, Pose pose) {

        binaryWriter.Write(pose.position);
        binaryWriter.Write(pose.rotation);
    }

    public static Pose ReadPose(this BinaryReader binaryReader) {

        return new Pose(position: binaryReader.ReadVector3(), rotation: binaryReader.ReadQuaternion());
    }

    public static void WriteListOf<T>(this BinaryWriter binaryWriter, IList<T> list, Action<BinaryWriter, T> elementWriter) {

        int listCount = list.Count;
        binaryWriter.Write(listCount);
        for (int i = 0; i < listCount; i++) {
            elementWriter(binaryWriter, list[i]);
        }
    }

    public static List<T> ReadListOf<T>(this BinaryReader binaryReader,  Func<BinaryReader, T> elementReader) {

        int listCount = binaryReader.ReadInt32();
        var result = new List<T>(listCount);
        for (int i = 0; i < listCount; i++) {
            result.Add(elementReader(binaryReader));
        }
        return result;
    }
}
