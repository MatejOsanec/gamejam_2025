#nullable enable
using System.Diagnostics.CodeAnalysis;
using NUnit.Framework;

public static class NullableAssert {

// suppressing check that parameter must have a non-null value when exiting because the execution will stop with an exception in case it's null.
#pragma warning disable CS8777
    public static void IsNotNull([NotNull] object? anObject, string? message = null, params object[] args) {

        Assert.That(anObject!, Is.Not.Null, message, args);
    }

#pragma warning restore CS8777
}
