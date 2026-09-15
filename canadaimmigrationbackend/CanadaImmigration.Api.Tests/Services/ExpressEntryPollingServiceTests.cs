using CanadaImmigration.Api.Services;
using Xunit;

namespace CanadaImmigration.Api.Tests.Services;

public class ExpressEntryPollingServiceTests
{
    [Theory]
    [InlineData(10, true)]
    [InlineData(21, true)]
    [InlineData(9, false)]
    [InlineData(22, false)]
    [InlineData(23, false)]
    public void IsWithinSendingWindow_OnWeekday_RespectsBrasiliaSevenAmToSevenPmWindow(int utcHour, bool expected)
    {
        var utcNow = new DateTimeOffset(2026, 1, 15, utcHour, 0, 0, TimeSpan.Zero);

        var result = ExpressEntryPollingService.IsWithinSendingWindow(utcNow);

        Assert.Equal(expected, result);
    }

    [Fact]
    public void IsWithinSendingWindow_OnSunday_ReturnsFalseEvenDuringBusinessHours()
    {
        var utcNow = new DateTimeOffset(2026, 1, 18, 12, 0, 0, TimeSpan.Zero);

        var result = ExpressEntryPollingService.IsWithinSendingWindow(utcNow);

        Assert.False(result);
    }
}
