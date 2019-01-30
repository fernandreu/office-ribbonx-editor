// --------------------------------------------------------------------------------------------------------------------
// <copyright file="OfficeDocumentTests.cs" company="FA">
//   Fernando Andreu
// </copyright>
// <summary>
//   Defines the OfficeDocumentTests type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace CustomUIEditor.Data
{
    using System.IO;
    using NUnit.Framework;

    [TestFixture]
    public class OfficeDocumentTests
    {
        [Test]
        public void SaveTest()
        {
            var testFile = Path.Combine(TestContext.CurrentContext.TestDirectory, "Resources/Blank.xlsx");
            
            var doc = new OfficeDocument(testFile);
            Assert.IsNotNull(doc);
            var part = doc.CreateCustomPart(XmlParts.RibbonX12);
            Assert.IsNotNull(part);

            var savePath = Path.Combine(TestContext.CurrentContext.TestDirectory, "Output/BlankSaved.xlsx");
            // ReSharper disable once AssignNullToNotNullAttribute
            Directory.CreateDirectory(Path.GetDirectoryName(savePath));

            if (File.Exists(savePath))
            {
                File.Delete(savePath);
            }
            
            doc.Save(savePath);
            Assert.IsTrue(File.Exists(savePath), "File was not saved");
        }
        
    }
}
