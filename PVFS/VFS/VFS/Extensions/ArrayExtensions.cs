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
            int count = arr.Count();
            if (count == 0)
                return "";
            if (count == 1)
                return arr.First();
            return arr.Skip(1).Aggregate(arr.First(), (agg, s) => agg + (glue + s));
        }
    }
}
