using GreyRelay.Infrastructure.Persistence;

namespace GreyRelay.Infrastructure.Tests;

public sealed class DatabasePathResolverTests
{
    private static readonly string ContentRoot = Path.Combine(Path.GetTempPath(), "repo", "src", "api", "GreyRelay.Api");

    [Fact]
    public void Relative_configured_path_resolves_against_content_root() =>
        Assert.Equal(
            Path.Combine(Path.GetTempPath(), "repo", ".data", "greyrelay.dev.db"),
            DatabasePathResolver.Resolve("../../../.data/greyrelay.dev.db", ContentRoot));

    [Fact]
    public void Absolute_configured_path_is_used_as_is()
    {
        var absolute = Path.Combine(Path.GetTempPath(), "elsewhere", "campaigns.db");

        Assert.Equal(absolute, DatabasePathResolver.Resolve(absolute, ContentRoot));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("  ")]
    public void Unconfigured_path_defaults_to_per_user_application_data(string? configured)
    {
        var expected = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData, Environment.SpecialFolderOption.DoNotVerify),
            "GreyRelay",
            "greyrelay.db");

        Assert.Equal(expected, DatabasePathResolver.Resolve(configured, ContentRoot));
    }
}
