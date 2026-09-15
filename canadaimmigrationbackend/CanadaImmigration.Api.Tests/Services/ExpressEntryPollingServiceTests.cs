using CanadaImmigration.Api.Services;
using Xunit;

namespace CanadaImmigration.Api.Tests.Services;

public class ExpressEntryPollingServiceTests
{
    [Theory]
    [InlineData(9, true)]
    [InlineData(12, true)]
    [InlineData(1, true)]
    [InlineData(2, false)]
    [InlineData(8, false)]
    [InlineData(3, false)]
    public void IsWithinSendingWindow_RespectsBrasiliaSixAmToElevenPmWindow(int utcHour, bool expected)
    {
        var utcNow = new DateTimeOffset(2026, 1, 15, utcHour, 0, 0, TimeSpan.Zero);

        var result = ExpressEntryPollingService.IsWithinSendingWindow(utcNow);

        Assert.Equal(expected, result);
    }
}
