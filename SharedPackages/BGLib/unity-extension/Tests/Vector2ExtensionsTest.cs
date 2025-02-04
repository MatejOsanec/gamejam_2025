using UnityEngine;
using UnityEditor;
using NUnit.Framework;

public class Vector2ExtensionsTest {

    [Test]
    public void AngleToLine() {

        Assert.IsTrue(Mathf.Approximately(new Vector2(1.0f, 0.0f).SignedAngleToLine(new Vector2(1.0f, 0.0f)), 0.0f));
        Assert.IsTrue(Mathf.Approximately(new Vector2(1.0f, 0.0f).SignedAngleToLine(new Vector2(0.5f, 0.0f)), 0.0f));
        Assert.IsTrue(Mathf.Approximately(new Vector2(1.0f, 0.0f).SignedAngleToLine(new Vector2(-1.0f, 0.0f)), 0.0f));

        Assert.IsTrue(Mathf.Approximately(new Vector2(0.5f, 0.0f).SignedAngleToLine(new Vector2(1.0f, 0.0f)), 0.0f));
        Assert.IsTrue(Mathf.Approximately(new Vector2(0.5f, 0.0f).SignedAngleToLine(new Vector2(0.5f, 0.0f)), 0.0f));
        Assert.IsTrue(Mathf.Approximately(new Vector2(0.5f, 0.0f).SignedAngleToLine(new Vector2(-1.0f, 0.0f)), 0.0f));

        Assert.IsTrue(Mathf.Approximately(new Vector2(0.5f, 0.0f).SignedAngleToLine(new Vector2(-1.0f, 0.0f)), 0.0f));

        Assert.IsTrue(Mathf.Approximately(new Vector2(1.0f, 0.1f).SignedAngleToLine(new Vector2(-1.0f, 0.0f)), -new Vector2(1.0f, -0.1f).SignedAngleToLine(new Vector2(-1.0f, 0.0f))));

        Assert.IsTrue(Mathf.Approximately(new Vector2(1.0f, 0.1f).SignedAngleToLine(new Vector2(1.0f, 0.0f)), -new Vector2(1.0f, -0.1f).SignedAngleToLine(new Vector2(-1.0f, 0.0f))));

        Assert.IsTrue(Mathf.Approximately(new Vector2(-1.0f, 0.1f).SignedAngleToLine(new Vector2(1.0f, 0.0f)), new Vector2(1.0f, -0.1f).SignedAngleToLine(new Vector2(-1.0f, 0.0f))));

        Assert.IsTrue(Mathf.Approximately(new Vector2(0.1f, 1.0f).SignedAngleToLine(new Vector2(0.0f, 1.0f)), -new Vector2(-0.1f, 1.0f).SignedAngleToLine(new Vector2(0.0f, 1.0f))));
    }
}
