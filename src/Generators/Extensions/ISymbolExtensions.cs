using System.Text;
using System.Text.RegularExpressions;
using Microsoft.CodeAnalysis;

namespace Generators.Extensions
{
    public static class ISymbolExtensions
    {
        private static readonly Regex regex = new Regex("\r\n|\r|\n");

        public static string GetDocstring(this ISymbol self, int indentation = 8)
        {
            // This should be the actual docstring but enclosed in <member> and without the leading triple slahes
            var raw = self.GetDocumentationCommentXml();
            if (raw == null)
            {
                return null;
            }

            var lines = regex.Split(raw);
            var prefix = new string(' ', indentation) + "/// ";
            
            var builder = new StringBuilder();
            foreach (var line in lines)
            {
                var trimmed = line.Trim();
                if (trimmed.Length == 0 || trimmed.StartsWith("<member name=") || trimmed.EndsWith("</member>"))
                {
                    continue;
                }

                builder.AppendLine();
                builder.Append(prefix);
                builder.Append(trimmed);
            }

            return builder.ToString();
        }
    }
}
