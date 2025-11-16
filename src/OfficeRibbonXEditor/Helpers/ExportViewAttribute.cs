namespace OfficeRibbonXEditor.Helpers;

[AttributeUsage(AttributeTargets.Class, Inherited = false)]
public class ExportViewAttribute(Type viewModelType) : Attribute
{
    public Type ViewModelType { get; } = viewModelType;
}

[AttributeUsage(AttributeTargets.Class, Inherited = false)]
public class ExportViewAttribute<T>() : ExportViewAttribute(typeof(T));