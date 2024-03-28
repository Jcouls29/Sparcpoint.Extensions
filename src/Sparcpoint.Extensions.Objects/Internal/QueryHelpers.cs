namespace Sparcpoint.Extensions.Objects;

internal static class QueryHelpers
{
    public static IEnumerable<ISparcpointObject> PerformQuery(this List<ISparcpointObject> entries, ObjectQueryParameters parameters, object lockObject)
    {
        var query = entries.AsQueryable();

        if (parameters.Id != null)
            query = query.Where(e => e.Id == parameters.Id);
        if (parameters.ParentScope != null)
            query = query.Where(e => e.Id.Back(1) == parameters.ParentScope);
        if (parameters.Name != null)
            query = query.Where(e => e.Name == parameters.Name);
        if (parameters.NameStartsWith != null)
            query = query.Where(e => e.Name.StartsWith(parameters.NameStartsWith));
        if (parameters.NameEndsWith != null)
            query = query.Where(e => e.Name.EndsWith(parameters.NameEndsWith));
        if (parameters.WithProperties != null && parameters.WithProperties.Count > 0)
        {
            foreach (var prop in parameters.WithProperties)
                query = query.Where(e => e.GetProperty(prop.Key) == prop.Value);
        }
        if (parameters.WithType != null)
            query = query.Where(e => e.GetType() == parameters.WithType);

        lock (lockObject)
        {
            return query.ToArray();
        }
    }
}