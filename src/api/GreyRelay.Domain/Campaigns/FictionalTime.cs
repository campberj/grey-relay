using System.Globalization;

namespace GreyRelay.Domain.Campaigns;

/// <summary>
/// When something happened on the campaign clock, or explicitly <see cref="Unknown"/>.
/// Unknown is a first-class value so precision is never fabricated (CONTENT_CONTRACTS section 8).
/// </summary>
public sealed record FictionalTime
{
    private FictionalTime(int? day, string? timeZ, string? precision)
    {
        Day = day;
        TimeZ = timeZ;
        Precision = precision;
    }

    public static FictionalTime Unknown { get; } = new(null, null, null);

    public bool IsKnown => Day is not null;

    /// <summary>Campaign day number; null when unknown.</summary>
    public int? Day { get; }

    /// <summary>Zulu time as HH:mm; null when unknown.</summary>
    public string? TimeZ { get; }

    /// <summary>Optional free-text precision qualifier from the source, e.g. "narrative".</summary>
    public string? Precision { get; }

    public static FictionalTime At(int day, string timeZ, string? precision = null)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(day);
        ArgumentException.ThrowIfNullOrWhiteSpace(timeZ);
        if (!TimeOnly.TryParseExact(timeZ, "HH:mm", CultureInfo.InvariantCulture, DateTimeStyles.None, out _))
        {
            throw new ArgumentException($"Time '{timeZ}' must be HH:mm (00:00-23:59).", nameof(timeZ));
        }

        return new FictionalTime(day, timeZ, string.IsNullOrWhiteSpace(precision) ? null : precision);
    }
}
