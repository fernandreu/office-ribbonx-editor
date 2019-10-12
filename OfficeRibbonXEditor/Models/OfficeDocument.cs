using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Packaging;

namespace OfficeRibbonXEditor.Models
{
    public enum XmlParts
    {
        Qat12,
        RibbonX12,
        RibbonX14,
        LastEntry // Always Last
    }

    public enum OfficeApplications
    {
        Word,
        Excel,
        PowerPoint,
        Xml,
    }

    public class OfficeDocument : IDisposable
    {
        public const string CustomUiPartRelType = "http://schemas.microsoft.com/office/2006/relationships/ui/extensibility";

        public const string CustomUi14PartRelType = "http://schemas.microsoft.com/office/2007/relationships/ui/extensibility";

        public const string QatPartRelType = "http://schemas.microsoft.com/office/2006/relationships/ui/customization";

        public const string ImagePartRelType = "http://schemas.openxmlformats.org/officeDocument/2006/relationships/image";

        private string tempFileName;
        
        private bool disposed;

        public OfficeDocument(string fileName)
        {
            if (fileName == null)
            {
                throw new ArgumentNullException(nameof(fileName));
            }

            if (fileName.Length == 0)
            {
                throw new ArgumentException("File name cannot be empty.");
            }

            this.Name = fileName;

            this.tempFileName = Path.GetTempFileName();

            File.Copy(this.Name, this.tempFileName, true /*overwrite*/);
            File.SetAttributes(this.tempFileName, FileAttributes.Normal);

            this.Init();
            this.IsDirty = false;
        }

        ~OfficeDocument()
        {
            this.Dispose(true /*isFramework*/);
        }

        public Package UnderlyingPackage { get; private set; }

        public List<OfficePart> Parts { get; private set; }

        public string Name { get; private set; }

        public bool IsDirty { get; set; }

        public bool HasCustomUi
        {
            get
            {
                if (this.Parts == null || this.Parts.Count == 0)
                {
                    return false;
                }

                foreach (var part in this.Parts)
                {
                    if (part.PartType == XmlParts.RibbonX12 || part.PartType == XmlParts.RibbonX14)
                    {
                        return true;
                    }
                }

                return false;
            }
        }
        
        public OfficeApplications FileType => MapFileType(Path.GetExtension(this.Name));

        public static OfficeApplications MapFileType(string extension)
        {
            extension = extension.ToLower();

            if (extension.StartsWith(".do"))
            {
                return OfficeApplications.Word;
            }

            if (extension.StartsWith(".xl"))
            {
                return OfficeApplications.Excel;
            }

            if (extension.StartsWith(".pp"))
            {
                return OfficeApplications.PowerPoint;
            }

            Debug.Assert(false, "Unrecognized extension passed");
            return OfficeApplications.Xml;
        }

        /// <summary>
        /// Determines whether the file loaded for this OfficeDocument has suffered external
        /// modifications since this instance was created
        /// </summary>
        /// <returns>
        /// Whether there were external changes or not
        /// </returns>
        public bool HasExternalChanges()
        {
            // TODO: This doesn't work, due to tempFileName being already open. For this to work properly, an extra temporary copy of tempFileName might be required
            var first = new FileInfo(this.Name);
            var second = new FileInfo(this.tempFileName);

            if (first.Length != second.Length)
            {
                return true;
            }

            const int BytesToRead = sizeof(long);

            var iterations = (int)Math.Ceiling((double)first.Length / BytesToRead);
            
            using (var fs1 = File.Open(this.Name, FileMode.Open, FileAccess.Read, FileShare.Read))
            using (var fs2 = File.Open(this.tempFileName, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                var one = new byte[BytesToRead];
                var two = new byte[BytesToRead];

                for (var i = 0; i < iterations; i++)
                {
                    fs1.Read(one, 0, BytesToRead);
                    fs2.Read(two, 0, BytesToRead);

                    if (BitConverter.ToInt64(one, 0) != BitConverter.ToInt64(two, 0))
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        public void Save(string customFileName = null, bool preserveAttributes = true)
        {
            if (string.IsNullOrEmpty(customFileName))
            {
                customFileName = this.Name;
            }

            Debug.Assert(this.UnderlyingPackage != null, "Failed to get package.");

            if (!this.IsDirty && customFileName == this.Name)
            {
                return;
            }
            
            this.UnderlyingPackage.Flush();
            this.UnderlyingPackage.Close();

            var info = new FileInfo(customFileName);
            if (info.Exists)
            {
                File.SetAttributes(customFileName, FileAttributes.Normal);
            }

            try
            {
                File.Copy(this.tempFileName, customFileName, true /*overwrite*/);
                if (info.Exists && preserveAttributes)
                {
                    File.SetAttributes(customFileName, info.Attributes);
                }
            }
            finally
            {
                this.Init();
            }

            this.IsDirty = false;
        }

        public void RemoveCustomPart(XmlParts partType)
        {
            for (var i = this.Parts.Count - 1; i >= 0; i--)
            {
                if (this.Parts[i].PartType != partType)
                {
                    continue;
                }

                var part = this.Parts[i];
                part.Remove();
                
                this.Parts.RemoveAt(i);

                this.UnderlyingPackage.Flush();
                this.IsDirty = true;
            }
        }

        public void SaveCustomPart(XmlParts partType, string text)
        {
            this.SaveCustomPart(partType, text, false /*isCreatingNewPart*/);
        }

        public void SaveCustomPart(XmlParts partType, string text, bool isCreatingNewPart)
        {
            var targetPart = this.RetrieveCustomPart(partType);

            if (targetPart == null)
            {
                if (isCreatingNewPart)
                {
                    targetPart = this.CreateCustomPart(partType);
                }
                else
                {
                    return;
                }
            }

            Debug.Assert(targetPart != null, "targetPart is null when saving custom part");
            targetPart.Save(text);
            this.IsDirty = true;
        }

        public OfficePart CreateCustomPart(XmlParts partType)
        {
            string relativePath;
            string relType;

            switch (partType)
            {
                case XmlParts.RibbonX12:
                    relativePath = "/customUI/customUI.xml";
                    relType = CustomUiPartRelType;
                    break;
                case XmlParts.RibbonX14:
                    relativePath = "/customUI/customUI14.xml";
                    relType = CustomUi14PartRelType;
                    break;
                case XmlParts.Qat12:
                    relativePath = "/customUI/qat.xml";
                    relType = QatPartRelType;
                    break;
                default:
                    Debug.Assert(false, "Unknown type");
                    // ReSharper disable once HeuristicUnreachableCode
                    return null;
            }

            var customUiUri = new Uri(relativePath, UriKind.Relative);
            var relationship = this.UnderlyingPackage.CreateRelationship(customUiUri, TargetMode.Internal, relType);

            OfficePart part;
            if (!this.UnderlyingPackage.PartExists(customUiUri))
            {
                part = new OfficePart(this.UnderlyingPackage.CreatePart(customUiUri, "application/xml"), partType, relationship.Id);
            }
            else
            {
                part = new OfficePart(this.UnderlyingPackage.GetPart(customUiUri), partType, relationship.Id);
            }

            Debug.Assert(part != null, "Fail to create custom part.");

            this.Parts.Add(part);
            this.IsDirty = true;

            return part;
        }

        public OfficePart RetrieveCustomPart(XmlParts partType)
        {
            Debug.Assert(this.Parts != null, "Document has no xmlParts to retrieve");
            if (this.Parts == null || this.Parts.Count == 0)
            {
                return null;
            }

            foreach (var part in this.Parts)
            {
                if (part.PartType == partType)
                {
                    return part;
                }
            }

            return null;
        }

        public void Dispose()
        {
            this.Dispose(false /*isFramework*/);
            GC.SuppressFinalize(this);
        }

        private void Dispose(bool isFramework)
        {
            if (this.disposed)
            {
                return;
            }

            if (!isFramework)
            {
                this.Name = null;
                if (this.Parts != null && this.Parts.Count > 0)
                {
                    for (int i = 0; i < this.Parts.Count; i++)
                    {
                        this.Parts[i] = null;
                    }
                }

                if (this.UnderlyingPackage != null)
                {
                    try
                    {
                        this.UnderlyingPackage.Close();
                    }
                    catch (ObjectDisposedException ex)
                    {
                        Debug.Fail(ex.Message);
                    }

                    this.UnderlyingPackage = null;
                }

                if (this.tempFileName != null)
                {
                    try
                    {
                        File.Delete(this.tempFileName);
                    }
                    catch (Exception ex)
                    {
                        Debug.Fail(ex.Message);
                    }

                    this.tempFileName = null;
                }
            }

            this.disposed = true;
        }

        private void Init()
        {
            this.UnderlyingPackage = Package.Open(this.tempFileName, FileMode.Open, FileAccess.ReadWrite);

            Debug.Assert(this.UnderlyingPackage != null, "Failed to get package.");
            if (this.UnderlyingPackage == null)
            {
                return;
            }

            this.Parts = new List<OfficePart>();

            foreach (var relationship in this.UnderlyingPackage.GetRelationshipsByType(CustomUi14PartRelType))
            {
                var customUiUri = PackUriHelper.ResolvePartUri(relationship.SourceUri, relationship.TargetUri);
                if (this.UnderlyingPackage.PartExists(customUiUri))
                {
                    this.Parts.Add(new OfficePart(this.UnderlyingPackage.GetPart(customUiUri), XmlParts.RibbonX14, relationship.Id));
                }

                break;
            }

            foreach (var relationship in this.UnderlyingPackage.GetRelationshipsByType(CustomUiPartRelType))
            {
                var customUiUri = PackUriHelper.ResolvePartUri(relationship.SourceUri, relationship.TargetUri);
                if (this.UnderlyingPackage.PartExists(customUiUri))
                {
                    this.Parts.Add(new OfficePart(this.UnderlyingPackage.GetPart(customUiUri), XmlParts.RibbonX12, relationship.Id));
                }

                break;
            }

            foreach (var relationship in this.UnderlyingPackage.GetRelationshipsByType(QatPartRelType))
            {
                var qatUri = PackUriHelper.ResolvePartUri(relationship.SourceUri, relationship.TargetUri);
                if (this.UnderlyingPackage.PartExists(qatUri))
                {
                    this.Parts.Add(new OfficePart(this.UnderlyingPackage.GetPart(qatUri), XmlParts.Qat12, relationship.Id));
                }

                break;
            }
        }
    }
}
