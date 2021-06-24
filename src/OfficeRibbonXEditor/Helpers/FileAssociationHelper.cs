﻿using System;
using System.Reflection;
using Microsoft.Win32;

namespace OfficeRibbonXEditor.Helpers
{
    public class FileAssociationHelper
    {
        private const string MenuEntryName = "OfficeXRibbonEdit";

        private readonly string _extension;

        public FileAssociationHelper(string extension)
        {
            _extension = extension;
        }

        public bool CheckAssociation()
        {
            var type = GetFileType();
            if (string.IsNullOrEmpty(type))
            {
                return false;
            }

#pragma warning disable CS8604 // Possible null reference argument (warning triggered after string.IsNullOrEmpty, which is incorrect).
            var menuKey = GetMenuKey(type, false);
#pragma warning restore CS8604 // Possible null reference argument.
            return menuKey != null;
        }

        public void RemoveAssociation()
        {
            var type = GetFileType();
            if (string.IsNullOrEmpty(type))
            {
                return;
            }

            Registry.CurrentUser.DeleteSubKeyTree($@"Software\Classes\{type}\shell\{MenuEntryName}");
        }

        public void AddAssociation()
        {
            var type = GetFileType();
            if (string.IsNullOrEmpty(type))
            {
                return;
            }

            var key = Registry.CurrentUser.CreateSubKey($@"Software\Classes\{type}\shell\{MenuEntryName}");
            if (key == null)
            {
                // TODO: Throw
                return;
            }
            key.SetValue(null, "Edit with OfficeRibbonXEditor");

            var exePath = Assembly.GetExecutingAssembly().Location;
            if (exePath.EndsWith(".dll", StringComparison.OrdinalIgnoreCase))
            {
                // .NET Core splits the executable and the main entry assembly into two
                exePath = exePath.Substring(0, exePath.Length - "dll".Length) + "exe";
            }

            key.SetValue("Icon", exePath);
            var subKey = key.CreateSubKey("command");
            subKey?.SetValue(null, $"\"{exePath}\" \"%1\"");
        }

        private string? GetFileType()
        {
            var key = Registry.ClassesRoot.OpenSubKey(_extension)?.GetValue(null)?.ToString();
            return key;
        }

        private static RegistryKey GetMenuKey(string fileType, bool writable = true)
        {
            var key = Registry.CurrentUser.OpenSubKey($@"Software\Classes\{fileType}\shell\{MenuEntryName}", writable);
            return key;
        }
    }
}
