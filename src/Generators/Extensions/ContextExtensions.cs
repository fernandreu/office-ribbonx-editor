using System.Linq;
using Microsoft.CodeAnalysis;

namespace Generators.Extensions
{
    public static class ContextExtensions
    {
        public static void ReportDiagnostic(this GeneratorExecutionContext context, ISymbol symbol, string id, string message)
        {
            context.ReportDiagnostic(Diagnostic.Create(id, "Generators", message, DiagnosticSeverity.Warning, DiagnosticSeverity.Warning, true, 4, location: GetLocation(symbol)));
        }

        private static Location GetLocation(ISymbol symbol)
        {
            var reference = symbol.DeclaringSyntaxReferences.FirstOrDefault();
            return reference != null ? Location.Create(reference.SyntaxTree, reference.Span) : null;
        }
    }
}
