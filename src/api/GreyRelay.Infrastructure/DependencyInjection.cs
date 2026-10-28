using GreyRelay.Application.Campaigns;
using GreyRelay.Infrastructure.Campaigns;
using Microsoft.Extensions.DependencyInjection;

namespace GreyRelay.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services)
    {
        services.AddSingleton<ICampaignRepository, InMemoryCampaignRepository>();
        return services;
    }
}
