using System.Collections.Generic;
using System.Linq;
using System.Text;
using Generators.Extensions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

namespace Generators
{
    [Generator]
    public class CommandGenerator : ISourceGenerator
    {
        private const string AttributeText = @"
using System;
namespace Generators
{
    [AttributeUsage(AttributeTargets.Method, Inherited = false, AllowMultiple = false)]
    sealed class GenerateCommandAttribute : Attribute
    {
        public GenerateCommandAttribute() {}
        public string Name { get; set; }
    }
}
";

        private const string RelayCommandReference = "GalaSoft.MvvmLight.Command.RelayCommand";

        private const string AsyncCommandReference = "AsyncAwaitBestPractices.MVVM.AsyncCommand";

        public void Initialize(GeneratorInitializationContext context)
        {
            // Register a syntax receiver that will be created for each generation pass
            context.RegisterForSyntaxNotifications(() => new SyntaxReceiver());
        }

        public void Execute(GeneratorExecutionContext context)
        {
            // Add the attribute text
            context.AddSource("GenerateCommandAttribute.cs", SourceText.From(AttributeText, Encoding.UTF8));

            // Retrieve the populated receiver
            if (!(context.SyntaxReceiver is SyntaxReceiver receiver))
            {
                return;
            }

            // We're going to create a new compilation that contains the attribute
            // TODO: We should allow source generators to provide source during initialize, so that this step isn't required
            var options = ((CSharpCompilation)context.Compilation).SyntaxTrees[0].Options as CSharpParseOptions;
            var compilation = context.Compilation.AddSyntaxTrees(CSharpSyntaxTree.ParseText(SourceText.From(AttributeText, Encoding.UTF8), options));

            // Get the newly bound attributes, and RelayCommand
            var attributeSymbol = compilation.GetTypeByMetadataName("Generators.GenerateCommandAttribute");
            var taskSymbol = compilation.GetTypeByMetadataName("System.Threading.Tasks.Task");
            
            // Loop over the candidate fields, and keep the ones that are actually annotated
            var methodSymbols = new List<IMethodSymbol>();
            foreach (var method in receiver.CandidateMethods)
            {
                var model = compilation.GetSemanticModel(method.SyntaxTree);
                var methodSymbol = model.GetDeclaredSymbol(method);
                if (methodSymbol?.GetAttributes().Any(x => x?.AttributeClass?.Equals(attributeSymbol, SymbolEqualityComparer.Default) ?? false) ?? false)
                {
                    methodSymbols.Add(methodSymbol);
                }
            }

            // Group the fields by class, and generate the source
            foreach (var group in methodSymbols.GroupBy(x => x.ContainingType))
            {
                var classSource = ProcessClass(context, group.Key, group.ToList(), attributeSymbol, taskSymbol);
                if (classSource == null)
                {
                    continue;
                }

                context.AddSource($"{group.Key.Name}.CommandGenerator.cs", SourceText.From(classSource, Encoding.UTF8));
            }
        }

        private static string ProcessClass(GeneratorExecutionContext context, INamedTypeSymbol classSymbol, List<IMethodSymbol> methods, ISymbol attributeSymbol, ISymbol taskSymbol)
        {
            if (!classSymbol.ContainingSymbol.Equals(classSymbol.ContainingNamespace, SymbolEqualityComparer.Default))
            {
                context.ReportDiagnostic(classSymbol, "SG0001", "Cannot generate commands in a nested class");
                return null;
            }

            var namespaceName = classSymbol.ContainingNamespace.ToDisplayString();

            var anyProcessed = false;

            // Begin building the generated source
            var source = new StringBuilder($@"
namespace {namespaceName}
{{
    public partial class {classSymbol.Name}
    {{
");
            // Create backing field / property for each command
            foreach (var methodSymbol in methods)
            {
                var processed = ProcessMethod(context, source, methodSymbol, attributeSymbol, taskSymbol);
                anyProcessed = anyProcessed || processed;
            }

            source.Append(@"
    }
}");
            return anyProcessed ? source.ToString() : null;
        }

        private static bool ProcessMethod(GeneratorExecutionContext context, StringBuilder source, IMethodSymbol methodSymbol, ISymbol attributeSymbol, ISymbol taskSymbol)
        {
            // Get the name and type of the field
            var methodName = methodSymbol.Name;
            var methodType = methodSymbol.ReturnType;
            if (methodType.SpecialType != SpecialType.System_Void && !methodType.Equals(taskSymbol, SymbolEqualityComparer.Default))
            {
                context.ReportDiagnostic(methodSymbol, "SG0002", $"Only methods with return type of void or Task can use {attributeSymbol.Name}");
                return false;
            }

            // Get the Command attribute from the method, and any associated data
            var attributeData = methodSymbol.GetAttributes().Single(x => x.AttributeClass.Equals(attributeSymbol, SymbolEqualityComparer.Default));
            var overridenNameOpt = attributeData.NamedArguments.SingleOrDefault(x => x.Key == "Name").Value;

            var propertyName = ChoosePropertyName(methodName, overridenNameOpt);
            if (propertyName.Length == 0)
            {
                context.ReportDiagnostic(methodSymbol, "SG0003", $"The resulting command property name cannot be processed for method {methodName}");
                return false;
            }

            if (methodSymbol.Parameters.Length > 1)
            {
                context.ReportDiagnostic(methodSymbol, "SG0004", $"Only methods with 0 or 1 parameters can use {attributeSymbol.Name}");
                return false;
            }

            var fieldName = propertyName.ToFieldName();

            var commandType = methodType.SpecialType == SpecialType.System_Void ? RelayCommandReference : AsyncCommandReference;
            var suffix = methodSymbol.Parameters.Length == 0 ? string.Empty : $"<{methodSymbol.Parameters[0].Type.ToDisplayString()}>";

            //context.ReportDiagnostic(methodSymbol, "SG9999", $"Doc ID for {methodSymbol.Name}: {methodSymbol.GetDocumentationCommentId()}");

            var text = $@"
        private {commandType}{suffix}? {fieldName};
{methodSymbol.GetDocstring()}
        public {commandType}{suffix} {propertyName} => {fieldName} ??= new {commandType}{suffix}({methodName});
";
            source.Append(text);

            return true;
        }

        private static string ChoosePropertyName(string methodName, TypedConstant overridenNameOpt)
        {
            if (!overridenNameOpt.IsNull)
            {
                return overridenNameOpt.Value.ToString();
            }

            methodName = methodName.StripStart("Execute");
            methodName = methodName.StripStart("Raise");
            methodName = methodName.StripEnd("Async");
            methodName = methodName.StripEnd("Command");

            if (methodName.Length == 0)
            {
                return string.Empty;
            }

            return $"{methodName}Command";
        }

        /// <summary>
        /// Created on demand before each generation pass
        /// </summary>
        class SyntaxReceiver : ISyntaxReceiver
        {
            public List<MethodDeclarationSyntax> CandidateMethods { get; } = new List<MethodDeclarationSyntax>();

            /// <summary>
            /// Called for every syntax node in the compilation, we can inspect the nodes and save any information useful for generation
            /// </summary>
            public void OnVisitSyntaxNode(SyntaxNode syntaxNode)
            {
                // Any property with at least one attribute is a candidate for command generation
                if (syntaxNode is MethodDeclarationSyntax syntax && syntax.AttributeLists.Count > 0)
                {
                    CandidateMethods.Add(syntax);
                }
            }
        }
    }
}
