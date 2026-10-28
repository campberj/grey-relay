using GreyRelay.Application.Campaigns;
using Microsoft.Extensions.DependencyInjection;

namespace GreyRelay.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<ListCampaigns>();
        return services;
    }
}
