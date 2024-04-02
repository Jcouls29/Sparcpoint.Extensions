using System.Reflection;

namespace Sparcpoint.Extensions.Azure.Objects.BlobStorage;

[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
public sealed class SparcpointObjectAttribute : Attribute
{
    public SparcpointObjectAttribute(string typeName)
    {
        TypeName = typeName;
    }

    public string TypeName { get; }

    private static Dictionary<Type, string> _RegisteredTypes = new Dictionary<Type, string>();
    private static Dictionary<string, Type> _RegisteredNames = new Dictionary<string, Type>();
    public static string GetTypeName(Type type)
    {
        if (_RegisteredTypes.TryGetValue(type, out var typeName))
            return typeName;

        typeName = type.AssemblyQualifiedName;
        var attr = type.GetCustomAttribute<SparcpointObjectAttribute>();
        if (attr != null)
            typeName = attr.TypeName;

        Ensure.NotNullOrWhiteSpace(typeName);

        _RegisteredTypes.Add(type, typeName);
        _RegisteredNames.Add(typeName, type);

        return typeName;
    }

    public static Type GetTypeFromName(string typeName)
    {
        if (!_RegisteredNames.TryGetValue(typeName, out var type))
            throw new InvalidOperationException($"Invalid type name '{typeName}'.");

        return type;
    }
}