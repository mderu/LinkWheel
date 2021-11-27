using System.Collections.Generic;
using System.Linq;

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

        public static void Update<T1, T2>(this IDictionary<T1, T2> to, IDictionary<T1, T2> from)
        {
            from.ToList().ForEach(x => to[x.Key] = x.Value);
        }
    }
}
