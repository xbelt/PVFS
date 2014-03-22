using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VFS.VFS
{
    static class ArrayExtensions
    {
        public static T[] SubArray<T>(this T[] data, int index, int length)
        {
            var result = new T[length];
            Array.Copy(data, index, result, 0, length);
            return result;
        }

        public static int IndexOf<T>(this T[] arr, T value)
        {
            for (int i = 0; i < arr.Length; i++)
            {
                if (arr[i].Equals(value))
                    return i;
            }
            return -1;
        }
    }
}
