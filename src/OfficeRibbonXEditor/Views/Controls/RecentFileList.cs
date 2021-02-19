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
using OfficeRibbonXEditor.Events;
using OfficeRibbonXEditor.Extensions;
using OfficeRibbonXEditor.Helpers;
using OfficeRibbonXEditor.Interfaces;

namespace OfficeRibbonXEditor.Views.Controls
{
    public class RecentFileList : Separator
    {
        public static readonly DependencyProperty ClickCommandProperty = 
            DependencyProperty.Register(
                nameof(ClickCommand),
                typeof(ICommand),
                typeof(RecentFileList));

        private Separator? _separator;

        private List<RecentFile>? _recentFiles;

        public RecentFileList()
        {
            MaxNumberOfFiles = 9;
            MaxPathLength = 50;
            MenuItemFormatOneToNine = "_{0}:  {2}";
            MenuItemFormatTenPlus = "{0}:  {2}";

            Loaded += (s, e) => HookFileMenu();
        }
        
        public delegate string GetMenuItemTextDelegate(int index, string filepath);

        public event EventHandler<MenuClickEventArgs>? MenuClick;

        public ICommand ClickCommand
        {
            get => (ICommand)GetValue(ClickCommandProperty);
            set => SetValue(ClickCommandProperty, value);
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
        
        public List<string> RecentFiles => Persister.RecentFiles(MaxNumberOfFiles);
        
        public void UseRegistryPersister()
        {
            Persister = new RegistryPersister();
        }

        public void UseRegistryPersister(string key)
        {
            Persister = new RegistryPersister(key);
        }

        public void UseXmlPersister()
        {
            Persister = new XmlPersister();
        }

        public void UseXmlPersister(string filepath)
        {
            Persister = new XmlPersister(filepath);
        }

        public void UseXmlPersister(Stream stream)
        {
            Persister = new XmlPersister(stream);
        }

        public void RemoveFile(string filepath)
        {
            Persister.RemoveFile(filepath, MaxNumberOfFiles);
        }

        public void InsertFile(string filepath)
        {
            Persister.InsertFile(filepath, MaxNumberOfFiles);
        }

        protected virtual void OnMenuClick(MenuItem menuItem)
        {
            var filepath = GetFilepath(menuItem);

            if (string.IsNullOrEmpty(filepath))
            {
                return;
            }

            MenuClick?.Invoke(menuItem, new MenuClickEventArgs(filepath));
            if (ClickCommand?.CanExecute(filepath) ?? false)
            {
                ClickCommand?.Execute(filepath);
            }
        }
        
        // ReSharper disable StyleCop.SA1126
        private void HookFileMenu()
        {
            if (!(Parent is MenuItem parent))
            {
                throw new InvalidOperationException("Parent must be a MenuItem");
            }
            
            if (ReferenceEquals(FileMenu, parent))
            {
                return;
            }

            if (FileMenu != null)
            {
                FileMenu.SubmenuOpened -= FileMenuSubmenuOpened;
            }

            FileMenu = parent;
            FileMenu.SubmenuOpened += FileMenuSubmenuOpened;
        }

        private void FileMenuSubmenuOpened(object sender, RoutedEventArgs e)
        {
            SetMenuItems();
        }

        private void SetMenuItems()
        {
            RemoveMenuItems();

            LoadRecentFiles();

            InsertMenuItems();
        }

        private void RemoveMenuItems()
        {
            if (_separator != null)
            {
                FileMenu?.Items.Remove(_separator);
            }

            if (_recentFiles != null)
            {
                foreach (var r in _recentFiles)
                {
                    if (r.MenuItem != null)
                    {
                        FileMenu?.Items.Remove(r.MenuItem);
                    }
                }
            }

            _separator = null;
            _recentFiles = null;
        }

        private void InsertMenuItems()
        {
            if (FileMenu == null)
            {
                return;
            }

            if (_recentFiles == null)
            {
                return;
            }

            if (_recentFiles.Count == 0)
            {
                return;
            }

            var index = FileMenu.Items.IndexOf(this);

            foreach (var r in _recentFiles)
            {
                var header = GetMenuItemText(r.Number + 1, r.Filepath, r.DisplayPath);

                r.MenuItem = new MenuItem { Header = header };
                r.MenuItem.Click += MenuItemClick;

                FileMenu.Items.Insert(++index, r.MenuItem);
            }

            _separator = new Separator();
            FileMenu.Items.Insert(++index, _separator);
        }

        private string GetMenuItemText(int index, string filepath, string displayPath)
        {
            var delegateGetMenuItemText = GetMenuItemTextHandler;
            if (delegateGetMenuItemText != null)
            {
                return delegateGetMenuItemText(index, filepath);
            }

            var format = index < 10 ? MenuItemFormatOneToNine : MenuItemFormatTenPlus;

            var shortPath = StringUtils.ShortenPathName(displayPath, MaxPathLength);

            return string.Format(CultureInfo.CurrentCulture, format, index, filepath, shortPath);
        }

        private void LoadRecentFiles()
        {
            _recentFiles = LoadRecentFilesCore();
        }

        private List<RecentFile> LoadRecentFilesCore()
        {
            var list = RecentFiles;

            var files = new List<RecentFile>(list.Count);

            foreach (var (filepath, index) in list.Enumerated())
            {
                files.Add(new RecentFile(index, filepath));
            }

            return files;
        }

        private void MenuItemClick(object sender, EventArgs e)
        {
            if (sender is MenuItem menuItem)
            {
                OnMenuClick(menuItem);
            }
        }

        private string GetFilepath(MenuItem menuItem)
        {
            if (_recentFiles == null)
            {
                return string.Empty;
            }

            foreach (var r in _recentFiles)
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

                    Version = assembly?.GetName().Version?.ToString() ?? string.Empty;
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
                Number = number;
                Filepath = filepath;
            }

            public int Number { get; }

            public string Filepath { get; }

            public MenuItem? MenuItem { get; set; }

            public string DisplayPath
            {
                get
                {
                    var directory = Path.GetDirectoryName(Filepath);
                    var fileName = Path.GetFileName(Filepath);
                    if (string.IsNullOrEmpty(directory) || string.IsNullOrEmpty(fileName))
                    {
                        // Filepath seems ill-formed for some reason, so don't try to shorten it
                        return Filepath;
                    }
                    
                    return Path.Combine(directory, fileName);
                }
            }
        }
        
        private class RegistryPersister : IPersist
        {
            public RegistryPersister()
            {
                RegistryKey =
                    "Software\\" +
                    ApplicationAttributes.CompanyName + "\\" +
                    ApplicationAttributes.ProductName + "\\" +
                    "RecentFileList";
            }

            public RegistryPersister(string key)
            {
                RegistryKey = key;
            }

            private string RegistryKey { get; }

            public List<string> RecentFiles(int max)
            {
                var k = Registry.CurrentUser.OpenSubKey(RegistryKey) ?? Registry.CurrentUser.CreateSubKey(RegistryKey);

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
                var k = Registry.CurrentUser.OpenSubKey(RegistryKey, true) ?? Registry.CurrentUser.CreateSubKey(RegistryKey, true);

                RemoveFile(filepath, max);

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
                var k = Registry.CurrentUser.OpenSubKey(RegistryKey);
                if (k == null)
                {
                    return;
                }

                for (var i = 0; i < max; i++)
                {
                    var s = (string)k.GetValue(Key(i));
                    if (s == null || !s.Equals(filepath, StringComparison.CurrentCultureIgnoreCase))
                    {
                        continue;
                    }

                    RemoveFile(i, max);
                    i--; // Repeat this loop iteration again
                }
            }

            private static string Key(int i)
            {
                return i.ToString("00", CultureInfo.InvariantCulture);
            }

            private void RemoveFile(int index, int max)
            {
                var k = Registry.CurrentUser.OpenSubKey(RegistryKey, true);
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
                Filepath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), ApplicationAttributes.CompanyName + "\\" + ApplicationAttributes.ProductName + "\\" + "RecentFileList.xml");
            }

            public XmlPersister(string filepath)
            {
                Filepath = filepath;
            }

            public XmlPersister(Stream stream)
            {
                Stream = stream;
            }

            private string? Filepath { get; }

            private Stream? Stream { get; }

            public List<string> RecentFiles(int max)
            {
                return Load(max);
            }

            public void InsertFile(string filepath, int max)
            {
                Update(filepath, true, max);
            }

            public void RemoveFile(string filepath, int max)
            {
                Update(filepath, false, max);
            }

            private void Update(string filepath, bool insert, int max)
            {
                var old = Load(max);

                var list = new List<string>(old.Count + 1);

                if (insert)
                {
                    list.Add(filepath);
                }

                CopyExcluding(old, filepath, list, max);

                Save(list, max);
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
                if (!string.IsNullOrEmpty(Filepath))
                {
                    return new SmartStream(Filepath!, mode);
                }
                else
                {
                    return new SmartStream(Stream!);
                }
            }

            private List<string> Load(int max)
            {
                var list = new List<string>(max);

                using (var ms = new MemoryStream())
                {
                    using (var ss = OpenStream(FileMode.OpenOrCreate))
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

                    using (var ss = OpenStream(FileMode.Create))
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
                private readonly bool _isStreamOwned;

                public SmartStream(string filepath, FileMode mode)
                {
                    _isStreamOwned = true;

                    var directory = Path.GetDirectoryName(filepath);
                    if (directory != null)
                    {
                        Directory.CreateDirectory(directory);
                    }

                    Stream = File.Open(filepath, mode);
                }

                public SmartStream(Stream stream)
                {
                    _isStreamOwned = false;
                    Stream = stream;
                }

                public Stream Stream { get; private set; }

                public static implicit operator Stream(SmartStream me)
                {
                    return me.Stream;
                }

                public void Dispose()
                {
                    if (_isStreamOwned)
                    {
                        Stream?.Dispose();
                    }
                }
            }
        }
    }
}
