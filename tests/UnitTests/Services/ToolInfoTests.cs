using System;
using NUnit.Framework;
using OfficeRibbonXEditor.Services;

namespace OfficeRibbonXEditor.UnitTests.Services
{
    [TestFixture]
    public class ToolInfoTests
    {
        [Test]
        public void CanGetTitle()
        {
            // Arrange
            var service = new ToolInfo();

            // Act
            var title = service.AssemblyTitle;

            // Assert
            Assert.That(title, Does.Match(@".*Office.*RibbonX.*Editor.*"));
        }

        [Test]
        public void CanGetVersion()
        {
            // Arrange
            var service = new ToolInfo();

            // Act
            var version = service.AssemblyVersion;

            // Assert
            Assert.That(version, Does.Match(@"\d+\.\d+\.\d+\.\d+"));
        }

        [Test]
        public void CanGetCopyright()
        {
            // Arrange
            var service = new ToolInfo();

            // Act
            var copyright = service.AssemblyCopyright;

            // Assert
            Assert.That(copyright, Does.Match(@".*\(c\).*Fernando Andreu.*"), "Author does not appear in copyright");
            Assert.That(copyright, Does.Match($".*{DateTime.Now.Year}.*"), "Copyright does not show current year");
        }

        [Test]
        public void CanGetCompany()
        {
            // Arrange
            var service = new ToolInfo();

            // Act
            var company = service.AssemblyCompany;

            // Assert
            Assert.That(company, Does.Match(@".*Fernando Andreu.*"), "Company should be set to the author for now");
        }

        [Test]
        public void CanGetDescription()
        {
            // Arrange
            var service = new ToolInfo();

            // Act
            var description = service.AssemblyDescription;

            // Assert
            Assert.That(description, Is.Not.Null.And.Not.Empty, "Assembly should have a description");
        }

        [Test]
        public void CanGetRuntimeVersion()
        {
            // Arrange
            var service = new ToolInfo();

            // Act
            var version = service.RuntimeVersion;

            // Assert
            Assert.That(version, Does.Match(".*Core.*").Or.Match(".*Framework.*"), "Unknown runtime");
        }

        [Test]
        public void CanGetOperatingSystemVersion()
        {
            // Arrange
            var service = new ToolInfo();

            // Act
            var version = service.OperatingSystemVersion;

            // Assert
            Assert.That(version, Does.Match(".*Windows.*"), "Unknown operating system");
        }

        [Test]
        public void CanGetProduct()
        {
            // Arrange
            var service = new ToolInfo();

            // Act
            var product = service.AssemblyProduct;

            // Assert
            Assert.That(product, Does.Match(@".*Office.*RibbonX.*Editor.*"));
        }
    }
}
