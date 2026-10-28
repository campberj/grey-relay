using GreyRelay.Domain.Campaigns;

namespace GreyRelay.Domain.Tests;

public sealed class FictionalTimeTests
{
    [Fact]
    public void Known_time_keeps_day_time_and_precision()
    {
        var time = FictionalTime.At(2, "18:45", "narrative");

        Assert.True(time.IsKnown);
        Assert.Equal(2, time.Day);
        Assert.Equal("18:45", time.TimeZ);
        Assert.Equal("narrative", time.Precision);
    }

    [Fact]
    public void Unknown_has_no_fabricated_values()
    {
        Assert.False(FictionalTime.Unknown.IsKnown);
        Assert.Null(FictionalTime.Unknown.Day);
        Assert.Null(FictionalTime.Unknown.TimeZ);
        Assert.Null(FictionalTime.Unknown.Precision);
    }

    [Theory]
    [InlineData("24:00")]
    [InlineData("1845")]
    [InlineData("18:45Z")]
    [InlineData("6:45")]
    public void Malformed_time_is_rejected(string timeZ) =>
        Assert.Throws<ArgumentException>(() => FictionalTime.At(2, timeZ));

    [Fact]
    public void Negative_day_is_rejected() =>
        Assert.Throws<ArgumentOutOfRangeException>(() => FictionalTime.At(-1, "18:45"));

    [Fact]
    public void Equal_values_are_equal() =>
        Assert.Equal(FictionalTime.At(1, "20:50"), FictionalTime.At(1, "20:50"));
}
