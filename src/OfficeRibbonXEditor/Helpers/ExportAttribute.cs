namespace OfficeRibbonXEditor.Helpers;

[AttributeUsage(AttributeTargets.Class, Inherited = false)]
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

[AttributeUsage(AttributeTargets.Class, Inherited = false)]
public class ExportAttribute<T>() : ExportAttribute(typeof(T));