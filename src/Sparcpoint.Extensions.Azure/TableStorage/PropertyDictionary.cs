using System.Reflection;

namespace Sparcpoint.Extensions.Azure;

internal class PropertyDictionary : Dictionary<string, PropertyInfo>
{
    public PropertyDictionary() { }
    public PropertyDictionary(Dictionary<string, PropertyInfo> values) : base(values) { }
}
