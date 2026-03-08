using System;
using System.Diagnostics.CodeAnalysis;

namespace CoreAPI.Utils
{
    public static class TaskUtils
    {
        public static bool Try<T>(
            ValueTuple<bool, T> awaitedResult, 
            [NotNullWhen(true)]out T outVar)
        {
            outVar = awaitedResult.Item2;
            return awaitedResult.Item1;
        }

        public static bool Try<T1, T2>(
            ValueTuple<bool, T1, T2> awaitedResult,
            [NotNullWhen(true)] out T1 outVar1,
            [NotNullWhen(true)] out T2 outVar2)
        {
            outVar1 = awaitedResult.Item2;
            outVar2 = awaitedResult.Item3;
            return awaitedResult.Item1;
        }
    }
}
