using Microsoft.VisualStudio.TestTools.UnitTesting;
using CustomUIEditor.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

// ReSharper disable once CheckNamespace
namespace CustomUIEditor.Data.Tests
{
    using System.IO;

    [TestClass]
    public class OfficeDocumentTests
    {
        [TestMethod]
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