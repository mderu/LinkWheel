using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace LinkWheel.Utils
{
    public static class TaskUtils
    {
        public static bool Try<T>(ValueTuple<bool, T> awaitedResult, out T outVar)
        {
            outVar = awaitedResult.Item2;
            return awaitedResult.Item1;
        }

        public static bool TryDeadlock<T>(Task<ValueTuple<bool, T>> awaitedResult, out T outVar)
        {
            var result = awaitedResult.GetAwaiter().GetResult();
            outVar = result.Item2;
            return result.Item1;
        }

        public static void AddRange<T>(this ConcurrentBag<T> bag, IEnumerable<T> items)
        {
            foreach(T item in items)
            {
                bag.Add(item);
            }
        }    
    }
}
