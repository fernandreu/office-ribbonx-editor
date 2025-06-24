using System;
using System.IO;
using System.Linq;
using OfficeRibbonXEditor.Common;

namespace OfficeRibbonXEditor.ViewModels.Documents;

public class OfficeDocumentViewModel : TreeViewItemViewModel, IDisposable
{
    private bool _partsAddedOrRemoved;

    private bool _disposed;

    public OfficeDocumentViewModel(OfficeDocument document) 
        : base(null, false, false)
    {
        Document = document;
        LoadParts();
    }

    public OfficeDocument Document { get; private set; }

    public override string Name => Path.GetFileName(Document.Name);

    public void RaiseNameChanged()
    {
        OnPropertyChanged(nameof(Name));
    }
        
    /// <summary>
    /// Gets a value indicating whether any of the parts of this document has unsaved changes.
    /// </summary>
    public bool HasUnsavedChanges
    {
        get
        {
            if (_partsAddedOrRemoved)
            {
                return true;
            }

            foreach (var child in Children)
            {
                if (child is OfficePartViewModel part && part.HasUnsavedChanges)
                {
                    return true;
                }
            }

            return false;
        }
    }

    public string? ImageSource => Document.FileType switch
    {
        OfficeApplication.Excel => "/Resources/Images/excelwkb.png",
        OfficeApplication.PowerPoint => "/Resources/Images/pptpre.png",
        OfficeApplication.Word => "/Resources/Images/worddoc.png",
        OfficeApplication.Visio => "/Resources/Images/visiodoc.png",
        OfficeApplication.Xml => "/Resources/Images/xml.png",
        _ => null,
    };
        
    /// <summary>
    /// Reloads the associated Office document, but keeping the OfficeParts currently shown in the GUI. This
    /// ensures that, if the files have been modified externally, the program is still looking at their latest
    /// version. Otherwise, we might accidentally lose those external changes when saving
    /// </summary>
    public void Reload()
    {
        // Store the file name (otherwise, it will have been erased after calling Dispose)
        var fileName = Document.Name;

        // Dispose current document (not needed as references to its parts are stored in their View models anyway)
        Document.Dispose();

        // Then, reload it
        Document = new OfficeDocument(fileName);
            
        // Delete all its original parts
        foreach (var type in Enum.GetValues<XmlPart>())
        {
            Document.RemoveCustomPart(type);
        }

        // Instead, use the parts currently shown in the editor
        foreach (var part in Children.OfType<OfficePartViewModel>())
        {
            if (part.Part == null)
            {
                continue;
            }

            Document.SaveCustomPart(part.Part.PartType, part.OriginalContents, true);
                
            // Re-map the Part. This ensures that the PackagePart stored internally in OfficePart points to
            // the right location, in case it is needed
            part.Reload();
        }
    }

    public void Save(bool reloadFirst = false, string? fileName = null, bool preserveAttributes = true)
    {
        if (reloadFirst)
        {
            Reload();
        }

        // Save each individual part
        foreach (var part in Children.OfType<OfficePartViewModel>())
        {
            part.Save();
        }

        // Now save the actual document
        Document.Save(fileName, preserveAttributes);

        // The save operation closes the internal package and re-opens it. Hence, parts need to be re-mapped
        foreach (var part in Children.OfType<OfficePartViewModel>())
        {
            if (part.Part == null)
            {
                continue;
            }

            part.Part = Document.RetrieveCustomPart(part.Part.PartType);
        }

        _partsAddedOrRemoved = false;
    }

    public void InsertPart(XmlPart type)
    {
        // Check if the part does not exist yet
        var part = Document.RetrieveCustomPart(type);
        if (part != null)
        {
            return;
        }

        part = Document.CreateCustomPart(type);
        var partModel = new OfficePartViewModel(part, this);
        Children.Add(partModel);
        partModel.IsSelected = true;
        _partsAddedOrRemoved = true;
    }

    public void RemovePart(XmlPart type)
    {
        Document.RemoveCustomPart(type);
        _partsAddedOrRemoved = true;

        for (var i = 0; i < Children.Count; ++i)
        {
            if (Children[i] is not OfficePartViewModel part || part.Part == null)
            {
                continue;
            }

            if (part.Part.PartType == type)
            {
                Children.RemoveAt(i);
                return;
            }
        }
    }

    protected override void LoadChildren()
    {
        LoadParts();
    }

    private void LoadParts()
    {
        foreach (var part in Document.Parts ?? Enumerable.Empty<OfficePart>())
        {
            Children.Add(new OfficePartViewModel(part, this));
        }
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (_disposed)
        {
            return;
        }

        if (disposing)
        {
            Document?.Dispose();
        }

        _disposed = true;
    }
}