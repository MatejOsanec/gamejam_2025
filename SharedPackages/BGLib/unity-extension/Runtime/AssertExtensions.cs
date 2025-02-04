using System.Diagnostics;
using UnityEngine.Assertions;

[DebuggerStepThrough]
public static class AssertExtensions {

    private const string kUnityAssertions = "UNITY_ASSERTIONS";

    [Conditional(kUnityAssertions)]
    public static void LessThan(float value, float expectedGreaterValue, string message = null) {

        Assert.IsTrue(value < expectedGreaterValue, GetMessage("{0} < {1}", value, expectedGreaterValue, message));
    }

    [Conditional(kUnityAssertions)]
    public static void GreaterOrEqual(float value, float expectedLessValue, string message = null) {

        Assert.IsTrue(value >= expectedLessValue, GetMessage("{0} >= {1}", value, expectedLessValue, message));
    }

    private static string GetMessage<T>(string pattern, T value, T other, string message = null) {

        var result = string.Format(pattern, value, other);
        if (!string.IsNullOrWhiteSpace(message)) {
            result += $". {message}";
        }
        return result;
    }
}
