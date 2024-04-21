using Microsoft.AspNetCore.Authentication;

namespace Sparcpoint.Extensions.Multitenancy.Tests.Web;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        builder.Services.AddMultitenancy<AccountTenant, AccountTenantResolver>((b) =>
        {
            b.Add<TenantSpecificService>();
            return b;
        });

        builder.Services.AddSingleton(new CustomAuthenticationHandlerOptions());
        builder.Services.AddAuthentication("default")
            .AddScheme<AuthenticationSchemeOptions, CustomAuthenticationHandler>("default", o => { });

        builder.Services.AddControllers();

        var app = builder.Build();

        app.UseHttpsRedirection();
        app.UseRouting();

        app.UseAuthentication();
        app.UseAuthorization();

        app.UseMultitenancy<AccountTenant>();

        app.MapControllers();

        app.Run();
    }
}
