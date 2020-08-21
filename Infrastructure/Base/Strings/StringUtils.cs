using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Base.Strings
{
    public static class StringUtils
    {
        public static string AddPathPart(params string[] p)
        {
            return AddPathPart(Path.DirectorySeparatorChar, p);
        }

        public static string AddPathPart(char directorySeparatorChar, params string[] p)
        {
            if (p.Length == 0)
                return null;

            var res = p[0];

            for (var i = 1; i < p.Length; i++)
                res = AddPathPart(res, p[i], directorySeparatorChar);

            return res;
        }

        public static string AddPathPart(this string p1, string p2)
        {
            return AddPathPart(p1, p2, Path.DirectorySeparatorChar);
        }

        public static string AddPathPart(this string p1, string p2, string p3)
        {
            return p1.AddPathPart(p2, Path.DirectorySeparatorChar).AddPathPart(p3, Path.DirectorySeparatorChar);
        }

        public static string AddPathPart(this string p1, string p2, string p3, string p4)
        {
            return p1.AddPathPart(p2, p3, Path.DirectorySeparatorChar).AddPathPart(p4, Path.DirectorySeparatorChar);
        }

        public static string AddPathPart(this string p1, string p2, char directorySeparatorChar)
        {
            var ds = string.Empty + directorySeparatorChar;
            if (!string.IsNullOrEmpty(p1) && p1.Length > 0 && p1.Substring(p1.Length - 1, 1) != ds)
                p1 += ds;

            return p1 + p2;
        }

        public static string AddPathPart(this string p1, string p2, string p3, char directorySeparatorChar)
        {
            return p1.AddPathPart(p2, directorySeparatorChar).AddPathPart(p3, directorySeparatorChar);
        }

        public static string AddPathPart(this string p1, string p2, string p3, string p4, char directorySeparatorChar)
        {
            return p1.AddPathPart(p2, p3, directorySeparatorChar).AddPathPart(p4, directorySeparatorChar);
        }

        public static string AggregateLinesToString(this IEnumerable<string> lines)
        {
            var linesArray = lines.ToArray();
            var builder = new StringBuilder();
            var lastIndex = linesArray.Length - 1;
            for (var i = 0; i <= lastIndex; i++)
                if (i == lastIndex)
                    builder.Append(linesArray[i]);
                else
                    builder.AppendLine(linesArray[i]);

            return builder.ToString();
        }

        public static string AggregateToString<T>(this IEnumerable<T> items, string delimiter = ",")
        {
            if (items.Any())
                return items.Select(x => x.ToString()).Aggregate((s1, next) => s1 + delimiter + next);
            return null;
        }

        public static string GetBeforeLast(this string s, string marker)
        {
            var lastIndexOfMarker = s.LastIndexOf(marker);

            if (!string.IsNullOrEmpty(s) && s.Length > 0 && lastIndexOfMarker > -1)
                return s.Substring(0, lastIndexOfMarker);

            return s;
        }

        public static string GetAfterLast(this string s, char marker)
        {
            int lastIndexOfMarker = s.LastIndexOf(marker);

            if (!string.IsNullOrEmpty(s) && s.Length > 0 && lastIndexOfMarker > -1)
                return s.Substring(lastIndexOfMarker + 1, s.Length - lastIndexOfMarker - 1);

            return s;
        }

        public static string ReplaceInvalidFileNameChars(this string filename, string replaceOn)
        {
            return string.Join(replaceOn, filename.Split(Path.GetInvalidFileNameChars()));
        }

        public static string ReplaceInvalidPathChars(this string filename, string replaceOn)
        {
            return string.Join(replaceOn, filename.Split(Path.GetInvalidPathChars()));
        }
    }
}