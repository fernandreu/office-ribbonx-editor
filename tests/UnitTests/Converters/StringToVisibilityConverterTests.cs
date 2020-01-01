using System;
using System.Windows;
using NUnit.Framework;
using OfficeRibbonXEditor.Converters;

namespace OfficeRibbonXEditor.UnitTests.Converters
{
    public class StringToVisibilityConverterTests : ConverterTestsBase<StringToVisibilityConverter, string, Visibility>
    {
        [Test]
        [TestCase("string", ExpectedResult = Visibility.Visible)]
        [TestCase("", ExpectedResult = Visibility.Collapsed)]
        [TestCase(null, ExpectedResult = Visibility.Collapsed)]
        public object? ConvertTest(object? original)
        {
            return this.Convert(original);
        }

        [Test]
        [TestCase(null)]
        [TestCase(Visibility.Visible)]
        public void ShouldNotConvertBack(object? original)
        {
            Assert.Throws<InvalidOperationException>(() => this.ConvertBack(original));
        }
    }
}
