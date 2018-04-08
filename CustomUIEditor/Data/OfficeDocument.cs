using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.IO.Packaging;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using Image = System.Windows.Controls.Image;

namespace CustomUIEditor
{
	public class OfficeDocument : IDisposable
	{
		public const string CustomUIPartRelType = "http://schemas.microsoft.com/office/2006/relationships/ui/extensibility";
		public const string CustomUI14PartRelType = "http://schemas.microsoft.com/office/2007/relationships/ui/extensibility";
		public const string QATPartRelType = "http://schemas.microsoft.com/office/2006/relationships/ui/customization";
		public const string ImagePartRelType = "http://schemas.openxmlformats.org/officeDocument/2006/relationships/image";

		private Package _package;

		private List<OfficePart> _xmlParts;

		private bool _isDirty;
		private bool _isReadOnly;

		private string _fileName;
		private string _tempFileName;

		public OfficeDocument(string fileName)
		{
			if (fileName == null) throw new ArgumentNullException(nameof(fileName));
			if (fileName.Length == 0) throw new ArgumentException("File name cannot be empty.");

			_fileName = fileName;
			_isReadOnly = ((int)(File.GetAttributes(_fileName) & FileAttributes.ReadOnly) != 0);

			_tempFileName = Path.GetTempFileName();

			File.Copy(_fileName, _tempFileName, true /*overwrite*/);
			File.SetAttributes(_tempFileName, FileAttributes.Normal);

			this.Init();
			_isDirty = false;
		}

		~OfficeDocument()
		{
			this.Dispose(true /*isFramework*/);
		}

		private void Init()
		{
			if (_isReadOnly)
			{
				_package = Package.Open(_tempFileName, System.IO.FileMode.Open, FileAccess.Read);
			}
			else
			{
				_package = Package.Open(_tempFileName, System.IO.FileMode.Open, FileAccess.ReadWrite);
			}

			Debug.Assert(_package != null, "Failed to get packge.");
			if (_package == null) return;

			_xmlParts = new List<OfficePart>();

			foreach (PackageRelationship relationship in _package.GetRelationshipsByType(CustomUI14PartRelType))
			{
				Uri customUIUri = PackUriHelper.ResolvePartUri(relationship.SourceUri, relationship.TargetUri);
				if (_package.PartExists(customUIUri))
				{
					_xmlParts.Add(new OfficePart(_package.GetPart(customUIUri), XMLParts.RibbonX14, relationship.Id));
				}
				break;
			}

			foreach (PackageRelationship relationship in _package.GetRelationshipsByType(CustomUIPartRelType))
			{
				Uri customUIUri = PackUriHelper.ResolvePartUri(relationship.SourceUri, relationship.TargetUri);
				if (_package.PartExists(customUIUri))
				{
					_xmlParts.Add(new OfficePart(_package.GetPart(customUIUri), XMLParts.RibbonX12, relationship.Id));
				}
				break;
			}

			foreach (PackageRelationship relationship in _package.GetRelationshipsByType(QATPartRelType))
			{
				Uri qatUri = PackUriHelper.ResolvePartUri(relationship.SourceUri, relationship.TargetUri);
				if (_package.PartExists(qatUri))
				{
					_xmlParts.Add(new OfficePart(_package.GetPart(qatUri), XMLParts.QAT12, relationship.Id));
				}
				break;
			}
		}

		#region Basic Accessors
		public Package UnderlyingPackage => _package;

	    public List<OfficePart> Parts => _xmlParts;

	    public string Name => _fileName;

	    public bool IsDirty
		{
			get => _isDirty;
	        set => _isDirty = value;
	    }

		public bool ReadOnly => _isReadOnly;

	    public bool HasCustomUI
		{
			get
			{
				if (_xmlParts == null || _xmlParts.Count == 0) return false;

				foreach (var part in _xmlParts)
				{
				    if (part.PartType == XMLParts.RibbonX12 || part.PartType == XMLParts.RibbonX14)
				    {
				        return true;
				    }
				}

				return false;
			}
		}

	    public OfficeApplications FileType => MapFileType(Path.GetExtension(_fileName));
		#endregion

		public static OfficeApplications MapFileType(string extension)
		{
			extension = extension.ToLower();
			if (extension.StartsWith(".do"))
				return OfficeApplications.Word;
			if (extension.StartsWith(".xl"))
				return OfficeApplications.Excel;
			if (extension.StartsWith(".pp"))
				return OfficeApplications.PowerPoint;

			Debug.Assert(false);
			return OfficeApplications.XML;
		}

		public void Save()
		{
			Debug.Assert(_package != null, "Failed to get packge.");
			Debug.Assert(!_isReadOnly, "File is ReadOnly!");

			if (_package == null || _isReadOnly) return;
			if (!_isDirty) return;

			_package.Flush();
			_package.Close();

			try
			{
				File.Copy(_tempFileName, _fileName, true /*overwrite*/);
			}
			finally
			{
				Init();
			}

			_isDirty = false;
		}

		public void RemoveCustomPart(XMLParts partType)
		{
			Debug.Assert(!_isReadOnly, "File is ReadOnly!");
			if (_isReadOnly) return;

			for (int i = _xmlParts.Count - 1; i >= 0; i--)
			{
				if (_xmlParts[i].PartType != partType) continue;

				OfficePart part = _xmlParts[i];
				part.Remove();

				part = null;
				_xmlParts.RemoveAt(i);

				_package.Flush();
				_isDirty = true;
			}
		}

		public void SaveCustomPart(XMLParts partType, string text)
		{
			SaveCustomPart(partType, text, false /*isCreatingNewPart*/);
		}

		public void SaveCustomPart(XMLParts partType, string text, bool isCreatingNewPart)
		{
			Debug.Assert(!_isReadOnly, "File is ReadOnly!");
			if (_isReadOnly) return;

			OfficePart targetPart = RetrieveCustomPart(partType);

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

			Debug.Assert(targetPart != null);
			targetPart.Save(text);
			_isDirty = true;
		}

		public OfficePart CreateCustomPart(XMLParts partType)
		{
			string relativePath;
			string relType;

			switch (partType)
			{
				case XMLParts.RibbonX12:
					relativePath = "/customUI/customUI.xml";
					relType = CustomUIPartRelType;
					break;
				case XMLParts.RibbonX14:
					relativePath = "/customUI/customUI14.xml";
					relType = CustomUI14PartRelType;
					break;
				case XMLParts.QAT12:
					relativePath = "/customUI/qat.xml";
					relType = QATPartRelType;
					break;
				default:
					Debug.Assert(false, "Unknown type");
					return null;
			}

			Uri customUIUri = new Uri(relativePath, UriKind.Relative);
			PackageRelationship relationship = _package.CreateRelationship(customUIUri, TargetMode.Internal, relType);

			OfficePart part = null;
			if (!_package.PartExists(customUIUri))
			{
				part = new OfficePart(_package.CreatePart(customUIUri, "application/xml"), partType, relationship.Id);
			}
			else
			{
				part = new OfficePart(_package.GetPart(customUIUri), partType, relationship.Id);
			}
			Debug.Assert(part != null, "Fail to create custom part.");

			_xmlParts.Add(part);
			_isDirty = true;

			return part;
		}

		public OfficePart RetrieveCustomPart(XMLParts partType)
		{
			Debug.Assert(_xmlParts != null);
			if (_xmlParts == null || _xmlParts.Count == 0) return null;

			for (int i = 0; i < _xmlParts.Count; i++)
			{
				if (_xmlParts[i].PartType == partType)
				{
					return _xmlParts[i];
				}
			}
			return null;
		}

		#region IDisposable Members

		private bool _disposed;

		public void Dispose()
		{
			this.Dispose(false /*isFramework*/);
			GC.SuppressFinalize(this);
		}

		private void Dispose(bool isFramework)
		{
			if (_disposed) return;

			if (!isFramework)
			{
				_fileName = null;
				if (_xmlParts != null && _xmlParts.Count > 0)
				{
					for (int i = 0; i < _xmlParts.Count; i++)
					{
						_xmlParts[i] = null;
					}
				}

				if (_package != null)
				{
					try
					{
						_package.Close();
					}
					catch (ObjectDisposedException ex)
					{
						Debug.Assert(false, ex.Message);
						Debug.Fail(ex.Message);
					}
					_package = null;
				}

				if (_tempFileName != null)
				{
					try
					{
						File.Delete(_tempFileName);
					}
					catch (Exception ex)
					{
						Debug.Assert(false, ex.Message);
						Debug.Fail(ex.Message);
					}
					_tempFileName = null;
				}
			}
			_disposed = true;
		}

		#endregion
	}

	public class OfficePart
	{
		XMLParts _partType;
		PackagePart _part;
		string _id;
		string _name;

		public OfficePart(PackagePart part, XMLParts partType, string relationshipId)
		{
			_part = part;
			_partType = partType;
			_id = relationshipId;
			_name = System.IO.Path.GetFileName(_part.Uri.ToString());
		}

		public PackagePart Part => _part;

	    public XMLParts PartType => _partType;

	    public string Name => _name;

	    public string ReadContent()
		{
			TextReader rd = new StreamReader(_part.GetStream(System.IO.FileMode.Open, System.IO.FileAccess.Read));

			Debug.Assert(rd != null, "Fail to get TextReader.");
			if (rd == null) return null;

			string text = rd.ReadToEnd();
			rd.Close();
			return text;
		}

		public void Save(string text)
		{
			Debug.Assert(text != null);
			if (text == null) return;

			TextWriter tw = new StreamWriter(_part.GetStream(FileMode.Create, FileAccess.Write));

			Debug.Assert(tw != null, "Fail to get TextWriter.");
			if (tw == null) return;

			tw.Write(text);
			tw.Flush();
			tw.Close();
		}
        
        // TODO: Previous Windows Forms approach of returning the TreeNodes directly does not make sense in a ViewModel approach
        // TOOD: Leading understocre  removed for
        public Dictionary<string, BitmapImage> GetImages()
        {
            var imageCollection = new Dictionary<string, BitmapImage>();

            foreach (PackageRelationship relationship in _part.GetRelationshipsByType(OfficeDocument.ImagePartRelType))
            {
                Uri customImageUri = PackUriHelper.ResolvePartUri(relationship.SourceUri, relationship.TargetUri);
                if (!_part.Package.PartExists(customImageUri)) continue;

                PackagePart imagePart = _part.Package.GetPart(customImageUri);

                Stream imageStream = imagePart.GetStream(FileMode.Open, FileAccess.Read);

                var image = new BitmapImage();
                image.BeginInit();
                image.StreamSource = imageStream;
                image.CacheOption = BitmapCacheOption.OnLoad;
                image.EndInit();
                
                //var key = "_" + relationship.Id;

                imageCollection.Add(relationship.Id, image);
                
                imageStream.Close();
            }

            return imageCollection;
        }

        public string AddImage(string fileName, string id)
		{
			if (_partType != XMLParts.RibbonX12 && _partType != XMLParts.RibbonX14)
			{
				Debug.Assert(false);
				return null;
			}

			if (fileName == null) throw new ArgumentNullException(nameof(fileName));
			if (fileName.Length == 0) return null;

			if (id == null) throw new ArgumentNullException(nameof(id));
			if (id.Length == 0) throw new ArgumentException(StringsResource.idsNonEmptyId);

			if (_part.RelationshipExists(id))
			{
				id = "rId";
			}
			return AddImageHelper(fileName, id);
		}

		private string AddImageHelper(string fileName, string id)
		{
			if (fileName == null) throw new ArgumentNullException(nameof(fileName));

			Debug.Assert(File.Exists(fileName), fileName + "does not exist.");
			if (!File.Exists(fileName)) return null;

			BinaryReader br = new BinaryReader(File.Open(fileName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite));
			Debug.Assert(br != null, "Fail to create a BinaryReader from file.");
			if (br == null) return null;

			Uri imageUri = new Uri("images/" + Path.GetFileName(fileName), UriKind.Relative);
			int fileIndex = 0;
			while (true)
			{
				if (_part.Package.PartExists(PackUriHelper.ResolvePartUri(_part.Uri, imageUri)))
				{
					Debug.Write(imageUri.ToString() + " already exists.");
					imageUri = new Uri(
						"images/" +
						Path.GetFileNameWithoutExtension(fileName) +
						(fileIndex++).ToString() +
						Path.GetExtension(fileName),
						UriKind.Relative);
					continue;
				}
				break;
			}

			if (id != null)
			{
				int idIndex = 0;
				string testId = id;
				while (true)
				{
					if (_part.RelationshipExists(testId))
					{
						Debug.Write(testId + " already exists.");
						testId = id + (idIndex++);
						continue;
					}
					id = testId;
					break;
				}
			}

			PackageRelationship imageRel = _part.CreateRelationship(imageUri, TargetMode.Internal, OfficeDocument.ImagePartRelType, id);

			Debug.Assert(imageRel != null, "Fail to create image relationship.");
			if (imageRel == null) return null;

			PackagePart imagePart = _part.Package.CreatePart(
				PackUriHelper.ResolvePartUri(imageRel.SourceUri, imageRel.TargetUri),
				OfficePart.MapImageContentType(Path.GetExtension(fileName)));

			Debug.Assert(imagePart != null, "Fail to create image part.");
			if (imagePart == null) return null;

			BinaryWriter bw = new BinaryWriter(imagePart.GetStream(FileMode.Create, FileAccess.Write));
			Debug.Assert(bw != null, "Fail to create a BinaryWriter to write to part.");
			if (bw == null) return null;

			byte[] buffer = new byte[1024];
			int byteCount = 0;
			while ((byteCount = br.Read(buffer, 0, buffer.Length)) > 0)
			{
				bw.Write(buffer, 0, byteCount);
			}

			bw.Flush();
			bw.Close();
			br.Close();

			return imageRel.Id;
		}

		public void RemoveImage(string id)
		{
			if (id == null) throw new ArgumentNullException("id");
			if (id.Length == 0) return;

			if (!_part.RelationshipExists(id)) return;

			PackageRelationship imageRel = _part.GetRelationship(id);

			Uri imageUri = PackUriHelper.ResolvePartUri(imageRel.SourceUri, imageRel.TargetUri);
			if (_part.Package.PartExists(imageUri))
			{
				_part.Package.DeletePart(imageUri);
			}

			_part.DeleteRelationship(id);
		}

		public void Remove()
		{
			// Remove all image parts first
			foreach (PackageRelationship relationship in _part.GetRelationships())
			{
				Uri relUri = PackUriHelper.ResolvePartUri(relationship.SourceUri, relationship.TargetUri);
				if (_part.Package.PartExists(relUri))
				{
					_part.Package.DeletePart(relUri);
				}
			}

			_part.Package.DeleteRelationship(_id);
			_part.Package.DeletePart(_part.Uri);

			_part = null;
			_id = null;
		}

		public void ChangeImageId(string source, string target)
		{
			if (source == null) throw new ArgumentNullException("source");
			if (target == null) throw new ArgumentNullException("target");
			if (target.Length == 0) throw new ArgumentException(StringsResource.idsNonEmptyId);

			if (source == target)
			{
				return;
			}

			if (!_part.RelationshipExists(source)) return;
			if (_part.RelationshipExists(target))
			{
				throw new Exception(StringsResource.idsDuplicateId.Replace("|1", target));
			}

			PackageRelationship imageRel = _part.GetRelationship(source);

			_part.CreateRelationship(imageRel.TargetUri, imageRel.TargetMode, imageRel.RelationshipType, target);
			_part.DeleteRelationship(source);
		}

		private static string MapImageContentType(string extension)
		{
			if (extension == null) throw new ArgumentNullException(nameof(extension));
			if (extension.Length == 0) throw new ArgumentException("Extension cannot be empty.");

			var extLowerCase = extension.ToLower();

			switch (extLowerCase)
			{
				case "jpg":
					return "image/jpeg";
				default:
					return "image/" + extLowerCase;
			}
		}
	}

	public enum XMLParts
	{
		QAT12,
		RibbonX12,
		RibbonX14,
		LastEntry //Always Last
	}

	public enum OfficeApplications
	{
		Word,
		Excel,
		PowerPoint,
		XML,
	}
}
