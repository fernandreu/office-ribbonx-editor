﻿using System.IO;
using NUnit.Framework;
using OfficeRibbonXEditor.Common;
using OfficeRibbonXEditor.ViewModels.Documents;

namespace OfficeRibbonXEditor.FunctionalTests.Documents;

[TestFixture]
public class OfficeDocumentViewModelTests
{
    private readonly string _sourceFile = Path.Combine(TestContext.CurrentContext.TestDirectory, "Resources/Blank.xlsx");

    private readonly string _destFile = Path.Combine(TestContext.CurrentContext.TestDirectory, "Output/BlankSaved.xlsx");

    private readonly string _undoIcon = Path.Combine(TestContext.CurrentContext.TestDirectory, "Resources/undo.png");

    private readonly string _redoIcon = Path.Combine(TestContext.CurrentContext.TestDirectory, "Resources/redo.png");

    [SetUp]
    public void SetUp()
    {
        var directory = Path.GetDirectoryName(_destFile);
        if (directory == null)
        {
            Assert.Fail("Wrong _destFile path");
            return; // Not needed, but suppreses nullable warnings below
        }

        Directory.CreateDirectory(directory);

        if (File.Exists(_destFile))
        {
            File.Delete(_destFile);
        }
    }

    [Test]
    public void PartShouldBeInserted()
    {
        // Arrange
        using var doc = new OfficeDocument(_sourceFile);
        using var viewModel = new OfficeDocumentViewModel(doc);
            
        // Act
        viewModel.InsertPart(XmlPart.RibbonX12);

        // Assert
        Assert.That(viewModel.Children.Count, Is.EqualTo(1));
    }

    [Test]
    public void DocumentShouldBeSaved()
    {
        // Arrange
        using var doc = new OfficeDocument(_sourceFile);
        using var viewModel = new OfficeDocumentViewModel(doc);
        viewModel.InsertPart(XmlPart.RibbonX12);
        Assume.That(File.Exists(_destFile), Is.False, "File was not deleted before test");

        // Act
        doc.Save(_destFile);

        // Assert
        Assert.That(File.Exists(_destFile), Is.True, "File was not saved");
    }
        
    /// <summary>
    /// The reload before saving can be tricky, especially if icons are involved
    /// </summary>
    [Test]
    public void ReloadOnSaveTest()
    {
        // Arrange
        static void CheckIntegrity(OfficeDocumentViewModel innerModel)
        {
            Assert.That(innerModel.Children.Count, Is.EqualTo(2));

            for (var i = 0; i < 2; ++i)
            {
                var innerPart = (OfficePartViewModel)innerModel.Children[i];

                if (innerPart.Part?.PartType == XmlPart.RibbonX12)
                {
                    Assert.That(innerPart.Children.Count, Is.EqualTo(1));
                    Assert.That(((IconViewModel)innerPart.Children[0]).Name, Is.EqualTo("redo"));
                }
                else
                {
                    Assert.That(innerPart.Children.Count, Is.EqualTo(2));
                    Assert.That(((IconViewModel)innerPart.Children[0]).Name, Is.EqualTo("changedId"));
                    Assert.That(((IconViewModel)innerPart.Children[1]).Name, Is.EqualTo("redo"));
                }
            }
        }

        using (var doc = new OfficeDocument(_sourceFile))
        using (var viewModel = new OfficeDocumentViewModel(doc))
        {
            viewModel.InsertPart(XmlPart.RibbonX12);
            viewModel.InsertPart(XmlPart.RibbonX14);

            var part = (OfficePartViewModel) viewModel.Children[0];
            part.InsertIcon(_undoIcon);
            part.InsertIcon(_redoIcon);
            part.RemoveIcon("undo");
            part = (OfficePartViewModel) viewModel.Children[1];
            part.InsertIcon(_redoIcon);
            var icon = (IconViewModel) part.Children[0];
            icon.Name = "changedId";
            part.InsertIcon(_redoIcon);

            // Act / assert
            CheckIntegrity(viewModel);

            viewModel.Save(false, _destFile);
        }

        using (var doc = new OfficeDocument(_destFile))
        using (var viewModel = new OfficeDocumentViewModel(doc))
        {
            CheckIntegrity(viewModel);
            viewModel.Save(true, _destFile);
        }

        using (var doc = new OfficeDocument(_destFile))
        using (var viewModel = new OfficeDocumentViewModel(doc))
        {
            CheckIntegrity(viewModel);
        }
    }
}