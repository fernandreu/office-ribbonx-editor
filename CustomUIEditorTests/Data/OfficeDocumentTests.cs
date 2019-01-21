// ReSharper disable once CheckNamespace
namespace CustomUIEditor.Data.Tests
{
    using System.IO;
    using NUnit.Framework;

    [TestFixture]
    public class OfficeDocumentTests
    {
        [Test]
        public void SaveTest()
        {
            var doc = new OfficeDocument(@"Resources\Blank.xlsx");
            Assert.IsNotNull(doc);
            var part = doc.CreateCustomPart(XmlParts.RibbonX12);
            Assert.IsNotNull(part);

            var path = @"Resources\BlankCopy.xlsx";
            doc.Save(path);
            Assert.IsTrue(File.Exists(path), "File was not saved");
        }
        
    }
}