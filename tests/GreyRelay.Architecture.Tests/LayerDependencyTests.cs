using System.Reflection;
using GreyRelay.Application.Campaigns;
using GreyRelay.Domain.Campaigns;
using GreyRelay.Infrastructure.Campaigns;

namespace GreyRelay.Architecture.Tests;

/// <summary>Enforces the dependency rule from ADR 0001 section 2.6: dependencies point inward only.</summary>
public sealed class LayerDependencyTests
{
    private static readonly Assembly Domain = typeof(Campaign).Assembly;
    private static readonly Assembly ApplicationLayer = typeof(ListCampaigns).Assembly;
    private static readonly Assembly InfrastructureLayer = typeof(InMemoryCampaignRepository).Assembly;

    [Fact]
    public void Domain_depends_on_no_other_project() =>
        Assert.Empty(ProjectReferencesOf(Domain));

    [Fact]
    public void Application_depends_only_on_domain() =>
        Assert.Equal(["GreyRelay.Domain"], ProjectReferencesOf(ApplicationLayer));

    [Fact]
    public void Infrastructure_does_not_depend_on_api() =>
        Assert.DoesNotContain("GreyRelay.Api", ProjectReferencesOf(InfrastructureLayer));

    [Theory]
    [InlineData("Microsoft.EntityFrameworkCore")]
    [InlineData("Microsoft.AspNetCore")]
    [InlineData("Microsoft.Data.Sqlite")]
    public void Inner_layers_do_not_reference_frameworks(string frameworkPrefix)
    {
        Assert.DoesNotContain(Domain.GetReferencedAssemblies(), name => name.Name!.StartsWith(frameworkPrefix, StringComparison.Ordinal));
        Assert.DoesNotContain(ApplicationLayer.GetReferencedAssemblies(), name => name.Name!.StartsWith(frameworkPrefix, StringComparison.Ordinal));
    }

    private static string[] ProjectReferencesOf(Assembly assembly) =>
        assembly.GetReferencedAssemblies()
            .Select(name => name.Name!)
            .Where(name => name.StartsWith("GreyRelay.", StringComparison.Ordinal))
            .Order(StringComparer.Ordinal)
            .ToArray();
}
