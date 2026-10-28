namespace GreyRelay.Api.Logging;

internal static partial class StartupLog
{
    [LoggerMessage(Level = LogLevel.Information, Message = "Campaign database: {DatabasePath}")]
    public static partial void CampaignDatabase(this ILogger logger, string databasePath);
}
