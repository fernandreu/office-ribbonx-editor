using System.IO;
using System.Xml;
using NUnit.Framework;
using OfficeRibbonXEditor.Helpers;

namespace OfficeRibbonXEditor.UnitTests.Xml
{
    public class CallbackTests
    {
        private const string XmlWithDuplicatedAttributeValues =
            "<customUI xmlns=\"http://schemas.microsoft.com/office/2009/07/customui\">" +
            "<ribbon><tabs><tab label=\"CustomTab\"><group label=\"CustomGroup\">" +
            "<button id=\"CustomButton\" label=\"CustomButton\" size=\"large\" onAction=\"CustomButton\" />" +
            "</group></tab></tabs></ribbon></customUI>";

        private const string XmlWithDuplicatedCallbacks =
            "<customUI xmlns=\"http://schemas.microsoft.com/office/2009/07/customui\">" +
            "<ribbon><tabs><tab label=\"CustomTab\"><group label=\"CustomGroup\">" +
            "<button id=\"CustomButton\" label=\"CustomButton\" size=\"large\" onAction=\"CustomButton\" />" +
            "<button id=\"CustomButton2\" label=\"CustomButton\" size=\"large\" onAction=\"CustomButton\" />" +
            "</group></tab></tabs></ribbon></customUI>";

        private const string ExpectedCode =
            "'Callback for CustomButton onAction\n" +
            "Sub CustomButton(control As IRibbonControl)\n" +
            "End Sub";

        [SetUp]
        public void Setup()
        {
        }

        [TestCase(XmlWithDuplicatedAttributeValues, ExpectedCode)]
        [TestCase(XmlWithDuplicatedCallbacks, ExpectedCode)]
        public void GeneratedCallbacksAreCorrect(string xml, string expected)
        {
            // Arrange
            var doc = new XmlDocument { XmlResolver = null };
            using (var stringReader = new StringReader(xml))
            using (var xmlReader = XmlReader.Create(stringReader, new XmlReaderSettings { XmlResolver = null }))
            {
                doc.Load(xmlReader);
            }

            // Act
            var callbacks = CallbacksBuilder.GenerateCallback(doc)?
                .ToString()
                .Trim();

            // Assert
            Assert.AreEqual(expected, callbacks);
        }
    }
}