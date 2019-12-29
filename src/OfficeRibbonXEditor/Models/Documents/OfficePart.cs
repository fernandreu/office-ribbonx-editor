using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Packaging;
using System.Windows.Media.Imaging;
using System.Xml;
using OfficeRibbonXEditor.Resources;

namespace OfficeRibbonXEditor.Models.Documents
{
    public class OfficePart
    {
        private string id;

        public OfficePart(PackagePart part, XmlPart partType, string relationshipId)
        {
            this.Part = part;
            this.PartType = partType;
            this.id = relationshipId;
            this.Name = Path.GetFileName(this.Part.Uri.ToString());
        }

        public PackagePart Part { get; private set; }

        public XmlPart PartType { get; }

        public string Name { get; }

        public string ReadContent()
        {
            var rd = new StreamReader(this.Part.GetStream(FileMode.Open, FileAccess.Read));
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

            var tw = new StreamWriter(this.Part.GetStream(FileMode.Create, FileAccess.Write));

            tw.Write(text);
            tw.Flush();
            tw.Close();
        }

        public Dictionary<string, BitmapImage> GetImages()
        {
            var imageCollection = new Dictionary<string, BitmapImage>();

            foreach (var relationship in this.Part.GetRelationshipsByType(OfficeDocument.ImagePartRelType))
            {
                var customImageUri = PackUriHelper.ResolvePartUri(relationship.SourceUri, relationship.TargetUri);
                if (!this.Part.Package.PartExists(customImageUri))
                {
                    continue;
                }

                var imagePart = this.Part.Package.GetPart(customImageUri);

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

        public string AddImage(string filePath, string imageId, Func<string, string, bool> alreadyExistingAction = null)
        {
            if (this.PartType != XmlPart.RibbonX12 && this.PartType != XmlPart.RibbonX14)
            {
                throw new NotSupportedException($"The part type must be either RibbonX12 or RibbonX14, not {this.PartType}");
            }

            if (filePath == null)
            {
                throw new ArgumentNullException(nameof(filePath));
            }

            if (filePath.Length == 0)
            {
                throw new ArgumentException("File path cannot be empty");
            }

            var fileName = Path.GetFileNameWithoutExtension(filePath);
            if (imageId == null)
            {
                imageId = XmlConvert.EncodeName(fileName);
            }

            if (imageId == null)
            {
                throw new ArgumentNullException(nameof(imageId));
            }

            if (imageId.Length == 0)
            {
                throw new ArgumentException(StringsResource.idsNonEmptyId);
            }

            return this.AddImageHelper(filePath, imageId, alreadyExistingAction);
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

            if (!this.Part.RelationshipExists(imageId))
            {
                return;
            }

            var imageRel = this.Part.GetRelationship(imageId);

            var imageUri = PackUriHelper.ResolvePartUri(imageRel.SourceUri, imageRel.TargetUri);
            if (this.Part.Package.PartExists(imageUri))
            {
                this.Part.Package.DeletePart(imageUri);
            }

            this.Part.DeleteRelationship(imageId);
        }

        public void Remove()
        {
            // Remove all image parts first
            foreach (PackageRelationship relationship in this.Part.GetRelationships())
            {
                var relUri = PackUriHelper.ResolvePartUri(relationship.SourceUri, relationship.TargetUri);
                if (this.Part.Package.PartExists(relUri))
                {
                    this.Part.Package.DeletePart(relUri);
                }
            }

            this.Part.Package.DeleteRelationship(this.id);
            this.Part.Package.DeletePart(this.Part.Uri);

            this.Part = null;
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

            if (!this.Part.RelationshipExists(source))
            {
                return;
            }

            if (this.Part.RelationshipExists(target))
            {
                throw new Exception(string.Format(StringsResource.idsDuplicateId, target));
            }

            var imageRel = this.Part.GetRelationship(source);

            this.Part.CreateRelationship(imageRel.TargetUri, imageRel.TargetMode, imageRel.RelationshipType, target);
            this.Part.DeleteRelationship(source);
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

        private string AddImageHelper(string fileName, string imageId, Func<string, string, bool> alreadyExistingAction = null)
        {
            if (fileName == null)
            {
                throw new ArgumentNullException(nameof(fileName));
            }

            Debug.Assert(File.Exists(fileName), fileName + " does not exist.");
            if (!File.Exists(fileName))
            {
                return null;
            }

            var br = new BinaryReader(File.Open(fileName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite));

            var imageUri = new Uri("images/" + Path.GetFileName(fileName), UriKind.Relative);
            var fileIndex = 0;
            while (true)
            {
                if (this.Part.Package.PartExists(PackUriHelper.ResolvePartUri(this.Part.Uri, imageUri)))
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

            var originalId = imageId;
            if (imageId != null)
            {
                int idIndex = 0;
                string testId = imageId;
                while (true)
                {
                    if (this.Part.RelationshipExists(testId))
                    {
                        Debug.Write(testId + " already exists.");
                        testId = imageId + (idIndex++);
                        continue;
                    }

                    imageId = testId;
                    break;
                }
            }

            if (imageId != originalId && !(alreadyExistingAction?.Invoke(originalId, imageId) ?? true))
            {
                return null;
            }

            var imageRel = this.Part.CreateRelationship(imageUri, TargetMode.Internal, OfficeDocument.ImagePartRelType, imageId);

            var imagePart = this.Part.Package.CreatePart(
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