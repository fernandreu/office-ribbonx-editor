﻿using System.IO;
using NUnit.Framework;
using OfficeRibbonXEditor.Common;

namespace OfficeRibbonXEditor.IntegrationTests.Documents;

[TestFixture]
public class OfficeDocumentTests
{
    private readonly string _sourceFile = Path.Combine(TestContext.CurrentContext.TestDirectory, "Resources/Blank.xlsx");

    private readonly string _destFile = Path.Combine(TestContext.CurrentContext.TestDirectory, "Output/BlankSaved.xlsx");

    [SetUp]
    public void SetUp()
    {
        // ReSharper disable once AssignNullToNotNullAttribute
        Directory.CreateDirectory(Path.GetDirectoryName(_destFile)!);

        if (File.Exists(_destFile))
        {
            File.Delete(_destFile);
        }
    }

    [Test]
    public void DocumentShouldBeOpened()
    {
        // Arrange / act
        using var doc = new OfficeDocument(_sourceFile);
        
        // Assert
        Assert.That(doc.UnderlyingPackage, Is.Not.Null, "Package was not opened");
    }

    [Test]
    public void PartShouldBeCreated()
    {
        // Arrange
        using var doc = new OfficeDocument(_sourceFile);
        
        // Act
        var part = doc.CreateCustomPart(XmlPart.RibbonX12);

        // Assert
        Assert.That(part, Is.Not.Null, "Part was not inserted");
    }

    [Test]
    public void DocumentShouldBeSaved()
    {
        // Arrange
        using var doc = new OfficeDocument(_sourceFile);
        
        // Act
        doc.Save(_destFile);

        // Assert
        Assert.That(File.Exists(_destFile), Is.True, "File was not saved");
    }
}