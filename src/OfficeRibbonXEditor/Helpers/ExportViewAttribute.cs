using System;

namespace OfficeRibbonXEditor.Helpers
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public class ExportViewAttribute : Attribute
    {
        public Type ViewModelType { get; }

        public ExportViewAttribute(Type viewModelType)
        {
            ViewModelType = viewModelType;
        }
    }
}
