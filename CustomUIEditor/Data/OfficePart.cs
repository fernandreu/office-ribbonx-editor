// --------------------------------------------------------------------------------------------------------------------
// <copyright file="OfficePart.cs" company="FA">
//   Fernando Andreu
// </copyright>
// <summary>
//   Defines the OfficePart type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace CustomUIEditor.Data
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.IO.Packaging;
    using System.Windows.Media.Imaging;

    public class OfficePart
    {
        private readonly XmlParts partType;
        private readonly string name;
        private PackagePart part;
        private string id;

        public OfficePart(PackagePart part, XmlParts partType, string relationshipId)
        {
            this.part = part;
            this.partType = partType;
            this.id = relationshipId;
            this.name = Path.GetFileName(this.part.Uri.ToString());
        }

        public PackagePart Part => this.part;

        public XmlParts PartType => this.partType;

        public string Name => this.name;

        public string ReadContent()
        {
            var rd = new StreamReader(this.part.GetStream(FileMode.Open, FileAccess.Read));
            var text = rd.ReadToEnd();
            rd.Close();
            return text;
        }
        
        public void Save(string text)
        {
            if (text == null)
            {
                Debug.Print("Trying to save a null string");
                return;
            }

            var tw = new StreamWriter(this.part.GetStream(FileMode.Create, FileAccess.Write));

            tw.Write(text);
            tw.Flush();
            tw.Close();
        }
        
        // TODO: Previous Windows Forms approach of returning the TreeNodes directly does not make sense in a ViewModel approach
        // TOOD: Leading understocre  removed for
        public Dictionary<string, BitmapImage> GetImages()
        {
            var imageCollection = new Dictionary<string, BitmapImage>();

            foreach (var relationship in this.part.GetRelationshipsByType(OfficeDocument.ImagePartRelType))
            {
                var customImageUri = PackUriHelper.ResolvePartUri(relationship.SourceUri, relationship.TargetUri);
                if (!this.part.Package.PartExists(customImageUri))
                {
                    continue;
                }

                var imagePart = this.part.Package.GetPart(customImageUri);

                var imageStream = imagePart.GetStream(FileMode.Open, FileAccess.Read);

                var image = new BitmapImage();
                image.BeginInit();
                image.StreamSource = imageStream;
                image.CacheOption = BitmapCacheOption.OnLoad;
                image.EndInit();

                imageCollection.Add(relationship.Id, image);
                
                imageStream.Close();
            }

            return imageCollection;
        }

        public string AddImage(string fileName, string imageId)
        {
            if (this.partType != XmlParts.RibbonX12 && this.partType != XmlParts.RibbonX14)
            {
                return null;
            }

            if (fileName == null)
            {
                throw new ArgumentNullException(nameof(fileName));
            }

            if (fileName.Length == 0)
            {
                return null;
            }

            if (imageId == null)
            {
                throw new ArgumentNullException(nameof(imageId));
            }

            if (imageId.Length == 0)
            {
                throw new ArgumentException(StringsResource.idsNonEmptyId);
            }

            if (this.part.RelationshipExists(imageId))
            {
                imageId = "rId";
            }

            return this.AddImageHelper(fileName, imageId);
        }
        
        public void RemoveImage(string imageId)
        {
            if (imageId == null)
            {
                throw new ArgumentNullException(nameof(imageId));
            }

            if (imageId.Length == 0)
            {
                return;
            }

            if (!this.part.RelationshipExists(imageId))
            {
                return;
            }

            var imageRel = this.part.GetRelationship(imageId);

            var imageUri = PackUriHelper.ResolvePartUri(imageRel.SourceUri, imageRel.TargetUri);
            if (this.part.Package.PartExists(imageUri))
            {
                this.part.Package.DeletePart(imageUri);
            }

            this.part.DeleteRelationship(imageId);
        }

        public void Remove()
        {
            // Remove all image parts first
            foreach (PackageRelationship relationship in this.part.GetRelationships())
            {
                Uri relUri = PackUriHelper.ResolvePartUri(relationship.SourceUri, relationship.TargetUri);
                if (this.part.Package.PartExists(relUri))
                {
                    this.part.Package.DeletePart(relUri);
                }
            }

            this.part.Package.DeleteRelationship(this.id);
            this.part.Package.DeletePart(this.part.Uri);

            this.part = null;
            this.id = null;
        }

        public void ChangeImageId(string source, string target)
        {
            if (source == null)
            {
                throw new ArgumentNullException(nameof(source));
            }

            if (target == null)
            {
                throw new ArgumentNullException(nameof(target));
            }

            if (target.Length == 0)
            {
                throw new ArgumentException(StringsResource.idsNonEmptyId);
            }

            if (source == target)
            {
                return;
            }

            if (!this.part.RelationshipExists(source))
            {
                return;
            }

            if (this.part.RelationshipExists(target))
            {
                throw new Exception(StringsResource.idsDuplicateId.Replace("|1", target));
            }

            var imageRel = this.part.GetRelationship(source);

            this.part.CreateRelationship(imageRel.TargetUri, imageRel.TargetMode, imageRel.RelationshipType, target);
            this.part.DeleteRelationship(source);
        }

        private static string MapImageContentType(string extension)
        {
            if (extension == null)
            {
                throw new ArgumentNullException(nameof(extension));
            }

            if (extension.Length == 0)
            {
                throw new ArgumentException("Extension cannot be empty.");
            }

            var extLowerCase = extension.ToLower();

            switch (extLowerCase)
            {
                case "jpg":
                    return "image/jpeg";
                default:
                    return "image/" + extLowerCase;
            }
        }

        private string AddImageHelper(string fileName, string imageId)
        {
            if (fileName == null)
            {
                throw new ArgumentNullException(nameof(fileName));
            }

            Debug.Assert(File.Exists(fileName), fileName + "does not exist.");
            if (!File.Exists(fileName))
            {
                return null;
            }

            var br = new BinaryReader(File.Open(fileName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite));

            var imageUri = new Uri("images/" + Path.GetFileName(fileName), UriKind.Relative);
            var fileIndex = 0;
            while (true)
            {
                if (this.part.Package.PartExists(PackUriHelper.ResolvePartUri(this.part.Uri, imageUri)))
                {
                    Debug.Write(imageUri + " already exists.");
                    imageUri = new Uri(
                        "images/" +
                        Path.GetFileNameWithoutExtension(fileName) +
                        (fileIndex++) +
                        Path.GetExtension(fileName),
                        UriKind.Relative);
                    continue;
                }

                break;
            }

            if (imageId != null)
            {
                int idIndex = 0;
                string testId = imageId;
                while (true)
                {
                    if (this.part.RelationshipExists(testId))
                    {
                        Debug.Write(testId + " already exists.");
                        testId = imageId + (idIndex++);
                        continue;
                    }

                    imageId = testId;
                    break;
                }
            }

            var imageRel = this.part.CreateRelationship(imageUri, TargetMode.Internal, OfficeDocument.ImagePartRelType, imageId);

            var imagePart = this.part.Package.CreatePart(
                PackUriHelper.ResolvePartUri(imageRel.SourceUri, imageRel.TargetUri),
                MapImageContentType(Path.GetExtension(fileName)));

            if (imagePart == null)
            {
                Debug.Print("Fail to create image part.");
                return null;
            }

            var bw = new BinaryWriter(imagePart.GetStream(FileMode.Create, FileAccess.Write));

            var buffer = new byte[1024];
            int byteCount;
            while ((byteCount = br.Read(buffer, 0, buffer.Length)) > 0)
            {
                bw.Write(buffer, 0, byteCount);
            }

            bw.Flush();
            bw.Close();
            br.Close();

            return imageRel.Id;
        }
    }
}