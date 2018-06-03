// --------------------------------------------------------------------------------------------------------------------
// <copyright file="RecentFileList.cs" company="CodeProject">
//   This source code is licensed under The Code Project Open License (CPOL)
// </copyright>
// <summary>
//   Defines the RecentFileList type. This is originary from:
//      https://www.codeproject.com/Articles/23731/RecentFileList-a-WPF-MRU
//   Modifications made:
//   - Code has been formatted to be compatible with StyleCop
//   - New IPersist added for storing the files in user settings
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace CustomUIEditor
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.Reflection;
    using System.Text;
    using System.Windows;
    using System.Windows.Controls;
    using System.Xml;

    using Microsoft.Win32;

    public class RecentFileList : Separator
    {
        public interface IPersist
        {
            List<string> RecentFiles(int max);

            void InsertFile(string filepath, int max);

            void RemoveFile(string filepath, int max);

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

        public IPersist Persister { get; set; }

        public int MaxNumberOfFiles { get; set; }

        public int MaxPathLength { get; set; }

        public MenuItem FileMenu { get; private set; }
        
        public GetMenuItemTextDelegate GetMenuItemTextHandler { get; set; }

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
        
        public delegate string GetMenuItemTextDelegate(int index, string filepath);

        public event EventHandler<MenuClickEventArgs> MenuClick;

        private Separator separator = null;

        private List<RecentFile> recentFiles = null;

        public RecentFileList()
        {
            this.Persister = new RegistryPersister();

            this.MaxNumberOfFiles = 9;
            this.MaxPathLength = 50;
            this.MenuItemFormatOneToNine = "_{0}:  {2}";
            this.MenuItemFormatTenPlus = "{0}:  {2}";

            this.Loaded += (s, e) => this.HookFileMenu();
        }
        
        private void HookFileMenu()
        {
            if (!(this.Parent is MenuItem parent))
            {
                throw new ApplicationException("Parent must be a MenuItem");
            }

            if (object.ReferenceEquals(this.FileMenu, parent))
            {
                return;
            }

            if (this.FileMenu != null)
            {
                this.FileMenu.SubmenuOpened -= _FileMenu_SubmenuOpened;
            }

            this.FileMenu = parent;
            this.FileMenu.SubmenuOpened += _FileMenu_SubmenuOpened;
        }

        public List<string> RecentFiles { get { return Persister.RecentFiles( MaxNumberOfFiles ); } }
        public void RemoveFile( string filepath ) { Persister.RemoveFile( filepath, MaxNumberOfFiles ); }
        public void InsertFile( string filepath ) { Persister.InsertFile( filepath, MaxNumberOfFiles ); }

        void _FileMenu_SubmenuOpened( object sender, RoutedEventArgs e )
        {
            SetMenuItems();
        }

        void SetMenuItems()
        {
            RemoveMenuItems();

            LoadRecentFiles();

            InsertMenuItems();
        }

        void RemoveMenuItems()
        {
            if ( this.separator != null ) FileMenu.Items.Remove( this.separator );

            if ( this.recentFiles != null )
                foreach ( RecentFile r in this.recentFiles )
                    if ( r.MenuItem != null )
                        FileMenu.Items.Remove( r.MenuItem );

            this.separator = null;
            this.recentFiles = null;
        }

        void InsertMenuItems()
        {
            if ( this.recentFiles == null ) return;
            if ( this.recentFiles.Count == 0 ) return;

            int iMenuItem = FileMenu.Items.IndexOf( this );
            foreach ( RecentFile r in this.recentFiles )
            {
                string header = GetMenuItemText( r.Number + 1, r.Filepath, r.DisplayPath );

                r.MenuItem = new MenuItem { Header = header };
                r.MenuItem.Click += MenuItem_Click;

                FileMenu.Items.Insert( ++iMenuItem, r.MenuItem );
            }

            this.separator = new Separator();
            FileMenu.Items.Insert( ++iMenuItem, this.separator );
        }

        string GetMenuItemText( int index, string filepath, string displaypath )
        {
            GetMenuItemTextDelegate delegateGetMenuItemText = GetMenuItemTextHandler;
            if ( delegateGetMenuItemText != null ) return delegateGetMenuItemText( index, filepath );

            string format = ( index < 10 ? MenuItemFormatOneToNine : MenuItemFormatTenPlus );

            string shortPath = ShortenPathname( displaypath, MaxPathLength );

            return String.Format( format, index, filepath, shortPath );
        }

        // This method is taken from Joe Woodbury's article at: http://www.codeproject.com/KB/cs/mrutoolstripmenu.aspx

        /// <summary>
        /// Shortens a pathname for display purposes.
        /// </summary>
        /// <param labelName="pathname">The pathname to shorten.</param>
        /// <param labelName="maxLength">The maximum number of characters to be displayed.</param>
        /// <remarks>Shortens a pathname by either removing consecutive components of a path
        /// and/or by removing characters from the end of the filename and replacing
        /// then with three elipses (...)
        /// <para>In all cases, the root of the passed path will be preserved in it's entirety.</para>
        /// <para>If a UNC path is used or the pathname and maxLength are particularly short,
        /// the resulting path may be longer than maxLength.</para>
        /// <para>This method expects fully resolved pathnames to be passed to it.
        /// (Use Path.GetFullPath() to obtain this.)</para>
        /// </remarks>
        /// <returns></returns>
        public static string ShortenPathname( string pathname, int maxLength )
        {
            if ( pathname.Length <= maxLength )
                return pathname;

            string root = Path.GetPathRoot( pathname );
            if ( root.Length > 3 )
                root += Path.DirectorySeparatorChar;

            string[] elements = pathname.Substring( root.Length ).Split( Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar );

            int filenameIndex = elements.GetLength( 0 ) - 1;

            if ( elements.GetLength( 0 ) == 1 ) // pathname is just a root and filename
            {
                if ( elements[ 0 ].Length > 5 ) // long enough to shorten
                {
                    // if path is a UNC path, root may be rather long
                    if ( root.Length + 6 >= maxLength )
                    {
                        return root + elements[ 0 ].Substring( 0, 3 ) + "...";
                    }
                    else
                    {
                        return pathname.Substring( 0, maxLength - 3 ) + "...";
                    }
                }
            }
            else if ( ( root.Length + 4 + elements[ filenameIndex ].Length ) > maxLength ) // pathname is just a root and filename
            {
                root += "...\\";

                int len = elements[ filenameIndex ].Length;
                if ( len < 6 )
                    return root + elements[ filenameIndex ];

                if ( ( root.Length + 6 ) >= maxLength )
                {
                    len = 3;
                }
                else
                {
                    len = maxLength - root.Length - 3;
                }
                return root + elements[ filenameIndex ].Substring( 0, len ) + "...";
            }
            else if ( elements.GetLength( 0 ) == 2 )
            {
                return root + "...\\" + elements[ 1 ];
            }
            else
            {
                int len = 0;
                int begin = 0;

                for ( int i = 0 ; i < filenameIndex ; i++ )
                {
                    if ( elements[ i ].Length > len )
                    {
                        begin = i;
                        len = elements[ i ].Length;
                    }
                }

                int totalLength = pathname.Length - len + 3;
                int end = begin + 1;

                while ( totalLength > maxLength )
                {
                    if ( begin > 0 )
                        totalLength -= elements[ --begin ].Length - 1;

                    if ( totalLength <= maxLength )
                        break;

                    if ( end < filenameIndex )
                        totalLength -= elements[ ++end ].Length - 1;

                    if ( begin == 0 && end == filenameIndex )
                        break;
                }

                // assemble final string

                for ( int i = 0 ; i < begin ; i++ )
                {
                    root += elements[ i ] + '\\';
                }

                root += "...\\";

                for ( int i = end ; i < filenameIndex ; i++ )
                {
                    root += elements[ i ] + '\\';
                }

                return root + elements[ filenameIndex ];
            }
            return pathname;
        }

        void LoadRecentFiles()
        {
            this.recentFiles = LoadRecentFilesCore();
        }

        List<RecentFile> LoadRecentFilesCore()
        {
            List<string> list = RecentFiles;

            List<RecentFile> files = new List<RecentFile>( list.Count );

            int i = 0;
            foreach ( string filepath in list )
                files.Add( new RecentFile( i++, filepath ) );

            return files;
        }

        private class RecentFile
        {
            public int Number = 0;
            public string Filepath = "";
            public MenuItem MenuItem = null;

            public string DisplayPath
            {
                get
                {
                    return Path.Combine(
                        Path.GetDirectoryName( Filepath ),
                        Path.GetFileNameWithoutExtension( Filepath ) );
                }
            }

            public RecentFile( int number, string filepath )
            {
                this.Number = number;
                this.Filepath = filepath;
            }
        }

        public class MenuClickEventArgs : EventArgs
        {
            public string Filepath { get; private set; }

            public MenuClickEventArgs( string filepath )
            {
                this.Filepath = filepath;
            }
        }

        void MenuItem_Click( object sender, EventArgs e )
        {
            MenuItem menuItem = sender as MenuItem;

            OnMenuClick( menuItem );
        }

        protected virtual void OnMenuClick( MenuItem menuItem )
        {
            string filepath = GetFilepath( menuItem );

            if ( String.IsNullOrEmpty( filepath ) ) return;

            EventHandler<MenuClickEventArgs> dMenuClick = MenuClick;
            if ( dMenuClick != null ) dMenuClick( menuItem, new MenuClickEventArgs( filepath ) );
        }

        string GetFilepath( MenuItem menuItem )
        {
            foreach ( RecentFile r in this.recentFiles )
                if ( r.MenuItem == menuItem )
                    return r.Filepath;

            return String.Empty;
        }

        //-----------------------------------------------------------------------------------------

        static class ApplicationAttributes
        {
            static readonly Assembly _Assembly = null;

            static readonly AssemblyTitleAttribute _Title = null;
            static readonly AssemblyCompanyAttribute _Company = null;
            static readonly AssemblyCopyrightAttribute _Copyright = null;
            static readonly AssemblyProductAttribute _Product = null;

            public static string Title { get; private set; }
            public static string CompanyName { get; private set; }
            public static string Copyright { get; private set; }
            public static string ProductName { get; private set; }

            static Version _Version = null;
            public static string Version { get; private set; }

            static ApplicationAttributes()
            {
                try
                {
                    Title = String.Empty;
                    CompanyName = String.Empty;
                    Copyright = String.Empty;
                    ProductName = String.Empty;
                    Version = String.Empty;

                    _Assembly = Assembly.GetEntryAssembly();

                    if ( _Assembly != null )
                    {
                        object[] attributes = _Assembly.GetCustomAttributes( false );

                        foreach ( object attribute in attributes )
                        {
                            Type type = attribute.GetType();

                            if ( type == typeof( AssemblyTitleAttribute ) ) _Title = ( AssemblyTitleAttribute ) attribute;
                            if ( type == typeof( AssemblyCompanyAttribute ) ) _Company = ( AssemblyCompanyAttribute ) attribute;
                            if ( type == typeof( AssemblyCopyrightAttribute ) ) _Copyright = ( AssemblyCopyrightAttribute ) attribute;
                            if ( type == typeof( AssemblyProductAttribute ) ) _Product = ( AssemblyProductAttribute ) attribute;
                        }

                        _Version = _Assembly.GetName().Version;
                    }

                    if ( _Title != null ) Title = _Title.Title;
                    if ( _Company != null ) CompanyName = _Company.Company;
                    if ( _Copyright != null ) Copyright = _Copyright.Copyright;
                    if ( _Product != null ) ProductName = _Product.Product;
                    if ( _Version != null ) Version = _Version.ToString();
                }
                catch { }
            }
        }

        //-----------------------------------------------------------------------------------------

        private class RegistryPersister : IPersist
        {
            public string RegistryKey { get; set; }

            public RegistryPersister()
            {
                RegistryKey =
                    "Software\\" +
                    ApplicationAttributes.CompanyName + "\\" +
                    ApplicationAttributes.ProductName + "\\" +
                    "RecentFileList";
            }

            public RegistryPersister( string key )
            {
                RegistryKey = key;
            }

            string Key( int i ) { return i.ToString( "00" ); }

            public List<string> RecentFiles( int max )
            {
                RegistryKey k = Registry.CurrentUser.OpenSubKey( RegistryKey );
                if ( k == null ) k = Registry.CurrentUser.CreateSubKey( RegistryKey );

                List<string> list = new List<string>( max );

                for ( int i = 0 ; i < max ; i++ )
                {
                    string filename = ( string ) k.GetValue( Key( i ) );

                    if ( String.IsNullOrEmpty( filename ) ) break;

                    list.Add( filename );
                }

                return list;
            }

            public void InsertFile( string filepath, int max )
            {
                RegistryKey k = Registry.CurrentUser.OpenSubKey( RegistryKey );
                if ( k == null ) Registry.CurrentUser.CreateSubKey( RegistryKey );
                k = Registry.CurrentUser.OpenSubKey( RegistryKey, true );

                RemoveFile( filepath, max );

                for ( int i = max - 2 ; i >= 0 ; i-- )
                {
                    string sThis = Key( i );
                    string sNext = Key( i + 1 );

                    object oThis = k.GetValue( sThis );
                    if ( oThis == null ) continue;

                    k.SetValue( sNext, oThis );
                }

                k.SetValue( Key( 0 ), filepath );
            }

            public void RemoveFile( string filepath, int max )
            {
                RegistryKey k = Registry.CurrentUser.OpenSubKey( RegistryKey );
                if ( k == null ) return;

                for ( int i = 0 ; i < max ; i++ )
                {
again:
                    string s = ( string ) k.GetValue( Key( i ) );
                    if ( s != null && s.Equals( filepath, StringComparison.CurrentCultureIgnoreCase ) )
                    {
                        RemoveFile( i, max );
                        goto again;
                    }
                }
            }

            void RemoveFile( int index, int max )
            {
                RegistryKey k = Registry.CurrentUser.OpenSubKey( RegistryKey, true );
                if ( k == null ) return;

                k.DeleteValue( Key( index ), false );

                for ( int i = index ; i < max - 1 ; i++ )
                {
                    string sThis = Key( i );
                    string sNext = Key( i + 1 );

                    object oNext = k.GetValue( sNext );
                    if ( oNext == null ) break;

                    k.SetValue( sThis, oNext );
                    k.DeleteValue( sNext );
                }
            }
        }

        //-----------------------------------------------------------------------------------------

        private class XmlPersister : IPersist
        {
            public string Filepath { get; set; }

            public Stream Stream { get; set; }

            public XmlPersister()
            {
                Filepath =
                    Path.Combine(
                        Environment.GetFolderPath( Environment.SpecialFolder.ApplicationData ),
                        ApplicationAttributes.CompanyName + "\\" +
                        ApplicationAttributes.ProductName + "\\" +
                        "RecentFileList.xml" );
            }

            public XmlPersister( string filepath )
            {
                Filepath = filepath;
            }

            public XmlPersister( Stream stream )
            {
                Stream = stream;
            }

            public List<string> RecentFiles( int max )
            {
                return Load( max );
            }

            public void InsertFile( string filepath, int max )
            {
                Update( filepath, true, max );
            }

            public void RemoveFile( string filepath, int max )
            {
                Update( filepath, false, max );
            }

            void Update( string filepath, bool insert, int max )
            {
                List<string> old = Load( max );

                List<string> list = new List<string>( old.Count + 1 );

                if ( insert ) list.Add( filepath );

                CopyExcluding( old, filepath, list, max );

                Save( list, max );
            }

            void CopyExcluding( List<string> source, string exclude, List<string> target, int max )
            {
                foreach ( string s in source )
                    if ( !String.IsNullOrEmpty( s ) )
                        if ( !s.Equals( exclude, StringComparison.OrdinalIgnoreCase ) )
                            if ( target.Count < max )
                                target.Add( s );
            }

            class SmartStream : IDisposable
            {
                bool _IsStreamOwned = true;
                Stream _Stream = null;

                public Stream Stream { get { return _Stream; } }

                public static implicit operator Stream( SmartStream me ) { return me.Stream; }

                public SmartStream( string filepath, FileMode mode )
                {
                    _IsStreamOwned = true;

                    Directory.CreateDirectory( Path.GetDirectoryName( filepath ) );

                    _Stream = File.Open( filepath, mode );
                }

                public SmartStream( Stream stream )
                {
                    _IsStreamOwned = false;
                    _Stream = stream;
                }

                public void Dispose()
                {
                    if ( _IsStreamOwned && _Stream != null ) _Stream.Dispose();

                    _Stream = null;
                }
            }

            SmartStream OpenStream( FileMode mode )
            {
                if ( !String.IsNullOrEmpty( Filepath ) )
                {
                    return new SmartStream( Filepath, mode );
                }
                else
                {
                    return new SmartStream( Stream );
                }
            }

            List<string> Load( int max )
            {
                List<string> list = new List<string>( max );

                using ( MemoryStream ms = new MemoryStream() )
                {
                    using ( SmartStream ss = OpenStream( FileMode.OpenOrCreate ) )
                    {
                        if ( ss.Stream.Length == 0 ) return list;

                        ss.Stream.Position = 0;

                        byte[] buffer = new byte[ 1 << 20 ];
                        for ( ; ; )
                        {
                            int bytes = ss.Stream.Read( buffer, 0, buffer.Length );
                            if ( bytes == 0 ) break;
                            ms.Write( buffer, 0, bytes );
                        }

                        ms.Position = 0;
                    }

                    XmlTextReader x = null;

                    try
                    {
                        x = new XmlTextReader( ms );

                        while ( x.Read() )
                        {
                            switch ( x.NodeType )
                            {
                                case XmlNodeType.XmlDeclaration:
                                case XmlNodeType.Whitespace:
                                    break;

                                case XmlNodeType.Element:
                                    switch ( x.Name )
                                    {
                                        case "RecentFiles": break;

                                        case "RecentFile":
                                            if ( list.Count < max ) list.Add( x.GetAttribute( 0 ) );
                                            break;

                                        default: Debug.Assert( false ); break;
                                    }
                                    break;

                                case XmlNodeType.EndElement:
                                    switch ( x.Name )
                                    {
                                        case "RecentFiles": return list;
                                        default: Debug.Assert( false ); break;
                                    }
                                    break;

                                default:
                                    Debug.Assert( false );
                                    break;
                            }
                        }
                    }
                    finally
                    {
                        if ( x != null ) x.Close();
                    }
                }
                return list;
            }

            void Save( List<string> list, int max )
            {
                using ( MemoryStream ms = new MemoryStream() )
                {
                    XmlTextWriter x = null;

                    try
                    {
                        x = new XmlTextWriter( ms, Encoding.UTF8 );
                        if ( x == null ) { Debug.Assert( false ); return; }

                        x.Formatting = Formatting.Indented;

                        x.WriteStartDocument();

                        x.WriteStartElement( "RecentFiles" );

                        foreach ( string filepath in list )
                        {
                            x.WriteStartElement( "RecentFile" );
                            x.WriteAttributeString( "Filepath", filepath );
                            x.WriteEndElement();
                        }

                        x.WriteEndElement();

                        x.WriteEndDocument();

                        x.Flush();

                        using ( SmartStream ss = OpenStream( FileMode.Create ) )
                        {
                            ss.Stream.SetLength( 0 );

                            ms.Position = 0;

                            byte[] buffer = new byte[ 1 << 20 ];
                            for ( ; ; )
                            {
                                int bytes = ms.Read( buffer, 0, buffer.Length );
                                if ( bytes == 0 ) break;
                                ss.Stream.Write( buffer, 0, bytes );
                            }
                        }
                    }
                    finally
                    {
                        if ( x != null ) x.Close();
                    }
                }
            }
        }

        //-----------------------------------------------------------------------------------------

    }
}
