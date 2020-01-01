using System;
using NUnit.Framework;
using OfficeRibbonXEditor.Converters;

namespace OfficeRibbonXEditor.UnitTests.Converters
{
    public class IsOfTypeConverterTests : ConverterTestsBase<IsOfTypeConverter, object, bool>
    {
        [Test]
        [TestCase("string", typeof(string), ExpectedResult = true)]
        [TestCase(2, typeof(int), ExpectedResult = true)]
        [TestCase(null, typeof(string), ExpectedResult = false)]
        [TestCase("string", typeof(int), ExpectedResult = false)]
        [TestCase("string", null, ExpectedResult = false)]
        public object? ConvertTest(object? original, object? parameter)
        {
            return this.Convert(original, parameter);
        }

        [Test]
        [TestCase(null, null)]
        [TestCase("string", typeof(string))]
        public void ShouldNotConvertBack(object? original, object? parameter)
        {
            Assert.Throws<InvalidOperationException>(() => this.ConvertBack(original, parameter));
        }
    }
}
