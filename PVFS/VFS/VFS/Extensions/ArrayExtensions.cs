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
    }
}
