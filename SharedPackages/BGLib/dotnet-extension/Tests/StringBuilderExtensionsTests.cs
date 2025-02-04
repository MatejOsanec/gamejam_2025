using System.Text;
using NUnit.Framework;
using UnityEngine.TestTools.Constraints;
using Is = NUnit.Framework.Is;

public class StringBuilderExtensionsTests {

    [DoesNotRequireDomainReloadInit]
    private static readonly (int input, string output)[] kExpectedValues = {
        (0, "0"), (123123123, "123123123"),
        (-1, "-1"), (-978654321, "-978654321")
    };

    [Test]
    public void AppendNumber_ReturnsTheExpectedNumberString(
        [ValueSource(nameof(kExpectedValues))] (int input, string output) expectedValue
    ) {

        var sb = new StringBuilder();
        sb.AppendNumber(expectedValue.input);
        Assert.AreEqual(expectedValue.output, sb.ToString());
    }

    [Test]
    public void AppendNumber_DoesNotAllocateGCMemory(
        [ValueSource(nameof(kExpectedValues))] (int input, string output) expectedValue
    ) {

        var sb = new StringBuilder();
        Assert.That(() => { sb.AppendNumber(expectedValue.input); }, Is.Not.AllocatingGCMemory());
    }
}
