using System;
using System.IO;
using System.Linq;
using System.Text;

namespace OfficeRibbonXEditor.Helpers
{
    public static class StringUtils
    {
        /// <summary>
        /// Shortens a pathname for display purposes.
        /// This method is taken from Joe Woodbury's article at: http://www.codeproject.com/KB/cs/mrutoolstripmenu.aspx
        /// </summary>
        /// <param name="pathName">The pathname to shorten.</param>
        /// <param name="maxLength">The maximum number of characters to be displayed.</param>
        /// <remarks>Shortens a pathname by either removing consecutive components of a path
        /// and/or by removing characters from the end of the filename and replacing
        /// then with three ellipses (...)
        /// <para>In all cases, the root of the passed path will be preserved in it's entirety.</para>
        /// <para>If a UNC path is used or the pathname and maxLength are particularly short,
        /// the resulting path may be longer than maxLength.</para>
        /// <para>This method expects fully resolved path names to be passed to it.
        /// (Use Path.GetFullPath() to obtain this.)</para>
        /// </remarks>
        /// <returns>The shortened path</returns>
        public static string ShortenPathName(string pathName, int maxLength)
        {
            if (pathName.Length <= maxLength)
            {
                return pathName;
            }

            var root = Path.GetPathRoot(pathName) ?? string.Empty;
            if (root.Length > 3)
            {
                root += Path.DirectorySeparatorChar;
            }

            // RemoveEmptyEntries shouldn't really be needed for paths, but it is a way to call the char[] overload for sure without code analysis warnings
            var elements = pathName[root.Length..].Split(new[] { Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar }, StringSplitOptions.RemoveEmptyEntries);

            if (elements.Length == 1)
            {
                return ShortenRoot(root, pathName, elements[0], maxLength);
            }

            if (root.Length + 4 + elements.Last().Length > maxLength)
            {
                return ShortenRootAndFileName(root, elements.Last(), maxLength);
            }
            
            if (elements.Length == 2)
            {
                return root + "...\\" + elements[1];
            }

            return ShortenFull(root, pathName, elements, maxLength);
        }

        private static string ShortenRoot(string root, string pathName, string lastElement, int maxLength)
        {
            if (lastElement.Length <= 5)
            {
                return pathName;
            }

            // Long enough to shorten. If path is a UNC path, root may be rather long
            if (root.Length + 6 >= maxLength)
            {
                return root + lastElement[..3] + "...";
            }

            return pathName[..(maxLength - 3)] + "...";
        }

        private static string ShortenRootAndFileName(string root, string lastElement, int maxLength)
        {
            root += "...\\";

            var len = lastElement.Length;
            if (len < 6)
            {
                return root + lastElement;
            }

            if (root.Length + 6 >= maxLength)
            {
                len = 3;
            }
            else
            {
                len = maxLength - root.Length - 3;
            }

            return root + lastElement[..len] + "...";
        }

        private static string ShortenFull(string root, string pathName, string[] elements, int maxLength)
        {
            var len = 0;
            var begin = 0;

            for (var i = 0; i < elements.Length - 1; i++)
            {
                if (elements[i].Length <= len)
                {
                    continue;
                }

                begin = i;
                len = elements[i].Length;
            }

            var totalLength = pathName.Length - len + 3;
            var end = begin + 1;

            while (totalLength > maxLength)
            {
                if (begin > 0)
                {
                    totalLength -= elements[--begin].Length - 1;
                }

                if (totalLength <= maxLength)
                {
                    break;
                }

                if (end < elements.Length - 1)
                {
                    totalLength -= elements[++end].Length - 1;
                }

                if (begin == 0 && end == elements.Length - 1)
                {
                    break;
                }
            }
            
            // Assemble final string
            var sb = new StringBuilder(root);
            for (var i = 0; i < begin; i++)
            {
                sb.Append(elements[i] + '\\');
            }

            sb.Append("...\\");

            for (var i = end; i < elements.Length - 1; i++)
            {
                sb.Append(elements[i] + '\\');
            }

            sb.Append(elements.Last());
            return sb.ToString();
        }
    }
}
