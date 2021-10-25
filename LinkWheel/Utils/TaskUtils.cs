using System;

namespace LinkWheel.Utils
{
    public static class TaskUtils
    {
        public static bool Try<T>(ValueTuple<bool, T> awaitedResult, out T outVar)
        {
            outVar = awaitedResult.Item2;
            return awaitedResult.Item1;
        }
    }
}
