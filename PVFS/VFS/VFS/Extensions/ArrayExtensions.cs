using System.Collections.Generic;
using System.Linq;

namespace VFS.VFS
{
    public static class ArrayExtensions
    {
        public static int IndexOf<T>(this T[] arr, T value)
        {
            for (int i = 0; i < arr.Length; i++)
            {
                if (arr[i].Equals(value))
                    return i;
            }
            return -1;
        }

        public static string Concat(this IEnumerable<string> arr, string glue)
        {
            if (arr.Count() == 0)
                return "";
            return arr.First() + arr.Skip(1).Aggregate((agg, s) => agg + (glue + s));
        }
    }
}
