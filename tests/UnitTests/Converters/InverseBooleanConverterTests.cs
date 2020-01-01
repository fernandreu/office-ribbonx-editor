using System;
using NUnit.Framework;
using OfficeRibbonXEditor.Converters;

namespace OfficeRibbonXEditor.UnitTests.Converters
{
    public class InverseBooleanConverterTests : ConverterTestsBase<InverseBooleanConverter, bool, bool>
    {
        [Test]
        [TestCase(true, ExpectedResult = false)]
        [TestCase(false, ExpectedResult = true)]
        public object? ConvertTest(object? original)
        {
            return this.Convert(original);
        }

        [Test]
        [TestCase(true, ExpectedResult = false)]
        [TestCase(false, ExpectedResult = true)]
        public object? ConvertBackTest(object? original)
        {
            return this.ConvertBack(original);
        }

        [Test]
        [TestCase(null, typeof(NullReferenceException))]
        [TestCase("wrong type", typeof(InvalidCastException))]
        public void InvalidTypesShouldThrow(object? original, Type exceptionType)
        {
            Assert.Throws(exceptionType, () => this.Convert(original));
            Assert.Throws(exceptionType, () => this.ConvertBack(original));
        }
    }
}
