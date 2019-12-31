// --------------------------------------------------------------------------------------------------------------------
// <copyright file="RecentFileList.cs" company="CodeProject">
//   This source code is licensed under The Code Project Open License (CPOL)
// </copyright>
// <summary>
//   Defines the RecentFileList type. This is originally from:
//      https://www.codeproject.com/Articles/23731/RecentFileList-a-WPF-MRU
//   Main modifications made:
//   - Code has been formatted to be compatible with FxCop / nullable reference types
//   - New IPersist interface added for storing the files in user settings
// </summary>
// --------------------------------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Xml;

using Microsoft.Win32;
using OfficeRibbonXEditor.Interfaces;
using OfficeRibbonXEditor.Models.Events;

namespace OfficeRibbonXEditor.Models
{
    public class RecentFileList : Separator
    {
        public static readonly DependencyProperty ClickCommandProperty = 
            DependencyProperty.Register(
                nameof(ClickCommand),
                typeof(ICommand),
                typeof(RecentFileList));

        private Separator? separator;

        private List<RecentFile>? recentFiles;

        public RecentFileList()
        {
            this.MaxNumberOfFiles = 9;
            this.MaxPathLength = 50;
            this.MenuItemFormatOneToNine = "_{0}:  {2}";
            this.MenuItemFormatTenPlus = "{0}:  {2}";

            this.Loaded += (s, e) => this.HookFileMenu();
        }
        
        public delegate string GetMenuItemTextDelegate(int index, string filepath);

        public event EventHandler<MenuClickEventArgs>? MenuClick;

        public ICommand ClickCommand
        {
            get => (ICommand)this.GetValue(ClickCommandProperty);
            set => this.SetValue(ClickCommandProperty, value);
        }

        /// <summary>
        /// Gets or sets the formatting string used for files from 1 to 9 in the list
        /// Used in: String.Format(MenuItemFormat, index, filepath, displayPath);
        /// Default = "_{0}:  {2}"
        /// </summary>
        public string MenuItemFormatOneToNine { get; set; }

        /// <summary>
        /// Gets or sets the formatting string used for files 10 onwards in the list
        /// Used in: String.Format(MenuItemFormat, index, filepath, displayPath);
        /// Default = "{0}:  {2}"
        /// </summary>
        public string MenuItemFormatTenPlus { get; set; }

        public IPersist Persister { get; set; } = new XmlPersister();

        public int MaxNumberOfFiles { get; set; }

        public int MaxPathLength { get; set; }

        public MenuItem? FileMenu { get; private set; }
        
        public GetMenuItemTextDelegate? GetMenuItemTextHandler { get; set; }
        
        public List<string> RecentFiles => this.Persister.RecentFiles(this.MaxNumberOfFiles);
        
        /// <summary>
        /// Shortens a pathname for display purposes.
        /// This method is taken from Joe Woodbury's article at: http://www.codeproject.com/KB/cs/mrutoolstripmenu.aspx
        /// </summary>
        /// <param name="pathname">The pathname to shorten.</param>
        /// <param name="maxLength">The maximum number of characters to be displayed.</param>
        /// <remarks>Shortens a pathname by either removing consecutive components of a path
        /// and/or by removing characters from the end of the filename and replacing
        /// then with three elipses (...)
        /// <para>In all cases, the root of the passed path will be preserved in it's entirety.</para>
        /// <para>If a UNC path is used or the pathname and maxLength are particularly short,
        /// the resulting path may be longer than maxLength.</para>
        /// <para>This method expects fully resolved path names to be passed to it.
        /// (Use Path.GetFullPath() to obtain this.)</para>
        /// </remarks>
        /// <returns>The shortened path</returns>
        public static string ShortenPathname(string pathname, int maxLength)
        {
            if (pathname.Length <= maxLength)
            {
                return pathname;
            }

            var root = Path.GetPathRoot(pathname) ?? string.Empty;
            if (root.Length > 3)
            {
                root += Path.DirectorySeparatorChar;
            }

            var elements = pathname.Substring(root.Length).Split(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);

            var filenameIndex = elements.GetLength(0) - 1;

            if (elements.GetLength(0) == 1) 
            {
                // pathname is just a root and filename
                if (elements[0].Length > 5)
                {
                    // Long enough to shorten. If path is a UNC path, root may be rather long
                    if (root.Length + 6 >= maxLength)
                    {
                        return root + elements[0].Substring(0, 3) + "...";
                    }
                    else
                    {
                        return pathname.Substring(0, maxLength - 3) + "...";
                    }
                }
            }
            else if (root.Length + 4 + elements[filenameIndex].Length > maxLength)
            {
                // pathname is just a root and filename
                root += "...\\";

                var len = elements[filenameIndex].Length;
                if (len < 6)
                {
                    return root + elements[filenameIndex];
                }

                if ((root.Length + 6) >= maxLength)
                {
                    len = 3;
                }
                else
                {
                    len = maxLength - root.Length - 3;
                }

                return root + elements[filenameIndex].Substring(0, len) + "...";
            }
            else if (elements.GetLength(0) == 2)
            {
                return root + "...\\" + elements[1];
            }
            else
            {
                var len = 0;
                var begin = 0;

                for (var i = 0; i < filenameIndex; i++)
                {
                    if (elements[i].Length > len)
                    {
                        begin = i;
                        len = elements[i].Length;
                    }
                }

                int totalLength = pathname.Length - len + 3;
                int end = begin + 1;

                while (totalLength > maxLength)
                {
                    if (begin > 0)
                    {
                        totalLength -= elements[--begin].Length - 1;
                    }

                    if (totalLength <= maxLength)
                    {
                        break;
                    }

                    if (end < filenameIndex)
                    {
                        totalLength -= elements[++end].Length - 1;
                    }

                    if (begin == 0 && end == filenameIndex)
                    {
                        break;
                    }
                }

                // assemble final string
                for (int i = 0; i < begin; i++)
                {
                    root += elements[i] + '\\';
                }

                root += "...\\";

                for (int i = end; i < filenameIndex; i++)
                {
                    root += elements[i] + '\\';
                }

                return root + elements[filenameIndex];
            }

            return pathname;
        }

        public void UseRegistryPersister()
        {
            this.Persister = new RegistryPersister();
        }

        public void UseRegistryPersister(string key)
        {
            this.Persister = new RegistryPersister(key);
        }

        public void UseXmlPersister()
        {
            this.Persister = new XmlPersister();
        }

        public void UseXmlPersister(string filepath)
        {
            this.Persister = new XmlPersister(filepath);
        }

        public void UseXmlPersister(Stream stream)
        {
            this.Persister = new XmlPersister(stream);
        }

        public void RemoveFile(string filepath)
        {
            this.Persister.RemoveFile(filepath, this.MaxNumberOfFiles);
        }

        public void InsertFile(string filepath)
        {
            this.Persister.InsertFile(filepath, this.MaxNumberOfFiles);
        }

        protected virtual void OnMenuClick(MenuItem menuItem)
        {
            var filepath = this.GetFilepath(menuItem);

            if (string.IsNullOrEmpty(filepath))
            {
                return;
            }

            this.MenuClick?.Invoke(menuItem, new MenuClickEventArgs(filepath));
            if (this.ClickCommand?.CanExecute(filepath) ?? false)
            {
                this.ClickCommand?.Execute(filepath);
            }
        }
        
        // ReSharper disable StyleCop.SA1126
        private void HookFileMenu()
        {
            if (!(this.Parent is MenuItem parent))
            {
                throw new ApplicationException("Parent must be a MenuItem");
            }
            
            if (ReferenceEquals(this.FileMenu, parent))
            {
                return;
            }

            if (this.FileMenu != null)
            {
                this.FileMenu.SubmenuOpened -= this.FileMenuSubmenuOpened;
            }

            this.FileMenu = parent;
            this.FileMenu.SubmenuOpened += this.FileMenuSubmenuOpened;
        }

        private void FileMenuSubmenuOpened(object sender, RoutedEventArgs e)
        {
            this.SetMenuItems();
        }

        private void SetMenuItems()
        {
            this.RemoveMenuItems();

            this.LoadRecentFiles();

            this.InsertMenuItems();
        }

        private void RemoveMenuItems()
        {
            if (this.separator != null)
            {
                this.FileMenu?.Items.Remove(this.separator);
            }

            if (this.recentFiles != null)
            {
                foreach (var r in this.recentFiles)
                {
                    if (r.MenuItem != null)
                    {
                        this.FileMenu?.Items.Remove(r.MenuItem);
                    }
                }
            }

            this.separator = null;
            this.recentFiles = null;
        }

        private void InsertMenuItems()
        {
            if (this.FileMenu == null)
            {
                return;
            }

            if (this.recentFiles == null)
            {
                return;
            }

            if (this.recentFiles.Count == 0)
            {
                return;
            }

            var index = this.FileMenu.Items.IndexOf(this);

            foreach (var r in this.recentFiles)
            {
                var header = this.GetMenuItemText(r.Number + 1, r.Filepath, r.DisplayPath);

                r.MenuItem = new MenuItem { Header = header };
                r.MenuItem.Click += this.MenuItemClick;

                this.FileMenu.Items.Insert(++index, r.MenuItem);
            }

            this.separator = new Separator();
            this.FileMenu.Items.Insert(++index, this.separator);
        }

        private string GetMenuItemText(int index, string filepath, string displayPath)
        {
            var delegateGetMenuItemText = this.GetMenuItemTextHandler;
            if (delegateGetMenuItemText != null)
            {
                return delegateGetMenuItemText(index, filepath);
            }

            var format = index < 10 ? this.MenuItemFormatOneToNine : this.MenuItemFormatTenPlus;

            var shortPath = ShortenPathname(displayPath, this.MaxPathLength);

            return string.Format(CultureInfo.CurrentCulture, format, index, filepath, shortPath);
        }

        private void LoadRecentFiles()
        {
            this.recentFiles = this.LoadRecentFilesCore();
        }

        private List<RecentFile> LoadRecentFilesCore()
        {
            var list = this.RecentFiles;

            var files = new List<RecentFile>(list.Count);

            var i = 0;

            foreach (var filepath in list)
            {
                files.Add(new RecentFile(i++, filepath));
            }

            return files;
        }

        private void MenuItemClick(object sender, EventArgs e)
        {
            if (sender is MenuItem menuItem)
            {
                this.OnMenuClick(menuItem);
            }
        }

        private string GetFilepath(MenuItem menuItem)
        {
            if (this.recentFiles == null)
            {
                return string.Empty;
            }

            foreach (var r in this.recentFiles)
            {
                if (ReferenceEquals(r.MenuItem, menuItem))
                {
                    return r.Filepath;
                }
            }

            return string.Empty;
        }
        
        private static class ApplicationAttributes
        {
            [SuppressMessage("Design", "CA1031:Do not catch general exception types", Justification = "Need to investigate which specific exception could be thrown first")]
            static ApplicationAttributes()
            {
                Title = string.Empty;
                CompanyName = string.Empty;
                Copyright = string.Empty;
                ProductName = string.Empty;
                Version = string.Empty;

                try
                {
                    var assembly = Assembly.GetEntryAssembly();

                    if (assembly == null)
                    {
                        return;
                    }
                    
                    var attributes = assembly.GetCustomAttributes(false);

                    foreach (var attribute in attributes)
                    {
                        if (attribute is AssemblyTitleAttribute titleAttribute)
                        {
                            Title = titleAttribute.Title;
                        }

                        if (attribute is AssemblyCompanyAttribute companyAttribute)
                        {
                            CompanyName = companyAttribute.Company;
                        }

                        if (attribute is AssemblyCopyrightAttribute copyrightAttribute)
                        {
                            Copyright = copyrightAttribute.Copyright;
                        }

                        if (attribute is AssemblyProductAttribute productAttribute)
                        {
                            ProductName = productAttribute.Product;
                        }
                    }

                    Version = assembly.GetName().Version.ToString();
                }
                catch (Exception)
                {
                    // If accessing one of the attributes fails, the remaining ones will simply be left as string.Empty
                }
            }
            
            // ReSharper disable once UnusedAutoPropertyAccessor.Local
            // ReSharper disable once MemberCanBePrivate.Local
            public static string Title { get; }

            public static string CompanyName { get; }

            // ReSharper disable once UnusedAutoPropertyAccessor.Local
            // ReSharper disable once MemberCanBePrivate.Local
            public static string Copyright { get; }

            public static string ProductName { get; }

            // ReSharper disable once UnusedAutoPropertyAccessor.Local
            // ReSharper disable once MemberCanBePrivate.Local
            public static string Version { get; }
        }

        private class RecentFile
        {
            public RecentFile(int number, string filepath)
            {
                this.Number = number;
                this.Filepath = filepath;
            }

            public int Number { get; }

            public string Filepath { get; }

            public MenuItem? MenuItem { get; set; }

            public string DisplayPath
            {
                get
                {
                    var directory = Path.GetDirectoryName(this.Filepath);
                    var fileName = Path.GetFileName(this.Filepath);
                    if (string.IsNullOrEmpty(directory) || string.IsNullOrEmpty(fileName))
                    {
                        // Filepath seems ill-formed for some reason, so don't try to shorten it
                        return this.Filepath;
                    }
                    
                    return Path.Combine(directory, fileName);
                }
            }
        }
        
        private class RegistryPersister : IPersist
        {
            public RegistryPersister()
            {
                this.RegistryKey =
                    "Software\\" +
                    ApplicationAttributes.CompanyName + "\\" +
                    ApplicationAttributes.ProductName + "\\" +
                    "RecentFileList";
            }

            public RegistryPersister(string key)
            {
                this.RegistryKey = key;
            }

            private string RegistryKey { get; }

            public List<string> RecentFiles(int max)
            {
                var k = Registry.CurrentUser.OpenSubKey(this.RegistryKey) ?? Registry.CurrentUser.CreateSubKey(this.RegistryKey);

                var list = new List<string>(max);

                for (var i = 0; i < max; i++)
                {
                    var filename = (string?)k?.GetValue(Key(i));

                    if (string.IsNullOrEmpty(filename))
                    {
                        break;
                    }

                    list.Add(filename!);
                }

                return list;
            }

            public void InsertFile(string filepath, int max)
            {
                var k = Registry.CurrentUser.OpenSubKey(this.RegistryKey, true) ?? Registry.CurrentUser.CreateSubKey(this.RegistryKey, true);

                this.RemoveFile(filepath, max);

                for (var i = max - 2; i >= 0; i--)
                {
                    var thisKey = Key(i);
                    var nextKey = Key(i + 1);

                    var thisValue = k.GetValue(thisKey);
                    if (thisValue == null)
                    {
                        continue;
                    }

                    k.SetValue(nextKey, thisValue);
                }

                k.SetValue(Key(0), filepath);
            }

            public void RemoveFile(string filepath, int max)
            {
                var k = Registry.CurrentUser.OpenSubKey(this.RegistryKey);
                if (k == null)
                {
                    return;
                }

                for (int i = 0; i < max; i++)
                {
                    string s = (string)k.GetValue(Key(i));
                    if (s != null && s.Equals(filepath, StringComparison.CurrentCultureIgnoreCase))
                    {
                        this.RemoveFile(i, max);
                        i--; // Repeat this loop iteration again
                    }
                }
            }

            private static string Key(int i)
            {
                return i.ToString("00", CultureInfo.InvariantCulture);
            }

            private void RemoveFile(int index, int max)
            {
                var k = Registry.CurrentUser.OpenSubKey(this.RegistryKey, true);
                if (k == null)
                {
                    return;
                }

                k.DeleteValue(Key(index), false);

                for (var i = index; i < max - 1; i++)
                {
                    var thisKey = Key(i);
                    var nextKey = Key(i + 1);

                    var nextValue = k.GetValue(nextKey);
                    if (nextValue == null)
                    {
                        break;
                    }

                    k.SetValue(thisKey, nextValue);
                    k.DeleteValue(nextKey);
                }
            }
        }
        
        private class XmlPersister : IPersist
        {
            public XmlPersister()
            {
                this.Filepath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), ApplicationAttributes.CompanyName + "\\" + ApplicationAttributes.ProductName + "\\" + "RecentFileList.xml");
            }

            public XmlPersister(string filepath)
            {
                this.Filepath = filepath;
            }

            public XmlPersister(Stream stream)
            {
                this.Stream = stream;
            }

            private string? Filepath { get; }

            private Stream? Stream { get; }

            public List<string> RecentFiles(int max)
            {
                return this.Load(max);
            }

            public void InsertFile(string filepath, int max)
            {
                this.Update(filepath, true, max);
            }

            public void RemoveFile(string filepath, int max)
            {
                this.Update(filepath, false, max);
            }

            private void Update(string filepath, bool insert, int max)
            {
                var old = this.Load(max);

                var list = new List<string>(old.Count + 1);

                if (insert)
                {
                    list.Add(filepath);
                }

                CopyExcluding(old, filepath, list, max);

                this.Save(list, max);
            }

            private static void CopyExcluding(List<string> source, string exclude, List<string> target, int max)
            {
                foreach (var s in source)
                {
                    if (string.IsNullOrEmpty(s) || s.Equals(exclude, StringComparison.OrdinalIgnoreCase))
                    {
                        continue;
                    }
                    
                    if (target.Count >= max)
                    {
                        break;
                    }

                    target.Add(s);
                }
            }

            private SmartStream OpenStream(FileMode mode)
            {
                if (!string.IsNullOrEmpty(this.Filepath))
                {
                    return new SmartStream(this.Filepath!, mode);
                }
                else
                {
                    return new SmartStream(this.Stream!);
                }
            }

            private List<string> Load(int max)
            {
                List<string> list = new List<string>(max);

                using (var ms = new MemoryStream())
                {
                    using (var ss = this.OpenStream(FileMode.OpenOrCreate))
                    {
                        if (ss.Stream.Length == 0)
                        {
                            return list;
                        }

                        ss.Stream.Position = 0;

                        var buffer = new byte[1 << 20];
                        while (true)
                        {
                            var bytes = ss.Stream.Read(buffer, 0, buffer.Length);
                            if (bytes == 0)
                            {
                                break;
                            }

                            ms.Write(buffer, 0, bytes);
                        }

                        ms.Position = 0;
                    }

                    using (var x = XmlReader.Create(ms, new XmlReaderSettings { XmlResolver = null }))
                    {
                        while (x.Read())
                        {
                            switch (x.NodeType)
                            {
                                case XmlNodeType.XmlDeclaration:
                                case XmlNodeType.Whitespace:
                                    break;

                                case XmlNodeType.Element:
                                    switch (x.Name)
                                    {
                                        case "RecentFiles": break;

                                        case "RecentFile":
                                            if (list.Count < max)
                                            {
                                                list.Add(x.GetAttribute(0));
                                            }

                                            break;

                                        default:
                                            Debug.Assert(false, "Missing an XmlNodeType.Element name");
                                            break;
                                    }

                                    break;

                                case XmlNodeType.EndElement:
                                    switch (x.Name)
                                    {
                                        case "RecentFiles": return list;
                                        default:
                                            Debug.Assert(false, "Missing an XmlNodeType.EndElement name");
                                            break;
                                    }

                                    break;

                                default:
                                    Debug.Assert(false, "Missing an XmlNodeType");
                                    break;
                            }
                        }
                    }
                }

                return list;
            }

            private void Save(IEnumerable<string> list, int max)
            {
                _ = max;
                using (var ms = new MemoryStream())
                using (var x = new XmlTextWriter(ms, Encoding.UTF8))
                {
                    x.Formatting = Formatting.Indented;

                    x.WriteStartDocument();

                    x.WriteStartElement("RecentFiles");

                    foreach (string filepath in list)
                    {
                        x.WriteStartElement("RecentFile");
                        x.WriteAttributeString("Filepath", filepath);
                        x.WriteEndElement();
                    }

                    x.WriteEndElement();

                    x.WriteEndDocument();

                    x.Flush();

                    using (var ss = this.OpenStream(FileMode.Create))
                    {
                        ss.Stream.SetLength(0);

                        ms.Position = 0;

                        byte[] buffer = new byte[1 << 20];
                        while (true)
                        {
                            var bytes = ms.Read(buffer, 0, buffer.Length);
                            if (bytes == 0)
                            {
                                break;
                            }

                            ss.Stream.Write(buffer, 0, bytes);
                        }
                    }
                }
            }

            private class SmartStream : IDisposable
            {
                private readonly bool isStreamOwned;

                public SmartStream(string filepath, FileMode mode)
                {
                    this.isStreamOwned = true;

                    var directory = Path.GetDirectoryName(filepath);
                    if (directory != null)
                    {
                        Directory.CreateDirectory(directory);
                    }

                    this.Stream = File.Open(filepath, mode);
                }

                public SmartStream(Stream stream)
                {
                    this.isStreamOwned = false;
                    this.Stream = stream;
                }

                public Stream Stream { get; private set; }

                public static implicit operator Stream(SmartStream me)
                {
                    return me.Stream;
                }

                public void Dispose()
                {
                    if (this.isStreamOwned)
                    {
                        this.Stream?.Dispose();
                    }
                }
            }
        }
    }
}
