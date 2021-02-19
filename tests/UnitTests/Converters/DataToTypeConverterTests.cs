using System;
using NUnit.Framework;
using OfficeRibbonXEditor.Converters;

namespace OfficeRibbonXEditor.UnitTests.Converters
{
    public class DataToTypeConverterTests : ConverterTestsBase<DataToTypeConverter, object, Type>
    {
        [Test]
        [TestCase("string", ExpectedResult = typeof(string))]
        [TestCase(2, ExpectedResult = typeof(int))]
        [TestCase(null, ExpectedResult = null)]
        public object? ConvertTest(object? original)
        {
            return Convert(original);
        }

        [Test]
        [TestCase(null)]
        [TestCase("string")]
        public void ShouldNotConvertBack(object? original)
        {
            Assert.Throws<InvalidOperationException>(() => this.ConvertBack(original));
        }
    }
}
