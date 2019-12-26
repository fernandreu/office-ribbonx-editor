using System.Reflection;
using Microsoft.Win32;

namespace OfficeRibbonXEditor.Models
{
    public class FileAssociationHelper
    {
        private const string MenuEntryName = "OfficeXRibbonEdit";

        private readonly string extension;

        public FileAssociationHelper(string extension)
        {
            this.extension = extension;
        }

        public bool CheckAssociation()
        {
            var type = this.GetFileType();
            if (string.IsNullOrEmpty(type))
            {
                return false;
            }

            var menuKey = this.GetMenuKey(type, false);
            return menuKey != null;
        }

        public void RemoveAssociation()
        {
            var type = this.GetFileType();
            if (string.IsNullOrEmpty(type))
            {
                return;
            }

            Registry.CurrentUser.DeleteSubKeyTree($@"Software\Classes\{type}\shell\{MenuEntryName}");
        }

        public void AddAssociation()
        {
            var type = this.GetFileType();
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
            var subKey = key.CreateSubKey("command");
            subKey?.SetValue(null, $"\"{exePath}\" \"%1\"");
        }

        private string GetFileType()
        {
            var key = Registry.ClassesRoot.OpenSubKey(this.extension)?.GetValue(null)?.ToString();
            return key;
        }

        private RegistryKey GetMenuKey(string fileType, bool writable = true)
        {
            var key = Registry.CurrentUser.OpenSubKey($@"Software\Classes\{fileType}\shell\{MenuEntryName}", writable);
            return key;
        }
    }
}
