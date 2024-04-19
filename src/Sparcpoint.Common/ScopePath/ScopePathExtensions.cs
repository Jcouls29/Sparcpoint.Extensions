using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sparcpoint;

public static class ScopePathExtensions
{
    public static ScopePath GetBranchPoint(this ScopePath scope, ScopePath other)
    {
        List<string> branch = new();

        var enumerator1 = scope.Segments.GetEnumerator();
        var enumerator2 = other.Segments.GetEnumerator();

        while (enumerator1.MoveNext())
        {
            var e1 = enumerator1.Current;
            if (e1 == null)
                break;

            if (!enumerator2.MoveNext())
                break;
            var e2 = enumerator2.Current;
            if (e2 == null || !e1.Equals(e2))
                break;

            branch.Add(e1.ToString()!);
        }

        return new ScopePath(branch.ToArray());
    }

    public static ScopePath Back(this ScopePath scope, int numberLevels = 1)
    {
        if (numberLevels < 0)
            throw new ArgumentOutOfRangeException(nameof(numberLevels), "Number of levels to go back cannot be less than 0.");

        if (numberLevels == 0)
            return scope;

        if (scope.Segments.Length <= numberLevels)
            return ScopePath.RootScope;

        return new ScopePath(scope.Segments.Take(scope.Segments.Length - numberLevels).ToArray());
    }

    public static ScopePath Append(this ScopePath scope, ScopePath other)
    {
        var finalSegments = Array.Empty<string>();
        if (scope.Segments.Length > 0)
        {
            finalSegments = scope.Segments;
        }

        if (other.Segments.Length > 0)
        {
            finalSegments = finalSegments.Concat(other.Segments).ToArray();
        }

        return new ScopePath(finalSegments);
    }

    public static ScopePath[] GetHierarchy(this ScopePath scope, bool includeRootScope = false)
    {
        List<ScopePath> hierarchy = new List<ScopePath>();
        if (scope == ScopePath.RootScope)
            return includeRootScope ? new[] { ScopePath.RootScope } : Array.Empty<ScopePath>();

        hierarchy.Add(scope);
        ScopePath nextPath = scope;
        while ((nextPath = nextPath.Back()) != ScopePath.RootScope)
        {
            hierarchy.Add(nextPath);
        }

        if (includeRootScope)
            hierarchy.Add(ScopePath.RootScope);

        return hierarchy.ToArray();
    }

    public static ScopePath[] GetHierarchyAscending(this ScopePath scope, bool includeRootScope = false)
    {
        return GetHierarchy(scope, includeRootScope).Reverse().ToArray();
    }

    public static bool StartsWith(this ScopePath scope, ScopePath startsWith)
    {
        var itSequence = scope.Segments.GetEnumerator();
        var itStarts = startsWith.Segments.GetEnumerator();

        while (itStarts.MoveNext())
        {
            var el = itStarts.Current;

            if (!itSequence.MoveNext())
                return false;

            if (!el.Equals(itSequence.Current))
                return false;
        }

        return true;
    }

    public static bool EndsWith(this ScopePath scope, ScopePath endsWith)
    {
        return scope.Reverse().StartsWith(endsWith.Reverse());
    }

    public static ScopePath LastNSegments(this ScopePath scope, int count)
    {
        if (count > scope.Rank)
            throw new ArgumentOutOfRangeException($"Segment count cannot be larger than the scope's rank. [Rank = {scope.Rank}, Count = {count}]");

        var segments = scope.Segments.Reverse().Take(count).Reverse().ToArray();
        return new ScopePath(segments);
    }

    public static ScopePath Reverse(this ScopePath scope)
    {
        return new ScopePath(scope.Segments.Reverse().ToArray());
    }
}
