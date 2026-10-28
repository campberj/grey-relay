using GreyRelay.Application.Campaigns;
using Microsoft.AspNetCore.Http.HttpResults;

namespace GreyRelay.Api.Endpoints;

/// <summary>Player-visible campaign routes. GM routes will live in a separate group (ADR 0001 section 2.4).</summary>
public static class CampaignEndpoints
{
    public static IEndpointRouteBuilder MapCampaignEndpoints(this IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("/api/campaigns").WithTags("Campaigns");
        group.MapGet("/", ListAsync).WithName("ListCampaigns");
        return routes;
    }

    private static async Task<Ok<IReadOnlyList<CampaignSummary>>> ListAsync(
        ListCampaigns listCampaigns,
        CancellationToken cancellationToken)
    {
        var campaigns = await listCampaigns.ExecuteAsync(cancellationToken).ConfigureAwait(false);
        return TypedResults.Ok(campaigns);
    }
}
