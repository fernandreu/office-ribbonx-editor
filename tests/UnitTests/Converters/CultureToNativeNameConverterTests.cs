using System;
using System.Collections.Generic;
using System.Windows;
using NUnit.Framework;
using OfficeRibbonXEditor.Converters;
using RectConverter = OfficeRibbonXEditor.Converters.RectConverter;

namespace OfficeRibbonXEditor.UnitTests.Converters
{
    public class CultureToNativeNameConverterTests : ConverterTestsBase<CultureToNativeNameConverter, string, string>
    {
        [Test]
        [TestCase("en-US", ExpectedResult = "English (United States)")]
        public object? ConvertTest(object? original)
        {
            return Convert(null, original);
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
