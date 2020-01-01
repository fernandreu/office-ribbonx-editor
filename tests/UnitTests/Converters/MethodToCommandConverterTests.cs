using System;
using System.Collections;
using System.Reflection;
using System.Windows.Input;
using NUnit.Framework;
using OfficeRibbonXEditor.Converters;

namespace OfficeRibbonXEditor.UnitTests.Converters
{
    public class MethodToCommandConverterTests : ConverterTestsBase<MethodToCommandConverter, object, ICommand>
    {
        public static IEnumerable ConvertData => new[]
        {
            new TestCaseData("string", nameof(string.GetHashCode)),
        };

        [Test, TestCaseSource(nameof(ConvertData))]
        public void ConvertTest(object? original, string? methodName)
        {
            // Act
            var (methodInfo, sender) = this.ApplyConversion(original, methodName);

            // Assert
            Assert.NotNull(methodInfo);
            Assert.NotNull(sender);
            Assert.AreSame(original, sender);
            Assert.AreEqual(methodName, methodInfo!.Name);
        }

        [Test]
        [TestCase(null, null)]
        [TestCase("string", typeof(string))]
        public void ShouldNotConvertBack(object? original, object? parameter)
        {
            Assert.Throws<InvalidOperationException>(() => this.ConvertBack(original, parameter));
        }

        private (MethodInfo?, object?) ApplyConversion(object? original, string? methodName)
        {
            var result = this.Convert(original, methodName);
            if (result == null)
            {
                return (null, null);
            }

            var type = result.GetType();
            var methodFieldInfo = type.GetField("method", BindingFlags.NonPublic | BindingFlags.Instance);
            var methodInfo = methodFieldInfo?.GetValue(result) as MethodInfo;
            var senderFieldInfo = type.GetField("sender", BindingFlags.NonPublic | BindingFlags.Instance);
            var sender = senderFieldInfo?.GetValue(result);
            return (methodInfo, sender);
        }
    }
}
