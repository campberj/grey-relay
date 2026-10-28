namespace GreyRelay.Infrastructure.Persistence;

/// <summary>
/// Resolves the campaign database location (ADR 0002 section 2.9): the configured path if set,
/// relative paths being resolved against the content root; otherwise the per-user
/// application-data folder (<c>~/Library/Application Support/GreyRelay</c> on macOS).
/// </summary>
public static class DatabasePathResolver
{
    public const string DefaultDirectoryName = "GreyRelay";
    public const string DefaultFileName = "greyrelay.db";

    public static string Resolve(string? configuredPath, string contentRootPath)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(contentRootPath);

        if (!string.IsNullOrWhiteSpace(configuredPath))
        {
            return Path.GetFullPath(configuredPath, contentRootPath);
        }

        var applicationData = Environment.GetFolderPath(
            Environment.SpecialFolder.LocalApplicationData,
            Environment.SpecialFolderOption.DoNotVerify);
        return Path.Combine(applicationData, DefaultDirectoryName, DefaultFileName);
    }
}
