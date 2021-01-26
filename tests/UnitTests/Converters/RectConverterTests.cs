using System;
using System;
using System.Collections.Generic;
using System.Windows;
using NUnit.Framework;
using RectConverter = OfficeRibbonXEditor.Converters.RectConverter;

namespace OfficeRibbonXEditor.UnitTests.Converters
{
    public class RectConverterTests : MultiConverterTestsBase<RectConverter>
    {
        protected override List<Type> FromTypes { get; } = new List<Type> {typeof(double), typeof(double)};

        protected override Type ToType { get; } = typeof(Rect);

        [Test]
        [TestCase(64, 64)]
        [TestCase(0, 0)]
        public void ConvertTest(double width, double height)
        {
            // Act
            var rect = this.Convert(new object[] {width, height});
            Assert.NotNull(rect);
            Assert.IsTrue(rect?.GetType() == this.ToType);
        }

        [Test]
        [TestCase(null)]
        [TestCase(true)]
        public void ShouldNotConvertBack(object? original)
        {
            Assert.Throws<InvalidOperationException>(() => this.ConvertBack(original));
        }
    }
}
