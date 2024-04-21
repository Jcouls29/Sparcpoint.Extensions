namespace Sparcpoint.Extensions.Multitenancy;

public sealed class StaticTenant<TTenant> : ITenant<TTenant>
{
    public StaticTenant(TTenant value)
    {
        Ensure.NotNull(value);

        Value = value;
    }

    public TTenant Value { get; }
}
