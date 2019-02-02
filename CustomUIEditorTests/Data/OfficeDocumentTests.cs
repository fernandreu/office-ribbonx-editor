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
        private readonly string sourceFile = Path.Combine(TestContext.CurrentContext.TestDirectory, "Resources/Blank.xlsx");

        private readonly string destFile = Path.Combine(TestContext.CurrentContext.TestDirectory, "Output/BlankSaved.xlsx");

        [SetUp]
        public void SetUp()
        {
            // ReSharper disable once AssignNullToNotNullAttribute
            Directory.CreateDirectory(Path.GetDirectoryName(this.destFile));

            if (File.Exists(this.destFile))
            {
                File.Delete(this.destFile);
            }
        }

        [Test]
        public void SaveTest()
        {
            var doc = new OfficeDocument(this.sourceFile);
            Assert.IsNotNull(doc);
            var part = doc.CreateCustomPart(XmlParts.RibbonX12);
            Assert.IsNotNull(part);
            
            doc.Save(this.destFile);
            Assert.IsTrue(File.Exists(this.destFile), "File was not saved");
        }
    }
}
