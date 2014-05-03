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

        public static bool CompareContent<T>(this T[] arr1, int index, int length, T[] arr2)
        {
            if (arr1.Length < index + length)
                return false;
            if (arr2.Length != length)
                return false;

            for (int i = 0; i < length; i++)
            {
                if (arr1[i + index].Equals(arr2[i]))
                    return false;
            }
            return true;
        }
    }
}
