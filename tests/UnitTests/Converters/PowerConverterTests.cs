using NUnit.Framework;
using OfficeRibbonXEditor.Converters;

namespace OfficeRibbonXEditor.UnitTests.Converters
{
    public class PowerConverterTests : ConverterTestsBase<PowerConverter, double, double>
    {
        [Test]
        [TestCase(3, 10, 1000)]
        [TestCase(2.0, null, 4.0)]
        [TestCase(null, null, null)]
        public void ConvertTest(double? exponent, double? baseValue, double? expected)
        {
            // Act
            var converted = Convert(exponent, baseValue);

            // Assert
            if (expected == null)
            {
                Assert.Null(converted);
            }
            else
            {
                Assert.NotNull(converted);
                Assert.AreEqual(expected.Value, (double) converted!, 0.0001);
            }
        }

        [Test]
        [TestCase(1000, 10, 3d)]
        [TestCase(4.0, null, 2.0)]
        [TestCase(null, null, null)]
        public void ConvertBackTest(double? result, double? baseValue, double? expected)
        {
            // Act
            var converted = ConvertBack(result, baseValue);

            // Assert
            if (expected == null)
            {
                Assert.Null(converted);
            }
            else
            {
                Assert.NotNull(converted);
                Assert.AreEqual(expected.Value, (double)converted!, 0.0001);
            }
        }
    }
}
