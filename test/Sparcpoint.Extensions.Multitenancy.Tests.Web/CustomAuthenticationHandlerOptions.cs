namespace Sparcpoint.Extensions.Multitenancy;

public class CustomAuthenticationHandlerOptions
{
    public bool IsAuthenticated { get; set; } = false;
    public string? AccountId { get; set; } = null;
}
