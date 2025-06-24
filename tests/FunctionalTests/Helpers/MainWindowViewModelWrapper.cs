using NUnit.Framework;
using OfficeRibbonXEditor.Common;
using OfficeRibbonXEditor.Extensions;
using OfficeRibbonXEditor.ViewModels.Documents;
using OfficeRibbonXEditor.ViewModels.Windows;

namespace OfficeRibbonXEditor.FunctionalTests.Helpers;

public class MainWindowViewModelWrapper : ViewModelWrapper<MainWindowViewModel>
{
    public OfficeDocumentViewModel OpenDocument(string path, bool select = true)
    {
        FileToBeOpened = path;
        var count = ViewModel.DocumentList.Count;
        ViewModel.OpenDocument();
        Assert.That(ViewModel.DocumentList.Count, Is.EqualTo(count + 1), $"Document did not open: {path}");
        var doc = ViewModel.DocumentList[^1];
        if (select)
        {
            ViewModel.SelectedItem = doc;
        }

        return doc;
    }

    public (OfficeDocumentViewModel, OfficePartViewModel) OpenAndInsertPart(string path, XmlPart partType = XmlPart.RibbonX12, bool select = true)
    {
        var doc = OpenDocument(path);
        if (partType == XmlPart.RibbonX12)
        {
            ViewModel.InsertXml12();
        }
        else
        {
            ViewModel.InsertXml14();
        }

        Assert.That(doc.Children, Is.Not.Empty, "XML part was not interested");
        Assert.That(doc.Children[0], Is.InstanceOf<OfficePartViewModel>(), "Wrong class was inserted");

        if (select)
        {
            ViewModel.SelectedItem = doc.Children[0];
        }

        return (doc, (OfficePartViewModel)doc.Children[0]);
    }

    public void SaveAs(OfficeDocumentViewModel document, string destination)
    {
        ViewModel.SelectedItem = document;
        FileToBeSaved = destination;
        ViewModel.SaveAs();
        FileToBeSaved = null;
    }

    public void InsertIcons(OfficePartViewModel part, params string[] icons)
    {
        FilesToBeOpened = icons;
        ViewModel.SelectedItem = part;
        ViewModel.InsertIcons();
        FilesToBeOpened = null;
    }
}