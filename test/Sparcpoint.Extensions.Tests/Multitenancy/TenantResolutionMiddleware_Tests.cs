using Sparcpoint.Extensions.Multitenancy;
using Sparcpoint.Extensions.Tests.Multitenancy.Helpers;
using System.Net;
using Xunit.Abstractions;

namespace Sparcpoint.Extensions.Tests.Multitenancy;

public class TenantResolutionMiddleware_Tests
{
    [Fact]
    public async Task WhenNoTenantAuthorized_TenantSpecificService_Unauthorized()
    {
        var _Factory = new MultitenantWebApplicationFactory();
        var client = _Factory.CreateClient();
        var response = await client.GetAsync("/tenant");

        Assert.NotNull(response);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal(string.Empty, await response.Content.ReadAsStringAsync());
    }

    [Fact]
    public async Task WhenTenantAuthorized_ReturnsTenantAccountId()
    {
        var _Factory = new MultitenantWebApplicationFactory();

        var client = _Factory.CreateClient();
        var response = await client.GetAsync("/tenant/ACCOUNT 01");

        Assert.NotNull(response);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("ACCOUNT 01", await response.Content.ReadAsStringAsync());
    }

    [Fact]
    public async Task WhenMultipleTenantsInParallel_NoOverlap()
    {
        var _Factory = new MultitenantWebApplicationFactory();

        var client = _Factory.CreateClient();
        var response1Task = client.GetAsync("/tenant/ACCOUNT 01");
        var response2Task = client.GetAsync("/tenant/ACCOUNT 02");

        var responses = await Task.WhenAll(response1Task, response2Task);

        Assert.Equal(HttpStatusCode.OK, responses[0].StatusCode);
        Assert.Equal("ACCOUNT 01", await responses[0].Content.ReadAsStringAsync());

        Assert.Equal(HttpStatusCode.OK, responses[1].StatusCode);
        Assert.Equal("ACCOUNT 02", await responses[1].Content.ReadAsStringAsync());
    }
}
