using NUnit.Framework;
using UnityEngine;

[TestFixture]
public class UnityObjectExtensionsTest {

    [Test]
    public void GetHierarchyPath_ReturnsNameForRoot() {

        var gameObject = new GameObject("Loner");
        var hierarchyPath = gameObject.GetHierarchyPath();
        Assert.AreEqual("Loner", hierarchyPath);
    }

    [Test]
    public void GetHierarchyPath_ReturnsPathWithSlashes() {

        var parentGO = new GameObject("parent");
        var middleGO = new GameObject("middle");
        var childGO = new GameObject("child");
        middleGO.transform.SetParent(parentGO.transform);
        childGO.transform.SetParent(middleGO.transform);
        var hierarchyPath = childGO.GetHierarchyPath();
        Assert.AreEqual("parent/middle/child", hierarchyPath);
    }
}
