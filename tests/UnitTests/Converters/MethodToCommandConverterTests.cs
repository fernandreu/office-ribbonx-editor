using System;
using System.Reflection;
using System.Windows.Input;
using NUnit.Framework;
using OfficeRibbonXEditor.Converters;

namespace OfficeRibbonXEditor.UnitTests.Converters;

public class MethodToCommandConverterTests : ConverterTestsBase<MethodToCommandConverter, object, ICommand>
{
    [Test]
    [TestCase("string", nameof(string.GetHashCode))]
    public void ConvertTest(object? original, string? methodName)
    {
        // Act
        var (methodInfo, sender) = ApplyConversion(original, methodName);

        // Assert
        Assert.That(methodInfo, Is.Not.Null);
        Assert.That(sender, Is.Not.Null);
        Assert.That(sender, Is.SameAs(original));
        Assert.That(methodInfo!.Name, Is.EqualTo(methodName));
    }

    [Test]
    [TestCase(null, null)]
    [TestCase("string", typeof(string))]
    public void ShouldNotConvertBack(object? original, object? parameter)
    {
        Assert.Throws<InvalidOperationException>(() => ConvertBack(original, parameter));
    }

    private (MethodInfo?, object?) ApplyConversion(object? original, string? methodName)
    {
        var result = Convert(original, methodName);
        if (result == null)
        {
            return (null, null);
        }

        var type = result.GetType();
        var methodFieldInfo = type.GetField("_method", BindingFlags.NonPublic | BindingFlags.Instance);
        var methodInfo = methodFieldInfo?.GetValue(result) as MethodInfo;
        var senderFieldInfo = type.GetField("_sender", BindingFlags.NonPublic | BindingFlags.Instance);
        var sender = senderFieldInfo?.GetValue(result);
        return (methodInfo, sender);
    }
}