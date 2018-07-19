// --------------------------------------------------------------------------------------------------------------------
// <copyright file="XmlColorizer.cs" company="Microsoft">
//   Microsoft
// </copyright>
// <summary>
//   Class to apply RTF format to an XML file
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace CustomUIEditor
{
    using System;
    using System.Text;
    using System.Text.RegularExpressions;

    public class XmlColorizer
    {
        internal const string RtfString = @"{\rtf1\ansi\ansicpg1252\deff0\deflang1033{\fonttbl{\f0\fmodern\fprq1\fcharset0 Courier New;}}{\colortbl    ;\red0\green0\blue255;\red128\green0\blue0;\red255\green0\blue0;\red0\green128\blue0;}\pard\f0\fs20 ";
        internal const string RtfAttributeName = @"\cf3 ";
        internal const string RtfAttributeValue = @"\cf1 ";
        internal const string RtfDelimiter = @"\cf1 ";
        internal const string RtfAttributeQuote = @"\cf0 ";
        internal const string RtfName = @"\cf2 ";
        internal const string RtfComment = @"\cf4 ";

        private static readonly Regex XmlCommentRegex = new Regex(
            @"<!--(.*?)-->",
            RegexOptions.IgnoreCase | RegexOptions.Singleline | RegexOptions.CultureInvariant | RegexOptions.IgnorePatternWhitespace | RegexOptions.Compiled);

        private static readonly Regex AttributePairRegex = new Regex(
            @"(?<Keyword>\w+)(?<EqualSign>\s*=\s*)""(?<Value>.*?)""",
            RegexOptions.IgnoreCase | RegexOptions.Multiline | RegexOptions.CultureInvariant | RegexOptions.IgnorePatternWhitespace | RegexOptions.Compiled);
        
        public static string Colorize(string text)
        {
            text = ConvertUtfToRtf(text);

            if (string.IsNullOrEmpty(text))
            {
                return RtfString;
            }

            var rtf = new StringBuilder(3 * text.Length);
            rtf.Append(RtfString);

            text = text.Replace("{", "\\{");
            text = text.Replace("}", "\\}");
            rtf.Append(ParseXmlComment(text));
            
            /*jargil: there is an issue with the ToString: it adds an extra \ to every \ in the StringBuilder object,
             * that's why we need to erase it from the returning string.*/
            return rtf.ToString().Replace("\n", @"\par ").Replace("\\\\u", "\\u");
        }

        private static string ParseXmlComment(string text)
        {
            var rtfText = new StringBuilder(3 * text.Length);
            var matches = XmlCommentRegex.Matches(text);
            if (matches.Count == 0)
            {
                rtfText.Append(ParseXmlAttribute(text));
            }
            else
            {
                var current = 0;
                foreach (Match match in matches)
                {
                    rtfText.Append(ParseXmlAttribute(text.Substring(current, match.Index - current)));

                    rtfText.Append(RtfDelimiter + "<!--" + RtfComment);
                    rtfText.Append(match.Groups[1].Value);
                    rtfText.Append(RtfDelimiter + "-->");
                    current = match.Index + match.Length;
                }

                rtfText.Append(ParseXmlAttribute(text.Substring(current)));
            }

            return rtfText.ToString();
        }

        private static string ParseXmlAttribute(string text)
        {
            var rtfText = new StringBuilder(3 * text.Length);
            var matches = AttributePairRegex.Matches(text);
            if (matches.Count == 0)
            {
                rtfText.Append(ParseXmlTag(text));
            }
            else
            {
                var current = 0;
                foreach (Match match in matches)
                {
                    rtfText.Append(ParseXmlTag(text.Substring(current, match.Index - current)));

                    rtfText.Append(RtfAttributeName);
                    rtfText.Append(match.Groups["Keyword"].Value);
                    rtfText.Append(RtfDelimiter);
                    rtfText.Append(match.Groups["EqualSign"].Value);
                    rtfText.Append(RtfAttributeQuote);
                    rtfText.Append("\"");
                    rtfText.Append(RtfAttributeValue);
                    rtfText.Append(match.Groups["Value"].Value);
                    rtfText.Append(RtfAttributeQuote);
                    rtfText.Append("\"");

                    rtfText.Append(RtfDelimiter);
                    current = match.Index + match.Length;
                }

                rtfText.Append(ParseXmlTag(text.Substring(current)));
            }

            return rtfText.ToString();
        }

        private static string ParseXmlTag(string text)
        {
            var rtfText = new StringBuilder(2 * text.Length);
            foreach (var c in text)
            {
                switch (c)
                {
                    case '>':
                        rtfText.Append(RtfDelimiter + c);
                        break;
                    case '/':
                    case '<':
                        rtfText.Append(RtfDelimiter + c + RtfName);
                        break;
                    case '\\':
                        rtfText.Append("\\\\"); // JArgil:  This solves a bug where if you type \ you loose a line
                        break;
                    default:
                        rtfText.Append(c);
                        break;
                }
            }

            return rtfText.ToString();
        }

        /// <summary>
        /// Converts from UTF to Rtf.
        /// </summary>
        /// <param name="unicode">String with UTF characters.</param>
        /// <returns>String with Rtf formatting.</returns>
        private static string ConvertUtfToRtf(string unicode)
        {
            var rtf = new StringBuilder();
            foreach (var letter in unicode)
            {
                if (letter <= 0x7F)  
                {
                    // Before this is ASCII in UTF-8 and UTF-16 Encoding
                    rtf.Append(letter);
                }
                else
                {
                    // Starts Eurpean (except ASCII), arabic, hebrew, etc.
                    rtf.Append($@"\u{Convert.ToUInt32(letter)}?");
                }
            }

            return rtf.ToString(/*it has the same text but the utf characters where changed to something like \\u###?*/);
        }
    }
}
