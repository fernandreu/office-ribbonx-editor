using System;

namespace OfficeRibbonXEditor.Helpers
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public class ExportAttribute : Attribute
    {
        public Type? InterfaceType { get; }

        public ExportAttribute()
        {
        }

        public ExportAttribute(Type interfaceType)
        {
            InterfaceType = interfaceType;
        }

        public Lifetime Lifetime { get; set; } = Lifetime.Transient;
    }
}
