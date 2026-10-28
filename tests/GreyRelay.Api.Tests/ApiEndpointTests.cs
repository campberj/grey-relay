using System.Net;
using System.Net.Http.Json;
using GreyRelay.Application.Campaigns;
using Microsoft.AspNetCore.Mvc.Testing;

namespace GreyRelay.Api.Tests;

public sealed class ApiEndpointTests(WebApplicationFactory<Program> factory)
    : IClassFixture<WebApplicationFactory<Program>>
{
    [Fact]
    public async Task Health_returns_healthy()
    {
        using var client = factory.CreateClient();

        using var response = await client.GetAsync(new Uri("/api/health", UriKind.Relative), TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("Healthy", await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken));
    }

    [Fact]
    public async Task Campaigns_returns_empty_library()
    {
        using var client = factory.CreateClient();

        var campaigns = await client.GetFromJsonAsync<List<CampaignSummary>>(
            new Uri("/api/campaigns", UriKind.Relative),
            TestContext.Current.CancellationToken);

        Assert.NotNull(campaigns);
        Assert.Empty(campaigns);
    }

    [Fact]
    public async Task Requests_for_non_local_hosts_are_rejected()
    {
        using var client = factory.CreateClient();
        using var request = new HttpRequestMessage(HttpMethod.Get, new Uri("/api/health", UriKind.Relative));
        request.Headers.Host = "attacker.example";

        using var response = await client.SendAsync(request, TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }
}
