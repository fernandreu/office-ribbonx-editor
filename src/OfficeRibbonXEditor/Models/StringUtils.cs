using System.IO;
using System.Text;

namespace OfficeRibbonXEditor.Models
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

            var root = new StringBuilder(Path.GetPathRoot(pathName) ?? string.Empty);
            if (root.Length > 3)
            {
                root.Append(Path.DirectorySeparatorChar);
            }

            var elements = pathName.Substring(root.Length).Split(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);

            var filenameIndex = elements.GetLength(0) - 1;

            if (elements.GetLength(0) == 1)
            {
                // pathname is just a root and filename
                if (elements[0].Length > 5)
                {
                    // Long enough to shorten. If path is a UNC path, root may be rather long
                    if (root.Length + 6 >= maxLength)
                    {
                        root.Append(elements[0].Substring(0, 3) + "...");
                        return root.ToString();
                    }
                    else
                    {
                        return pathName.Substring(0, maxLength - 3) + "...";
                    }
                }
            }
            else if (root.Length + 4 + elements[filenameIndex].Length > maxLength)
            {
                // pathname is just a root and filename
                root.Append("...\\");

                var len = elements[filenameIndex].Length;
                if (len < 6)
                {
                    root.Append(elements[filenameIndex]);
                    return root.ToString();
                }

                if ((root.Length + 6) >= maxLength)
                {
                    len = 3;
                }
                else
                {
                    len = maxLength - root.Length - 3;
                }

                root.Append(elements[filenameIndex].Substring(0, len) + "...");
                return root.ToString();
            }
            else if (elements.GetLength(0) == 2)
            {
                root.Append("...\\" + elements[1]);
                return root.ToString();
            }
            else
            {
                var len = 0;
                var begin = 0;

                for (var i = 0; i < filenameIndex; i++)
                {
                    if (elements[i].Length > len)
                    {
                        begin = i;
                        len = elements[i].Length;
                    }
                }

                int totalLength = pathName.Length - len + 3;
                int end = begin + 1;

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

                    if (end < filenameIndex)
                    {
                        totalLength -= elements[++end].Length - 1;
                    }

                    if (begin == 0 && end == filenameIndex)
                    {
                        break;
                    }
                }

                // assemble final string
                for (int i = 0; i < begin; i++)
                {
                    root.Append(elements[i] + '\\');
                }

                root.Append("...\\");

                for (var i = end; i < filenameIndex; i++)
                {
                    root.Append(elements[i] + '\\');
                }

                root.Append(elements[filenameIndex]);
                return root.ToString();
            }

            return pathName;
        }
    }
}
