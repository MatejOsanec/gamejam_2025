using NUnit.Framework;
using UnityEngine;
using Assert = UnityEngine.Assertions.Assert;

public class Vector3ExtensionsTests {

    [Test]
    public void MirrorOnYZPlane_PreservesYZAxes() {

        Assert.AreEqual(Vector3.zero, Vector3.zero.MirrorOnYZPlane());
        Assert.AreEqual(Vector3.up, Vector3.up.MirrorOnYZPlane());
        Assert.AreEqual(Vector3.down, Vector3.down.MirrorOnYZPlane());
        Assert.AreEqual(Vector3.forward, Vector3.forward.MirrorOnYZPlane());
        Assert.AreEqual(Vector3.back, Vector3.back.MirrorOnYZPlane());
    }

    [Test]
    public void MirrorOnYZPlane_FlipsVectorXAxis() {

        Assert.AreEqual(new Vector3(34.29486f, .35193f, -.81371f), new Vector3(-34.29486f, .35193f, -.81371f).MirrorOnYZPlane());
        Assert.AreEqual(Vector3.right, Vector3.left.MirrorOnYZPlane());
        Assert.AreEqual(Vector3.left, Vector3.right.MirrorOnYZPlane());
    }

    [Test]
    public void MirrorEulerAnglesOnYZPlane_PreservesXAxisRotation() {

        Assert.AreEqual(Quaternion.identity, Quaternion.Euler(Vector3.zero.MirrorEulerAnglesOnYZPlane()));
        Assert.AreEqual(Quaternion.Euler(Vector3.right), Quaternion.Euler(Vector3.right.MirrorEulerAnglesOnYZPlane()));
        Assert.AreEqual(Quaternion.Euler(Vector3.left), Quaternion.Euler(Vector3.left.MirrorEulerAnglesOnYZPlane()));
    }

    [Test]
    public void MirrorEulerAnglesOnYZPlane_FlipsYZAxesRotation() {

        Assert.AreEqual(Quaternion.Euler(new Vector3(1, 1, 1)), Quaternion.Euler(new Vector3(1, -1, -1).MirrorEulerAnglesOnYZPlane()));
        Assert.AreEqual(Quaternion.Euler(new Vector3(-1, 1, 0)), Quaternion.Euler(new Vector3(-1, -1, 0).MirrorEulerAnglesOnYZPlane()));
        Assert.AreEqual(Quaternion.Euler(new Vector3(0, 0, 1)), Quaternion.Euler(new Vector3(0, 0, -1).MirrorEulerAnglesOnYZPlane()));
        Assert.AreEqual(Quaternion.Euler(new Vector3(1, -123.16526f, 8.23576f)), Quaternion.Euler(new Vector3(1, 123.16526f, -8.23576f).MirrorEulerAnglesOnYZPlane()));
    }
}
