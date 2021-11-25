using System.Collections.Generic;

namespace CoreAPI.Utils
{
    public static class LinqUtils
    {
        public static IEnumerable<T> RemoveNulls<T>(this IEnumerable<T?> list)
        {
            foreach (T? value in list)
            {
                if (value is not null)
                {
                    yield return value;
                }
            }
        }
    }
}
