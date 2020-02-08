using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Packaging;
using System.Linq;

namespace OfficeRibbonXEditor.Documents
{
    public enum XmlPart
    {
        Qat12,
        RibbonX12,
        RibbonX14,
        LastEntry // Always Last
    }

    public enum OfficeApplication
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

        private readonly string tempFileName;
        
        private bool disposed;

#pragma warning disable CS8618 // Non-nullable field is uninitialized. Consider declaring as nullable. Triggered due to UnderlyingPackage, but this is assigned in Init()
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
#pragma warning restore CS8618 // Non-nullable field is uninitialized. Consider declaring as nullable.

        public Package UnderlyingPackage { get; private set; }

        public List<OfficePart>? Parts { get; private set; }

        public string Name { get; set; }

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
                    if (part.PartType == XmlPart.RibbonX12 || part.PartType == XmlPart.RibbonX14)
                    {
                        return true;
                    }
                }

                return false;
            }
        }
        
        public OfficeApplication FileType => MapFileType(Path.GetExtension(this.Name));

        public static OfficeApplication MapFileType(string extension)
        {
            if (extension == null)
            {
                return OfficeApplication.Xml;
            }

            if (extension.StartsWith(".do", StringComparison.OrdinalIgnoreCase))
            {
                return OfficeApplication.Word;
            }

            if (extension.StartsWith(".xl", StringComparison.OrdinalIgnoreCase))
            {
                return OfficeApplication.Excel;
            }

            if (extension.StartsWith(".pp", StringComparison.OrdinalIgnoreCase))
            {
                return OfficeApplication.PowerPoint;
            }

            Debug.Assert(false, "Unrecognized extension passed");
            return OfficeApplication.Xml;
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

        public void Save(string? customFileName = null, bool preserveAttributes = true)
        {
            if (string.IsNullOrEmpty(customFileName))
            {
                customFileName = this.Name;
            }

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

        public void RemoveCustomPart(XmlPart partType)
        {
            if (this.Parts == null)
            {
                return;
            }

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

        public void SaveCustomPart(XmlPart partType, string text)
        {
            this.SaveCustomPart(partType, text, false /*isCreatingNewPart*/);
        }

        public void SaveCustomPart(XmlPart partType, string text, bool isCreatingNewPart)
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
            targetPart?.Save(text);
            this.IsDirty = true;
        }

        public OfficePart CreateCustomPart(XmlPart partType)
        {
            string relativePath;
            string relType;

            switch (partType)
            {
                case XmlPart.RibbonX12:
                    relativePath = "/customUI/customUI.xml";
                    relType = CustomUiPartRelType;
                    break;
                case XmlPart.RibbonX14:
                    relativePath = "/customUI/customUI14.xml";
                    relType = CustomUi14PartRelType;
                    break;
                case XmlPart.Qat12:
                    relativePath = "/customUI/qat.xml";
                    relType = QatPartRelType;
                    break;
                default:
                    throw new ArgumentException($"Unexpected {nameof(partType)}: {partType}");
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

            if (this.Parts == null)
            {
                this.Parts = new List<OfficePart>();
            }

            this.Parts.Add(part);
            this.IsDirty = true;

            return part;
        }

        public OfficePart? RetrieveCustomPart(XmlPart partType)
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
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (this.disposed)
            {
                return;
            }

            if (disposing)
            {
                this.Name = string.Empty;
                this.Parts?.Clear();

                this.UnderlyingPackage.Close();

                try
                {
                    File.Delete(this.tempFileName);
                }
                catch (IOException ex)
                {
                    Debug.Fail(ex.Message);
                }
            }

            this.disposed = true;
        }

        private void Init()
        {
            this.UnderlyingPackage = Package.Open(this.tempFileName, FileMode.Open, FileAccess.ReadWrite);

            if (this.UnderlyingPackage == null)
            {
                throw new InvalidOperationException("Failed to get package");
            }

            this.Parts = new List<OfficePart>();

            var collection = new[]
            {
                (CustomUi14PartRelType, XmlPart.RibbonX14),
                (CustomUiPartRelType, XmlPart.RibbonX12),
                (QatPartRelType, XmlPart.Qat12),
            };
            foreach (var (relType, partType) in collection)
            {
                var relationship = this.UnderlyingPackage.GetRelationshipsByType(relType).FirstOrDefault();
                if (relationship == null)
                {
                    continue;
                }

                var uri = PackUriHelper.ResolvePartUri(relationship.SourceUri, relationship.TargetUri);
                if (this.UnderlyingPackage.PartExists(uri))
                {
                    this.Parts.Add(new OfficePart(this.UnderlyingPackage.GetPart(uri), partType, relationship.Id));
                }
            }
        }
    }
}
