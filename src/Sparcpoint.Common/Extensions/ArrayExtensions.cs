using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sparcpoint.Common.Extensions;

public static class ArrayExtensions
{
    public static T[] Slice<T>(this T[] array, int start, int length)
    {
        Ensure.ArgumentNotNull(array);
        Ensure.ConditionMet(array.Length > 0);
        Ensure.ConditionMet(start >= 0);
        Ensure.ConditionMet(length > 0);
        Ensure.ConditionMet((start + length - 1) < array.Length);

        var span = new ReadOnlySpan<T>(array, start, length);
        return span.ToArray();
    }

    public static T[] Slice<T>(this T[] array, int start)
    {
        return Slice<T>(array, start, array.Length - start);
    }
}
